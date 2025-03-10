using System;
using System.Collections.Generic;

namespace RecipeRage.Leaderboards
{
    /// <summary>
    /// Utility class for formatting leaderboard data
    /// </summary>
    public static class LeaderboardsFormatter
    {
        /// <summary>
        /// Format a rank with the appropriate suffix (1st, 2nd, 3rd, etc.)
        /// </summary>
        /// <param name="rank">The rank to format</param>
        /// <returns>Formatted rank with suffix</returns>
        public static string FormatRank(int rank)
        {
            if (rank <= 0)
                return "-";

            string suffix = GetRankSuffix(rank);
            return $"{rank}{suffix}";
        }

        /// <summary>
        /// Get the suffix for a rank (st, nd, rd, th)
        /// </summary>
        /// <param name="rank">The rank to get a suffix for</param>
        /// <returns>The appropriate suffix</returns>
        private static string GetRankSuffix(int rank)
        {
            if (rank % 100 >= 11 && rank % 100 <= 13)
                return "th";

            switch (rank % 10)
            {
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
                default:
                    return "th";
            }
        }

        /// <summary>
        /// Format a score based on the leaderboard definition
        /// </summary>
        /// <param name="score">The score to format</param>
        /// <param name="definition">The leaderboard definition</param>
        /// <returns>Formatted score string</returns>
        public static string FormatScore(long score, LeaderboardDefinition definition)
        {
            if (definition == null)
                return score.ToString("N0");

            switch (definition.DisplayType)
            {
                case LeaderboardDisplayType.Time:
                    TimeSpan time = TimeSpan.FromSeconds(score);
                    return time.TotalHours >= 1
                        ? $"{(int)time.TotalHours}:{time.Minutes:D2}:{time.Seconds:D2}"
                        : $"{time.Minutes:D2}:{time.Seconds:D2}";

                case LeaderboardDisplayType.Currency:
                    return $"{score:N0}";

                case LeaderboardDisplayType.Numeric:
                default:
                    return score.ToString("N0");
            }
        }

        /// <summary>
        /// Format a timestamp relative to now
        /// </summary>
        /// <param name="timestamp">The timestamp to format</param>
        /// <returns>Relative time string (e.g. "2 days ago")</returns>
        public static string FormatRelativeTime(DateTime timestamp)
        {
            TimeSpan timeDiff = DateTime.UtcNow - timestamp;

            if (timeDiff.TotalSeconds < 60)
                return "Just now";
            
            if (timeDiff.TotalMinutes < 60)
            {
                int minutes = (int)timeDiff.TotalMinutes;
                return $"{minutes} {(minutes == 1 ? "minute" : "minutes")} ago";
            }
            
            if (timeDiff.TotalHours < 24)
            {
                int hours = (int)timeDiff.TotalHours;
                return $"{hours} {(hours == 1 ? "hour" : "hours")} ago";
            }
            
            if (timeDiff.TotalDays < 30)
            {
                int days = (int)timeDiff.TotalDays;
                return $"{days} {(days == 1 ? "day" : "days")} ago";
            }
            
            int months = (int)(timeDiff.TotalDays / 30);
            if (months < 12)
                return $"{months} {(months == 1 ? "month" : "months")} ago";
            
            int years = (int)(timeDiff.TotalDays / 365);
            return $"{years} {(years == 1 ? "year" : "years")} ago";
        }

        /// <summary>
        /// Format a timestamp as a short date and time
        /// </summary>
        /// <param name="timestamp">The timestamp to format</param>
        /// <returns>Formatted date and time</returns>
        public static string FormatTimestamp(DateTime timestamp)
        {
            if (timestamp.Date == DateTime.UtcNow.Date)
                return $"Today at {timestamp.ToString("h:mm tt")}";
            
            if (timestamp.Date == DateTime.UtcNow.Date.AddDays(-1))
                return $"Yesterday at {timestamp.ToString("h:mm tt")}";
            
            return timestamp.ToString("MMM d, yyyy h:mm tt");
        }
    }
} 