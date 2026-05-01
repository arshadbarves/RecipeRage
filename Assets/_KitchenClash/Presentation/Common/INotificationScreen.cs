using Cysharp.Threading.Tasks;

namespace KitchenClash.Presentation.Common
{
    /// <summary>
    /// Interface for notification/toast screen
    /// </summary>
    public interface INotificationScreen
    {
        UniTask Show(string message, NotificationType type, float duration);
        UniTask Show(string title, string message, NotificationType type, float duration);
    }
}
