using System;
using System.Threading.Tasks;
using RecipeRage.Modules.Auth.Interfaces;
using RecipeRage.Core.Services;
using UnityEngine;

namespace RecipeRage.Modules.Auth.Core
{
    /// <summary>
    /// Base class for authentication providers that implements common functionality.
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public abstract class BaseAuthProvider : IAuthProvider
    {
        /// <summary>
        /// The name of the authentication provider
        /// </summary>
        public abstract string ProviderName { get; }
        
        /// <summary>
        /// Whether this provider supports persistent login across app restarts
        /// </summary>
        public abstract bool SupportsPersistentLogin { get; }
        
        /// <summary>
        /// Authenticate the user
        /// </summary>
        /// <param name="onSuccess">Callback when authentication succeeds</param>
        /// <param name="onFailure">Callback when authentication fails</param>
        public abstract void Authenticate(Action<IAuthProviderUser> onSuccess, Action<string> onFailure);
        
        /// <summary>
        /// Authenticate the user asynchronously
        /// </summary>
        /// <returns>Task with auth result</returns>
        public virtual async Task<IAuthProviderUser> AuthenticateAsync()
        {
            TaskCompletionSource<IAuthProviderUser> tcs = new TaskCompletionSource<IAuthProviderUser>();
            
            Authenticate(
                onSuccess: user => tcs.SetResult(user),
                onFailure: error => tcs.SetException(new Exception(error))
            );
            
            return await tcs.Task;
        }
        
        /// <summary>
        /// Sign out the current user
        /// </summary>
        /// <param name="onComplete">Callback when sign out is complete</param>
        public virtual void SignOut(Action onComplete = null)
        {
            ClearCachedCredentials();
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// Check if the provider has cached credentials
        /// </summary>
        /// <returns>True if cached credentials exist</returns>
        public abstract bool HasCachedCredentials();
        
        /// <summary>
        /// Clear any cached credentials
        /// </summary>
        public abstract void ClearCachedCredentials();
        
        /// <summary>
        /// PlayerPrefs key prefix for storing provider-specific data
        /// </summary>
        protected string PlayerPrefsKeyPrefix => $"RecipeRage_Auth_{ProviderName}_";
        
        /// <summary>
        /// Helper to save data to PlayerPrefs
        /// </summary>
        /// <param name="key">Key to save</param>
        /// <param name="value">Value to save</param>
        protected void SaveToPlayerPrefs(string key, string value)
        {
            PlayerPrefs.SetString(PlayerPrefsKeyPrefix + key, value);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Helper to load data from PlayerPrefs
        /// </summary>
        /// <param name="key">Key to load</param>
        /// <param name="defaultValue">Default value if key not found</param>
        /// <returns>Loaded value or default</returns>
        protected string LoadFromPlayerPrefs(string key, string defaultValue = null)
        {
            return PlayerPrefs.GetString(PlayerPrefsKeyPrefix + key, defaultValue);
        }
        
        /// <summary>
        /// Helper to delete data from PlayerPrefs
        /// </summary>
        /// <param name="key">Key to delete</param>
        protected void DeleteFromPlayerPrefs(string key)
        {
            PlayerPrefs.DeleteKey(PlayerPrefsKeyPrefix + key);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Helper to check if a key exists in PlayerPrefs
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True if key exists</returns>
        protected bool HasPlayerPrefsKey(string key)
        {
            return PlayerPrefs.HasKey(PlayerPrefsKeyPrefix + key);
        }
    }
} 