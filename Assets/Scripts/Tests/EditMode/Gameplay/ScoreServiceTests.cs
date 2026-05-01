using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Domain;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class ScoreServiceTests
    {
        private DictionaryConfigService _cfg;
        private ScoreService _svc;

        [SetUp]
        public void SetUp()
        {
            _cfg = new DictionaryConfigService();
            _svc = new ScoreService(_cfg, new NullEventBus());
        }

        // --- Task 1: Base score = 10 ---

        [Test]
        public void BaseScore_DefaultIs10()
        {
            var e = new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 1f);
            _svc.AddScore(TeamId.TeamA, e);
            Assert.AreEqual(10, _svc.TeamAScore);
        }

        [Test]
        public void BaseScore_OverriddenViaConfig()
        {
            _cfg.Set("score_base", 20);
            var e = new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 1f);
            _svc.AddScore(TeamId.TeamA, e);
            Assert.AreEqual(20, _svc.TeamAScore);
        }

        // --- Task 3: Tier multipliers ---

        [Test]
        public void Tier1_MultiplierIs1x()
        {
            var e = new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 1f);
            _svc.AddScore(TeamId.TeamA, e);
            Assert.AreEqual(10, _svc.TeamAScore);
        }

        [Test]
        public void Tier2_MultiplierIs1_5x()
        {
            var e = new ScoreEvent(ScoreEventType.DishServed, recipeTier: 2, speedRatio: 1f);
            _svc.AddScore(TeamId.TeamA, e);
            Assert.AreEqual(15, _svc.TeamAScore); // 10 * 1.5
        }

        [Test]
        public void Tier3_MultiplierIs2x()
        {
            var e = new ScoreEvent(ScoreEventType.DishServed, recipeTier: 3, speedRatio: 1f);
            _svc.AddScore(TeamId.TeamA, e);
            Assert.AreEqual(20, _svc.TeamAScore); // 10 * 2.0
        }

        // --- Task 3: Speed bonus ---

        [Test]
        public void SpeedBonus_Plus5_WhenUnder50Percent()
        {
            var e = new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 0.3f);
            _svc.AddScore(TeamId.TeamA, e);
            Assert.AreEqual(15, _svc.TeamAScore); // 10 + 5
        }

        [Test]
        public void SpeedBonus_Plus3_WhenUnder75Percent()
        {
            var e = new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 0.6f);
            _svc.AddScore(TeamId.TeamA, e);
            Assert.AreEqual(13, _svc.TeamAScore); // 10 + 3
        }

        [Test]
        public void SpeedBonus_Zero_WhenOver75Percent()
        {
            var e = new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 0.8f);
            _svc.AddScore(TeamId.TeamA, e);
            Assert.AreEqual(10, _svc.TeamAScore);
        }

        // --- Task 3: Rhythm bonus ---

        [Test]
        public void RhythmBonus_Plus1_WhenActive()
        {
            var e = new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 1f, rhythmBonus: true);
            _svc.AddScore(TeamId.TeamA, e);
            Assert.AreEqual(11, _svc.TeamAScore); // 10 + 1
        }

        // --- Task 3: Combo bonus ---

        [Test]
        public void ComboBonus_Plus2_WhenComboCount3OrMore()
        {
            var e = new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 1f, comboCount: 3);
            _svc.AddScore(TeamId.TeamA, e);
            Assert.AreEqual(12, _svc.TeamAScore); // 10 + 2
        }

        [Test]
        public void ComboBonus_Zero_WhenComboCountBelow3()
        {
            var e = new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 1f, comboCount: 2);
            _svc.AddScore(TeamId.TeamA, e);
            Assert.AreEqual(10, _svc.TeamAScore);
        }

        // --- Task 3: Penalties ---

        [Test]
        public void BurnPenalty_Minus2()
        {
            // Give some score first so penalty is visible
            _svc.AddScore(TeamId.TeamA, new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 1f));
            _svc.AddScore(TeamId.TeamA, new ScoreEvent(ScoreEventType.BurnedServed));
            Assert.AreEqual(8, _svc.TeamAScore); // 10 - 2
        }

        [Test]
        public void FirePenalty_Minus5()
        {
            _svc.AddScore(TeamId.TeamA, new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 1f));
            _svc.AddScore(TeamId.TeamA, new ScoreEvent(ScoreEventType.FirePenalty));
            Assert.AreEqual(5, _svc.TeamAScore); // 10 - 5
        }

        [Test]
        public void Score_NeverGoesBelowZero()
        {
            _svc.AddScore(TeamId.TeamA, new ScoreEvent(ScoreEventType.FirePenalty));
            Assert.AreEqual(0, _svc.TeamAScore);
        }

        // --- Task 2: Rush mode ---

        [Test]
        public void RushMode_DefaultFalse()
        {
            Assert.IsFalse(_svc.IsRushMode);
        }

        [Test]
        public void SetRushMode_ActivatesRush()
        {
            _svc.SetRushMode(true);
            Assert.IsTrue(_svc.IsRushMode);
        }

        [Test]
        public void RushMode_MultipliesScoreBy1_5x()
        {
            _svc.SetRushMode(true);
            var e = new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 1f);
            _svc.AddScore(TeamId.TeamA, e);
            Assert.AreEqual(15, _svc.TeamAScore); // 10 * 1.5
        }

        [Test]
        public void RushMode_MultipliesPenaltiesBy1_5x()
        {
            _svc.AddScore(TeamId.TeamA, new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 1f));
            _svc.AddScore(TeamId.TeamA, new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 1f));
            _svc.SetRushMode(true);
            _svc.AddScore(TeamId.TeamA, new ScoreEvent(ScoreEventType.BurnedServed));
            // 10 + 10 - (2*1.5=3) = 17
            Assert.AreEqual(17, _svc.TeamAScore);
        }

        // --- Task 4: UpdateMatchTime auto-activates rush ---

        [Test]
        public void UpdateMatchTime_ActivatesRushAtThreshold()
        {
            Assert.IsFalse(_svc.IsRushMode);
            _svc.UpdateMatchTime(60f);
            Assert.IsTrue(_svc.IsRushMode);
        }

        [Test]
        public void UpdateMatchTime_DoesNotActivateAboveThreshold()
        {
            _svc.UpdateMatchTime(61f);
            Assert.IsFalse(_svc.IsRushMode);
        }

        [Test]
        public void UpdateMatchTime_CustomThreshold()
        {
            _cfg.Set("rush_mode_threshold_sec", 30f);
            _svc.UpdateMatchTime(30f);
            Assert.IsTrue(_svc.IsRushMode);
        }

        // --- End-of-match plate bonus ---

        [Test]
        public void EndOfMatchBonus_TopTeamGets10Percent()
        {
            // Score 100 points
            for (int i = 0; i < 10; i++)
                _svc.AddScore(TeamId.TeamA, new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 1f));

            int bonus = _svc.CalculateEndOfMatchBonus(TeamId.TeamA);
            Assert.AreEqual(10, bonus); // 100 * 0.10
        }

        // --- Combined scenario ---

        [Test]
        public void FullScoring_Tier2_FastDelivery_Rhythm_Combo_Rush()
        {
            _svc.SetRushMode(true);
            var e = new ScoreEvent(ScoreEventType.DishServed, recipeTier: 2, speedRatio: 0.3f,
                rhythmBonus: true, comboCount: 4);
            _svc.AddScore(TeamId.TeamA, e);
            // base: 10*1.5=15, speed: +5, rhythm: +1, combo: +2 = 23
            // rush: 23*1.5 = 34 (truncated)
            Assert.AreEqual(34, _svc.TeamAScore);
        }

        [Test]
        public void OnScoreChanged_FiresOnAddScore()
        {
            ScoreChangedEvent fired = null;
            _svc.OnScoreChanged += e => fired = e;
            _svc.AddScore(TeamId.TeamB, new ScoreEvent(ScoreEventType.DishServed, recipeTier: 1, speedRatio: 1f));
            Assert.IsNotNull(fired);
            Assert.AreEqual(TeamId.TeamB, fired.Team);
        }

        // --- Helpers ---

        private sealed class DictionaryConfigService : IConfigService
        {
            private readonly Dictionary<string, object> _values = new();

            public void Set(string key, object value) => _values[key] = value;

            public T Get<T>(string key, T fallback)
            {
                if (_values.TryGetValue(key, out object value))
                {
                    try { return (T)Convert.ChangeType(value, typeof(T)); }
                    catch { return fallback; }
                }
                return fallback;
            }

            public Task FetchAsync() => Task.CompletedTask;
        }

        private sealed class NullEventBus : IEventBus
        {
            public void Publish<T>(T evt) where T : class { }
            public void Subscribe<T>(Action<T> handler) where T : class { }
            public void Unsubscribe<T>(Action<T> handler) where T : class { }
            public void ClearAllSubscriptions() { }
        }
    }
}
