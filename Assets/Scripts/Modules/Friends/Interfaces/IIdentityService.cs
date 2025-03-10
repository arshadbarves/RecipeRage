using System;
using System.Collections.Generic;

namespace RecipeRage.Modules.Friends.Interfaces
{
    /// <summary>
    /// Interface for the identity service.
    /// Manages user identity and friend codes.
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public interface IIdentityService
    {
        /// <summary>
        /// Event triggered when a user's display name changes
        /// </summary>
        event Action<string> OnDisplayNameChanged;
        
        /// <summary>
        /// Initialize the identity service
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        void Initialize(Action<bool> onComplete = null);
        
        /// <summary>
        /// Get the current user's unique identifier
        /// </summary>
        /// <returns>Current user's ID</returns>
        string GetCurrentUserId();
        
        /// <summary>
        /// Get the current user's display name
        /// </summary>
        /// <returns>Current user's display name</returns>
        string GetCurrentDisplayName();
        
        /// <summary>
        /// Set the current user's display name
        /// </summary>
        /// <param name="displayName">New display name</param>
        /// <param name="onComplete">Callback when the operation is complete</param>
        void SetDisplayName(string displayName, Action<bool> onComplete = null);
        
        /// <summary>
        /// Get a user's display name by their ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Display name or null if not found</returns>
        string GetDisplayName(string userId);
        
        /// <summary>
        /// Get the current user's friend code
        /// </summary>
        /// <returns>Current user's friend code</returns>
        string GetMyFriendCode();
        
        /// <summary>
        /// Generate a friend code for the current user if one doesn't exist
        /// </summary>
        /// <param name="onComplete">Callback with the generated code</param>
        void GenerateFriendCode(Action<string> onComplete);
        
        /// <summary>
        /// Look up a user by their friend code
        /// </summary>
        /// <param name="friendCode">Friend code to look up</param>
        /// <param name="onComplete">Callback with the user ID if found</param>
        void LookupUserByFriendCode(string friendCode, Action<string, string> onComplete);
        
        /// <summary>
        /// Check if a friend code is valid
        /// </summary>
        /// <param name="friendCode">Friend code to check</param>
        /// <returns>True if the code is valid</returns>
        bool IsValidFriendCode(string friendCode);
    }
} 