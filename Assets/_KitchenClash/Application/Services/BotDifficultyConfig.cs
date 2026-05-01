using KitchenClash.Domain;

namespace KitchenClash.Application.Services
{
    public sealed class BotDifficultyConfig
    {
        public float ReactionDelay { get; }
        public float MistakeChance { get; }
        public bool CanExtinguishFires { get; }
        public float FireExtinguishChance { get; }

        private BotDifficultyConfig(float reactionDelay, float mistakeChance, bool canExtinguish, float fireChance)
        {
            ReactionDelay = reactionDelay;
            MistakeChance = mistakeChance;
            CanExtinguishFires = canExtinguish;
            FireExtinguishChance = fireChance;
        }

        public static BotDifficultyConfig FromDifficulty(BotDifficulty difficulty)
        {
            return difficulty switch
            {
                BotDifficulty.Easy   => new BotDifficultyConfig(2.0f, 0.30f, false, 0f),
                BotDifficulty.Medium => new BotDifficultyConfig(1.0f, 0.10f, true,  0.5f),
                BotDifficulty.Hard   => new BotDifficultyConfig(0.5f, 0.00f, true,  1.0f),
                _                    => new BotDifficultyConfig(1.0f, 0.10f, true,  0.5f),
            };
        }
    }
}
