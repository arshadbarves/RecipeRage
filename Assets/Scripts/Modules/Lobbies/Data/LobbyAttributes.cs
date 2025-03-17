namespace RecipeRage.Modules.Lobbies.Data
{
    /// <summary>
    /// Constants for commonly used lobby attributes.
    /// </summary>
    public static class LobbyAttributes
    {
        #region Core Lobby Attributes

        /// <summary>
        /// Game mode of the lobby
        /// </summary>
        public const string GameMode = "GameMode";

        /// <summary>
        /// Map or level being played
        /// </summary>
        public const string Map = "Map";

        /// <summary>
        /// Game version
        /// </summary>
        public const string GameVersion = "GameVersion";

        /// <summary>
        /// Whether the game is in progress
        /// </summary>
        public const string GameInProgress = "GameInProgress";

        /// <summary>
        /// Time when the game started
        /// </summary>
        public const string GameStartTime = "GameStartTime";

        /// <summary>
        /// Whether the lobby allows join-in-progress
        /// </summary>
        public const string AllowJoinInProgress = "AllowJoinInProgress";

        /// <summary>
        /// Minimum player level required to join
        /// </summary>
        public const string MinLevel = "MinLevel";

        /// <summary>
        /// Maximum player level allowed to join
        /// </summary>
        public const string MaxLevel = "MaxLevel";

        /// <summary>
        /// Whether the lobby is password protected
        /// </summary>
        public const string HasPassword = "HasPassword";

        /// <summary>
        /// Whether the lobby has voice chat enabled
        /// </summary>
        public const string VoiceChatEnabled = "VoiceChatEnabled";

        /// <summary>
        /// Whether the lobby is a competitive match
        /// </summary>
        public const string IsCompetitive = "IsCompetitive";

        /// <summary>
        /// Whether the lobby is friends-only
        /// </summary>
        public const string IsFriendsOnly = "IsFriendsOnly";

        /// <summary>
        /// The region of the lobby
        /// </summary>
        public const string Region = "Region";

        /// <summary>
        /// Ping/latency to the lobby server
        /// </summary>
        public const string Ping = "Ping";

        /// <summary>
        /// Average skill rating of lobby members
        /// </summary>
        public const string AverageSkill = "AverageSkill";

        #endregion

        #region Matchmaking Attributes

        /// <summary>
        /// Whether the lobby was created by matchmaking
        /// </summary>
        public const string IsMatchmade = "IsMatchmade";

        /// <summary>
        /// Minimum players required to start the game
        /// </summary>
        public const string MinPlayers = "MinPlayers";

        /// <summary>
        /// Maximum players allowed in the game
        /// </summary>
        public const string MaxPlayers = "MaxPlayers";

        /// <summary>
        /// Whether skill-based matchmaking is enabled
        /// </summary>
        public const string SkillBasedMatchmaking = "SkillBasedMatchmaking";

        /// <summary>
        /// Matchmaking ticket ID
        /// </summary>
        public const string MatchmakingTicket = "MatchmakingTicket";

        /// <summary>
        /// Time when matchmaking started
        /// </summary>
        public const string MatchmakingStartTime = "MatchmakingStartTime";

        /// <summary>
        /// Matchmaking queue name
        /// </summary>
        public const string MatchmakingQueue = "MatchmakingQueue";

        #endregion

        #region Player Attributes

        /// <summary>
        /// Whether the player is ready to start the game
        /// </summary>
        public const string PlayerReady = "Ready";

        /// <summary>
        /// Player's team
        /// </summary>
        public const string PlayerTeam = "Team";

        /// <summary>
        /// Player's role or character
        /// </summary>
        public const string PlayerRole = "Role";

        /// <summary>
        /// Player's skill rating
        /// </summary>
        public const string PlayerSkill = "Skill";

        /// <summary>
        /// Player's level
        /// </summary>
        public const string PlayerLevel = "Level";

        /// <summary>
        /// Player's preferred voice chat setting
        /// </summary>
        public const string PlayerVoiceChat = "VoiceChat";

        /// <summary>
        /// Whether the player is a host
        /// </summary>
        public const string PlayerIsHost = "IsHost";

        /// <summary>
        /// Whether the player is a spectator
        /// </summary>
        public const string PlayerIsSpectator = "IsSpectator";

        /// <summary>
        /// Player's connection quality
        /// </summary>
        public const string PlayerConnectionQuality = "ConnectionQuality";

        #endregion

        #region RecipeRage Game-Specific Attributes

        /// <summary>
        /// Game difficulty level
        /// </summary>
        public const string Difficulty = "Difficulty";

        /// <summary>
        /// Time limit for the match in seconds
        /// </summary>
        public const string TimeLimit = "TimeLimit";

        /// <summary>
        /// Score needed to win
        /// </summary>
        public const string ScoreToWin = "ScoreToWin";

        /// <summary>
        /// Whether friendly fire is enabled
        /// </summary>
        public const string FriendlyFire = "FriendlyFire";

        /// <summary>
        /// Whether special items are enabled
        /// </summary>
        public const string SpecialItems = "SpecialItems";

        /// <summary>
        /// Number of recipes required to win
        /// </summary>
        public const string RecipesToWin = "RecipesToWin";

        /// <summary>
        /// Whether kitchen hazards are enabled
        /// </summary>
        public const string KitchenHazards = "KitchenHazards";

        /// <summary>
        /// Game theme or modifier
        /// </summary>
        public const string GameTheme = "GameTheme";

        #endregion
    }
}