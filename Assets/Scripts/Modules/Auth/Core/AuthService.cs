using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlayEveryWare.EpicOnlineServices;
using RecipeRage.Core.Interfaces;
using RecipeRage.Core.Patterns;
using RecipeRage.Core.Services;
using RecipeRage.Modules.Auth.Interfaces;
using RecipeRage.Modules.Auth.Providers;
using UnityEngine;

namespace RecipeRage.Modules.Auth.Core
{
    /// <summary>
    /// Main authentication service that manages all authentication providers and user state.
    /// Implements the Singleton pattern for global access with self-initialization.
    /// 
    /// Complexity Rating: 3
    /// </summary>
    public class AuthService : Singleton<AuthService>, IAuthService
    {
        /// <summary>
        /// The currently authenticated user, or null if not authenticated
        /// </summary>
        public IAuthProviderUser CurrentUser { get; private set; }
        
        /// <summary>
        /// Event triggered when authentication state changes
        /// </summary>
        public event Action<IAuthProviderUser> OnAuthStateChanged;
        
        /// <summary>
        /// PlayerPrefs key for storing the last used provider
        /// </summary>
        private const string LAST_PROVIDER_KEY = "RecipeRage_LastAuthProvider";
        
        /// <summary>
        /// Collection of registered auth providers
        /// </summary>
        private readonly List<IAuthProvider> _providers = new List<IAuthProvider>();
        
        /// <summary>
        /// Flag indicating whether the service has been initialized
        /// </summary>
        private bool _isInitialized = false;
        
        /// <summary>
        /// Settings for which providers to enable by default
        /// </summary>
        private bool _enableGuestLogin = true;
        private bool _enableFacebookLogin = true;
        private bool _enableEOSDeviceLogin = true;
        
        /// <summary>
        /// Private constructor for singleton pattern
        /// </summary>
        private AuthService()
        {
            // This will be called when the singleton is first accessed
            Logger.Info("AuthService", "Initializing");
            
            // Register itself with the ServiceLocator
            ServiceLocator.Instance.Register<IAuthService>(this);
        }
        
        /// <summary>
        /// Static method to create and initialize the auth service with default settings
        /// </summary>
        /// <param name="enableGuest">Whether to enable guest login</param>
        /// <param name="enableFacebook">Whether to enable Facebook login</param>
        /// <param name="enableEOS">Whether to enable EOS device login</param>
        /// <param name="onComplete">Callback when initialization is complete</param>
        /// <returns>The initialized auth service instance</returns>
        public static AuthService CreateAndInitialize(
            bool enableGuest = true,
            bool enableFacebook = true,
            bool enableEOS = true,
            Action<bool> onComplete = null)
        {
            var instance = Instance;
            
            instance._enableGuestLogin = enableGuest;
            instance._enableFacebookLogin = enableFacebook;
            instance._enableEOSDeviceLogin = enableEOS;
            
            // Register default providers if they're enabled
            if (enableGuest)
            {
                instance.RegisterProvider(new GuestAuthProvider());
                Logger.Info("AuthService", "Registered Guest auth provider");
            }
            
            if (enableFacebook)
            {
                instance.RegisterProvider(new FacebookAuthProvider());
                Logger.Info("AuthService", "Registered Facebook auth provider");
            }
            
            if (enableEOS)
            {
                // Only register if EOS is available
                if (EOSManager.Instance != null)
                {
                    instance.RegisterProvider(new EOSDeviceAuthProvider());
                    Logger.Info("AuthService", "Registered EOS Device auth provider");
                }
                else
                {
                    Logger.Warning("AuthService", "EOS Manager not found, EOS Device auth provider not registered");
                }
            }
            
            // Initialize the auth service
            instance.Initialize(onComplete);
            
            return instance;
        }
        
        /// <summary>
        /// Get all registered auth providers
        /// </summary>
        /// <returns>Read-only list of providers</returns>
        public IReadOnlyList<IAuthProvider> GetProviders()
        {
            return _providers.AsReadOnly();
        }
        
        /// <summary>
        /// Get a provider by type
        /// </summary>
        /// <typeparam name="T">Type of provider to get</typeparam>
        /// <returns>Provider instance or null if not found</returns>
        public T GetProvider<T>() where T : class, IAuthProvider
        {
            return _providers.OfType<T>().FirstOrDefault();
        }
        
