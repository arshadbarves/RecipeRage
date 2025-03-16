using System;
using System.Collections.Generic;

namespace RecipeRage.Modules.Lobbies.Data
{
    /// <summary>
    /// Contains information about a player in a lobby
    /// </summary>
    [Serializable]
    public class LobbyPlayerInfo
    {
        /// <summary>
        /// Unique identifier for the player
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the player
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Whether the player is the owner of the lobby
        /// </summary>
        public bool IsOwner { get; set; } = false;

        /// <summary>
        /// Whether the player is connected to the lobby
        /// </summary>
        public bool IsConnected { get; set; } = true;

        /// <summary>
        /// Current status of the player
        /// </summary>
        public LobbyPlayerStatus Status { get; set; } = LobbyPlayerStatus.Online;

        /// <summary>
        /// Whether the player is ready to play
        /// </summary>
        public bool IsReady { get; set; } = false;

        /// <summary>
        /// Time when the player joined the lobby
        /// </summary>
        public DateTime JoinTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Attributes associated with the player
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Avatar or profile image URL for the player
        /// </summary>
        public string AvatarUrl { get; set; } = string.Empty;

        /// <summary>
        /// Platform the player is playing on
        /// </summary>
        public string Platform { get; set; } = string.Empty;

        /// <summary>
        /// Whether the player has a microphone connected
        /// </summary>
        public bool HasMicrophone { get; set; } = false;

        /// <summary>
        /// Whether the player is currently speaking
        /// </summary>
        public bool IsSpeaking { get; set; } = false;

        /// <summary>
        /// Whether the player is muted in voice chat
        /// </summary>
        public bool IsMuted { get; set; } = false;

        /// <summary>
        /// Team or role assigned to the player
        /// </summary>
        public string Team { get; set; } = string.Empty;

        /// <summary>
        /// Skill rating or ELO of the player
        /// </summary>
        public float SkillRating { get; set; } = 0f;

        /// <summary>
        /// Create a default LobbyPlayerInfo object
        /// </summary>
        public LobbyPlayerInfo()
        {
            // Default constructor
        }

        /// <summary>
        /// Create a LobbyPlayerInfo with specific player ID and display name
        /// </summary>
        /// <param name="playerId">Unique ID of the player</param>
        /// <param name="displayName">Display name of the player</param>
        public LobbyPlayerInfo(string playerId, string displayName)
        {
            PlayerId = playerId;
            DisplayName = displayName;
            JoinTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Create a copy of another player info instance
        /// </summary>
        /// <param name="other">The player info to copy</param>
        public LobbyPlayerInfo(LobbyPlayerInfo other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            PlayerId = other.PlayerId;
            DisplayName = other.DisplayName;
            IsOwner = other.IsOwner;
            IsConnected = other.IsConnected;
            Status = other.Status;
            IsReady = other.IsReady;
            JoinTime = other.JoinTime;
            AvatarUrl = other.AvatarUrl;
            Platform = other.Platform;
            HasMicrophone = other.HasMicrophone;
            IsSpeaking = other.IsSpeaking;
            IsMuted = other.IsMuted;
            Team = other.Team;
            SkillRating = other.SkillRating;
            
            // Deep copy attributes
            Attributes = new Dictionary<string, string>(other.Attributes);
        }

        /// <summary>
        /// Get the value of an attribute or a default if not found
        /// </summary>
        /// <param name="key">Key of the attribute</param>
        /// <param name="defaultValue">Default value to return if not found</param>
        /// <returns>The attribute value or default</returns>
        public string GetAttributeOrDefault(string key, string defaultValue)
        {
            if (Attributes != null && Attributes.TryGetValue(key, out string value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Get an attribute as a boolean
        /// </summary>
        /// <param name="key">Key of the attribute</param>
        /// <param name="defaultValue">Default value if not found or invalid</param>
        /// <returns>The attribute as a boolean</returns>
        public bool GetAttributeAsBool(string key, bool defaultValue)
        {
            string value = GetAttributeOrDefault(key, defaultValue.ToString());
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Get an attribute as an integer
        /// </summary>
        /// <param name="key">Key of the attribute</param>
        /// <param name="defaultValue">Default value if not found or invalid</param>
        /// <returns>The attribute as an integer</returns>
        public int GetAttributeAsInt(string key, int defaultValue)
        {
            string value = GetAttributeOrDefault(key, defaultValue.ToString());
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Get an attribute as a float
        /// </summary>
        /// <param name="key">Key of the attribute</param>
        /// <param name="defaultValue">Default value if not found or invalid</param>
        /// <returns>The attribute as a float</returns>
        public float GetAttributeAsFloat(string key, float defaultValue)
        {
            string value = GetAttributeOrDefault(key, defaultValue.ToString());
            if (float.TryParse(value, out float result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Check if the player has the attribute with the given key
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True if the attribute exists</returns>
        public bool HasAttribute(string key)
        {
            return Attributes != null && Attributes.ContainsKey(key);
        }

        /// <summary>
        /// Create a string representation of the player info
        /// </summary>
        public override string ToString()
        {
            return $"LobbyPlayerInfo[ID={PlayerId}, Name={DisplayName}, Owner={IsOwner}, Ready={IsReady}]";
        }
    }

    /// <summary>
    /// Status of a player in a lobby
    /// </summary>
    public enum LobbyPlayerStatus
    {
        /// <summary>
        /// Player is online and connected
        /// </summary>
        Online,
        
        /// <summary>
        /// Player is away or inactive
        /// </summary>
        Away,
        
        /// <summary>
        /// Player is busy and may not respond
        /// </summary>
        Busy,
        
        /// <summary>
        /// Player is offline or disconnected
        /// </summary>
        Offline,
        
        /// <summary>
        /// Player is in game
        /// </summary>
        InGame,
        
        /// <summary>
        /// Player is in spectator mode
        /// </summary>
        Spectating
    }
} 