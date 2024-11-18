namespace Core.GameFramework.Event.Core
{
    public interface IEventHandler
    {
        void HandleEvent(IGameEvent gameEvent);
    }
}