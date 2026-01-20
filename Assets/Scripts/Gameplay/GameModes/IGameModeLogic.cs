using Gameplay.Cooking;
using Gameplay.Scoring;

namespace Gameplay.GameModes
{
    /// <summary>
    /// Interface for game mode logic implementations.
    /// Pure C# class that defines game rules, phases, and win conditions.
    /// </summary>
    public interface IGameModeLogic
    {
        /// <summary>
        /// Initialize the game mode with configuration and manager references
        /// </summary>
        void Initialize(GameMode config, OrderManager orderManager, ScoreManager scoreManager);

        /// <summary>
        /// Called when the match starts
        /// </summary>
        void OnMatchStart();

        /// <summary>
        /// Called when the match ends
        /// </summary>
        void OnMatchEnd();

        /// <summary>
        /// Update called every frame
        /// </summary>
        void Update(float deltaTime);

        /// <summary>
        /// Current game phase
        /// </summary>
        GamePhase CurrentPhase { get; }

        /// <summary>
        /// Time remaining in current phase (seconds)
        /// </summary>
        float TimeRemaining { get; }
    }
}
