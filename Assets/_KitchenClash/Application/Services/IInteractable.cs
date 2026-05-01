namespace KitchenClash.Application.Services
{
    public interface IInteractable
    {
        void Interact(object player);
        string GetInteractionPrompt();
        bool CanInteract(object player);
    }
}
