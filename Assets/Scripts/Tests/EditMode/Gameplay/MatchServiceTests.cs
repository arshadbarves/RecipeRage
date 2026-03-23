using System.Collections.Generic;
using Gameplay.Match;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class MatchServiceTests
    {
        [Test]
        public void GetQueues_ReturnsKitchenClashLaunchQueuesByDefault()
        {
            MatchService service = new(new DictionaryConfigService());

            IReadOnlyList<MatchQueueDefinition> queues = service.GetQueues();

            Assert.AreEqual(3, queues.Count);
            Assert.AreEqual("classic", queues[0].ModeId);
            Assert.AreEqual(180, queues[0].DurationSeconds);
            Assert.AreEqual("team_battle", queues[1].ModeId);
            Assert.AreEqual(3, queues[1].PlayersPerTeam);
            Assert.AreEqual("ranked", queues[2].ModeId);
            Assert.AreEqual(300, queues[2].DurationSeconds);
        }

        [Test]
        public void TryGetQueue_AppliesRemoteOverridesAndRespectsDisableFlag()
        {
            DictionaryConfigService config = new();
            config.Set("queues.team_battle.duration_seconds", 240);
            config.Set("queues.ranked.enabled", false);

            MatchService service = new(config);

            Assert.IsTrue(service.TryGetQueue("team_battle", out MatchQueueDefinition teamBattle));
            Assert.AreEqual(240, teamBattle.DurationSeconds);
            Assert.IsFalse(service.TryGetQueue("ranked", out _));
        }

        private sealed class DictionaryConfigService : IConfigService
        {
            private readonly Dictionary<string, object> _values = new();

            public void Set(string key, object value)
            {
                _values[key] = value;
            }

            public int GetInt(string key, int defaultValue)
            {
                return _values.TryGetValue(key, out object value) ? int.Parse(value.ToString()) : defaultValue;
            }

            public float GetFloat(string key, float defaultValue)
            {
                return _values.TryGetValue(key, out object value) ? float.Parse(value.ToString()) : defaultValue;
            }

            public bool GetBool(string key, bool defaultValue)
            {
                return _values.TryGetValue(key, out object value) ? bool.Parse(value.ToString()) : defaultValue;
            }

            public string GetString(string key, string defaultValue)
            {
                return _values.TryGetValue(key, out object value) ? value.ToString() : defaultValue;
            }
        }
    }
}
