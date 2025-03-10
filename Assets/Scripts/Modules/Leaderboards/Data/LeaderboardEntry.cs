using System;
using UnityEngine;

namespace RecipeRage.Leaderboards
{
    /// <summary>
    /// Represents a single entry on a leaderboard
    /// </summary>
    [Serializable]
    public class LeaderboardEntry
    {
        /// <summary>
        /// ID of the leaderboard this entry belongs to
        /// </summary>
        public string LeaderboardId { get; set; }
        
        /// <summary>
        /// User ID of the player
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Display name of the player
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Rank of the player on the leaderboard (1-based)
        /// </summary>
        public int Rank { get; set; }
        
        /// <summary>
        /// Score value
        /// </summary>
        public long Score { get; set; }
        
        /// <summary>
        /// Additional metadata for the score (if any)
        /// </summary>
        public string Metadata { get; set; }
        
        /// <summary>
        /// Timestamp when the score was achieved
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Indicates if this is the local player's entry
        /// </summary>
        public bool IsCurrentUser { get; set; }
        
        /// <summary>
        /// Indicates if this player is a friend of the current user
        /// </summary>
        public bool IsFriend { get; set; }
        
        /// <summary>
        /// Optional avatar or icon for the player
        /// </summary>
        public Sprite Avatar { get; set; }
        
        /// <summary>
        /// The name of the provider this entry came from
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public LeaderboardEntry()
        {
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a new leaderboard entry
        /// </summary>
        /// <param name="leaderboardId">ID of the leaderboard</param>
        /// <param name="userId">User ID of the player</param>
        /// <param name="displayName">Display name of the player</param>
        /// <param name="rank">Rank of the player on the leaderboard</param>
        /// <param name="score">Score value</param>
        /// <param name="providerName">Name of the provider</param>
        public LeaderboardEntry(string leaderboardId, string userId, string displayName, int rank, long score, string providerName)
        {
            LeaderboardId = leaderboardId;
            UserId = userId;
            DisplayName = displayName;
            Rank = rank;
            Score = score;
            ProviderName = providerName;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Formatted display version of the score based on leaderboard definition settings
        /// </summary>
        /// <param name="definition">Leaderboard definition to use for formatting</param>
        /// <returns>Formatted score string</returns>
        public string GetFormattedScore(LeaderboardDefinition definition)
        {
            if (definition == null)
                return Score.ToString();

            switch (definition.DisplayType)
            {
                case LeaderboardDisplayType.Time:
                    TimeSpan time = TimeSpan.FromSeconds(Score);
                    // Format as MM:SS for times less than an hour, otherwise HH:MM:SS
                    return time.TotalHours >= 1
                        ? $"{(int)time.TotalHours}:{time.Minutes:D2}:{time.Seconds:D2}"
                        : $"{time.Minutes:D2}:{time.Seconds:D2}";

                case LeaderboardDisplayType.Currency:
                    return $"{Score:N0}";

                case LeaderboardDisplayType.Numeric:
                default:
                    return Score.ToString("N0");
            }
        }
    }
} 