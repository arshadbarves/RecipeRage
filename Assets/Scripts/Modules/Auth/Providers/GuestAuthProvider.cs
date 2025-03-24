using System;
using System.Security.Cryptography;
using System.Text;
using RecipeRage.Modules.Auth.Core;
using RecipeRage.Modules.Auth.Interfaces;
using RecipeRage.Modules.Logging;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RecipeRage.Modules.Auth.Providers
{
    /// <summary>
    /// Auth provider for guest login using device ID.
    /// This provider allows users to login without creating an account.
    /// Complexity Rating: 2
    /// </summary>
    public class GuestAuthProvider : BaseAuthProvider
    {
        /// <summary>
        /// PlayerPrefs keys
        /// </summary>
        private const string KEY_GUEST_ID = "GuestID";

        private const string KEY_GUEST_DISPLAY_NAME = "GuestDisplayName";

        /// <summary>
        /// The name of this authentication provider
        /// </summary>
        public override string ProviderName => "Guest";

        /// <summary>
        /// Guest login supports persistent login across app restarts
        /// </summary>
        public override bool SupportsPersistentLogin => true;

        /// <summary>
        /// Authenticate as a guest using device ID
        /// </summary>
        /// <param name="onSuccess"> Callback when authentication succeeds </param>
        /// <param name="onFailure"> Callback when authentication fails </param>
        public override void Authenticate(Action<IAuthProviderUser> onSuccess, Action<string> onFailure)
        {
            try
            {
                // Check if we have a cached guest ID
                string guestId = LoadFromPlayerPrefs(KEY_GUEST_ID);
                string displayName = LoadFromPlayerPrefs(KEY_GUEST_DISPLAY_NAME);

                // If no guest ID exists, create a new one
                if (string.IsNullOrEmpty(guestId))
                {
                    guestId = GenerateGuestId();
                    displayName = $"Guest{Random.Range(1000, 9999)}";

                    // Save the guest ID for future logins
                    SaveToPlayerPrefs(KEY_GUEST_ID, guestId);
                    SaveToPlayerPrefs(KEY_GUEST_DISPLAY_NAME, displayName);

                    LogHelper.Info("GuestAuthProvider", $"Created new guest ID '{guestId}'");
                }
                else
                {
                    LogHelper.Info("GuestAuthProvider", $"Using existing guest ID '{guestId}'");
                }

                // Create the user object
                IAuthProviderUser user = new AuthProviderUser(
                    guestId,
                    this,
                    displayName,
                    null,
                    true
                );

                // Invoke the success callback
                onSuccess?.Invoke(user);
            }
            catch (Exception ex)
            {
                LogHelper.Exception("GuestAuthProvider", ex, "Failed to authenticate as guest");
                onFailure?.Invoke($"Failed to authenticate as guest: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if the provider has cached guest credentials
        /// </summary>
        /// <returns> True if cached credentials exist </returns>
        public override bool HasCachedCredentials()
        {
            return HasPlayerPrefsKey(KEY_GUEST_ID);
        }

        /// <summary>
        /// Clear any cached guest credentials
        /// </summary>
        public override void ClearCachedCredentials()
        {
            DeleteFromPlayerPrefs(KEY_GUEST_ID);
            DeleteFromPlayerPrefs(KEY_GUEST_DISPLAY_NAME);
            LogHelper.Info("GuestAuthProvider", "Cleared cached credentials");
        }

        /// <summary>
        /// Generate a unique guest ID based on device information
        /// </summary>
        /// <returns> A unique guest ID </returns>
        private string GenerateGuestId()
        {
            // Combine device information to create a unique ID
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            string deviceName = SystemInfo.deviceName;
            string deviceModel = SystemInfo.deviceModel;

            // Create a hash of the combined information
            string combinedInfo = $"{deviceId}_{deviceName}_{deviceModel}_{DateTime.UtcNow.Ticks}";
            string hash = GenerateHash(combinedInfo);

            // Return a formatted guest ID
            return $"guest_{hash}";
        }

        /// <summary>
        /// Generate a simple hash from a string
        /// </summary>
        /// <param name="input"> String to hash </param>
        /// <returns> Hashed string </returns>
        private string GenerateHash(string input)
        {
            // Use a simple hash function for demonstration purposes
            // In a production app, use a more secure hashing algorithm
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to a hex string
                var sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++) sb.Append(hashBytes[i].ToString("x2"));
                return sb.ToString();
            }
        }
    }
}