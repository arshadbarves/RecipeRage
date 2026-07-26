using System;
using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    /// <summary>
    /// Read-only match surface for HUD and results UI.
    /// Hides NGO NetworkBehaviours and scene MonoBehaviours from Presentation.
    /// </summary>
    public interface IMatchHudPort
    {
        void Refresh();

        ulong? LocalClientId { get; }
        int LocalTeamId { get; }

        int GetLocalPlayerScore();
        int GetTeamScore(int teamId);

        float TimeRemaining { get; }
        bool IsTimerRunning { get; }

        GamePhase CurrentPhase { get; }

        bool HasMatchResult { get; }
        MatchResultSnapshot CurrentMatchResult { get; }

        IReadOnlyList<RecipeOrderState> GetActiveOrders();
        string GetRecipeDisplayName(int recipeId);

        /// <summary>
        /// Returns true when the local player can interact with something in front of them.
        /// </summary>
        bool TryGetInteractionPrompt(out string prompt);

        event Action<ulong, int> PlayerScoreUpdated;
        event Action<float> TimeUpdated;
        event Action TimerExpired;
        event Action<GamePhase, GamePhase> PhaseChanged;
        event Action<MatchResultSnapshot, MatchResultSnapshot> MatchResultChanged;
        event Action<RecipeOrderState> OrderCreated;
        event Action<RecipeOrderState> OrderCompleted;
        event Action<RecipeOrderState> OrderExpired;

        void Subscribe();
        void Unsubscribe();
    }
}
