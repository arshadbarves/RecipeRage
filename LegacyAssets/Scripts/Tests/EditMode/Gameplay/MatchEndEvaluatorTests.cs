using System;
using System.Collections.Generic;
using System.Reflection;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Network;
using KitchenClash.Presentation.Screens;
using KitchenClash.Presentation.ViewModels;
using NUnit.Framework;
using Playcenter.GameFlow;
using RecipeRage.Tests.EditMode.Gameplay.Fakes;
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

            UnityEngine.Object.DestroyImmediate(gameObject);
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

            UnityEngine.Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void GameplayHudViewModel_DoesNotTransitionToGameOver_WhenPhaseChangesWithoutResult()
        {
            FakeAppFlow appFlow = new();
            FakeMatchHudPort hud = new() { CurrentPhase = GamePhase.GameOver, HasMatchResult = false };
            GameplayHudViewModel viewModel = new(hud, appFlow);

            MethodInfo handlePhaseChanged = typeof(GameplayHudViewModel).GetMethod(
                "HandlePhaseChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(handlePhaseChanged);

            handlePhaseChanged.Invoke(viewModel, new object[] { GamePhase.Playing, GamePhase.GameOver });

            Assert.AreEqual(0, appFlow.NotifyMatchCompletedCount);
        }

        [Test]
        public void GameplayHudViewModel_TransitionsToGameOver_WhenPhaseAndResultAreReady()
        {
            FakeAppFlow appFlow = new();
            MatchResultSnapshot result = new(true, -1, 1200, true, MatchEndReason.TimerExpired);
            FakeMatchHudPort hud = new()
            {
                CurrentPhase = GamePhase.GameOver,
                HasMatchResult = true,
                CurrentMatchResult = result
            };
            GameplayHudViewModel viewModel = new(hud, appFlow);

            MethodInfo handlePhaseChanged = typeof(GameplayHudViewModel).GetMethod(
                "HandlePhaseChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo handleMatchResultChanged = typeof(GameplayHudViewModel).GetMethod(
                "HandleMatchResultChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(handlePhaseChanged);
            Assert.IsNotNull(handleMatchResultChanged);

            handlePhaseChanged.Invoke(viewModel, new object[] { GamePhase.Playing, GamePhase.GameOver });
            handleMatchResultChanged.Invoke(viewModel, new object[] { MatchResultSnapshot.None, result });
            handleMatchResultChanged.Invoke(viewModel, new object[] { MatchResultSnapshot.None, result });

            Assert.AreEqual(1, appFlow.NotifyMatchCompletedCount);
        }

        [Test]
        public void GameplayHudViewModel_TransitionsToGameOver_WhenResultArrivesBeforePhase()
        {
            FakeAppFlow appFlow = new();
            MatchResultSnapshot result = new(true, 0, 1000, false, MatchEndReason.ScoreLimitReached);
            FakeMatchHudPort hud = new()
            {
                CurrentPhase = GamePhase.Playing,
                HasMatchResult = true,
                CurrentMatchResult = result
            };
            GameplayHudViewModel viewModel = new(hud, appFlow);

            MethodInfo handlePhaseChanged = typeof(GameplayHudViewModel).GetMethod(
                "HandlePhaseChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo handleMatchResultChanged = typeof(GameplayHudViewModel).GetMethod(
                "HandleMatchResultChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(handlePhaseChanged);
            Assert.IsNotNull(handleMatchResultChanged);

            handleMatchResultChanged.Invoke(viewModel, new object[] { MatchResultSnapshot.None, result });

            Assert.AreEqual(0, appFlow.NotifyMatchCompletedCount);

            hud.CurrentPhase = GamePhase.GameOver;
            handlePhaseChanged.Invoke(viewModel, new object[] { GamePhase.Playing, GamePhase.GameOver });

            Assert.AreEqual(1, appFlow.NotifyMatchCompletedCount);
        }

        [TestCase(0, false, "TEAM 1 WINS!")]
        [TestCase(1, false, "TEAM 2 WINS!")]
        [TestCase(-1, true, "DRAW!")]
        public void ResultsScreen_GetWinnerText_MapsResultToExpectedLabel(int winningTeamId, bool isDraw, string expected)
        {
            MatchResultSnapshot result = new(
                true,
                winningTeamId,
                1000,
                isDraw,
                MatchEndReason.TimerExpired);

            Assert.AreEqual(expected, ResultsScreen.GetWinnerText(result));
        }

        [Test]
        public void ResultsScreen_GetWinnerText_UsesNeutralFallback_WhenResultIsMissing()
        {
            Assert.AreEqual("MATCH COMPLETE", ResultsScreen.GetWinnerText(MatchResultSnapshot.None));
        }

        private sealed class FakeMatchHudPort : IMatchHudPort
        {
            public ulong? LocalClientId { get; set; }
            public int LocalTeamId { get; set; }
            public float TimeRemaining { get; set; }
            public bool IsTimerRunning { get; set; }
            public GamePhase CurrentPhase { get; set; } = GamePhase.Waiting;
            public bool HasMatchResult { get; set; }
            public MatchResultSnapshot CurrentMatchResult { get; set; } = MatchResultSnapshot.None;

            public event Action<ulong, int> PlayerScoreUpdated;
            public event Action<float> TimeUpdated;
            public event Action TimerExpired;
            public event Action<GamePhase, GamePhase> PhaseChanged;
            public event Action<MatchResultSnapshot, MatchResultSnapshot> MatchResultChanged;
            public event Action<RecipeOrderState> OrderCreated;
            public event Action<RecipeOrderState> OrderCompleted;
            public event Action<RecipeOrderState> OrderExpired;

            public void Refresh()
            {
            }

            public int GetLocalPlayerScore() => 0;
            public int GetTeamScore(int teamId) => 0;
            public IReadOnlyList<RecipeOrderState> GetActiveOrders() => Array.Empty<RecipeOrderState>();
            public string GetRecipeDisplayName(int recipeId) => null;
            public bool TryGetInteractionPrompt(out string prompt)
            {
                prompt = string.Empty;
                return false;
            }

            public void Subscribe()
            {
            }

            public void Unsubscribe()
            {
            }
        }
    }
}
