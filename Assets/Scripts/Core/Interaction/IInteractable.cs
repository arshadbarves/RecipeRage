using System;
using RecipeRage.Core.Player;

namespace RecipeRage.Core.Interaction
{
    /// <summary>
    /// Interface for objects that can be interacted with by players
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Whether the object can currently be interacted with
        /// </summary>
        bool CanInteract { get; }

        /// <summary>
        /// The type of interaction this object provides
        /// </summary>
        InteractionType InteractionType { get; }

        /// <summary>
        /// The current state of the interaction
        /// </summary>
        InteractionState CurrentState { get; }

        /// <summary>
        /// Start an interaction with this object
        /// </summary>
        /// <param name="player"> The player starting the interaction </param>
        /// <param name="onComplete"> Callback when interaction completes </param>
        /// <returns> True if interaction started successfully </returns>
        bool StartInteraction(PlayerController player, Action onComplete);

        /// <summary>
        /// Cancel the current interaction
        /// </summary>
        /// <param name="player"> The player canceling the interaction </param>
        void CancelInteraction(PlayerController player);

        /// <summary>
        /// Continue an existing interaction
        /// </summary>
        /// <param name="player"> The player continuing the interaction </param>
        /// <returns> True if interaction can continue </returns>
        bool ContinueInteraction(PlayerController player);
    }

    /// <summary>
    /// Types of interactions available in the game
    /// </summary>
    public enum InteractionType
    {
        None,
        Cook,
        Container,
        Plate,
        Trash,
        Serve
    }

    /// <summary>
    /// Possible states of an interaction
    /// </summary>
    public enum InteractionState
    {
        Idle,
        InProgress,
        Completed,
        Canceled
    }
}