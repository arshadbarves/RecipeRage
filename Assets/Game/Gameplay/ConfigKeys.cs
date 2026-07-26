namespace RecipeRage
{
    /// <summary>
    /// All tunables. Every value externalized via IConfigService.Get(key, default).
    /// </summary>
    public static class ConfigKeys
    {
        public const string MatchDurationSec = "match_duration_sec";
        public const string BurnGraceSec = "burn_grace_sec";
        public const string PlayerMoveSpeed = "player_move_speed";
        public const string CarryCapacity = "carry_capacity";
        public const string PlateCapacity = "plate_capacity";
        public const string InteractRange = "interact_range";
        public const string RecipesEasy2v2 = "recipes_easy_2v2";
        public const string RecipesMedium2v2 = "recipes_medium_2v2";
        public const string RecipesHard2v2 = "recipes_hard_2v2";

        public static class Defaults
        {
            public const float MatchDurationSec = 300f;
            public const float BurnGraceSec = 5f;
            public const float PlayerMoveSpeed = 5f;
            public const int CarryCapacity = 2;
            public const int PlateCapacity = 4;
            public const float InteractRange = 2f;
            public const int RecipesEasy2v2 = 4;
            public const int RecipesMedium2v2 = 4;
            public const int RecipesHard2v2 = 4;
        }
    }
}
