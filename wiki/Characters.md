# Characters

## Chef Roster

| Chef | Passive | Active (CD) | Super | Gadget | Unlock |
|------|---------|-------------|-------|--------|--------|
| Rosa | Quick Hands: -1 chop tap | Sprint Dash (8s) | Kitchen Rush 6s (all instant) | Sticky Mat | Free |
| Marco | Long Toss: +2 tile range | Flavor Burst (10s) | Grand Service: completes longest order | Recipe Shortcut | 5 wins |
| Yuki | Zen Focus: +3s burn delay | Calm Step (10s) | Perfect Plating: 2x speed bonus x3 | Fireproof Gloves | 200 trophies |
| Grandpa | Secret Recipe: 5% double pts | Stumble Charge (12s) | Family Feast: auto-serves all T1 | Vintage Spice: T1->T2 | 20 matches |
| Bella | Conductor: adj teammates +10% speed | Prep Relay (10s) | Symphony: team sees all orders+buff | Mise en Place: 3 pre-prepped items | S1 Battle Pass T30 |
| Raj | Hot Hands: cook 20% faster | Spice Blast (15s) | Curry Overdrive: all stations instant | Pressure Cooker: 3x cook speed 15s | 500 trophies S1 |

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

1. Create `RajHotHands.cs` (passive)
2. Create `RajSpiceBlast.cs` (active)
3. Create `RajCurryOverdrive.cs` (super)
4. Register in `ChefAbilityRegistrySO`
5. No changes to `AbilityService`

## Character Details

See [Gameplay](Gameplay.md) for scoring multipliers per tier.

See [Technical](Technical.md) for DI registration pattern.
