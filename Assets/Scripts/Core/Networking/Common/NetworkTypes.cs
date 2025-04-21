using System.Collections.Generic;
using Epic.OnlineServices;

namespace Core.Networking.Common
{
    /// <summary>
    /// Enum representing the different game modes in RecipeRage.
    /// </summary>
    public enum GameMode
    {
        Classic,
        TimeAttack,
        TeamBattle
    }

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

    /// <summary>
    /// Enum representing the different team IDs in RecipeRage.
    /// </summary>
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
        /// The player's EOS Product User ID.
        /// </summary>
        public ProductUserId ProductUserId { get; set; }

        /// <summary>
        /// Custom data associated with the player.
        /// </summary>
        public Dictionary<string, string> CustomData { get; set; }

        /// <summary>
        /// Create a new player info.
        /// </summary>
        public PlayerInfo()
        {
            PlayerId = string.Empty;
            DisplayName = "Player";
            IsLocal = false;
            IsHost = false;
            Team = TeamId.TeamA;
            CharacterClass = CharacterClass.Chef;
            IsReady = false;
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
        /// The game mode of the session.
        /// </summary>
        public GameMode GameMode { get; set; }

        /// <summary>
        /// The map name of the session.
        /// </summary>
        public string MapName { get; set; }

        /// <summary>
        /// Create a new game session info.
        /// </summary>
        public GameSessionInfo()
        {
            SessionId = string.Empty;
            SessionName = "New Game";
            PlayerCount = 0;
            MaxPlayers = 4;
            IsPrivate = false;
            HostName = string.Empty;
            GameMode = GameMode.Classic;
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

    /// <summary>
    /// Constants for network message types.
    /// </summary>
    public static class NetworkMessageType
    {
        // System messages (0-9)
        public const byte Heartbeat = 0;
        public const byte Disconnect = 1;
        public const byte PlayerInfo = 2;
        
        // Lobby messages (10-19)
        public const byte LobbyState = 10;
        public const byte PlayerReady = 11;
        public const byte TeamChange = 12;
        public const byte CharacterChange = 13;
        public const byte GameModeChange = 14;
        public const byte MapChange = 15;
        
        // Game messages (20-29)
        public const byte GameState = 20;
        public const byte TeamScore = 21;
        public const byte GameEnded = 22;
        public const byte PlayerAction = 23;
        public const byte ObjectState = 24;
        
        // Gameplay messages (30-39)
        public const byte IngredientSpawned = 30;
        public const byte IngredientPickedUp = 31;
        public const byte IngredientDropped = 32;
        public const byte IngredientProcessed = 33;
        public const byte RecipeCompleted = 34;
        public const byte RecipeFailed = 35;
        
        // Chat messages (40-49)
        public const byte ChatMessage = 40;
        public const byte Emote = 41;
        
        // Custom messages (50+)
        public const byte Custom = 50;
    }
}
