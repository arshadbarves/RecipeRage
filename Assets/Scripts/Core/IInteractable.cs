using UnityEngine;

namespace RecipeRage.Core
{
    /// <summary>
    /// Interface for objects that can be interacted with by the player.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Called when a player interacts with this object.
        /// </summary>
        /// <param name="interactor">The GameObject that is interacting with this object.</param>
        void Interact(GameObject interactor);
        
        /// <summary>
        /// Get the interaction prompt text for this object.
        /// </summary>
        /// <returns>The interaction prompt text.</returns>
        string GetInteractionPrompt();
        
        /// <summary>
        /// Check if this object can be interacted with.
        /// </summary>
        /// <param name="interactor">The GameObject that is trying to interact with this object.</param>
        /// <returns>True if the object can be interacted with.</returns>
        bool CanInteract(GameObject interactor);
    }
}
