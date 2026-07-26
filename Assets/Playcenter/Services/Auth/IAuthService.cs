using System.Collections;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Authentication. Providers: Facebook, Google, Guest. NO Epic account login.
    /// </summary>
    public interface IAuthService
    {
        bool IsReady { get; }
        bool IsSignedIn { get; }
        string UserId { get; }
        string DisplayName { get; }

        IEnumerator Initialize();
        Task<AuthResult> SignInWithFacebook();
        Task<AuthResult> SignInWithGoogle();
        Task<AuthResult> SignInAsGuest();
        void SignOut();
    }

    public readonly struct AuthResult
    {
        public bool Success { get; }
        public string UserId { get; }
        public string DisplayName { get; }
        public string Error { get; }

        public AuthResult(bool success, string userId, string displayName, string error = null)
        {
            Success = success;
            UserId = userId;
            DisplayName = displayName;
            Error = error;
        }
    }
}
