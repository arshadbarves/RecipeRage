using System.Collections.Generic;
using Epic.OnlineServices;

namespace Core.Networking.Common
{


    /// <summary>
    /// Enum representing the different character classes in RecipeRage.
    /// </summary>
    public enum CharacterClass
    {
        Chef,
        Waiter,
        Dishwasher,
        Manager
    }

    // TODO: Need to have multiple teams
    public enum TeamId
    {
        TeamA = 0,
        TeamB = 1
    }

    /// <summary>
    /// Class representing a player in the game.
    /// </summary>
    public class PlayerInfo
    {
        /// <summary>
        /// The player's unique ID.
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// The player's display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Whether this player is the local player.
        /// </summary>
        public bool IsLocal { get; set; }

        /// <summary>
        /// Whether this player is the host.
        /// </summary>
        public bool IsHost { get; set; }

        /// <summary>
        /// The player's team ID.
        /// </summary>
        public TeamId Team { get; set; }

        /// <summary>
        /// The player's character class.
        /// </summary>
        public CharacterClass CharacterClass { get; set; }

        /// <summary>
        /// Whether the player is ready to start the game.
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// Whether this player is a bot.
        /// </summary>
        public bool IsBot { get; set; }

        /// <summary>
        /// The player's EOS Product User ID.
        /// </summary>
        public ProductUserId ProductUserId { get; set; }

        /// <summary>
        /// Custom data associated with the player.
        /// </summary>
        public Dictionary<string, string> CustomData { get; set; }

        // TODO: This needs to be create by somewhere
        public PlayerInfo()
        {
            PlayerId = string.Empty;
            DisplayName = "Player";
            IsLocal = false;
            IsHost = false;
            Team = TeamId.TeamA;
            CharacterClass = CharacterClass.Chef;
            IsReady = false;
            IsBot = false;
            ProductUserId = null;
            CustomData = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Class representing a game session.
    /// </summary>
    public class GameSessionInfo
    {
        /// <summary>
        /// The session's unique ID.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// The session's name.
        /// </summary>
        public string SessionName { get; set; }

        /// <summary>
        /// The number of players in the session.
        /// </summary>
        public int PlayerCount { get; set; }

        /// <summary>
        /// The maximum number of players allowed in the session.
        /// </summary>
        public int MaxPlayers { get; set; }

        /// <summary>
        /// Whether the session is private.
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// The host's display name.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// The game mode ID of the session.
        /// </summary>
        public string GameModeId { get; set; }

        /// <summary>
        /// The map name of the session.
        /// </summary>
        public string MapName { get; set; }

        // TODO: This needs to be create by somewhere
        public GameSessionInfo()
        {
            SessionId = string.Empty;
            SessionName = "New Game";
            PlayerCount = 0;
            MaxPlayers = 4;
            IsPrivate = false;
            HostName = string.Empty;
            GameModeId = "classic";
            MapName = "Kitchen";
        }
    }

    /// <summary>
    /// Class representing a player action in the game.
    /// </summary>
    public class PlayerAction
    {
        /// <summary>
        /// The type of action.
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// The target of the action (e.g., an ingredient ID).
        /// </summary>
        public string TargetId { get; set; }

        /// <summary>
        /// The position of the action.
        /// </summary>
        public UnityEngine.Vector3 Position { get; set; }

        /// <summary>
        /// Additional data for the action.
        /// </summary>
        public Dictionary<string, string> ActionData { get; set; }

        /// <summary>
        /// Create a new player action.
        /// </summary>
        public PlayerAction()
        {
            ActionType = string.Empty;
            TargetId = string.Empty;
            Position = UnityEngine.Vector3.zero;
            ActionData = new Dictionary<string, string>();
        }
    }
}