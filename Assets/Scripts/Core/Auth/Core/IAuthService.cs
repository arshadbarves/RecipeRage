using Cysharp.Threading.Tasks;

namespace Core.Core.Auth.Core
{
    public enum AuthType
    {
        DeviceID,
        // Future: Google, Facebook
    }

    public interface IAuthService
    {
        UniTask<bool> LoginAsync(AuthType type);
        UniTask LogoutAsync();
        bool IsLoggedIn();
        string GetCurrentUserId();
    }
}