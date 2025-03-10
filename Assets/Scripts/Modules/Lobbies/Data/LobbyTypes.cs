using System;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage.Lobbies
{
    /// <summary>
    /// Types of permission levels for lobbies
    /// </summary>
    public enum LobbyPermissionLevel
    {
        /// <summary>
        /// Anyone can find and join the lobby
        /// </summary>
        Public,
        
        /// <summary>
        /// Anyone can find the lobby, but joining requires an invitation
        /// </summary>
        InviteOnly,
        
        /// <summary>
        /// The lobby cannot be found, joining requires an invitation
        /// </summary>
        Private
    }

    /// <summary>
    /// Types of attributes for lobbies and lobby members
    /// </summary>
    public enum AttributeType
    {
        /// <summary>
        /// String attribute
        /// </summary>
        String,
        
        /// <summary>
        /// Integer attribute
        /// </summary>
        Int64,
        
        /// <summary>
        /// Double attribute
        /// </summary>
        Double,
        
        /// <summary>
        /// Boolean attribute
        /// </summary>
        Boolean
    }

    /// <summary>
    /// Visibility level for lobby attributes
    /// </summary>
    public enum AttributeVisibility
    {
        /// <summary>
        /// Attribute is visible to everyone
        /// </summary>
        Public,
        
        /// <summary>
        /// Attribute is only visible to members of the lobby
        /// </summary>
        Members
    }

    /// <summary>
    /// The status of a lobby member
    /// </summary>
    public enum LobbyMemberStatus
    {
        /// <summary>
        /// Member is joined and active
        /// </summary>
        Active,
        
        /// <summary>
        /// Member is in the process of joining
        /// </summary>
        Joining,
        
        /// <summary>
        /// Member has left
        /// </summary>
        Left,
        
        /// <summary>
        /// Member was kicked
        /// </summary>
        Kicked,
        
        /// <summary>
        /// Member disconnected
        /// </summary>
        Disconnected
    }

    /// <summary>
    /// Attribute for a lobby or lobby member
    /// </summary>
    public class LobbyAttribute
    {
        /// <summary>
        /// The key of the attribute
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The type of the attribute
        /// </summary>
        public AttributeType Type { get; set; }

        /// <summary>
        /// The visibility of the attribute
        /// </summary>
        public AttributeVisibility Visibility { get; set; }

        /// <summary>
        /// Value as a string
        /// </summary>
        public string AsString { get; set; }

        /// <summary>
        /// Value as an integer
        /// </summary>
        public long? AsInt64 { get; set; }

        /// <summary>
        /// Value as a double
        /// </summary>
        public double? AsDouble { get; set; }

        /// <summary>
        /// Value as a boolean
        /// </summary>
        public bool? AsBool { get; set; }

        /// <summary>
        /// Provider-specific data
        /// </summary>
        public object ProviderData { get; set; }

        /// <summary>
        /// Creates a new string attribute
        /// </summary>
        /// <param name="key">Attribute key</param>
        /// <param name="value">String value</param>
        /// <param name="visibility">Attribute visibility</param>
        /// <returns>A new lobby attribute</returns>
        public static LobbyAttribute CreateString(string key, string value, AttributeVisibility visibility = AttributeVisibility.Public)
        {
            return new LobbyAttribute
            {
                Key = key,
                Type = AttributeType.String,
                AsString = value,
                Visibility = visibility
            };
        }

        /// <summary>
        /// Creates a new integer attribute
        /// </summary>
        /// <param name="key">Attribute key</param>
        /// <param name="value">Integer value</param>
        /// <param name="visibility">Attribute visibility</param>
        /// <returns>A new lobby attribute</returns>
        public static LobbyAttribute CreateInt64(string key, long value, AttributeVisibility visibility = AttributeVisibility.Public)
        {
            return new LobbyAttribute
            {
                Key = key,
                Type = AttributeType.Int64,
                AsInt64 = value,
                Visibility = visibility
            };
        }

        /// <summary>
        /// Creates a new double attribute
        /// </summary>
        /// <param name="key">Attribute key</param>
        /// <param name="value">Double value</param>
        /// <param name="visibility">Attribute visibility</param>
        /// <returns>A new lobby attribute</returns>
        public static LobbyAttribute CreateDouble(string key, double value, AttributeVisibility visibility = AttributeVisibility.Public)
        {
            return new LobbyAttribute
            {
                Key = key,
                Type = AttributeType.Double,
                AsDouble = value,
                Visibility = visibility
            };
        }

        /// <summary>
        /// Creates a new boolean attribute
        /// </summary>
        /// <param name="key">Attribute key</param>
        /// <param name="value">Boolean value</param>
        /// <param name="visibility">Attribute visibility</param>
        /// <returns>A new lobby attribute</returns>
        public static LobbyAttribute CreateBool(string key, bool value, AttributeVisibility visibility = AttributeVisibility.Public)
        {
            return new LobbyAttribute
            {
                Key = key,
                Type = AttributeType.Boolean,
                AsBool = value,
                Visibility = visibility
            };
        }
    }

    /// <summary>
    /// Voice chat state of a lobby member
    /// </summary>
    public class VoiceChatState
    {
        /// <summary>
        /// Whether the member is in a voice chat room
        /// </summary>
        public bool IsInVoiceChat { get; set; }

        /// <summary>
        /// Whether the member is currently talking
        /// </summary>
        public bool IsTalking { get; set; }

        /// <summary>
        /// Whether the member is muted locally
        /// </summary>
        public bool IsLocalMuted { get; set; }

        /// <summary>
        /// Whether the member has disabled their audio output
        /// </summary>
        public bool IsAudioOutputDisabled { get; set; }

        /// <summary>
        /// Whether press-to-talk is enabled for this member
        /// </summary>
        public bool IsPressToTalkEnabled { get; set; }

        /// <summary>
        /// Whether the member is blocked
        /// </summary>
        public bool IsBlocked { get; set; }
    }

    /// <summary>
    /// A member of a lobby
    /// </summary>
    public class LobbyMember
    {
        /// <summary>
        /// ID of the member
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display name of the member
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Current status of the member
        /// </summary>
        public LobbyMemberStatus Status { get; set; }

        /// <summary>
        /// Attributes of the member
        /// </summary>
        public Dictionary<string, LobbyAttribute> Attributes { get; set; } = new Dictionary<string, LobbyAttribute>();

        /// <summary>
        /// Voice chat state of the member
        /// </summary>
        public VoiceChatState VoiceState { get; set; } = new VoiceChatState();

        /// <summary>
        /// Provider-specific data
        /// </summary>
        public object ProviderData { get; set; }

        /// <summary>
        /// Whether this member is the local user
        /// </summary>
        public bool IsLocalMember { get; set; }
    }

    /// <summary>
    /// A searchable lobby
    /// </summary>
    public class Lobby
    {
        /// <summary>
        /// ID of the lobby
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the lobby owner
        /// </summary>
        public string OwnerId { get; set; }

        /// <summary>
        /// Display name of the lobby owner
        /// </summary>
        public string OwnerDisplayName { get; set; }

        /// <summary>
        /// Bucket ID for matchmaking
        /// </summary>
        public string BucketId { get; set; }

        /// <summary>
        /// Maximum number of members allowed in the lobby
        /// </summary>
        public int MaxMembers { get; set; }

        /// <summary>
        /// Current number of members in the lobby
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// Permission level of the lobby
        /// </summary>
        public LobbyPermissionLevel PermissionLevel { get; set; }

        /// <summary>
        /// Whether invites are allowed
        /// </summary>
        public bool AllowInvites { get; set; }

        /// <summary>
        /// Whether voice chat is enabled
        /// </summary>
        public bool VoiceChatEnabled { get; set; }

        /// <summary>
        /// Whether host migration is disabled
        /// </summary>
        public bool DisableHostMigration { get; set; }

        /// <summary>
        /// Whether this lobby is from a search result
        /// </summary>
        public bool IsSearchResult { get; set; }

        /// <summary>
        /// Attributes of the lobby
        /// </summary>
        public Dictionary<string, LobbyAttribute> Attributes { get; set; } = new Dictionary<string, LobbyAttribute>();

        /// <summary>
        /// Members of the lobby
        /// </summary>
        public List<LobbyMember> Members { get; set; } = new List<LobbyMember>();

        /// <summary>
        /// Name of the provider that created this lobby
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Provider-specific data
        /// </summary>
        public object ProviderData { get; set; }

        /// <summary>
        /// Checks if the lobby is valid
        /// </summary>
        /// <returns>True if the lobby has a valid ID</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Id);
        }

        /// <summary>
        /// Checks if the local user is the owner of the lobby
        /// </summary>
        /// <returns>True if the local user is the owner</returns>
        public bool IsLocalUserOwner()
        {
            if (Members == null)
                return false;

            foreach (var member in Members)
            {
                if (member.IsLocalMember && member.Id == OwnerId)
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// An invitation to join a lobby
    /// </summary>
    public class LobbyInvite
    {
        /// <summary>
        /// ID of the invitation
        /// </summary>
        public string InviteId { get; set; }

        /// <summary>
        /// Lobby the invitation is for
        /// </summary>
        public Lobby Lobby { get; set; }

        /// <summary>
        /// ID of the user who sent the invitation
        /// </summary>
        public string SenderId { get; set; }

        /// <summary>
        /// Display name of the user who sent the invitation
        /// </summary>
        public string SenderDisplayName { get; set; }

        /// <summary>
        /// Provider-specific data
        /// </summary>
        public object ProviderData { get; set; }

        /// <summary>
        /// Checks if the invitation is valid
        /// </summary>
        /// <returns>True if the invitation has a valid ID and lobby</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(InviteId) && Lobby != null && Lobby.IsValid();
        }
    }

    /// <summary>
    /// Configuration for creating a new lobby
    /// </summary>
    public class LobbyConfig
    {
        /// <summary>
        /// Name of the lobby
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Maximum number of members allowed in the lobby
        /// </summary>
        public int MaxMembers { get; set; } = 4;

        /// <summary>
        /// Permission level of the lobby
        /// </summary>
        public LobbyPermissionLevel PermissionLevel { get; set; } = LobbyPermissionLevel.Public;

        /// <summary>
        /// Whether invites are allowed
        /// </summary>
        public bool AllowInvites { get; set; } = true;

        /// <summary>
        /// Whether voice chat is enabled
        /// </summary>
        public bool EnableVoiceChat { get; set; } = false;

        /// <summary>
        /// Whether host migration is disabled
        /// </summary>
        public bool DisableHostMigration { get; set; } = false;

        /// <summary>
        /// Bucket ID for matchmaking
        /// </summary>
        public string BucketId { get; set; }

        /// <summary>
        /// Initial attributes of the lobby
        /// </summary>
        public Dictionary<string, LobbyAttribute> Attributes { get; set; } = new Dictionary<string, LobbyAttribute>();

        /// <summary>
        /// Name of the provider to use
        /// </summary>
        public string ProviderName { get; set; }
    }

    /// <summary>
    /// Parameters for searching lobbies
    /// </summary>
    public class LobbySearchParams
    {
        /// <summary>
        /// Maximum number of results to return
        /// </summary>
        public int MaxResults { get; set; } = 20;

        /// <summary>
        /// Search by specific lobby ID
        /// </summary>
        public string LobbyId { get; set; }

        /// <summary>
        /// Search by bucket ID
        /// </summary>
        public string BucketId { get; set; }

        /// <summary>
        /// Attributes to search by
        /// </summary>
        public Dictionary<string, LobbyAttribute> Attributes { get; set; } = new Dictionary<string, LobbyAttribute>();

        /// <summary>
        /// Name of the provider to use
        /// </summary>
        public string ProviderName { get; set; }
    }
} 