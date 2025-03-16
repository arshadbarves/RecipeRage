using System;
using System.Collections.Generic;

namespace RecipeRage.Modules.Lobbies.Data
{
    /// <summary>
    /// Contains information about a lobby
    /// </summary>
    [Serializable]
    public class LobbyInfo
    {
        /// <summary>
        /// Unique identifier for the lobby
        /// </summary>
        public string LobbyId { get; set; } = string.Empty;

        /// <summary>
        /// Name of the lobby
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Maximum number of players allowed in the lobby
        /// </summary>
        public int MaxPlayers { get; set; } = 0;

        /// <summary>
        /// Current number of players in the lobby
        /// </summary>
        public int CurrentPlayers { get; set; } = 0;

        /// <summary>
        /// Whether the lobby is public
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
        /// Players in the lobby
        /// </summary>
        public List<LobbyPlayerInfo> Players { get; set; } = new List<LobbyPlayerInfo>();

        /// <summary>
        /// Owner of the lobby
        /// </summary>
        public LobbyPlayerInfo OwnerPlayer { get; set; } = new LobbyPlayerInfo();

        /// <summary>
        /// Attributes associated with the lobby
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Time when the lobby was created
        /// </summary>
        public DateTime CreationTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Time when the lobby was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Whether the lobby connection is active
        /// </summary>
        public bool IsConnected { get; set; } = false;

        /// <summary>
        /// Whether the lobby is ready to play
        /// </summary>
        public bool IsReady { get; set; } = false;

        /// <summary>
        /// Provider that created this lobby
        /// </summary>
        public string ProviderName { get; set; } = string.Empty;

        /// <summary>
        /// Whether voice chat is enabled for this lobby
        /// </summary>
        public bool VoiceChatEnabled { get; set; } = false;

        /// <summary>
        /// Bucket ID used for matchmaking
        /// </summary>
        public string BucketId { get; set; } = string.Empty;

        /// <summary>
        /// Whether this lobby was created through matchmaking
        /// </summary>
        public bool IsMatchmakingLobby { get; set; } = false;

        /// <summary>
        /// Server region for this lobby
        /// </summary>
        public string Region { get; set; } = string.Empty;

        /// <summary>
        /// Game mode for this lobby
        /// </summary>
        public string GameMode => GetAttributeOrDefault("GameMode", "default");

        /// <summary>
        /// Whether this lobby is currently in a match
        /// </summary>
        public bool IsInMatch => GetAttributeAsBool("IsInMatch", false);

        /// <summary>
        /// Match ID if this lobby is in a match
        /// </summary>
        public string MatchId => GetAttributeOrDefault("MatchId", string.Empty);

        /// <summary>
        /// ID of the matchmaking session that created this lobby
        /// </summary>
        public string MatchmakingSessionId => GetAttributeOrDefault("MatchmakingSessionId", string.Empty);

        /// <summary>
        /// Whether this lobby is for a skill-based match
        /// </summary>
        public bool IsSkillBasedMatch => GetAttributeAsBool("IsSkillBasedMatch", false);

        /// <summary>
        /// Average skill rating of players in this lobby
        /// </summary>
        public float AverageSkillRating => GetAttributeAsFloat("AverageSkillRating", 0f);

        /// <summary>
        /// Whether this lobby is for a rematch of a previous game
        /// </summary>
        public bool IsRematch => GetAttributeAsBool("IsRematch", false);

        /// <summary>
        /// ID of the previous match if this is a rematch
        /// </summary>
        public string PreviousMatchId => GetAttributeOrDefault("PreviousMatchId", string.Empty);

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
        /// Check if the lobby has the attribute with the given key
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True if the attribute exists</returns>
        public bool HasAttribute(string key)
        {
            return Attributes != null && Attributes.ContainsKey(key);
        }

        /// <summary>
        /// Convert matchmaking options to lobby attributes
        /// </summary>
        /// <param name="options">Matchmaking options to convert</param>
        public void ApplyMatchmakingOptions(MatchmakingOptions options)
        {
            if (options == null) return;

            // Set matchmaking attributes
            Attributes["GameMode"] = options.GameMode;
            Attributes["MinPlayers"] = options.MinPlayers.ToString();
            Attributes["MaxPlayers"] = options.MaxPlayers.ToString();
            Attributes["AllowJoinInProgress"] = options.AllowJoinInProgress.ToString();
            Attributes["IsMatchmakingLobby"] = "true";
            Attributes["MatchmakingSessionId"] = options.SessionId;
            
            if (options.IsRematch)
            {
                Attributes["IsRematch"] = "true";
                Attributes["PreviousMatchId"] = options.PreviousMatchId;
            }
            
            if (options.UseSkillBasedMatching)
            {
                Attributes["IsSkillBasedMatch"] = "true";
                Attributes["MaxSkillRatingDifference"] = options.MaxSkillRatingDifference.ToString();
            }
            
            // Copy any custom attributes
            foreach (var kvp in options.Attributes)
            {
                Attributes[kvp.Key] = kvp.Value;
            }
            
            // Mark this as a matchmaking lobby
            IsMatchmakingLobby = true;
            
            // Set region from preferred regions if available
            if (options.PreferredRegions.Count > 0)
            {
                Region = options.PreferredRegions[0];
                Attributes["Region"] = Region;
            }
        }

        /// <summary>
        /// Create a string representation of the lobby info
        /// </summary>
        public override string ToString()
        {
            return $"LobbyInfo[ID={LobbyId}, Name={Name}, Players={CurrentPlayers}/{MaxPlayers}]";
        }
    }
} 