# Gameplay Design — Kitchen Brawler v2

> **Status: REDESIGN LOCKED (2026-06-02, supersedes v1) — IMPLEMENTATION IN PROGRESS**
> This page replaces the v1 "shared-arena cooking" design rejected as "too boring."
> Design intent here is the source of truth for all gameplay implementation.
> Scaffold is present in tree (`PlayerCombatController`, `LootPickup`, `AutonomousCookingStation`,
> `MatchWinConditionCoordinator`, mode assets/maps, archetype abilities). **Not playable/complete.**
> Values below remain design targets pending playtesting. Do not claim v2 is done.

---

## Player Fantasy & Pillars

**Fantasy:** *Kitchen Brawler* — players are food-themed fighters who happen to cook. Combat dominates the moment-to-moment feel; cooking is the scoring verb.

| Pillar | Description |
|--------|-------------|
| Combat-first | Melee brawling is the dominant activity. Cooking is the *objective*, not the *gameplay*. |
| Mechanical mastery | The best players win on timing, positioning, and input precision. |
| Clutch comeback | The highlight-reel moment is stealing victory from defeat — enabled by emergent loot economy, never by rubber-banding. |
| Flat intensity | No scripted escalation. Tension is constant from second 1 to last second. |
| Skill expression visible | Every win or loss can be traced to identifiable player decisions. |

---

## Core Loop (≈15 second cycle)

```
1. PRIME a station    → tap-burst input (e.g. 6 taps), fire-and-forget
2. FIGHT              → melee combat, deny enemy stations, push for control
3. RETURN             → collect finished dish before burn timer expires
4. DELIVER            → carry to mode-specific objective; carrying = vulnerable
5. KO / GET KO'd      → KO drops 100 % of carried items as floor loot
6. RESPAWN (3s)       → re-engage
```

The defining shift from v1: **the player never stands still to cook.** Cooking is autonomous. The player's job is to fight for the *station*, not to stand at the station.

---

## Station State Machine

```
IDLE
  ↓  (player taps prime input — e.g. 6 right-stick rapid taps)
PRIMED         ← can be sabotaged: enemy attack interrupts priming
  ↓  (cook timer starts, ~5s)
COOKING        ← autonomous; no player presence required
  ↓  (timer completes)
READY          ← 3s grace window to collect; visible to all teams
  ↓  (grace expires uncollected)
BURNT          ← dish destroyed, station LOCKED for 5s
  ↓
IDLE
```

### Station Rules

| Rule | Detail |
|------|--------|
| Anyone can prime | But only the priming team's chef can collect a `READY` dish. |
| Sabotage | A `COOKING` station can be hit by an enemy ability to force burn early. |
| Burn lockout | A `BURNT` station is dead for 5s — denial is real. |
| No carry-while-cooking | Player walks away after priming; doesn't carry the in-progress dish. |
| Station clusters | Maps have 4–8 stations in clusters of 2–3. Contesting one cluster ≠ winning the kitchen. |

> **Why this is the central innovation:** It removes the "stand at station, vulnerable" boredom of v1 while keeping the cooking metaphor. Combat happens *near* stations because they're the contested points, not *because the player is rooted to one.*

---

## Combat Model

| Aspect | Spec |
|--------|------|
| Damage model | HP + KO + respawn |
| Respawn | 3 seconds (Brawl Stars baseline) |
| Range | Melee-dominant. 1–2 tile attack reach. Each character has ONE ranged ability on 6–8s cooldown. |
| KO points | KO grants **0 points** to the killing team. |
| KO loot | KO drops **100 %** of carried ingredients/dishes as physical floor pickups. Anyone (including the victim post-respawn) can grab them. |
| Loot persistence | Floor pickups exist for 8s before despawning. |

The KO-loot economy is the **only** comeback engine. Every KO of a carrier is a free conversion opportunity for whoever picks up first.

---

