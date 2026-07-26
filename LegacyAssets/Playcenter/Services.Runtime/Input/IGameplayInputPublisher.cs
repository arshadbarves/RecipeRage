namespace Playcenter.Services
{
    /// <summary>
    /// Write side for local device input feeding <see cref="IGameplayInput"/>.
    /// </summary>
    public interface IGameplayInputPublisher
    {
        void Publish(InputAxis2 move, bool interactPressed, bool abilityPressed);
    }
}
