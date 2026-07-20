namespace KitchenClash.Domain.Enums
{
    /// <summary>
    /// v2 autonomous cooking station lifecycle.
    /// Transitions:  IDLE → PRIMED → COOKING → READY → BURNT → IDLE
    ///               (or COOKING → READY → collected → IDLE)
    /// </summary>
    public enum StationPhase
    {
        /// <summary>No active cook. Available for priming by any player.</summary>
        Idle,

        /// <summary>
        /// A player has completed the prime tap-burst input.
        /// Cook timer will start on next server tick.
        /// </summary>
        Primed,

        /// <summary>
        /// Autonomous cook in progress. Player has walked away.
        /// Timer counts down station_cook_duration_sec.
        /// Can be sabotaged by Controller archetype ability.
        /// </summary>
        Cooking,

        /// <summary>
        /// Cook complete. Dish waiting for collection.
        /// station_burn_grace_sec window before transitioning to Burnt.
        /// Only the priming team's player may collect.
        /// </summary>
        Ready,

        /// <summary>
        /// Grace expired uncollected, or sabotaged.
        /// Dish destroyed. Station locked for station_sabotage_lockout_sec, then returns to Idle.
        /// </summary>
        Burnt,
    }
}
