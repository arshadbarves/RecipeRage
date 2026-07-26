namespace RecipeRage
{
    public interface IInteractable
    {
        bool CanInteract(PlayerController player);
        void Interact(PlayerController player);
        string GetPrompt();
    }
}
