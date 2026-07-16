using Cysharp.Threading.Tasks;
using Playcenter.UI;

namespace Playcenter.UI.Toolkit
{
    /// <summary>
    /// Interface for notification/toast screen.
    /// </summary>
    public interface INotificationScreen
    {
        UniTask Show(string message, NotificationType type, float duration);
        UniTask Show(string title, string message, NotificationType type, float duration);
    }
}
