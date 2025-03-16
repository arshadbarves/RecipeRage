using System;
using System.Collections.Generic;

namespace RecipeRage.Modules.Lobbies.Data
{
    /// <summary>
    /// Options for searching lobbies.
    /// </summary>
    [Serializable]
    public class LobbySearchOptions
    {
        /// <summary>
        /// Maximum number of results to return
        /// </summary>
        public int MaxResults { get; set; }
        
        /// <summary>
        /// Whether to search only for joinable lobbies
        /// </summary>
        public bool JoinableOnly { get; set; }
        
        /// <summary>
        /// Whether to search only for lobbies with friends
        /// </summary>
        public bool FriendsOnly { get; set; }
        
        /// <summary>
        /// Whether to search only for lobbies with available player slots
        /// </summary>
        public bool AvailableSlotsOnly { get; set; }
        
        /// <summary>
        /// Search text for lobby name (if supported by provider)
        /// </summary>
        public string SearchText { get; set; }
        
        /// <summary>
        /// Filters for lobby attributes
        /// </summary>
        public List<LobbyAttributeFilter> AttributeFilters { get; set; }
        
        /// <summary>
        /// Bucket ID for filtering lobbies
        /// </summary>
        public string BucketId { get; set; }
        
        /// <summary>
        /// Create a new LobbySearchOptions object with default values
        /// </summary>
        public LobbySearchOptions()
        {
            MaxResults = 50;
            JoinableOnly = true;
            FriendsOnly = false;
            AvailableSlotsOnly = true;
            SearchText = string.Empty;
            AttributeFilters = new List<LobbyAttributeFilter>();
            BucketId = "default";
        }
        
        /// <summary>
        /// Add an attribute filter for searching
        /// </summary>
        /// <param name="key">Attribute key to filter on</param>
        /// <param name="value">Value to filter for</param>
        /// <param name="comparison">Comparison operation to use</param>
        public void AddAttributeFilter(string key, string value, ComparisonOp comparison = ComparisonOp.Equal)
        {
            AttributeFilters.Add(new LobbyAttributeFilter
            {
                Key = key,
                Value = value,
                ComparisonType = comparison
            });
        }
    }
    
    /// <summary>
    /// Filter for lobby attributes in search
    /// </summary>
    [Serializable]
    public class LobbyAttributeFilter
    {
        /// <summary>
        /// Key of the attribute to filter on
        /// </summary>
        public string Key { get; set; }
        
        /// <summary>
        /// Value to compare against
        /// </summary>
        public string Value { get; set; }
        
        /// <summary>
        /// Type of comparison to perform
        /// </summary>
        public ComparisonOp ComparisonType { get; set; }
    }
    
    /// <summary>
    /// Comparison operations for attribute filtering
    /// </summary>
    public enum ComparisonOp
    {
        /// <summary>Equals the value</summary>
        Equal,
        
        /// <summary>Not equal to the value</summary>
        NotEqual,
        
        /// <summary>Greater than the value</summary>
        GreaterThan,
        
        /// <summary>Greater than or equal to the value</summary>
        GreaterThanOrEqual,
        
        /// <summary>Less than the value</summary>
        LessThan,
        
        /// <summary>Less than or equal to the value</summary>
        LessThanOrEqual,
        
        /// <summary>Contains the value (substring)</summary>
        Contains,
        
        /// <summary>Does not contain the value</summary>
        NotContains,
        
        /// <summary>Attribute exists with any value</summary>
        Exists,
        
        /// <summary>Attribute does not exist</summary>
        NotExists
    }
    
    /// <summary>
    /// Result of a lobby search.
    /// </summary>
    [Serializable]
    public class LobbySearchResult
    {
        /// <summary>
        /// Unique identifier for the lobby
        /// </summary>
        public string LobbyId { get; set; }
        
        /// <summary>
        /// Name of the lobby
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Owner ID of the lobby
        /// </summary>
        public string OwnerId { get; set; }
        
        /// <summary>
        /// Owner display name
        /// </summary>
        public string OwnerDisplayName { get; set; }
        
        /// <summary>
        /// Maximum number of players allowed in the lobby
        /// </summary>
        public int MaxPlayers { get; set; }
        
        /// <summary>
        /// Current number of players in the lobby
        /// </summary>
        public int CurrentPlayers { get; set; }
        
        /// <summary>
        /// Whether the lobby is available for joining
        /// </summary>
        public bool IsJoinable { get; set; }
        
        /// <summary>
        /// Whether the lobby has friends in it
        /// </summary>
        public bool HasFriends { get; set; }
        
        /// <summary>
        /// Number of friends in the lobby
        /// </summary>
        public int FriendCount { get; set; }
        
        /// <summary>
        /// Whether the lobby allows join-in-progress
        /// </summary>
        public bool AllowsJoinInProgress { get; set; }
        
        /// <summary>
        /// Time when the lobby was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Time when the lobby was last updated
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }
        
        /// <summary>
        /// Visible attributes of the lobby
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }
        
        /// <summary>
        /// Provider-specific data
        /// </summary>
        public object ProviderData { get; set; }
        
        /// <summary>
        /// Create a new LobbySearchResult object
        /// </summary>
        public LobbySearchResult()
        {
            Attributes = new Dictionary<string, string>();
            CreatedAt = DateTime.UtcNow;
            LastUpdatedAt = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Convert to a full LobbyInfo object
        /// </summary>
        /// <returns>A new LobbyInfo object with the available data</returns>
        public LobbyInfo ToLobbyInfo()
        {
            LobbyInfo info = new LobbyInfo
            {
                LobbyId = LobbyId,
                Name = Name,
                OwnerId = OwnerId,
                MaxPlayers = MaxPlayers,
                CurrentPlayers = CurrentPlayers,
                IsJoinable = IsJoinable,
                IsPublic = true,
                AllowsJoinInProgress = AllowsJoinInProgress,
                CreatedAt = CreatedAt,
                LastUpdatedAt = LastUpdatedAt,
                ProviderData = ProviderData
            };
            
            // Copy attributes
            foreach (var kvp in Attributes)
            {
                info.Attributes[kvp.Key] = kvp.Value;
            }
            
            // Add owner as member if we have owner data
            if (!string.IsNullOrEmpty(OwnerId) && !string.IsNullOrEmpty(OwnerDisplayName))
            {
                info.Members.Add(new LobbyMember(OwnerId, OwnerDisplayName, true));
            }
            
            return info;
        }
        
        /// <summary>
        /// Get the value of an attribute
        /// </summary>
        /// <param name="key">Key of the attribute</param>
        /// <returns>Value of the attribute, or null if not found</returns>
        public string GetAttribute(string key)
        {
            if (Attributes.TryGetValue(key, out string value))
            {
                return value;
            }
            return null;
        }
        
        /// <summary>
        /// Get the value of an attribute as a specific type
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="key">Key of the attribute</param>
        /// <param name="defaultValue">Default value if the attribute is not found or conversion fails</param>
        /// <returns>Value of the attribute as the specified type, or defaultValue if not found or conversion fails</returns>
        public T GetAttributeAs<T>(string key, T defaultValue = default)
        {
            string value = GetAttribute(key);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
} 