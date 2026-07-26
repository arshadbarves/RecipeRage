namespace KitchenClash.Application.Config
{
    /// <summary>
    /// GDD v3 / live-ops funnel analytics event name constants.
    /// </summary>
    public static class AnalyticsEvents
    {
        // ── Funnel / live-ops ──
        public const string BootGateOffline = "boot_gate_offline";
        public const string LoginSuccess = "login_success";
        public const string MatchStart = "match_start";
        public const string MatchEnd = "match_end";
        public const string MatchComplete = "match_complete"; // backward-compat alias of match end
        public const string WalletCredit = "wallet_credit";
        public const string PurchaseSuccess = "purchase_success";
        public const string PurchaseFail = "purchase_fail";

        // ── Existing product events ──
        public const string DishServed = "dish_served";
        public const string IapCompleted = "iap_completed";
        public const string AdShown = "ad_shown";
        public const string DailyStreakClaimed = "daily_streak_claimed";
        public const string ConnectivityLost = "connectivity_lost";
        public const string AuthCompleted = "auth_completed";

        // ── Parameter keys ──
        public static class Params
        {
            public const string MapId = "map_id";
            public const string Mode = "mode";
            public const string ChefId = "chef_id";
            public const string TrophyCount = "trophy_count";
            public const string Won = "won";
            public const string Score = "score";
            public const string DurationSec = "duration_sec";
            public const string RecipeId = "recipe_id";
            public const string TimeTaken = "time_taken";
            public const string ComboCount = "combo_count";
            public const string ItemId = "item_id";
            public const string UsdValue = "usd_value";
            public const string AdType = "ad_type";
            public const string Placement = "placement";
            public const string DayNumber = "day_number";
            public const string RewardType = "reward_type";
            public const string Context = "context";
            public const string Method = "method";
            public const string WasGuest = "was_guest";
            public const string Reason = "reason";
            public const string Amount = "amount";
            public const string Currency = "currency";
            public const string ProductId = "product_id";
            public const string Success = "success";
            public const string Phase = "phase";
            public const string IsHost = "is_host";
        }
    }
}