## Controls (Re-confirmed)

Unchanged from `wiki/LLM-Rules.md` controls table. No mechanic added in this redesign requires a new control.

| Input | Action |
|-------|--------|
| Left stick | Move (8-dir) |
| Right stick aim+release | Attack toward nearest aimed enemy / interact with nearest aimed station |
| Right stick rapid multi-tap | **Prime a station** (cooking input) |
| ABILITY button | Active ability (typically a melee shove or short-range threat) |
| SUPER button | Charged super (charged by completing dishes — see below) |
| GADGET button | 1-use per match |

---

## Game Modes (Triplet A — Brawl-Leaning)

Three structurally distinct modes. Different team sizes, maps, win conditions.

---

### Mode 1 — Rush Service (2v2)

| Field | Value |
|-------|-------|
| Format | 2v2 |
| Map shape | Two team-side kitchens + contested middle delivery zone |
| Win condition | Tug-of-war bar reaches 100 (team-relative score) |
| Length | ~3 min hard cap; sudden-death overtime if tied |
| Feel | Brawler-ball flow-state — quick deliveries, constant skirmish in the middle |

**Scoring:**
- Each delivered dish moves the bar +N points toward the delivering team
- Bar starts at 50 (centered)
- Getting hit while carrying a dish: dish dropped (loot)
- No combo chain. Score = sum of delivered dish base values.

**Why it works:** Two-kitchen layout means cooking is *relatively* safe at home. Combat clusters at the contested middle delivery zone. 2v2 makes every player matter.

---

### Mode 2 — Hell's Kitchen (3v3)

| Field | Value |
|-------|-------|
| Format | 3v3 |
| Map shape | Single shared open arena; stations scattered, two team spawns at opposite ends |
| Win condition | First team to score 150 |
| Length | Soft cap ~5 min; if neither team hits target, highest score wins |
| Feel | Maximum chaos, snowball-friendly, role specialization rewarded |

**Scoring:**
- Each delivered dish: +X to delivering team's score
- Stations are everywhere — no "safe zone" for cooking
- 3v3 enables a clean role split: 1 prime/cook, 1 carry/deliver, 1 disrupt enemy

---

### Mode 3 — Last Plate Standing (2v2 BO3)

| Field | Value |
|-------|-------|
| Format | 2v2, best of 3 rounds |
| Map shape | Multi-island control — 4 small kitchen islands separated by open ground |
| Win condition | First team to deliver 3 dishes in a round wins the round; first to 2 rounds wins the match |
| Length | 75 s per round; ~4 min total match |
| **No respawn within a round** | KO'd players sit out until round end |
| Feel | Tactical, life-cost decisions, every fight has weight |

**Round logic:**
- All islands start IDLE
- Players prime, fight for control of islands
- Each island delivered-from grants 1 round point; first to 3 wins the round
- KO'd players spectate until round resets — round resets after either win condition or 75s

This is the "tournament" mode for skilled players. No carry-from-loss thanks to round structure.

---

## Roles (4 Archetypes)

Roster is built from 4 archetypes. Roster size grows over time but archetype set is fixed.

| Archetype | Identity | Active Ability | Best At |
|-----------|----------|----------------|---------|
| **Rusher** | Speed + chase | Short-range shove (1 tile, 6s cd) | Hunting carriers, claiming loot drops first |
| **Cook** | Production specialist | Self-buff: next prime needs 2 fewer taps (8s cd) | Highest dish output, can prime under pressure |
| **Controller** | Map control / sabotage | Sabotage strike — interrupts a `COOKING` enemy station, forces early burn (10s cd, 2-tile range) | Denying enemy economy |
| **Disruptor** | Loot specialist | Steal-from-hands — 1-tile range, 12s cd, takes one carried item from target | Pure carrier hunter |

**Intentional cross-mode imbalance** (research-validated, retained from v1):