        /// <summary>
        /// Get a provider by name
        /// </summary>
        /// <param name="providerName">Name of the provider</param>
        /// <returns>Provider instance or null if not found</returns>
        public IAuthProvider GetProviderByName(string providerName)
        {
            return _providers.FirstOrDefault(p => p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Register an auth provider
        /// </summary>
        /// <param name="provider">Provider to register</param>
        public void RegisterProvider(IAuthProvider provider)
        {
            if (provider == null)
            {
                Logger.Error("AuthService", "Cannot register null provider");
                return;
            }
            
            // Check if provider with the same name already exists
            if (_providers.Any(p => p.ProviderName.Equals(provider.ProviderName, StringComparison.OrdinalIgnoreCase)))
            {
                Logger.Warning("AuthService", $"Provider with name '{provider.ProviderName}' is already registered");
                return;
            }
            
            _providers.Add(provider);
            Logger.Info("AuthService", $"Registered provider '{provider.ProviderName}'");
        }
        
        /// <summary>
        /// Initialize the auth service and attempt auto-login if possible
        /// </summary>
        /// <param name="onComplete">Callback when initialization completes</param>
        public void Initialize(Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                Logger.Warning("AuthService", "Already initialized");
                onComplete?.Invoke(true);
                return;
            }
            
            Logger.Info("AuthService", "Initializing auth service");
            
            // Check for auto-login ability
            string lastProviderName = GetLastUsedProviderName();
            if (!string.IsNullOrEmpty(lastProviderName))
            {
                IAuthProvider lastProvider = GetProviderByName(lastProviderName);
                
                if (lastProvider != null && lastProvider.HasCachedCredentials())
                {
                    Logger.Info("AuthService", $"Attempting auto-login with provider '{lastProviderName}'");
                    SignInWithProvider(lastProviderName, 
                        onSuccess: user => {
                            _isInitialized = true;
                            onComplete?.Invoke(true);
                        },
                        onFailure: error => {
                            Logger.Warning("AuthService", $"Auto-login failed - {error}");
                            _isInitialized = true;
                            onComplete?.Invoke(false);
                        });
                    return;
                }
            }
            
            // No auto-login possible
            _isInitialized = true;
            onComplete?.Invoke(false);
        }
        
        /// <summary>
        /// Sign in with a specific provider
        /// </summary>
        /// <param name="providerName">Name of the provider to use</param>
        /// <param name="onSuccess">Callback on successful authentication</param>
        /// <param name="onFailure">Callback on authentication failure</param>
        public void SignInWithProvider(string providerName, Action<IAuthProviderUser> onSuccess = null, Action<string> onFailure = null)
        {
            IAuthProvider provider = GetProviderByName(providerName);
            
            if (provider == null)
            {
                string errorMessage = $"Provider '{providerName}' not found";
                Logger.Error("AuthService", errorMessage);
                onFailure?.Invoke(errorMessage);
                return;
            }
            
            provider.Authenticate(
                onSuccess: user => {
                    SetCurrentUser(user);
                    PlayerPrefs.SetString(LAST_PROVIDER_KEY, providerName);
                    PlayerPrefs.Save();
                    Logger.Info("AuthService", $"Successfully authenticated with provider '{providerName}'");
                    onSuccess?.Invoke(user);
                },
                onFailure: error => {
                    Logger.Error("AuthService", $"Failed to authenticate with provider '{providerName}': {error}");
                    onFailure?.Invoke(error);
                }
            );
        }
        
        /// <summary>
        /// Sign in with a specific provider asynchronously
        /// </summary>
        /// <param name="providerName">Name of the provider to use</param>
        /// <returns>Task with auth result</returns>
        public async Task<IAuthProviderUser> SignInWithProviderAsync(string providerName)
        {
            IAuthProvider provider = GetProviderByName(providerName);
            
            if (provider == null)
            {
                string errorMessage = $"Provider '{providerName}' not found";
                Logger.Error("AuthService", errorMessage);
                throw new ArgumentException(errorMessage);
            }
            
            try
            {
                IAuthProviderUser user = await provider.AuthenticateAsync();
                SetCurrentUser(user);
                PlayerPrefs.SetString(LAST_PROVIDER_KEY, providerName);
                PlayerPrefs.Save();
                Logger.Info("AuthService", $"Successfully authenticated with provider '{providerName}'");
                return user;
            }
            catch (Exception ex)
            {
                Logger.Error("AuthService", $"Failed to authenticate with provider '{providerName}': {ex.Message}", ex);
                throw;
            }
        }
        
        /// <summary>
        /// Sign out the current user
        /// </summary>
        /// <param name="onComplete">Callback when sign out completes</param>
        public void SignOut(Action onComplete = null)
        {
            if (CurrentUser == null)
            {
                Logger.Warning("AuthService", "Cannot sign out - no user is signed in");
                onComplete?.Invoke();
                return;
            }
            
            Logger.Info("AuthService", $"Signing out user from provider '{CurrentUser.Provider.ProviderName}'");
            
            CurrentUser.Provider.SignOut(() => {
                SetCurrentUser(null);
                onComplete?.Invoke();
            });
        }
        
        /// <summary>
        /// Get the name of the last used auth provider
        /// </summary>
        /// <returns>Provider name or null if not found</returns>
        public string GetLastUsedProviderName()
        {
            return PlayerPrefs.GetString(LAST_PROVIDER_KEY, null);
        }
        
        /// <summary>
        /// Save the current provider as the preferred provider for auto-login
        /// </summary>
        public void SaveCurrentProviderAsPreferred()
        {
            if (CurrentUser == null || CurrentUser.Provider == null)
            {
                Logger.Warning("AuthService", "Cannot save preferred provider - no user is signed in");
                return;
            }
            
            PlayerPrefs.SetString(LAST_PROVIDER_KEY, CurrentUser.Provider.ProviderName);
            PlayerPrefs.Save();
            Logger.Info("AuthService", $"Saved '{CurrentUser.Provider.ProviderName}' as preferred provider");
        }
        
        /// <summary>
        /// Set the current user and trigger the auth state changed event
        /// </summary>
        /// <param name="user">New user or null if signed out</param>
        private void SetCurrentUser(IAuthProviderUser user)
        {
            CurrentUser = user;
            OnAuthStateChanged?.Invoke(user);
        }
    }
} 