using KitchenClash.Domain;
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace KitchenClash.Application
{
    /// <summary>
    /// GDD v3 Auth interface. EOS Connect primary, social logins link to EOS.
    /// </summary>
    public interface IAuthService
    {
        // ── GDD v3 contract ──
        Task<AuthResult> LoginAsGuestAsync();
        Task<AuthResult> LoginWithGoogleAsync();
        Task<AuthResult> LoginWithFacebookAsync();
        Task<AuthResult> LoginWithAppleAsync();
        Task LinkToGoogleAsync();
        string ProductUserId { get; }
        bool IsGuest { get; }
        event Action<AuthResult> OnAuthChanged;

        // ── Legacy members (kept for backward compat, callers migrating) ──
        bool IsInitialized { get; }
        bool IsSignedIn { get; }
        bool IsUgsSignedIn { get; }
        string PlayerId { get; }
        string EosProductUserId { get; }
        UniTask<bool> InitializeAsync();
        UniTask<bool> LoginAsync(AuthType type);
        UniTask LogoutAsync();
    }
}