| Archetype | Strong in | Weak in | Reason |
|-----------|-----------|---------|--------|
| Rusher | Rush Service, Last Plate | Hell's Kitchen | Fast comp shines in tight maps |
| Cook | Hell's Kitchen | Last Plate | Output advantage compounds in long matches |
| Controller | All three | None | Universal value but never dominant |
| Disruptor | Hell's Kitchen, Rush Service | Last Plate | Fewer carriers to hunt in BO3 short rounds |

Comp diversity emerges from no single archetype being best in all modes.

---

## Ability Slot Structure

Unchanged from existing chef framework. All chefs have:

| Slot | Trigger | Cooldown |
|------|---------|----------|
| Passive | Always active | n/a |
| Active | Button press | 6–12s (per-archetype tuning) |
| Super | Charged by deliveries (3 dishes = 1 Super charge) | Charge-based |
| Gadget | 1-use per match | n/a |

Super abilities are **always team-positive** (e.g., Cook's super: prime 3 nearby stations instantly; Rusher's super: 5s damage immunity + shove cd reset). Never solo-ult-clutch — comebacks come from loot economy, not ult timing.

---

## Comeback Engine (Emergent Only)

**No rubber-banding. No Desperation Aura. No score-gap auto-buffs. No environmental disruptions.**

The only comeback mechanism is the **KO loot economy**:

1. Trailing team focuses combat on enemy carriers
2. KO drops 100 % of carried items as floor pickups (8s window)
3. Trailing team converts pickups into deliveries
4. Snowball reverses if the trailing team is *better at fighting*

This is a pure skill-gated comeback. A team that is genuinely behind in skill stays behind. A team that's behind on score but ahead on combat skill catches up. The clutch-comeback fantasy is enabled by the loot drop, not gifted by code.

---

## Bot Fill Policy

| Trophy band | Bot fill |
|-------------|---------|
| <300 trophies | Bots fill after 30s wait. Trophies gained/lost normally. |
| ≥300 trophies | Strict all-human matchmaking. No bot fill, even at 60s+ wait. |

Skill expression preserved at high trophy. New players never wait too long.

---

## Removed Mechanics (from v1)

For full rationale, see [drift log entry 2026-06-02 redesign-v2](log.md). Items removed:

| Removed | Reason |
|---------|--------|
| Two-Ingredient Carry Limit | Stations are autonomous; carry mechanic doesn't apply. |
| Hand-Off Stations | Same. |
| Proximity Combo (5-tile radius) | No combo chain in v2. |
| Combo Chain ×1.0–×3.0 | Score is flat; loot economy provides the variance. |
| Type A 0.3s body bump | Replaced by full HP+KO combat. |
| Serving Zone 0.8–1.2s commit window | Delivery is now a melee-vulnerable carry, not a hold-to-commit. |
| 10–15s mode-winning countdown | Tug-of-war bar / score-target replaces it. |
| Environmental Disruptions every 60s | Flat intensity policy. |
| Desperation Aura | Comeback is now emergent only. |
| In-Match Heat Challenges | Scripted prospective challenges removed; emergent KO-loot drama replaces them. |
| Score-gap automatic buffs | No rubber-banding. |
| v1 modes (Rush Hour / Contested Counter / Iron Chef) | Replaced by Triplet A. |

---

## Remote Config Keys (v2)

All tunables follow `IConfigService.Get(key, fallback)` per `wiki/LLM-Rules.md`.

### Station / cooking
| Key | Default | Purpose |
|-----|---------|---------|
| `station_prime_taps` | 6 | Right-stick taps to prime a station |
| `station_cook_duration_sec` | 5.0 | Autonomous cook time after prime |
| `station_burn_grace_sec` | 3.0 | READY → BURNT window |
| `station_sabotage_lockout_sec` | 5.0 | BURNT → IDLE recovery |
| `station_sabotage_range_tiles` | 2 | Controller ability reach |

