using System;
using System.Collections.Generic;

namespace RecipeRage.Modules.Lobbies.Data
{
    /// <summary>
    /// Represents a member in a lobby, including their attributes and status.
    /// </summary>
    [Serializable]
    public class LobbyMember
    {

        /// <summary>
        /// Create a new LobbyMember object
        /// </summary>
        public LobbyMember()
        {
            Attributes = new Dictionary<string, string>();
            Status = LobbyMemberStatus.Online;
            JoinedAt = DateTime.UtcNow;
            LastUpdatedAt = DateTime.UtcNow;
            IsConnected = true;
        }

        /// <summary>
        /// Create a new LobbyMember object with specific values
        /// </summary>
        /// <param name="playerId"> ID of the player </param>
        /// <param name="displayName"> Display name of the player </param>
        /// <param name="isOwner"> Whether the player is the owner </param>
        public LobbyMember(string playerId, string displayName, bool isOwner = false)
            : this()
        {
            PlayerId = playerId;
            DisplayName = displayName;
            IsOwner = isOwner;
        }

        /// <summary>
        /// Unique identifier for the player
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// Alias for PlayerId, to maintain compatibility with some APIs
        /// </summary>
        public string UserId { get => PlayerId; set => PlayerId = value; }

        /// <summary>
        /// Display name of the player
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Status of the player in the lobby
        /// </summary>
        public LobbyMemberStatus Status { get; set; }

        /// <summary>
        /// Attributes of the player
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }

        /// <summary>
        /// Whether the player is the owner of the lobby
        /// </summary>
        public bool IsOwner { get; set; }

        /// <summary>
        /// Whether the player is ready to start the game
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// Whether the player is currently connected
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Whether the player has voice chat enabled
        /// </summary>
        public bool VoiceChatEnabled { get; set; }

        /// <summary>
        /// Time when the player joined the lobby
        /// </summary>
        public DateTime JoinedAt { get; set; }

        /// <summary>
        /// Time when the player's status was last updated
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }

        /// <summary>
        /// Provider-specific data for the player
        /// </summary>
        public object ProviderData { get; set; }

        /// <summary>
        /// Get the value of an attribute
        /// </summary>
        /// <param name="key"> Key of the attribute </param>
        /// <returns> Value of the attribute, or null if not found </returns>
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
        /// <typeparam name="T"> Type to convert to </typeparam>
        /// <param name="key"> Key of the attribute </param>
        /// <param name="defaultValue"> Default value if the attribute is not found or conversion fails </param>
        /// <returns> Value of the attribute as the specified type, or defaultValue if not found or conversion fails </returns>
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

        /// <summary>
        /// Set an attribute value
        /// </summary>
        /// <param name="key"> Key of the attribute </param>
        /// <param name="value"> Value to set </param>
        public void SetAttribute(string key, string value)
        {
            Attributes[key] = value;
            LastUpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Status of a member in a lobby
    /// </summary>
    public enum LobbyMemberStatus
    {
        /// <summary> Player is online and active </summary>
        Online,

        /// <summary> Player is away </summary>
        Away,

        /// <summary> Player is busy </summary>
        Busy,

        /// <summary> Player is offline </summary>
        Offline,

        /// <summary> Player is in a game </summary>
        InGame,

        /// <summary> Player is spectating </summary>
        Spectating
    }
}