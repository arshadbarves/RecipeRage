namespace RecipeRage.Modules.Auth.Interfaces
{
    /// <summary>
    /// Interface representing a user authenticated by an auth provider.
    /// Provides common user data regardless of authentication method.
    /// 
    /// Complexity Rating: 1
    /// </summary>
    public interface IAuthProviderUser
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        string UserId { get; }
        
        /// <summary>
        /// Display name of the user (may be null or empty)
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// Auth provider that authenticated this user
        /// </summary>
        IAuthProvider Provider { get; }
        
        /// <summary>
        /// Access token used to authenticate with backend services (if applicable)
        /// </summary>
        string AccessToken { get; }
        
        /// <summary>
        /// Whether this is a guest user
        /// </summary>
        bool IsGuest { get; }
        
        /// <summary>
        /// Whether the user data is valid and can be used
        /// </summary>
        bool IsValid { get; }
    }
} 