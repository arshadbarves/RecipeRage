using System;
using System.Collections;

namespace Core.Authentication
{
    /// <summary>
    /// Interface for authentication operations
    /// </summary>
    public interface IAuthenticationService
    {
        bool IsLoggedIn { get; }
        string LastLoginMethod { get; }
        
        IEnumerator AttemptAutoLogin();
        IEnumerator LoginAsGuest();
        IEnumerator LoginWithFacebook();
        void Logout();
        
        event Action OnLoginSuccess;
        event Action<string> OnLoginFailed;
        event Action<string> OnLoginStatusChanged;
    }
}
