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

        /// <summary>
        /// Initialize authentication - checks for existing session and shows appropriate UI
        /// Returns true if user is authenticated, false if login screen is shown
        /// </summary>
        UniTask<bool> InitializeAsync();
        
        UniTask<bool> AttemptAutoLoginAsync();
        UniTask<bool> LoginAsGuestAsync();
        UniTask<bool> LoginWithFacebookAsync();
        UniTask LogoutAsync();
        
        event Action OnLogoutComplete;
    }
}