### Combat / KO
| Key | Default | Purpose |
|-----|---------|---------|
| `ko_respawn_sec` | 3.0 | Respawn delay after KO |
| `ko_loot_drop_pct` | 1.0 | Fraction of carried items dropped on KO (1.0 = 100%) |
| `ko_loot_despawn_sec` | 8.0 | How long floor pickups persist |
| `melee_attack_range_tiles` | 1.5 | Default melee range |
| `ranged_ability_cooldown_sec` | 7.0 | Default ranged ability cd |

### Modes
| Key | Default | Purpose |
|-----|---------|---------|
| `rush_service_target_score` | 100 | Tug-of-war bar threshold |
| `rush_service_max_duration_sec` | 180 | Hard cap before sudden-death |
| `hell_kitchen_target_score` | 150 | Race-to-score target |
| `hell_kitchen_max_duration_sec` | 300 | Soft cap |
| `last_plate_round_seconds` | 75 | Per-round duration |
| `last_plate_round_dish_target` | 3 | Dishes-to-win-round count |

### Matchmaking
| Key | Default | Purpose |
|-----|---------|---------|
| `bot_fill_trophy_threshold` | 300 | Trophies under which bots may fill |
| `bot_fill_wait_sec` | 30 | Wait before bot fill kicks in |

### Abilities (per-archetype tuning)
| Key | Default | Purpose |
|-----|---------|---------|
| `rusher_shove_cooldown_sec` | 6.0 | |
| `cook_prime_buff_cooldown_sec` | 8.0 | |
| `cook_prime_buff_tap_reduction` | 2 | |
| `controller_sabotage_cooldown_sec` | 10.0 | |
| `disruptor_steal_cooldown_sec` | 12.0 | |
| `disruptor_steal_range_tiles` | 1 | |

---

## Playtesting Targets (Not Yet Validated)

| Variable | Start Value | Test Range | Decision Criteria |
|----------|-------------|------------|-------------------|
| `station_prime_taps` | 6 | 4, 6, 8 | Fast enough to feel responsive, slow enough to be interruptable |
| `station_cook_duration_sec` | 5.0 | 3.0, 5.0, 7.0 | Long enough to leave & fight, short enough to cycle stations |
| `station_burn_grace_sec` | 3.0 | 2.0, 3.0, 4.0 | Forgiving but real — enemy can race you to deny |
| `ko_loot_despawn_sec` | 8.0 | 5, 8, 12 | Enough time to convert, not enough to camp |
| `melee_attack_range_tiles` | 1.5 | 1.0, 1.5, 2.0 | Aim feel on mobile touch |
| `match_length_minutes` | 3–5 (per mode) | 3, 4, 5 | Final value pending playtest per mode |

---

## Open Questions (Post-Lock)

1. Should `Last Plate Standing` rounds increment a kill-count tiebreaker, or pure dish-delivery only?
2. Does `station_prime_taps` need to scale with chef archetype, or is `cook_prime_buff_tap_reduction` enough?
3. Should sabotage burn award any points to the saboteur (currently no — same KO-grants-zero rule)?
4. How does the `Disruptor` steal feel on mobile (1-tile range = very precise aim)?
5. Should KO-dropped loot have a brief invulnerability window before pickup (to prevent the killer immediately grabbing it themselves)?

---

## Drift Sensitivity

> Changes to anything on this page require a [Drift Warning](DRIFT-PROTOCOL.md) before implementation.

High-sensitivity areas: station state machine, KO-loot economy rules, mode win conditions, archetype identities, match length defaults, bot fill threshold.

---

## Source

This page was locked on 2026-06-02 after a 5-round design interview. The full decision log is preserved in [log.md](log.md) under entry `2026-06-02 redesign-v2`. Prior v1 design (research-backed shared-arena cooking) was rejected as "too boring" by the project owner; the v2 design pivots cooking from primary activity to objective-verb in a melee brawler frame.
