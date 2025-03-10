namespace RecipeRage.Modules.Analytics.Data
{
    /// <summary>
    /// Standard parameter names for analytics events.
    /// These align with common Firebase Analytics parameters where applicable.
    /// 
    /// Complexity Rating: 1
    /// </summary>
    public static class AnalyticsParameters
    {
        // General parameters
        public const string TIMESTAMP = "timestamp";
        public const string SUCCESS = "success";
        public const string ERROR_CODE = "error_code";
        public const string ERROR_MESSAGE = "error_message";
        public const string DURATION = "duration";
        public const string METHOD = "method";
        public const string PLATFORM = "platform";
        public const string SOURCE = "source";
        public const string MEDIUM = "medium";
        public const string CAMPAIGN = "campaign";
        
        // User parameters
        public const string USER_ID = "user_id";
        public const string USER_LEVEL = "user_level";
        public const string USER_XP = "user_xp";
        public const string IS_NEW_USER = "is_new_user";
        public const string ACCOUNT_AGE_DAYS = "account_age_days";
        
        // Game parameters
        public const string LEVEL_NAME = "level_name";
        public const string LEVEL_NUMBER = "level_number";
        public const string LEVEL_DIFFICULTY = "level_difficulty";
        public const string SCORE = "score";
        public const string CHARACTER = "character";
        public const string GAME_MODE = "game_mode";
        public const string ATTEMPTS = "attempts";
        public const string STARS = "stars";
        public const string POSITION = "position";
        
        // Recipe parameters
        public const string RECIPE_ID = "recipe_id";
        public const string RECIPE_NAME = "recipe_name";
        public const string RECIPE_TYPE = "recipe_type";
        public const string INGREDIENTS_COUNT = "ingredients_count";
        public const string COOKING_TIME = "cooking_time";
        public const string DIFFICULTY = "difficulty";
        
        // Match parameters
        public const string MATCH_ID = "match_id";
        public const string MATCH_TYPE = "match_type";
        public const string MATCH_DURATION = "match_duration";
        public const string PLAYERS_COUNT = "players_count";
        public const string IS_RANKED = "is_ranked";
        public const string RESULT = "result";
        
        // Tutorial parameters
        public const string TUTORIAL_ID = "tutorial_id";
        public const string TUTORIAL_NAME = "tutorial_name";
        public const string TUTORIAL_STEP = "tutorial_step";
        public const string TUTORIAL_COMPLETION = "tutorial_completion";
        
        // Economy parameters
        public const string CURRENCY_TYPE = "currency_type";
        public const string CURRENCY_NAME = "currency_name";
        public const string CURRENCY_AMOUNT = "currency_amount";
        public const string ITEM_ID = "item_id";
        public const string ITEM_NAME = "item_name";
        public const string ITEM_TYPE = "item_type";
        public const string ITEM_CATEGORY = "item_category";
        public const string ITEM_RARITY = "item_rarity";
        public const string PRICE = "price";
        public const string PAYMENT_TYPE = "payment_type";
        public const string IS_FIRST_PURCHASE = "is_first_purchase";
        public const string DISCOUNT_AMOUNT = "discount_amount";
        public const string DISCOUNT_PERCENTAGE = "discount_percentage";
        
        // Progression parameters
        public const string ACHIEVEMENT_ID = "achievement_id";
        public const string ACHIEVEMENT_NAME = "achievement_name";
        public const string QUEST_ID = "quest_id";
        public const string QUEST_NAME = "quest_name";
        public const string REWARD_TYPE = "reward_type";
        public const string REWARD_AMOUNT = "reward_amount";
        
        // Content parameters
        public const string CONTENT_ID = "content_id";
        public const string CONTENT_TYPE = "content_type";
        public const string CONTENT_NAME = "content_name";
        public const string SEARCH_TERM = "search_term";
        public const string SEARCH_RESULTS = "search_results";
        
        // Social parameters
        public const string FRIEND_ID = "friend_id";
        public const string SHARE_PLATFORM = "share_platform";
        public const string MESSAGE_TYPE = "message_type";
        public const string INVITATION_TYPE = "invitation_type";
        
        // Engagement parameters
        public const string SCREEN_NAME = "screen_name";
        public const string PREVIOUS_SCREEN = "previous_screen";
        public const string BUTTON_ID = "button_id";
        public const string RATING = "rating";
        public const string FEEDBACK_TYPE = "feedback_type";
        public const string AD_TYPE = "ad_type";
        public const string AD_LOCATION = "ad_location";
        public const string AD_PROVIDER = "ad_provider";
        
        // Performance parameters
        public const string LOADING_TYPE = "loading_type";
        public const string FPS_VALUE = "fps_value";
        public const string MEMORY_USED = "memory_used";
        public const string NETWORK_TYPE = "network_type";
        public const string EXCEPTION_TYPE = "exception_type";
        public const string DEVICE_MODEL = "device_model";
        public const string OS_VERSION = "os_version";
        public const string APP_VERSION = "app_version";
        
        // Settings parameters
        public const string SETTING_NAME = "setting_name";
        public const string SETTING_VALUE = "setting_value";
        public const string PREVIOUS_VALUE = "previous_value";
        
        // A/B testing parameters
        public const string EXPERIMENT_ID = "experiment_id";
        public const string EXPERIMENT_VARIANT = "experiment_variant";
    }
} 