using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipeRage.Modules.Auth.Interfaces
{
    /// <summary>
    /// Interface for the authentication service.
    /// Manages different auth providers and maintains current user state.
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// The currently authenticated user, or null if not authenticated
        /// </summary>
        IAuthProviderUser CurrentUser { get; }
        
        /// <summary>
        /// Event triggered when authentication state changes
        /// </summary>
        event Action<IAuthProviderUser> OnAuthStateChanged;
        
        /// <summary>
        /// Get all registered auth providers
        /// </summary>
        /// <returns>List of auth providers</returns>
        IReadOnlyList<IAuthProvider> GetProviders();
        
        /// <summary>
        /// Get an auth provider by type
        /// </summary>
        /// <typeparam name="T">Type of auth provider</typeparam>
        /// <returns>Auth provider instance or null if not registered</returns>
        T GetProvider<T>() where T : class, IAuthProvider;
        
        /// <summary>
        /// Get an auth provider by name
        /// </summary>
        /// <param name="providerName">Name of the provider</param>
        /// <returns>Auth provider instance or null if not found</returns>
        IAuthProvider GetProviderByName(string providerName);
        
        /// <summary>
        /// Register an auth provider
        /// </summary>
        /// <param name="provider">Provider to register</param>
        void RegisterProvider(IAuthProvider provider);
        
        /// <summary>
        /// Initialize the auth service and attempt auto-login if possible
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        void Initialize(Action<bool> onComplete = null);
        
        /// <summary>
        /// Sign in with a specific provider
        /// </summary>
        /// <param name="providerName">Name of the provider</param>
        /// <param name="onSuccess">Callback on successful authentication</param>
        /// <param name="onFailure">Callback on failed authentication</param>
        void SignInWithProvider(string providerName, Action<IAuthProviderUser> onSuccess = null, Action<string> onFailure = null);
        
        /// <summary>
        /// Sign in with a specific provider asynchronously
        /// </summary>
        /// <param name="providerName">Name of the provider</param>
        /// <returns>Task with auth result</returns>
        Task<IAuthProviderUser> SignInWithProviderAsync(string providerName);
        
        /// <summary>
        /// Sign out the current user
        /// </summary>
        /// <param name="onComplete">Callback when sign out is complete</param>
        void SignOut(Action onComplete = null);
        
        /// <summary>
        /// Get the last used auth provider name
        /// </summary>
        /// <returns>Name of the last used provider or null</returns>
        string GetLastUsedProviderName();
        
        /// <summary>
        /// Save the current provider as the preferred provider for auto-login
        /// </summary>
        void SaveCurrentProviderAsPreferred();
    }
} 