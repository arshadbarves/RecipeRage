using Cysharp.Threading.Tasks;

namespace Core.UI.Interfaces
{
    public interface INotificationScreen
    {
        UniTask Show(string message, NotificationType type, float duration);
        UniTask Show(string title, string message, NotificationType type, float duration);
    }
}
