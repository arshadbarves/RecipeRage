using RecipeRage.Core.Services;
using RecipeRage.Modules.Auth;
using RecipeRage.Modules.Auth.Interfaces;
using UnityEngine;

namespace RecipeRage.Examples
{
    /// <summary>
    /// Example script showing how to use the authentication system.
    /// This demonstrates using the AuthHelper static class without needing MonoBehaviour dependencies.
    /// 
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
                IAuthProviderUser currentUser = AuthHelper.CurrentUser;
                Logger.Info("AuthExample", $"User is already signed in: {currentUser.DisplayName} (Provider: {currentUser.Provider.ProviderName})");
            }
            else
            {
                Logger.Info("AuthExample", "No user is signed in");
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
        /// <param name="user">New user, or null if signed out</param>
        private void OnAuthStateChanged(IAuthProviderUser user)
        {
            if (user != null)
            {
                Logger.Info("AuthExample", $"User signed in: {user.DisplayName} (Provider: {user.Provider.ProviderName})");
            }
            else
            {
                Logger.Info("AuthExample", "User signed out");
            }
        }
        
        /// <summary>
        /// Example method showing how to sign in as a guest
        /// </summary>
        public void SignInAsGuest()
        {
            Logger.Info("AuthExample", "Signing in as guest...");
            
            AuthHelper.SignInAsGuest(
                onSuccess: user => {
                    Logger.Info("AuthExample", $"Guest sign-in successful: {user.DisplayName}");
                },
                onFailure: error => {
                    Logger.Error("AuthExample", $"Guest sign-in failed: {error}");
                }
            );
        }
        
        /// <summary>
        /// Example method showing how to sign in with Facebook
        /// </summary>
        public void SignInWithFacebook()
        {
            Logger.Info("AuthExample", "Signing in with Facebook...");
            
            AuthHelper.SignInWithFacebook(
                onSuccess: user => {
                    Logger.Info("AuthExample", $"Facebook sign-in successful: {user.DisplayName}");
                },
                onFailure: error => {
                    Logger.Error("AuthExample", $"Facebook sign-in failed: {error}");
                }
            );
        }
        
        /// <summary>
        /// Example method showing how to sign out
        /// </summary>
        public void SignOut()
        {
            if (AuthHelper.IsSignedIn())
            {
                Logger.Info("AuthExample", "Signing out...");
                
                AuthHelper.SignOut(() => {
                    Logger.Info("AuthExample", "Sign-out complete");
                });
            }
            else
            {
                Logger.Warning("AuthExample", "Cannot sign out - no user is signed in");
            }
        }
        
        /// <summary>
        /// Example method showing how to show the login UI
        /// </summary>
        public void ShowLoginUI()
        {
            Logger.Info("AuthExample", "Showing login UI...");
            
            AuthHelper.ShowLoginUI(
                parent: transform,
                onComplete: success => {
                    if (success)
                    {
                        Logger.Info("AuthExample", "Login successful");
                    }
                    else
                    {
                        Logger.Warning("AuthExample", "Login canceled or failed");
                    }
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
            
            Logger.Info("AuthExample", "Showing login UI without MonoBehaviours...");
            
            // The UI Document will be created dynamically
            AuthHelper.ShowLoginUI(
                onComplete: success => {
                    Logger.Info("AuthExample", $"Login {(success ? "successful" : "failed")}");
                }
            );
        }
    }
} 