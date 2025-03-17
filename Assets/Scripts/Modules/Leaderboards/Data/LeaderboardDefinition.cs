using System;
using UnityEngine;

namespace RecipeRage.Leaderboards
{
    /// <summary>
    /// Defines the ordering direction for a leaderboard
    /// </summary>
    public enum LeaderboardOrderingType
    {
        /// <summary>
        /// Higher scores are better (e.g., points)
        /// </summary>
        Descending,

        /// <summary>
        /// Lower scores are better (e.g., time)
        /// </summary>
        Ascending
    }

    /// <summary>
    /// Defines the display type for a leaderboard
    /// </summary>
    public enum LeaderboardDisplayType
    {
        /// <summary>
        /// Display as numeric value
        /// </summary>
        Numeric,

        /// <summary>
        /// Display as time (seconds)
        /// </summary>
        Time,

        /// <summary>
        /// Display as currency
        /// </summary>
        Currency
    }

    /// <summary>
    /// Represents metadata about a leaderboard
    /// </summary>
    [Serializable]
    public class LeaderboardDefinition
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public LeaderboardDefinition()
        {
            IsActive = true;
            OrderingType = LeaderboardOrderingType.Descending;
            DisplayType = LeaderboardDisplayType.Numeric;
        }

        /// <summary>
        /// Creates a new leaderboard definition
        /// </summary>
        /// <param name="leaderboardId"> Unique identifier for the leaderboard </param>
        /// <param name="displayName"> Display name for the leaderboard </param>
        /// <param name="statName"> Stat name this leaderboard is based on </param>
        /// <param name="providerName"> Provider this definition came from </param>
        public LeaderboardDefinition(string leaderboardId, string displayName, string statName, string providerName)
        {
            LeaderboardId = leaderboardId;
            DisplayName = displayName;
            StatName = statName;
            ProviderName = providerName;
            IsActive = true;
            OrderingType = LeaderboardOrderingType.Descending;
            DisplayType = LeaderboardDisplayType.Numeric;
        }

        /// <summary>
        /// Unique identifier for the leaderboard
        /// </summary>
        public string LeaderboardId { get; set; }

        /// <summary>
        /// Display name for the leaderboard
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Description of the leaderboard
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Stat name that this leaderboard is based on
        /// </summary>
        public string StatName { get; set; }

        /// <summary>
        /// Ordering type for the leaderboard
        /// </summary>
        public LeaderboardOrderingType OrderingType { get; set; }

        /// <summary>
        /// Display type for the leaderboard scores
        /// </summary>
        public LeaderboardDisplayType DisplayType { get; set; }

        /// <summary>
        /// Icon for the leaderboard (if available)
        /// </summary>
        public Sprite Icon { get; set; }

        /// <summary>
        /// Category of the leaderboard for grouping
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Indicates if the leaderboard is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Provider-specific data for the leaderboard
        /// </summary>
        public object ProviderData { get; set; }

        /// <summary>
        /// Name of the provider this definition came from
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Determines if a score is better than another based on the ordering type
        /// </summary>
        /// <param name="newScore"> New score to check </param>
        /// <param name="currentScore"> Current score to compare against </param>
        /// <returns> True if the new score is better than the current score </returns>
        public bool IsScoreBetter(long newScore, long currentScore)
        {
            return OrderingType == LeaderboardOrderingType.Descending
                ? newScore > currentScore
                : newScore < currentScore;
        }
    }
}