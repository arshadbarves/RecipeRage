using RecipeRage.Modules.Auth.Interfaces;

namespace RecipeRage.Modules.Auth.Core
{
    /// <summary>
    /// Base implementation of IAuthProviderUser that providers can extend.
    /// 
    /// Complexity Rating: 1
    /// </summary>
    public class AuthProviderUser : IAuthProviderUser
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public string UserId { get; }
        
        /// <summary>
        /// Display name of the user (may be null or empty)
        /// </summary>
        public string DisplayName { get; }
        
        /// <summary>
        /// Auth provider that authenticated this user
        /// </summary>
        public IAuthProvider Provider { get; }
        
        /// <summary>
        /// Access token used to authenticate with backend services (if applicable)
        /// </summary>
        public string AccessToken { get; }
        
        /// <summary>
        /// Whether this is a guest user
        /// </summary>
        public bool IsGuest { get; }
        
        /// <summary>
        /// Whether the user data is valid and can be used
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(UserId) && Provider != null;
        
        /// <summary>
        /// Constructor for creating an auth provider user
        /// </summary>
        /// <param name="userId">User's unique ID</param>
        /// <param name="provider">Auth provider that created this user</param>
        /// <param name="displayName">User's display name (optional)</param>
        /// <param name="accessToken">Access token (optional)</param>
        /// <param name="isGuest">Whether this is a guest user (default: false)</param>
        public AuthProviderUser(string userId, IAuthProvider provider, string displayName = null, string accessToken = null, bool isGuest = false)
        {
            UserId = userId;
            Provider = provider;
            DisplayName = displayName ?? userId;
            AccessToken = accessToken;
            IsGuest = isGuest;
        }
        
        /// <summary>
        /// Returns a string representation of the user
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"User[Id: {UserId}, Name: {DisplayName}, Provider: {Provider?.ProviderName ?? "None"}, Guest: {IsGuest}]";
        }
    }
} 