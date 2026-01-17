using Cysharp.Threading.Tasks;

namespace Core.Auth
{
    public enum AuthType
    {
        DeviceID,
        // Future: Google, Facebook
    }

    public interface IAuthService
    {
        // State
        bool IsInitialized { get; }
        bool IsSignedIn { get; }
        bool IsUgsSignedIn { get; }
        
        // Identity
        string PlayerId { get; } // Unity/UGS PlayerId
        string EosProductUserId { get; }

        // Core Methods
        UniTask<bool> InitializeAsync();
        UniTask<bool> LoginAsync(AuthType type);
        UniTask LogoutAsync();
    }
}