# Characters

> **Note (2026-06-02):** With the Kitchen Brawler v2 redesign, every chef now belongs to one
> of four archetypes (Rusher / Cook / Controller / Disruptor). The roster below predates v2
> and uses cooking-centric ability names; archetypes are how v2 systems reason about chefs.
> See [GameplayDesign.md § Roles](GameplayDesign.md#roles-4-archetypes) for archetype intent.

## Archetypes (v2)

Every chef maps to exactly one archetype. Archetype determines best-mode fit and the
shape of the active ability. Passive/Super/Gadget can flavor the chef freely.

| Archetype | Identity | Active Ability Shape | Strong In | Weak In |
|-----------|----------|----------------------|-----------|---------|
| **Rusher** | Speed + chase | Short-range shove, 6s cd | Rush Service, Last Plate | Hell's Kitchen |
| **Cook** | Production specialist | Self-buff: prime input speed-up, 8s cd | Hell's Kitchen | Last Plate |
| **Controller** | Sabotage / denial | Force-burn an enemy `COOKING` station, 10s cd, 2-tile range | All three | None (universal but never dominant) |
| **Disruptor** | Loot specialist | Steal-from-hands, 1-tile range, 12s cd | Hell's Kitchen, Rush Service | Last Plate |

Cross-mode imbalance is **intentional** (research-validated): no archetype is best in every
mode, which forces roster diversity at high trophy.

## Chef Roster

> **Status:** Roster originates from v1. Archetype assignments below are tentative and may
> need re-tuning against v2 abilities. Tracked as Open Question in
> [GameplayDesign.md § Open Questions](GameplayDesign.md#open-questions-post-lock).

| Chef | Archetype (tentative) | Passive | Active (CD) | Super | Gadget | Unlock |
|------|-----------------------|---------|-------------|-------|--------|--------|
| Rosa | Rusher | Quick Hands: -1 prime tap | Sprint Dash (8s) | Kitchen Rush 6s (all instant prime) | Sticky Mat | Free |
| Marco | Disruptor | Long Toss: +2 tile range | Flavor Burst (10s) | Grand Service: completes longest order | Recipe Shortcut | 5 wins |
| Yuki | Controller | Zen Focus: +3s burn-grace on team stations | Calm Step (10s) | Perfect Plating: protects 3 deliveries | Fireproof Gloves | 200 trophies |
| Grandpa | Cook | Secret Recipe: 5% double pts | Stumble Charge (12s) | Family Feast: auto-completes all PRIMED stations | Vintage Spice: T1→T2 | 20 matches |
| Bella | Cook | Conductor: adj teammates +10% prime speed | Prep Relay (10s) | Symphony: team sees all orders+buff | Mise en Place: 3 pre-primed stations | S1 Battle Pass T30 |
| Raj | Cook | Hot Hands: cook 20% faster | Spice Blast (15s) | Curry Overdrive: all stations instant cook | Pressure Cooker: 3x cook speed 15s | 500 trophies S1 |

## Ability System (Open/Closed)

Adding a new chef = new ability classes. Zero edits to existing code.

```csharp
public interface IAbility {
    AbilitySlot   Slot     { get; }
    float         Cooldown { get; }   // reads from IConfigService
    bool          CanActivate(AbilityContext ctx);
    AbilityResult Activate(AbilityContext ctx);
}
```

### Example: Rosa's Sprint Dash

```csharp
public sealed class RosaSprintDash : IAbility {
    private readonly IConfigService _cfg;
    public RosaSprintDash(IConfigService cfg) => _cfg = cfg;
    public AbilitySlot Slot     => AbilitySlot.Active;
    public float       Cooldown => _cfg.Get("ability_rosa_dash_cd", 8f);
    public bool CanActivate(AbilityContext ctx) => true;
    public AbilityResult Activate(AbilityContext ctx) =>
        new(AbilityEffectType.Dash, ctx.AimDir * _cfg.Get("ability_rosa_dash_tiles", 2f));
}
```

### Adding a New Chef

1. Choose an archetype (Rusher / Cook / Controller / Disruptor)
2. Create `[Chef]Passive.cs`, `[Chef]Active.cs`, `[Chef]Super.cs` implementing `IAbility`
3. Active ability must respect archetype shape (range / cooldown class)
4. Register in `ChefAbilityRegistrySO`
5. Add RC keys for all cooldowns + tunable values
6. No changes to `AbilityService` (Open/Closed)

## Character Details

See [GameplayDesign.md](GameplayDesign.md) for archetype detail and per-mode fit.
See [Technical.md](Technical.md) for DI registration pattern.
