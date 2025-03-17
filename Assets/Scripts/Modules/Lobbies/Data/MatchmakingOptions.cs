using System;
using System.Collections.Generic;

namespace RecipeRage.Modules.Lobbies.Data
{
    /// <summary>
    /// Defines parameters for a matchmaking session
    /// </summary>
    [Serializable]
    public class MatchmakingOptions
    {

        /// <summary>
        /// Create a default instance of matchmaking options
        /// </summary>
        public MatchmakingOptions()
        {
            // Default constructor
        }

        /// <summary>
        /// Create a copy of another matchmaking options instance
        /// </summary>
        /// <param name="other"> The instance to copy </param>
        public MatchmakingOptions(MatchmakingOptions other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            MinPlayers = other.MinPlayers;
            MaxPlayers = other.MaxPlayers;
            GameMode = other.GameMode;
            AllowJoinInProgress = other.AllowJoinInProgress;
            TimeoutSeconds = other.TimeoutSeconds;
            UseSkillBasedMatching = other.UseSkillBasedMatching;
            MaxSkillRatingDifference = other.MaxSkillRatingDifference;
            SessionId = other.SessionId;
            IsRematch = other.IsRematch;
            PreviousMatchId = other.PreviousMatchId;

            // Deep copy collections
            PreferredRegions = new List<string>(other.PreferredRegions);
            Attributes = new Dictionary<string, string>(other.Attributes);
        }

        /// <summary>
        /// Minimum number of players required to start a match
        /// </summary>
        public int MinPlayers { get; set; } = 2;

        /// <summary>
        /// Maximum number of players allowed in the match
        /// </summary>
        public int MaxPlayers { get; set; } = 4;

        /// <summary>
        /// The game mode to match into (e.g., "casual", "competitive")
        /// </summary>
        public string GameMode { get; set; } = "casual";

        /// <summary>
        /// Whether to allow joining in progress after match starts
        /// </summary>
        public bool AllowJoinInProgress { get; set; } = true;

        /// <summary>
        /// Maximum time in seconds to search for a match before timing out
        /// </summary>
        public int TimeoutSeconds { get; set; } = 120;

        /// <summary>
        /// Ordered list of preferred server regions
        /// </summary>
        public List<string> PreferredRegions { get; set; } = new List<string>();

        /// <summary>
        /// Additional attributes to use for matchmaking
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Whether to match based on skill rating
        /// </summary>
        public bool UseSkillBasedMatching { get; set; } = true;

        /// <summary>
        /// Maximum skill rating difference allowed (if UseSkillBasedMatching is true)
        /// </summary>
        public float MaxSkillRatingDifference { get; set; } = 500.0f;

        /// <summary>
        /// An ID to identify this matchmaking session
        /// </summary>
        public string SessionId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Whether this is a rematch of a previous game
        /// </summary>
        public bool IsRematch { get; set; }

        /// <summary>
        /// ID of the previous match if this is a rematch
        /// </summary>
        public string PreviousMatchId { get; set; } = string.Empty;

        /// <summary>
        /// Create a string representation of the matchmaking options
        /// </summary>
        public override string ToString()
        {
            return $"MatchmakingOptions[Mode={GameMode}, Players={MinPlayers}-{MaxPlayers}, Timeout={TimeoutSeconds}s]";
        }
    }
}