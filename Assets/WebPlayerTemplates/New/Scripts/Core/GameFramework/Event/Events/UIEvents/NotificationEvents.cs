using Core.GameFramework.Event.Core;

namespace Core.GameFramework.Event.Events.UIEvents
{
    public class NotificationEvents
    {
        public class ShowNotificationEvent : IGameEvent
        {

            public ShowNotificationEvent(string message, float duration = 3f)
            {
                Message = message;
                Duration = duration;
            }
            public string Message { get; }
            public float Duration { get; }
            public bool IsNetworked => false;
        }
    }
}