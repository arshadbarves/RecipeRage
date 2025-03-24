using RecipeRage.Modules.Auth;
using RecipeRage.Modules.Auth.Interfaces;
using RecipeRage.Modules.Logging;
using UnityEngine;

namespace RecipeRage.Examples
{
    /// <summary>
    /// Example script showing how to use the authentication system.
    /// This demonstrates using the AuthHelper static class without needing MonoBehaviour dependencies.
    /// Complexity Rating: 1
    /// </summary>
    public class AuthenticationExample : MonoBehaviour
    {
        private void Start()
        {
            // Register for auth state changes
            AuthHelper.RegisterAuthStateCallback(OnAuthStateChanged);

            // Check if user is already signed in
            if (AuthHelper.IsSignedIn())
            {
                var currentUser = AuthHelper.CurrentUser;
                LogHelper.Info("AuthExample",
                    $"User is already signed in: {currentUser.DisplayName} (Provider: {currentUser.Provider.ProviderName})");
            }
            else
            {
                LogHelper.Info("AuthExample", "No user is signed in");
            }
        }

        private void OnDestroy()
        {
            // Unregister from auth state changes
            AuthHelper.UnregisterAuthStateCallback(OnAuthStateChanged);
        }

        /// <summary>
        /// Called when the auth state changes
        /// </summary>
        /// <param name="user"> New user, or null if signed out </param>
        private void OnAuthStateChanged(IAuthProviderUser user)
        {
            if (user != null)
                LogHelper.Info("AuthExample",
                    $"User signed in: {user.DisplayName} (Provider: {user.Provider.ProviderName})");
            else
                LogHelper.Info("AuthExample", "User signed out");
        }

        /// <summary>
        /// Example method showing how to sign in as a guest
        /// </summary>
        public void SignInAsGuest()
        {
            LogHelper.Info("AuthExample", "Signing in as guest...");

            AuthHelper.SignInAsGuest(
                user => { LogHelper.Info("AuthExample", $"Guest sign-in successful: {user.DisplayName}"); },
                error => { LogHelper.Error("AuthExample", $"Guest sign-in failed: {error}"); }
            );
        }

        /// <summary>
        /// Example method showing how to sign in with Facebook
        /// </summary>
        public void SignInWithFacebook()
        {
            LogHelper.Info("AuthExample", "Signing in with Facebook...");

            AuthHelper.SignInWithFacebook(
                user => { LogHelper.Info("AuthExample", $"Facebook sign-in successful: {user.DisplayName}"); },
                error => { LogHelper.Error("AuthExample", $"Facebook sign-in failed: {error}"); }
            );
        }

        /// <summary>
        /// Example method showing how to sign out
        /// </summary>
        public void SignOut()
        {
            if (AuthHelper.IsSignedIn())
            {
                LogHelper.Info("AuthExample", "Signing out...");

                AuthHelper.SignOut(() => { LogHelper.Info("AuthExample", "Sign-out complete"); });
            }
            else
            {
                LogHelper.Warning("AuthExample", "Cannot sign out - no user is signed in");
            }
        }

        /// <summary>
        /// Example method showing how to show the login UI
        /// </summary>
        public void ShowLoginUI()
        {
            LogHelper.Info("AuthExample", "Showing login UI...");

            AuthHelper.ShowLoginUI(
                transform,
                success =>
                {
                    if (success)
                        LogHelper.Info("AuthExample", "Login successful");
                    else
                        LogHelper.Warning("AuthExample", "Login canceled or failed");
                }
            );
        }

        /// <summary>
        /// Example method showing how to use the UI without attaching new GameObjects
        /// </summary>
        public void ShowLoginWithoutMonoBehaviours()
        {
            // This uses the UI factory to create a login UI without any MonoBehaviour dependencies
            // The UI will be created, shown, and managed without the need for a MonoBehaviour component

            LogHelper.Info("AuthExample", "Showing login UI without MonoBehaviours...");

            // The UI Document will be created dynamically
            AuthHelper.ShowLoginUI(
                onComplete: success => { LogHelper.Info("AuthExample", $"Login {(success ? "successful" : "failed")}"); }
            );
        }
    }
}