# Gameplay

> **⚠️ Status note (2026-06-02):** The scoring formula and match flow on this page describe the
> v1 design. The active design is **Kitchen Brawler v2** — see [GameplayDesign.md](GameplayDesign.md).
> Sections below are kept for ScoreService implementation reference and daily-streak/controls
> sections that are unchanged. Score events, combo streak, fire penalty, and rush-phase
> mechanics are **superseded by v2** (flat score + KO-loot economy + flat intensity).

## Core Loop

Competitive multiplayer kitchen brawler. Teams of 2v2 or 3v3. Combat-first; cooking is the
scoring verb via autonomous stations. See [GameplayDesign.md § Core Loop](GameplayDesign.md#core-loop-15-second-cycle).

## Scoring System (v2 — active)

Flat per-dish base score. **No combo chain. No tier multiplier. No rush-phase multiplier.**
Score variance comes from the KO-loot economy (you can lose dishes you've already produced)
not from formulaic stacking.

### Score Events (v2)

| Event | Formula | RC Key | Default |
|-------|---------|--------|---------|
| Dish delivered (Tier 1) | flat | `score_dish_t1` | 10 |
| Dish delivered (Tier 2) | flat | `score_dish_t2` | 15 |
| Dish delivered (Tier 3) | flat | `score_dish_t3` | 25 |
| KO of enemy | n/a — not scored | n/a | 0 |
| Burned dish (READY → BURNT) | dish lost | n/a | 0 (loss is the penalty) |
| Stolen dish (Disruptor) | scored on delivery by stealing team | n/a | inherits dish tier value |

> Mode-specific score targets: `rush_service_target_score` (100), `hell_kitchen_target_score`
> (150), `last_plate_round_dish_target` (3 dishes per round). See
> [GameplayDesign.md § Game Modes](GameplayDesign.md#game-modes-triplet-a--brawl-leaning).

## Scoring System (v1 — superseded, kept for reference)

The earlier system used a base + speed + rhythm + combo + tier-multiplier formula. Removed
in v2 because:

- Combo chains rewarded uninterrupted production, contradicting the combat-first pillar
- Tier multipliers compounded with combo to make blowouts unsalvageable
- Speed and rhythm bonuses had no place in autonomous-station cooking (player isn't there)

The v1 score events table and ScoreService snippet that previously appeared here have been
removed from the source of truth. They survive in git history if needed.

## Controls

Unchanged from v1. See [LLM-Rules.md § Controls](LLM-Rules.md#controls-brawl-stars-fixed-dual-joystick).

| Input | Action |
|-------|--------|
| Left stick | Move chef (8-dir) |
| Right stick | Aim direction. Release = interact with nearest aimed station |
| Right stick rapid-tap (multi) | **Prime a station** (`station_prime_taps` taps, default 6) |
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

## Match Flow (v2)

1. Match starts (length per mode — see GameplayDesign.md)
2. All stations begin IDLE
3. Players prime, fight, return for dishes, deliver
4. KOs drop 100 % carried items as floor pickups (8s window)
5. **No rush phase, no fire spawns, no scripted disruptions** — flat intensity
6. Match ends per mode win condition (target score / round count / time cap)
7. Results screen shows team score, no individual MVP (Robinson 2021 retrospective-MVP guidance)

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
