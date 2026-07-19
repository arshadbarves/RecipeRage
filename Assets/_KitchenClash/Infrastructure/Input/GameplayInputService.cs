using Playcenter.Services;

namespace KitchenClash.Infrastructure.Input
{
    /// <summary>
    /// Mutable local-device gameplay input snapshot.
    /// Fed by the existing <c>IInputProvider</c> path (PlayerController) via
    /// <see cref="IGameplayInputPublisher"/> — does not replace that pipeline.
    /// </summary>
    public sealed class GameplayInputService : IGameplayInput, IGameplayInputPublisher
    {
        public InputAxis2 Move { get; private set; } = InputAxis2.Zero;

        public bool InteractPressed { get; private set; }

        public bool AbilityPressed { get; private set; }

        public void Publish(InputAxis2 move, bool interactPressed, bool abilityPressed)
        {
            Move = move;
            InteractPressed = interactPressed;
            AbilityPressed = abilityPressed;
        }

        public void Clear()
        {
            Move = InputAxis2.Zero;
            InteractPressed = false;
            AbilityPressed = false;
        }
    }
}