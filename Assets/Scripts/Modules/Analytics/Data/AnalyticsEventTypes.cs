namespace RecipeRage.Modules.Analytics.Data
{
    /// <summary>
    /// Standard event types for analytics tracking.
    /// These align with common Firebase Analytics events where applicable.
    /// 
    /// Complexity Rating: 1
    /// </summary>
    public static class AnalyticsEventTypes
    {
        // App lifecycle events
        public const string APP_OPEN = "app_open";
        public const string APP_UPDATE = "app_update";
        public const string APP_CLOSE = "app_close";
        public const string FIRST_OPEN = "first_open";
        
        // Session events
        public const string SESSION_START = "session_start";
        public const string SESSION_END = "session_end";
        
        // Game events
        public const string GAME_START = "game_start";
        public const string GAME_END = "game_end";
        public const string LEVEL_START = "level_start";
        public const string LEVEL_COMPLETE = "level_complete";
        public const string LEVEL_FAIL = "level_fail";
        public const string LEVEL_UP = "level_up";
        public const string SCORE_POSTED = "score_posted";
        
        // Tutorial events
        public const string TUTORIAL_BEGIN = "tutorial_begin";
        public const string TUTORIAL_COMPLETE = "tutorial_complete";
        public const string TUTORIAL_STEP = "tutorial_step";
        
        // User events
        public const string LOGIN = "login";
        public const string SIGN_UP = "sign_up";
        public const string PROFILE_UPDATE = "profile_update";
        
        // Social events
        public const string SHARE = "share";
        public const string INVITE = "invite";
        public const string FRIEND_ADDED = "friend_added";
        public const string MESSAGE_SENT = "message_sent";
        
        // Content events
        public const string CONTENT_VIEW = "content_view";
        public const string CONTENT_SELECT = "content_select";
        public const string SEARCH = "search";
        
        // Recipe-specific events
        public const string RECIPE_CREATED = "recipe_created";
        public const string RECIPE_VIEWED = "recipe_viewed";
        public const string RECIPE_SHARED = "recipe_shared";
        public const string INGREDIENT_USED = "ingredient_used";
        
        // Match events
        public const string MATCH_START = "match_start";
        public const string MATCH_END = "match_end";
        public const string MATCH_JOINED = "match_joined";
        
        // Economy events
        public const string PURCHASE = "purchase";
        public const string PURCHASE_FAILED = "purchase_failed";
        public const string VIRTUAL_CURRENCY_EARNED = "virtual_currency_earned";
        public const string VIRTUAL_CURRENCY_SPENT = "virtual_currency_spent";
        public const string ITEM_ACQUIRED = "item_acquired";
        public const string ITEM_EQUIPPED = "item_equipped";
        
        // Progression events
        public const string ACHIEVEMENT_UNLOCKED = "achievement_unlocked";
        public const string QUEST_STARTED = "quest_started";
        public const string QUEST_COMPLETED = "quest_completed";
        public const string DAILY_REWARD = "daily_reward";
        
        // Engagement events
        public const string AD_VIEW = "ad_view";
        public const string AD_CLICK = "ad_click";
        public const string AD_REWARD_EARNED = "ad_reward_earned";
        public const string RATE_GAME = "rate_game";
        public const string FEEDBACK = "feedback";
        
        // Performance events
        public const string LOADING_TIME = "loading_time";
        public const string FPS_DROP = "fps_drop";
        public const string NETWORK_ERROR = "network_error";
        public const string APP_EXCEPTION = "app_exception";
        
        // Settings events
        public const string SETTINGS_CHANGED = "settings_changed";
        public const string NOTIFICATION_ENABLED = "notification_enabled";
        public const string NOTIFICATION_DISABLED = "notification_disabled";
        
        // Custom conversion events
        public const string CONVERSION_START = "conversion_start";
        public const string CONVERSION_COMPLETE = "conversion_complete";
    }
} 