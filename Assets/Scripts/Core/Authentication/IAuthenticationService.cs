using System;
using Cysharp.Threading.Tasks;

namespace Core.Authentication
{
    /// <summary>
    /// Interface for authentication operations
    /// </summary>
    public interface IAuthenticationService
    {
        bool IsLoggedIn { get; }
        string LastLoginMethod { get; }
        
        UniTask<bool> AttemptAutoLoginAsync();
        UniTask<bool> LoginAsGuestAsync();
        UniTask<bool> LoginWithFacebookAsync();
        void Logout();
        
        event Action OnLoginSuccess;
        event Action<string> OnLoginFailed;
        event Action<string> OnLoginStatusChanged;
    }
}
