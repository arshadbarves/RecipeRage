namespace KitchenClash.Domain
{
    public enum SFXType
    {
        // UI
        ButtonClick,
        ButtonHover,
        ScreenTransition,

        // Gameplay
        Chop,
        Sizzle,
        Serve,
        OrderComplete,
        OrderExpired,
        FireStart,
        FireExtinguish,
        PickupItem,
        DropItem,
        PlateItem,

        // Match
        MatchStart,
        RushModeActivated,
        MatchEnd,
        ScorePoint,
        ComboHit,

        // Abilities
        AbilityActivate,
        AbilityReady,
        DashWhoosh,

        // Rewards
        CoinEarned,
        RewardClaim
    }
}
