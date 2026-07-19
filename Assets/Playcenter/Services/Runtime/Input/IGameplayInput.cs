namespace Playcenter.Services
{
    /// <summary>
    /// Read-only local gameplay input snapshot. Engine-free port.
    /// </summary>
    public interface IGameplayInput
    {
        InputAxis2 Move { get; }

        bool InteractPressed { get; }

        bool AbilityPressed { get; }
    }
}
