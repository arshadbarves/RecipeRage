using Playcenter.Services;
using UnityEngine;

namespace RecipeRage.Bots
{
    /// <summary>
    /// Maps human recipes/minute to a bot dwell scale. Scale >1 = bots act slower
    /// (easier); <1 = faster, but never below the human-optimal floor (0.85).
    /// </summary>
    public sealed class AdaptiveDifficulty
    {
        private readonly IConfigService _config;

        public AdaptiveDifficulty(IConfigService config)
        {
            _config = config;
        }

        public float ComputeDwellScale(float humanRecipesPerMin)
        {
            var baseline = _config.Get("bot_difficulty_baseline_rpm", 1.5f);
            var minScale = _config.Get("bot_difficulty_min_scale", 0.85f);
            var maxScale = _config.Get("bot_difficulty_max_scale", 1.3f);

            if (humanRecipesPerMin <= 0.01f)
            {
                return maxScale; // cold start: bots go easy until humans prove pace
            }

            var ratio = baseline / humanRecipesPerMin;
            return Mathf.Clamp(ratio, minScale, maxScale);
        }
    }
}
