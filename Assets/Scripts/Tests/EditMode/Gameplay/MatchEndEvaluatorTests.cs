using System.Reflection;
using Gameplay;
using Gameplay.App.State;
using Gameplay.App.State.States;
using Gameplay.Characters;
using Gameplay.Cooking;
using Gameplay.GameModes;
using Gameplay.Scoring;
using Gameplay.Shared;
using Gameplay.Spawning;
using Gameplay.UI;
using Gameplay.UI.Features.GameOver;
using Gameplay.UI.Features.Gameplay;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public class MatchEndEvaluatorTests
    {
        [Test]
        public void EvaluateScoreLimit_ReturnsNoEnd_WhenScoresAreBelowTarget()
        {
            MatchEndEvaluation result = MatchEndEvaluator.EvaluateScoreLimit(new[] { 800, 950 }, true, 1000);

            Assert.IsFalse(result.ShouldEnd);
        }

        [Test]
        public void EvaluateScoreLimit_EndsMatch_WhenTeamHitsExactTarget()
        {
            MatchEndEvaluation result = MatchEndEvaluator.EvaluateScoreLimit(new[] { 1000, 900 }, true, 1000);

            Assert.IsTrue(result.ShouldEnd);
            Assert.IsFalse(result.IsDraw);
            Assert.AreEqual(0, result.WinningTeamId);
        }

        [Test]
        public void EvaluateScoreLimit_UsesHighestScore_WhenMultipleTeamsExceedTarget()
        {
            MatchEndEvaluation result = MatchEndEvaluator.EvaluateScoreLimit(new[] { 1500, 1700 }, true, 1000);

            Assert.IsTrue(result.ShouldEnd);
            Assert.AreEqual(1, result.WinningTeamId);
            Assert.AreEqual(1700, result.WinningScore);
        }

        [Test]
        public void EvaluateFinalScores_ReturnsDraw_WhenTopScoresAreEqual()
        {
            MatchEndEvaluation result = MatchEndEvaluator.EvaluateFinalScores(new[] { 1200, 1200 });

            Assert.IsTrue(result.ShouldEnd);
            Assert.IsTrue(result.IsDraw);
            Assert.AreEqual(-1, result.WinningTeamId);
        }

        [Test]
        public void RoundTimer_DirectMethods_UpdateState_WhenUnspawned()
        {
            GameObject gameObject = new("RoundTimerTest");
            RoundTimer timer = gameObject.AddComponent<RoundTimer>();

            timer.StartTimer(120f);

            Assert.AreEqual(120f, timer.TimeRemaining);
            Assert.IsTrue(timer.IsRunning);

            timer.StopTimer();

            Assert.AreEqual(0f, timer.TimeRemaining);
            Assert.IsFalse(timer.IsRunning);

            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void MatchResultSync_DirectMethods_UpdateState_WhenUnspawned()
        {
            GameObject gameObject = new("MatchResultSyncTest");
            MatchResultSync resultSync = gameObject.AddComponent<MatchResultSync>();

            MatchResultState result = MatchResultState.FromEvaluation(
                MatchEndReason.ScoreLimitReached,
                new MatchEndEvaluation(true, 1, false, 1700));

            resultSync.SetResult(result);

            Assert.IsTrue(resultSync.HasResult);
            Assert.AreEqual(1, resultSync.CurrentResult.WinningTeamId);
            Assert.AreEqual(1700, resultSync.CurrentResult.WinningScore);
            Assert.AreEqual(MatchEndReason.ScoreLimitReached, resultSync.CurrentResult.EndReason);

            resultSync.ClearResult();

            Assert.IsFalse(resultSync.HasResult);
            Assert.IsFalse(resultSync.CurrentResult.HasResult);

            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void GameplayHudViewModel_DoesNotTransitionToGameOver_WhenPhaseChangesWithoutResult()
        {
            FakeGameStateManager stateManager = new();
            GameplayHudViewModel viewModel = new(new FakeMatchContext(), stateManager);

            GamePhaseSync phaseSync = CreatePhaseSync(GamePhase.GameOver);
            SetPrivateField(viewModel, "_gamePhaseSync", phaseSync);

            MethodInfo handlePhaseChanged = typeof(GameplayHudViewModel).GetMethod(
                "HandlePhaseChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(handlePhaseChanged);

            handlePhaseChanged.Invoke(viewModel, new object[] { GamePhase.Playing, GamePhase.GameOver });

            Assert.AreEqual(0, stateManager.GameOverTransitions);

            Object.DestroyImmediate(phaseSync.gameObject);
        }

        [Test]
        public void GameplayHudViewModel_TransitionsToGameOver_WhenPhaseAndResultAreReady()
        {
            FakeGameStateManager stateManager = new();
            GameplayHudViewModel viewModel = new(new FakeMatchContext(), stateManager);
            GamePhaseSync phaseSync = CreatePhaseSync(GamePhase.GameOver);
            MatchResultSync resultSync = CreateResultSync(MatchResultState.FromEvaluation(
                MatchEndReason.TimerExpired,
                new MatchEndEvaluation(true, -1, true, 1200)));

            SetPrivateField(viewModel, "_gamePhaseSync", phaseSync);
            SetPrivateField(viewModel, "_matchResultSync", resultSync);

            MethodInfo handlePhaseChanged = typeof(GameplayHudViewModel).GetMethod(
                "HandlePhaseChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo handleMatchResultChanged = typeof(GameplayHudViewModel).GetMethod(
                "HandleMatchResultChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(handlePhaseChanged);
            Assert.IsNotNull(handleMatchResultChanged);

            handlePhaseChanged.Invoke(viewModel, new object[] { GamePhase.Playing, GamePhase.GameOver });
            handleMatchResultChanged.Invoke(viewModel, new object[] { MatchResultState.None, resultSync.CurrentResult });
            handleMatchResultChanged.Invoke(viewModel, new object[] { MatchResultState.None, resultSync.CurrentResult });

            Assert.AreEqual(1, stateManager.GameOverTransitions);

            Object.DestroyImmediate(phaseSync.gameObject);
            Object.DestroyImmediate(resultSync.gameObject);
        }

        [TestCase(0, false, "TEAM 1 WINS!")]
        [TestCase(1, false, "TEAM 2 WINS!")]
        [TestCase(-1, true, "DRAW!")]
        public void GameOverScreen_GetWinnerText_MapsResultToExpectedLabel(int winningTeamId, bool isDraw, string expected)
        {
            MatchResultState result = new MatchResultState
            {
                HasResult = true,
                WinningTeamId = winningTeamId,
                WinningScore = 1000,
                IsDraw = isDraw,
                EndReason = MatchEndReason.TimerExpired
            };

            Assert.AreEqual(expected, GameOverScreen.GetWinnerText(result));
        }

        private sealed class FakeGameStateManager : IGameStateManager
        {
            public int GameOverTransitions { get; private set; }
            public IState CurrentState => null;
            public IState PreviousState => null;
            public event System.Action<IState, IState> OnStateChanged;

            public void Initialize(IState initialState)
            {
            }

            public void ChangeState(IState newState)
            {
            }

            public void ChangeState<T>() where T : IState
            {
                if (typeof(T) == typeof(GameOverState))
                {
                    GameOverTransitions++;
                }
            }

            public void Update(float deltaTime)
            {
            }

            public void FixedUpdate(float fixedDeltaTime)
            {
            }
        }

        private sealed class FakeMatchContext : IMatchContext
        {
            public NetworkManager NetworkManager => null;
            public ulong? LocalClientId => null;
            public int? LocalTeamId => null;
            public PlayerController LocalPlayer => null;
            public NetworkScoreManager NetworkScoreManager => null;
            public RoundTimer RoundTimer => null;
            public GamePhaseSync GamePhaseSync => null;
            public MatchResultSync MatchResultSync => null;
            public OrderManager OrderManager => null;
            public ScoreManager ScoreManager => null;
            public MobileControlsManager MobileControlsManager => null;
            public SpawnManager SpawnManager => null;
            public IngredientNetworkSpawner IngredientNetworkSpawner => null;
            public IBotKitchenRuntime BotKitchenRuntime => null;
            public IKitchenSupportRuntime KitchenSupportRuntime => null;
            public bool IsHost => false;
            public bool IsServer => false;
            public bool IsClient => false;

            public void Refresh()
            {
            }

            public void ShutdownNetworkSession()
            {
            }

            public bool TryGetSpawnedObject(ulong networkObjectId, out NetworkObject networkObject)
            {
                networkObject = null;
                return false;
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}' to exist.");
            field.SetValue(target, value);
        }

        private static GamePhaseSync CreatePhaseSync(GamePhase phase)
        {
            GameObject gameObject = new("GamePhaseSyncTest");
            GamePhaseSync phaseSync = gameObject.AddComponent<GamePhaseSync>();
            NetworkVariable<GamePhase> currentPhase = new(phase);
            SetPrivateField(phaseSync, "_currentPhase", currentPhase);
            return phaseSync;
        }

        private static MatchResultSync CreateResultSync(MatchResultState result)
        {
            GameObject gameObject = new("MatchResultSyncReadyTest");
            MatchResultSync resultSync = gameObject.AddComponent<MatchResultSync>();
            resultSync.SetResult(result);
            return resultSync;
        }
    }
}
