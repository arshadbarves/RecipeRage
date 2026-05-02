namespace KitchenClash.Domain
{
    public enum AbilitySlot
    {
        Passive,
        Active,
        Super,
        Gadget
    }

    /// <summary>
    /// High-level ability type categories.
    /// </summary>
    public enum AbilityType
    {
        None,
        SpeedBoost,
        Dash,
        FreezeTime,
        DoubleIngredients,
        InstantCook,
        InstantChop,
        TeleportToStation,
        PushOtherPlayers,
        StealIngredient,
        PreventBurning,
        AutoPlate,
        IngredientMagnet,
        AutoServeT1,
        CompleteOrder,
        TeamSpeedBuff,
        AllStationsInstant,
        DoubleSpeedBonus,
        PrePrepItems,
        CookSpeedBuff,
        FlavorBoost,
        PerfectSlice,
        KitchenWisdom,
        IngredientSwap,
        SpiceRush,
        SlightSpeedBoost,
        SlowerBurnRate,
        FireResistance,
        StealImmunity,
        CarryTwo,

        // ── GDD v3 Sec 10.1: Per-chef abilities ──
        // Rosa
        QuickHands,
        SprintDash,
        KitchenRush,
        StickyMat,
        // Marco
        LongToss,
        FlavorBurst,
        GrandService,
        RecipeShortcut,
        // Yuki
        ZenFocus,
        CalmStep,
        PerfectPlating,
        FireproofGloves,
        // Grandpa
        SecretRecipe,
        StumbleCharge,
        FamilyFeast,
        VintageSpice,
        // Bella
        Conductor,
        PrepRelay,
        Symphony,
        MiseEnPlace,
        // Raj
        HotHands,
        SpiceBlast,
        CurryOverdrive,
        PressureCooker
    }

    public enum AbilityEffectType
    {
        None,
        SpeedBoost,
        Dash,
        FreezeTime,
        DoubleIngredients,
        InstantCook,
        InstantChop,
        TeleportToStation,
        PushOtherPlayers,
        StealIngredient,
        PreventBurning,
        AutoPlate,
        IngredientMagnet,
        AutoServeT1,
        CompleteOrder,
        TeamSpeedBuff,
        AllStationsInstant,
        DoubleSpeedBonus,
        PrePrepItems,
        CookSpeedBuff
    }
}
