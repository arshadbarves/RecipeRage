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

        // GDD v3 — Passives
        QuickHands,
        LongToss,
        ZenFocus,
        SecretRecipe,
        Conductor,
        HotHands,

        // GDD v3 — Actives
        SprintDash,
        FlavorBurst,
        CalmStep,
        StumbleCharge,
        PrepRelay,
        SpiceBlast,

        // GDD v3 — Supers
        KitchenRush,
        GrandService,
        PerfectPlating,
        FamilyFeast,
        Symphony,
        CurryOverdrive,

        // GDD v3 — Gadgets
        StickyMat,
        RecipeShortcut,
        FireproofGloves,
        VintageSpice,
        MiseEnPlace,
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
        CookSpeedBuff,
        FlavorBoost,
        PerfectSlice,
        KitchenWisdom,
        IngredientSwap,
        SpiceRush
    }
}
