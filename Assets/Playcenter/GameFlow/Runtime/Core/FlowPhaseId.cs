namespace Playcenter.GameFlow
{
    /// <summary>
    /// Product-level phases for a Brawl-class multiplayer shell.
    /// Game-specific work happens behind ports; this enum is the public spine.
    /// </summary>
    public enum FlowPhaseId
    {
        None = 0,
        StudioSplash,
        Boot,
        Home,
        Matchmaking,
        MatchIntro,
        Countdown,
        Match,
        Results,

        // Side / gate phases (plugins)
        ForceUpdate,
        Maintenance,
        NoConnection,
        Login,
        Tutorial,
        AccountUpgrade
    }
}
