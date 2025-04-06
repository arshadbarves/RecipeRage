namespace RecipeRage.Core.Characters
{
    /// <summary>
    /// Interface for objects that can be interacted with by the player.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Interact with the object.
        /// </summary>
        /// <param name="player">The player that is interacting</param>
        void Interact(PlayerController player);
        
        /// <summary>
        /// Get the interaction prompt text.
        /// </summary>
        /// <returns>The interaction prompt text</returns>
        string GetInteractionPrompt();
        
        /// <summary>
        /// Check if the object can be interacted with.
        /// </summary>
        /// <param name="player">The player that wants to interact</param>
        /// <returns>True if the object can be interacted with</returns>
        bool CanInteract(PlayerController player);
    }
}
