using System;
using System.Threading.Tasks;

namespace RecipeRage.Modules.Auth.Interfaces
{
    /// <summary>
    /// Interface for authentication providers.
    /// Implements the provider pattern for different authentication methods.
    /// 
    /// Complexity Rating: 1
    /// </summary>
    public interface IAuthProvider
    {
        /// <summary>
        /// The name of the authentication provider
        /// </summary>
        string ProviderName { get; }
        
        /// <summary>
        /// Whether this provider supports persistent login across app restarts
        /// </summary>
        bool SupportsPersistentLogin { get; }
        
        /// <summary>
        /// Authenticate the user
        /// </summary>
        /// <param name="onSuccess">Callback when authentication succeeds</param>
        /// <param name="onFailure">Callback when authentication fails</param>
        void Authenticate(Action<IAuthProviderUser> onSuccess, Action<string> onFailure);
        
        /// <summary>
        /// Authenticate the user asynchronously
        /// </summary>
        /// <returns>Task with auth result</returns>
        Task<IAuthProviderUser> AuthenticateAsync();
        
        /// <summary>
        /// Sign out the current user
        /// </summary>
        /// <param name="onComplete">Callback when sign out is complete</param>
        void SignOut(Action onComplete = null);
        
        /// <summary>
        /// Check if the provider has cached credentials
        /// </summary>
        /// <returns>True if cached credentials exist</returns>
        bool HasCachedCredentials();
        
        /// <summary>
        /// Clear any cached credentials
        /// </summary>
        void ClearCachedCredentials();
    }
} 