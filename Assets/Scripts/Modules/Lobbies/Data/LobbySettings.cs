using System;
using System.Collections.Generic;

namespace RecipeRage.Modules.Lobbies.Data
{
    /// <summary>
    /// Settings for creating or updating a lobby
    /// </summary>
    [Serializable]
    public class LobbySettings
    {
        /// <summary>
        /// Name of the lobby
        /// </summary>
        public string Name { get; set; } = "New Lobby";

        /// <summary>
        /// Maximum number of players allowed in the lobby
        /// </summary>
        public int MaxPlayers { get; set; } = 4;

        /// <summary>
        /// Whether the lobby is publicly visible in searches
        /// </summary>
        public bool IsPublic { get; set; } = true;

        /// <summary>
        /// Whether the lobby allows joining in progress
        /// </summary>
        public bool AllowsJoinInProgress { get; set; } = true;

        /// <summary>
        /// Permission required to join the lobby
        /// </summary>
        public LobbyPermission JoinPermission { get; set; } = LobbyPermission.Public;

        /// <summary>
        /// Attributes to set on the lobby
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Whether the lobby uses presence (shows up in friends' presence)
        /// </summary>
        public bool UsesPresence { get; set; } = true;

        /// <summary>
        /// Whether voice chat is enabled
        /// </summary>
        public bool VoiceChatEnabled { get; set; } = true;

        /// <summary>
        /// Whether this is a matchmaking lobby
        /// </summary>
        public bool IsMatchmakingLobby { get; set; } = false;

        /// <summary>
        /// Bucket ID for matchmaking
        /// </summary>
        public string BucketId { get; set; } = "default";

        /// <summary>
        /// Server region for the lobby
        /// </summary>
        public string Region { get; set; } = "any";

        /// <summary>
        /// Create a new LobbySettings object with default values
        /// </summary>
        public LobbySettings()
        {
            // Default constructor - all properties have default values
        }

        /// <summary>
        /// Create a copy of another lobby settings instance
        /// </summary>
        /// <param name="other">The settings to copy</param>
        public LobbySettings(LobbySettings other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            Name = other.Name;
            MaxPlayers = other.MaxPlayers;
            IsPublic = other.IsPublic;
            AllowsJoinInProgress = other.AllowsJoinInProgress;
            JoinPermission = other.JoinPermission;
            UsesPresence = other.UsesPresence;
            VoiceChatEnabled = other.VoiceChatEnabled;
            IsMatchmakingLobby = other.IsMatchmakingLobby;
            BucketId = other.BucketId;
            Region = other.Region;
            
            // Deep copy attributes
            Attributes = new Dictionary<string, string>(other.Attributes);
        }

        /// <summary>
        /// Create a LobbySettings object from matchmaking options
        /// </summary>
        /// <param name="options">The matchmaking options to use</param>
        /// <returns>A new LobbySettings object configured for matchmaking</returns>
        public static LobbySettings FromMatchmakingOptions(MatchmakingOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var settings = new LobbySettings
            {
                Name = $"Match_{options.GameMode}_{Guid.NewGuid().ToString().Substring(0, 8)}",
                MaxPlayers = options.MaxPlayers,
                IsPublic = false, // Matchmaking lobbies are typically private until filled
                AllowsJoinInProgress = options.AllowJoinInProgress,
                JoinPermission = options.AllowJoinInProgress ? LobbyPermission.Public : LobbyPermission.Locked,
                IsMatchmakingLobby = true,
                BucketId = options.GameMode
            };

            // Set region from preferred regions if available
            if (options.PreferredRegions.Count > 0)
            {
                settings.Region = options.PreferredRegions[0];
            }

            // Set standard matchmaking attributes
            settings.Attributes["GameMode"] = options.GameMode;
            settings.Attributes["MinPlayers"] = options.MinPlayers.ToString();
            settings.Attributes["MaxPlayers"] = options.MaxPlayers.ToString();
            settings.Attributes["IsMatchmakingLobby"] = "true";
            settings.Attributes["MatchmakingSessionId"] = options.SessionId;

            if (options.IsRematch)
            {
                settings.Attributes["IsRematch"] = "true";
                settings.Attributes["PreviousMatchId"] = options.PreviousMatchId;
            }

            if (options.UseSkillBasedMatching)
            {
                settings.Attributes["IsSkillBasedMatch"] = "true";
                settings.Attributes["MaxSkillRatingDifference"] = options.MaxSkillRatingDifference.ToString();
            }

            // Copy any custom attributes from the options
            foreach (var kvp in options.Attributes)
            {
                settings.Attributes[kvp.Key] = kvp.Value;
            }

            return settings;
        }
        
        /// <summary>
        /// Add an attribute to the lobby settings
        /// </summary>
        /// <param name="key">Attribute key</param>
        /// <param name="value">Attribute value</param>
        public void AddAttribute(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
                
            Attributes[key] = value;
        }
        
        /// <summary>
        /// Create a string representation of the lobby settings
        /// </summary>
        public override string ToString()
        {
            return $"LobbySettings[Name={Name}, MaxPlayers={MaxPlayers}, IsPublic={IsPublic}]";
        }
    }
    
    /// <summary>
    /// Permission settings for joining a lobby
    /// </summary>
    public enum LobbyPermission
    {
        /// <summary>Anyone can join</summary>
        Public,
        
        /// <summary>Only friends can join</summary>
        FriendsOnly,
        
        /// <summary>Only invited players can join</summary>
        InviteOnly,
        
        /// <summary>No one can join (locked)</summary>
        Locked
    }
} 