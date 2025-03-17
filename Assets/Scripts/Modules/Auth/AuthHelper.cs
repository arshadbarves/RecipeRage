using System;
using RecipeRage.Core.Patterns;
using RecipeRage.Modules.Auth.Core;
using RecipeRage.Modules.Auth.Interfaces;
using RecipeRage.Modules.Auth.UI;
using UnityEngine;
using Logger = RecipeRage.Core.Services.Logger;

namespace RecipeRage.Modules.Auth
{
    /// <summary>
    /// Static helper class for easier access to authentication functionality.
    /// Provides a Unity-like API for auth operations without needing to get instances.
    /// Complexity Rating: 1
    /// </summary>
    public static class AuthHelper
    {
        /// <summary>
        /// The currently authenticated user, or null if not authenticated
        /// </summary>
        public static IAuthProviderUser CurrentUser
        {
            get
            {
                var authService = GetAuthService();
                return authService?.CurrentUser;
            }
        }

        /// <summary>
        /// Register a callback for when authentication state changes
        /// </summary>
        /// <param name="callback"> Callback to register </param>
        /// <returns> True if registration was successful </returns>
        public static bool RegisterAuthStateCallback(Action<IAuthProviderUser> callback)
        {
            var authService = GetAuthService();
            if (authService != null && callback != null)
            {
                authService.OnAuthStateChanged += callback;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unregister a callback for authentication state changes
        /// </summary>
        /// <param name="callback"> Callback to unregister </param>
        /// <returns> True if unregistration was successful </returns>
        public static bool UnregisterAuthStateCallback(Action<IAuthProviderUser> callback)
        {
            var authService = GetAuthService();
            if (authService != null && callback != null)
            {
                authService.OnAuthStateChanged -= callback;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Show the login UI
        /// </summary>
        /// <param name="parent"> Parent transform to attach to </param>
        /// <param name="onComplete"> Callback for when authentication is complete </param>
        public static void ShowLoginUI(Transform parent = null, Action<bool> onComplete = null)
        {
            AuthUIFactory.CreateLoginUI(parent, onComplete);
        }

        /// <summary>
        /// Hide the login UI
        /// </summary>
        public static void HideLoginUI()
        {
            AuthUIFactory.HideLoginUI();
        }

        /// <summary>
        /// Sign in with a specific provider
        /// </summary>
        /// <param name="providerName"> Name of the provider </param>
        /// <param name="onSuccess"> Callback on successful authentication </param>
        /// <param name="onFailure"> Callback on authentication failure </param>
        /// <returns> True if sign-in was initiated successfully </returns>
        public static bool SignIn(string providerName, Action<IAuthProviderUser> onSuccess = null,
            Action<string> onFailure = null)
        {
            var authService = GetAuthService();
            if (authService == null)
            {
                Logger.Error("AuthHelper", "Auth service not available");
                onFailure?.Invoke("Auth service not available");
                return false;
            }

            if (authService.GetProviderByName(providerName) == null)
            {
                Logger.Error("AuthHelper", $"Provider '{providerName}' not available");
                onFailure?.Invoke($"Provider '{providerName}' not available");
                return false;
            }

            authService.SignInWithProvider(providerName, onSuccess, onFailure);
            return true;
        }

        /// <summary>
        /// Sign in as a guest
        /// </summary>
        /// <param name="onSuccess"> Callback on successful authentication </param>
        /// <param name="onFailure"> Callback on authentication failure </param>
        /// <returns> True if sign-in was initiated successfully </returns>
        public static bool SignInAsGuest(Action<IAuthProviderUser> onSuccess = null, Action<string> onFailure = null)
        {
            return SignIn("Guest", onSuccess, onFailure);
        }

        /// <summary>
        /// Sign in with Facebook
        /// </summary>
        /// <param name="onSuccess"> Callback on successful authentication </param>
        /// <param name="onFailure"> Callback on authentication failure </param>
        /// <returns> True if sign-in was initiated successfully </returns>
        public static bool SignInWithFacebook(Action<IAuthProviderUser> onSuccess = null,
            Action<string> onFailure = null)
        {
            return SignIn("Facebook", onSuccess, onFailure);
        }

        /// <summary>
        /// Sign in with EOS Device
        /// </summary>
        /// <param name="onSuccess"> Callback on successful authentication </param>
        /// <param name="onFailure"> Callback on authentication failure </param>
        /// <returns> True if sign-in was initiated successfully </returns>
        public static bool SignInWithEOSDevice(Action<IAuthProviderUser> onSuccess = null,
            Action<string> onFailure = null)
        {
            return SignIn("EOSDevice", onSuccess, onFailure);
        }

        /// <summary>
        /// Sign out the current user
        /// </summary>
        /// <param name="onComplete"> Callback when sign out is complete </param>
        /// <returns> True if sign-out was initiated successfully </returns>
        public static bool SignOut(Action onComplete = null)
        {
            var authService = GetAuthService();
            if (authService == null)
            {
                Logger.Error("AuthHelper", "Auth service not available");
                onComplete?.Invoke();
                return false;
            }

            if (authService.CurrentUser == null)
            {
                Logger.Warning("AuthHelper", "No user is currently signed in");
                onComplete?.Invoke();
                return false;
            }

            authService.SignOut(onComplete);
            return true;
        }

        /// <summary>
        /// Check if the user is signed in
        /// </summary>
        /// <returns> True if user is signed in </returns>
        public static bool IsSignedIn()
        {
            return CurrentUser != null;
        }

        /// <summary>
        /// Check if a specific provider is available
        /// </summary>
        /// <param name="providerName"> Name of the provider </param>
        /// <returns> True if provider is available </returns>
        public static bool IsProviderAvailable(string providerName)
        {
            var authService = GetAuthService();
            return authService?.GetProviderByName(providerName) != null;
        }

        /// <summary>
        /// Initialize the auth system with default settings
        /// </summary>
        /// <param name="enableGuest"> Whether to enable guest login </param>
        /// <param name="enableFacebook"> Whether to enable Facebook login </param>
        /// <param name="enableEOS"> Whether to enable EOS device login </param>
        /// <param name="onComplete"> Callback when initialization is complete </param>
        public static void Initialize(bool enableGuest = true, bool enableFacebook = true, bool enableEOS = true,
            Action<bool> onComplete = null)
        {
            AuthService.CreateAndInitialize(enableGuest, enableFacebook, enableEOS, onComplete);
        }

        /// <summary>
        /// Get the auth service from the service locator
        /// </summary>
        /// <returns> Auth service or null if not available </returns>
        private static IAuthService GetAuthService()
        {
            if (!ServiceLocator.Instance.TryGet<IAuthService>(out var authService))
            {
                Logger.Error("AuthHelper", "Failed to get auth service from service locator");
                return null;
            }

            return authService;
        }
    }
}