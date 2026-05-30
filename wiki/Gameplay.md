# Gameplay

## Core Loop

Competitive multiplayer kitchen battle. Teams of 2v2 or 3v3 cook dishes under time pressure.

## Scoring System

Base score reduced from 100 to 10. Max match score ~80-150pts. Every value is an IConfigService key with fallback.

**Winning match score range: ~80-150pts. Tier 3 dishes are the main differentiator.**

### Score Events

| Event | Formula | RC Key | Default |
|-------|---------|--------|---------|
| Dish served (base) | score_base | score_base | 10 |
| Speed bonus | floor(ratio * max) | score_speed_max | 5 (0-5pts) |
| Rhythm bonus (no mis-tap) | flat bonus | score_rhythm | 1 |
| Combo streak (3+ dishes) | per dish | score_combo | 2 |
| Tier 2 multiplier | base * mult | score_tier2_mult | 1.5 (=15pts base) |
| Tier 3 multiplier | base * mult | score_tier3_mult | 2.0 (=20pts base) |
| Burned dish | -penalty | score_burn_penalty | -2 |
| Fire not doused (5s) | -penalty | score_fire_penalty | -5 |
| Clean plates end bonus | total * pct | score_plate_pct | 0.10 (+10%) |

### ScoreService Implementation

```csharp
public sealed class ScoreService : IScoreService {
    private readonly IConfigService _cfg;
    private int _a, _b;
    private readonly Subject<ScoreChangedEvent> _stream = new();

    public ScoreService(IConfigService cfg) => _cfg = cfg;

    public void AddScore(TeamId team, ScoreEvent e) {
        int d = Calc(e);
        if (team == TeamId.A) _a = Math.Max(0, _a + d);
        else                  _b = Math.Max(0, _b + d);
        _stream.OnNext(new ScoreChangedEvent(team, d, _a, _b));
    }

    private int Calc(ScoreEvent e) {
        if (e.Type == ScoreEventType.BurnedServed) return -_cfg.Get("score_burn_penalty", 2);
        if (e.Type == ScoreEventType.FirePenalty)  return -_cfg.Get("score_fire_penalty", 5);
        float mult = e.RecipeTier == 3 ? _cfg.Get("score_tier3_mult", 2.0f)
                   : e.RecipeTier == 2 ? _cfg.Get("score_tier2_mult", 1.5f) : 1.0f;
        int base_  = (int)(_cfg.Get("score_base", 10) * mult);
        int speed  = (int)(e.SpeedRatio * _cfg.Get("score_speed_max", 5));
        int rhythm = e.RhythmBonus ? _cfg.Get("score_rhythm", 1) : 0;
        int combo  = e.ComboCount >= 3 ? _cfg.Get("score_combo", 2) : 0;
        return base_ + speed + rhythm + combo;
    }
}
```

## Controls

Brawl Stars fixed dual-joystick.

| Input | Action |
|-------|--------|
| Left stick | Move chef (8-dir) |
| Right stick | Aim direction. Release = interact with nearest aimed station |
| Right stick rapid-tap (multi) | Chop at chop station (count per ingredient from RC) |
| ABILITY button | Chef active ability |
| SUPER button | Charged ability (by serving 3 dishes) |
| GADGET button | 1-use item per match |

### InputReceiver

```csharp
public interface IDualStickInput {
    Vector2 MoveInput       { get; }
    Vector2 AimInput        { get; }
    bool    AimJustReleased { get; }
    bool    AbilityPressed  { get; }
    bool    SuperPressed    { get; }
    bool    GadgetPressed   { get; }
}
```

## Match Flow

1. Match starts with timer (180s quick, 300s ranked)
2. Orders generate at configurable rate
3. Players cook, chop, plate, serve
4. Rush phase begins at 60s (1.5x order multiplier)
5. Fire hazards spawn periodically
6. Results screen shows score breakdown

## Daily Streak

Rules: Miss 2+ days in a row = reset to Day 1. Miss 1 = forgiven. Reset: 08:00 UTC. Popup on every app open.

RC key: `daily_streak_cycle_days` (default 60). Stored in EOS Player Data Storage 'daily_streak_v1'.

| Days | Reward |
|------|--------|
| 1-4 | 50-100 Coins + Power Points |
| 5 | 3 Gems + 200 Coins |
| 10 | Common Skin Crate + 150 Coins |
| 20 | New Chef Trial (24h tryout) |
| 30 | Battle Pass XP Token |
| 45 | Legendary Skin Crate |
| 60 | HYPERCHARGE Skin Crate (cycle resets) |
