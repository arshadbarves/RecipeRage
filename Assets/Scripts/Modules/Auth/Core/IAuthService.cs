using Cysharp.Threading.Tasks;

namespace RecipeRage.Modules.Auth.Core
{
    public enum AuthType
    {
        DevAuth,
        DeviceID,
        AccountPortal
    }

    public interface IAuthService
    {
        UniTask<bool> LoginAsync(AuthType type);
        UniTask LogoutAsync();
        bool IsLoggedIn();
        string GetCurrentUserId();
    }
}
