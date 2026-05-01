using Cysharp.Threading.Tasks;

namespace KitchenClash.Application.Services
{
    public interface INotificationScreen
    {
        UniTask Show(string message, float duration);
        UniTask Show(string title, string message, float duration);
    }
}
