namespace KitchenClash.Application.Services
{
    /// <summary>
    /// Interface for objects that can be interacted with by players.
    /// Uses object parameter to avoid Application→Infrastructure dependency.
    /// Implementations should cast to PlayerController.
    /// </summary>
    public interface IInteractable
    {
        void Interact(object player);
        string GetInteractionPrompt();
        bool CanInteract(object player);
    }
}
