using System;

namespace RecipeRage.Leaderboards
{
    /// <summary>
    /// Defines the relationship between a stat and a leaderboard
    /// </summary>
    [Serializable]
    public class LeaderboardStatInfo
    {
        /// <summary>
        /// ID of the leaderboard
        /// </summary>
        public string LeaderboardId { get; set; }
        
        /// <summary>
        /// Name of the stat that feeds into this leaderboard
        /// </summary>
        public string StatName { get; set; }
        
        /// <summary>
        /// Indicates if scores should be automatically submitted when the stat changes
        /// </summary>
        public bool AutoSubmit { get; set; }
        
        /// <summary>
        /// Function to apply to the stat value before submitting to the leaderboard (if any)
        /// </summary>
        public LeaderboardStatTransformType TransformType { get; set; }
        
        /// <summary>
        /// Optional value to use in the transform function
        /// </summary>
        public float TransformValue { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public LeaderboardStatInfo()
        {
            AutoSubmit = true;
            TransformType = LeaderboardStatTransformType.None;
            TransformValue = 1.0f;
        }

        /// <summary>
        /// Creates a new leaderboard stat info
        /// </summary>
        /// <param name="leaderboardId">ID of the leaderboard</param>
        /// <param name="statName">Name of the stat</param>
        /// <param name="autoSubmit">Whether to auto-submit scores</param>
        public LeaderboardStatInfo(string leaderboardId, string statName, bool autoSubmit = true)
        {
            LeaderboardId = leaderboardId;
            StatName = statName;
            AutoSubmit = autoSubmit;
            TransformType = LeaderboardStatTransformType.None;
            TransformValue = 1.0f;
        }

        /// <summary>
        /// Applies the transform function to a stat value to get the leaderboard score
        /// </summary>
        /// <param name="statValue">The stat value to transform</param>
        /// <returns>The transformed score for the leaderboard</returns>
        public long TransformStatToScore(double statValue)
        {
            switch (TransformType)
            {
                case LeaderboardStatTransformType.Multiply:
                    return (long)(statValue * TransformValue);
                
                case LeaderboardStatTransformType.Divide:
                    if (TransformValue != 0)
                        return (long)(statValue / TransformValue);
                    return (long)statValue;
                
                case LeaderboardStatTransformType.Add:
                    return (long)(statValue + TransformValue);
                
                case LeaderboardStatTransformType.Subtract:
                    return (long)(statValue - TransformValue);
                
                case LeaderboardStatTransformType.Round:
                    return (long)Math.Round(statValue);
                
                case LeaderboardStatTransformType.Ceiling:
                    return (long)Math.Ceiling(statValue);
                
                case LeaderboardStatTransformType.Floor:
                    return (long)Math.Floor(statValue);
                
                case LeaderboardStatTransformType.None:
                default:
                    return (long)statValue;
            }
        }
    }

    /// <summary>
    /// Defines transform types that can be applied to stats before submitting to leaderboards
    /// </summary>
    public enum LeaderboardStatTransformType
    {
        /// <summary>
        /// No transformation, use the raw value
        /// </summary>
        None,
        
        /// <summary>
        /// Multiply the value by a factor
        /// </summary>
        Multiply,
        
        /// <summary>
        /// Divide the value by a divisor
        /// </summary>
        Divide,
        
        /// <summary>
        /// Add a value to the stat
        /// </summary>
        Add,
        
        /// <summary>
        /// Subtract a value from the stat
        /// </summary>
        Subtract,
        
        /// <summary>
        /// Round to the nearest integer
        /// </summary>
        Round,
        
        /// <summary>
        /// Round up to the next integer
        /// </summary>
        Ceiling,
        
        /// <summary>
        /// Round down to the previous integer
        /// </summary>
        Floor
    }
} 