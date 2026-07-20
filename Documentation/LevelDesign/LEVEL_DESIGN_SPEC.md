# RecipeRage — Production Level Design Specification

**Version:** 1.0  
**Date:** 2026-06-02  
**Author:** Level Design  
**Engine:** Unity 6.0 (6000.3.0f1)  
**Format:** 2v2 / 3v3 / 4v4 (planned)  

---

## Design Philosophy

RecipeRage is a top-down competitive kitchen game played on mobile.
The camera is fixed overhead (like Brawl Stars) — **every player and every station
must be visible on a single screen at all times.**

### Core visual constraints

| Constraint | Rule |
|---|---|
| Screen visibility | All players + all stations must fit on one screen with no scroll |
| Grid size | **10 × 8 tiles** (base footprint) — team zones each get **4 × 8**, shared aisle is **2 × 8** |
| Tile size | 1 Unity unit = 1 tile. Sprites are designed at 64 px / tile |
| Team zones | Teams always have mirrored station sets so neither side is advantaged |
| Camera | Orthographic, sized to show the 10 × 8 grid with ≈ 1 tile margin on each edge |

### Scoring design principles

| Quality | Multiplier | Condition |
|---|---|---|
| Perfect | ×2 | All required ingredients, no extras |
| Good | ×1 | All required + 1 extra |
| Acceptable | ×0.5 | All required + 2+ extras |
| Time bonus | +2 pts / sec remaining | Applied after quality multiplier |

**Tier points (base before multiplier):**  
Tier 1 = 50 pts · Tier 2 = 100 pts · Tier 3 = 150 pts

### Station types (StationType enum)

| Type | Role in kitchen |
|---|---|
| `Ingredient` | Crate that provides raw ingredients — one per ingredient type needed |
| `Prep` | Cutting board — produces cut ingredient |
| `Cooking` | Stove / grill / wok — cooks ingredient or assembles hot dish |
| `Serving` | Plate station — assembles final dish and submits order |
| `Sink` | Cleans plates and extinguishes fires |

### Layout notation

Grid origin `(0,0)` is bottom-left of the playfield.  
Team A occupies columns 0-3; shared aisle is columns 4-5; Team B occupies columns 6-9.  
Rows run 0 (bottom) → 7 (top).  
Station notation: `(col, row) [Type] "label"`.

### Recipe bank (RecipeCatalog — authoritative IDs)

All recipe IDs below are the **canonical IDs** from `RecipeCatalog.BuildDefaultRecipes()`.
Maps may only reference IDs that exist in that catalog.
New IDs required for themed maps must be added to `RecipeCatalog` AND `IngredientType` before use.

**Existing IDs:**
- `basic_salad` T1 50pts · `toast` T1 50pts · `fried_egg` T1 50pts
- `sushi_roll` T2 100pts · `burger` T2 100pts · `pasta_dish` T2 100pts
- `pizza` T3 150pts · `ramen` T3 150pts · `wedding_cake` T3 150pts

**New IDs defined in this document** (must be added to `RecipeCatalog`):
- `taco` T2 100pts · `burrito` T3 150pts · `nachos` T1 50pts
- `miso_soup` T1 50pts · `tempura` T2 100pts · `teriyaki_bowl` T3 150pts
- `grilled_steak` T2 100pts · `bbq_ribs` T3 150pts · `corn_on_cob` T1 50pts
- `lava_meatball` T2 100pts · `volcano_stew` T3 150pts · `ember_bread` T1 50pts
- `fish_stew` T2 100pts · `sea_biscuit` T1 50pts · `kraken_roll` T3 150pts
- `space_wrap` T2 100pts · `protein_cube` T1 50pts · `orbit_ramen` T3 150pts

---

## Map Index

| # | Map ID | Theme | Game Mode | Difficulty | Hazard | Notes |
|---|---|---|---|---|---|---|
| 1 | `rookie_kitchen` | Bistro | 2v2 | Easy | None | Tutorial / onboarding map |
| 2 | `burger_boulevard` | American Diner | 2v2 | Medium | Crosswalk NPC | First hazard map |
| 3 | `sushi_shuffle` | Japanese | 2v2 | Medium | Conveyor belt | Spatial puzzle |
| 4 | `pirate_pot` | Nautical | 2v2 | Hard | Sliding counters | Chaos map |
| 5 | `taco_truck` | Mexican Street | 3v3 | Medium | Dual trucks | Coordination |
| 6 | `clash_kitchen` | Mixed | 3v3 | Hard | Shared stations | Highest conflict |
| 7 | `volcano_kitchen` | Volcanic | 3v3 | Hard | Lava vents | Fire + zone denial |
| 8 | `space_station` | Sci-Fi | 3v3 | Hard | Zero-g throws | Projectile hazard |

---

---

# MAP 1 — Rookie Kitchen

**Scene:** `rookie_kitchen`  
**Game Mode:** 2v2  
**Difficulty:** Easy (0)  
**Kitchen Theme:** `bistro`  
**Fire Chance Multiplier:** 1.0 (normal, but no fire hazards triggered by design)  
**Special Hazard:** None  

## Concept

The training ground. Both teams get identical, clearly mirrored kitchens with no hazards and only Tier-1 recipes. Stations are spaced far apart so new players can read the layout. The shared centre aisle has counters for passing ingredients, teaching cooperation.

This map IS the tutorial. It must be the first map players ever play, and the layout must be self-explanatory through visual affordance alone.

## Target Recipes

| ID | Tier | Points | Ingredients |
|---|---|---|---|
| `basic_salad` | 1 | 50 | Lettuce + Tomato |
| `toast` | 1 | 50 | Bread + Butter |
| `fried_egg` | 1 | 50 | Egg + Butter |

All three are Tier 1. Max order queue = 2 active orders per team.

## Station Layout (10 × 8 grid)

```
Col:  0    1    2    3  |  4    5  |  6    7    8    9
Row 7 [IG] ....  ....  [IG]  [CT] [CT]  [IG] ....  ....  [IG]
Row 6 [PR] ....  ....  [PR]  ....  ....  [PR] ....  ....  [PR]
Row 5 ....  ....  ....  ....  [CT] [CT]  ....  ....  ....  ....
Row 4 ....  ....  ....  ....  ....  ....  ....  ....  ....  ....
Row 3 [CK] ....  ....  [CK]  [CT] [CT]  [CK] ....  ....  [CK]
Row 2 ....  ....  ....  ....  ....  ....  ....  ....  ....  ....
Row 1 [SK] ....  ....  [SV]  [CT] [CT]  [SK] ....  ....  [SV]
Row 0 [IG] ....  ....  [IG]  ....  ....  [IG] ....  ....  [IG]

Key: IG=Ingredient  PR=Prep  CK=Cooking  SV=Serving  SK=Sink  CT=Counter (passthrough)
```

**Team A stations (cols 0-3):**

| Station ID | Type | GridX | GridY | Provides / Purpose |
|---|---|---|---|---|
| `a_ing_lettuce` | Ingredient | 0 | 7 | Lettuce |
| `a_ing_tomato` | Ingredient | 3 | 7 | Tomato |
| `a_ing_bread` | Ingredient | 0 | 0 | Bread |
| `a_ing_egg_butter` | Ingredient | 3 | 0 | Egg / Butter (shared crate) |
| `a_prep_top` | Prep | 0 | 6 | Cutting board — top lane |
| `a_prep_bot` | Prep | 3 | 6 | Cutting board — top-right |
| `a_cook_topleft` | Cooking | 0 | 3 | Stove |
| `a_cook_topright` | Cooking | 3 | 3 | Stove |
| `a_sink` | Sink | 0 | 1 | Extinguish / wash |
| `a_serve` | Serving | 3 | 1 | Submit orders |

**Team B stations (cols 6-9):** mirror of Team A.

**Shared aisle (cols 4-5):** Counter stations at rows 1, 3, 5, 7 — passthrough only (no processing).

## Hazard Specification

None. `FireChanceMultiplier = 0`. No special hazard. Designed to be a zero-pressure introduction.

## Difficulty Tuning

| Parameter | Value |
|---|---|
| Order timer | 40 s (base 30 + 10) |
| Active orders | 2 per team |
| Spawn interval | 25 s |
| Bot difficulty (fill) | `BotDifficulty.Easy` |
| Match duration | 3 min |

## Unlock Condition

Always available. Plays first in the onboarding flow via `Tutorial.unity`.

---

---

# MAP 2 — Burger Boulevard

**Scene:** `burger_boulevard`  
**Game Mode:** 2v2  
**Difficulty:** Medium (1)  
**Kitchen Theme:** `american`  
**Fire Chance Multiplier:** 0.8 (slightly lower — grill fires handled by the crosswalk penalty instead)  
**Special Hazard:** `crosswalk` — NPC customers walk across the aisle every 8–12 s, blocking movement for 1.5 s and potentially knocking a carried item to the floor if the player collides mid-carry.

## Concept

Classic American diner split in two halves. The hazard here is *environmental* rather than station-based: waves of NPC customers cross the shared aisle at intervals, meaning careful timing of passes between teams is required. Simple recipe set (T1 + T2), but the burger requires 4 ingredients, creating the first real coordination challenge.

## Target Recipes

| ID | Tier | Points | Ingredients | Notes |
|---|---|---|---|---|
| `toast` | 1 | 50 | Bread + Butter | Warmup recipe |
| `fried_egg` | 1 | 50 | Egg + Butter | Solo carry |
| `burger` | 2 | 100 | Bread + Beef + Lettuce + Tomato | Coordination required |

Max order queue = 3 active orders per team.

## Station Layout (10 × 8 grid)

```
Col:  0    1    2    3  |  4    5  |  6    7    8    9
Row 7 [IG] ....  [IG] ....  [CT] [CT]  .... [IG]  ....  [IG]
Row 6 ....  ....  ....  ....  ....  ....  ....  ....  ....  ....
Row 5 [PR] ....  [PR] ....  ← CROSSWALK ROW →  .... [PR]  ....  [PR]
Row 4 [CK] ....  [CK] ....  [CT] [CT]  .... [CK]  ....  [CK]
Row 3 ....  ....  ....  ....  ....  ....  ....  ....  ....  ....
Row 2 [PR] ....  ....  [CK]  [CT] [CT]  [CK] ....  ....  [PR]
Row 1 [SK] ....  ....  [SV]  ← CROSSWALK ROW →  [SK] ....  ....  [SV]
Row 0 [IG] ....  ....  [IG]  ....  ....  [IG] ....  ....  [IG]

Crosswalk rows: 5 and 1 (NPCs walk col 4→9 and 9→4 alternately)
```

**Team A stations (cols 0-3):**

| Station ID | Type | GridX | GridY | Provides / Purpose |
|---|---|---|---|---|
| `a_ing_bread` | Ingredient | 0 | 7 | Bread |
| `a_ing_lettuce_tomato` | Ingredient | 2 | 7 | Lettuce / Tomato |
| `a_ing_beef` | Ingredient | 0 | 0 | Beef |
| `a_ing_egg_butter` | Ingredient | 3 | 0 | Egg / Butter |
| `a_prep_top` | Prep | 0 | 5 | Cutting board |
| `a_prep_mid` | Prep | 2 | 5 | Cutting board |
| `a_prep_bot` | Prep | 0 | 2 | Cutting board |
| `a_cook_top_l` | Cooking | 0 | 4 | Grill |
| `a_cook_top_r` | Cooking | 2 | 4 | Grill |
| `a_cook_bot` | Cooking | 3 | 2 | Grill |
| `a_sink` | Sink | 0 | 1 | Wash / extinguish |
| `a_serve` | Serving | 3 | 1 | Submit orders |

**Team B stations:** mirror.

## Hazard Specification

**Crosswalk NPC** (`crosswalk`)

- Two crossing lanes: row 5 and row 1 in the shared aisle (cols 4-5)
- NPCs spawn off-screen at col 4.5 (centre) every 8–12 s
- NPC width: 1 tile, speed: 3 tiles/s across both team zones (full 10-col sweep)
- **Collision effect:** player carrying an ingredient drops it to floor (item becomes `IngredientItem` on the floor, recoverable)
- **Visual warning:** pedestrian crossing light flashes 2 s before each wave
- **Fire interaction:** fire chance multiplier *reduced* to 0.8 because the crosswalk is the primary chaos driver

## Difficulty Tuning

| Parameter | Value |
|---|---|
| Order timer | 50 s (base 30 + 20) |
| Active orders | 3 per team |
| Spawn interval | 20 s |
| Bot difficulty (fill) | `BotDifficulty.Medium` |
| Match duration | 4 min |

## Unlock Condition

Unlocked after playing 2 matches in `rookie_kitchen`.

---

---

# MAP 3 — Sushi Shuffle

**Scene:** `sushi_shuffle`  
**Game Mode:** 2v2  
**Difficulty:** Medium (1)  
**Kitchen Theme:** `japanese`  
**Fire Chance Multiplier:** 1.0  
**Special Hazard:** `conveyor_belt` — a conveyor belt runs along the shared aisle (row 3), carrying placed ingredients from Team A's side to Team B's side and back on a fixed cycle.

## Concept

Japanese restaurant with minimal station counts but a spatial twist: the shared conveyor belt lets players pass ingredients across the aisle *without* entering the opponent's territory, but it takes time (belt speed ~1.5 tiles/s). Getting items off the belt at the right moment is the core coordination puzzle. The ramen recipe requires 4 ingredients and naturally rewards conveyor use.

## Target Recipes

| ID | Tier | Points | Ingredients |
|---|---|---|---|
| `basic_salad` | 1 | 50 | Lettuce + Tomato |
| `sushi_roll` | 2 | 100 | Rice + Fish + Seaweed |
| `ramen` | 3 | 150 | Noodles + Broth + Egg + Vegetables |

Max order queue = 3 active orders per team.

## Station Layout (10 × 8 grid)

```
Col:  0    1    2    3  |  4    5  |  6    7    8    9
Row 7 [IG] ....  ....  [IG]  [CT] [CT]  [IG] ....  ....  [IG]
Row 6 ....  [PR] [PR]  ....  ....  ....  ....  [PR] [PR]  ....
Row 5 [CK] ....  ....  [CK]  [CT] [CT]  [CK] ....  ....  [CK]
Row 4 ....  ....  ....  ....  ....  ....  ....  ....  ....  ....
Row 3 ....  ....  ....  ....  ←——— BELT ———→  ....  ....  ....  ....
Row 4 ....  ....  ....  ....  ....  ....  ....  ....  ....  ....
Row 2 [CK] ....  ....  [CK]  [CT] [CT]  [CK] ....  ....  [CK]
Row 1 [SK] [SV] ....  ....  ....  ....  ....  ....  [SV] [SK]
Row 0 [IG] ....  ....  [IG]  ....  ....  [IG] ....  ....  [IG]

Belt runs across cols 4-5 at row 3; items placed on the belt at col 4 exit at col 5 after 1.3s
```

**Team A stations (cols 0-3):**

| Station ID | Type | GridX | GridY | Provides / Purpose |
|---|---|---|---|---|
| `a_ing_rice_fish` | Ingredient | 0 | 7 | Rice / Fish |
| `a_ing_seaweed_egg` | Ingredient | 3 | 7 | Seaweed / Egg |
| `a_ing_lettuce_veg` | Ingredient | 0 | 0 | Lettuce / Vegetables |
| `a_ing_noodles_broth` | Ingredient | 3 | 0 | Noodles / Broth |
| `a_prep_1` | Prep | 1 | 6 | Cutting board |
| `a_prep_2` | Prep | 2 | 6 | Cutting board |
| `a_cook_top_l` | Cooking | 0 | 5 | Wok |
| `a_cook_top_r` | Cooking | 3 | 5 | Wok |
| `a_cook_bot_l` | Cooking | 0 | 2 | Wok |
| `a_cook_bot_r` | Cooking | 3 | 2 | Wok |
| `a_sink` | Sink | 0 | 1 | Wash |
| `a_serve` | Serving | 1 | 1 | Submit |

**Team B:** mirror.

## Hazard Specification

**Conveyor Belt** (`conveyor_belt`)

- Belt lane: row 3, cols 4-5 (the aisle tiles at row 3 become a moving belt)
- Direction cycles: A→B for 15 s, pause 2 s, B→A for 15 s, repeat
- Belt speed: 1.5 tiles/s
- Players can place a held ingredient on the belt by dropping into a belt tile
- Players can pick up items off the belt while standing adjacent
- If an item reaches the far end of the belt without being collected, it falls to the floor of the opponent's side (still recoverable, but now in enemy territory)
- **Visual:** belt texture scrolls in current direction; arrows invert during reverse phase
- Fire items placed on the belt are extinguished mid-travel (reward for clever play)

## Difficulty Tuning

| Parameter | Value |
|---|---|
| Order timer | 50 s (T2), 60 s (T3) |
| Active orders | 3 per team |
| Spawn interval | 18 s |
| Bot difficulty (fill) | `BotDifficulty.Medium` |
| Match duration | 4 min |

## Unlock Condition

Unlocked after playing 2 matches in `burger_boulevard`.

---

---

# MAP 4 — Pirate Pot

**Scene:** `pirate_pot`  
**Game Mode:** 2v2  
**Difficulty:** Hard (2)  
**Kitchen Theme:** `seafood`  
**Fire Chance Multiplier:** 1.2  
**Special Hazard:** `sliding_counters` — the shared aisle counters shift 1 tile left or right every 12 s, simulating ship roll. Players mid-carry or mid-interaction at a shifting counter are briefly staggered (0.5 s).

## Concept

Pirate ship galley. The ship rolls with the waves, and the middle counters slide. Players must anticipate the roll timing to avoid losing their items mid-transfer. Recipe count is intentionally narrow (only 1 — sushi roll) but the sushi roll requires coordination because the 3 ingredients come from different corners. Fire chance is elevated. The hardest 2v2 map.

## Target Recipes

| ID | Tier | Points | Ingredients |
|---|---|---|---|
| `sea_biscuit` | 1 | 50 | Bread + Butter | Warmup |
| `fish_stew` | 2 | 100 | Fish + Broth + Vegetables | New ID |
| `sushi_roll` | 2 | 100 | Rice + Fish + Seaweed | Coordination |
| `kraken_roll` | 3 | 150 | Rice + Fish + Seaweed + Noodles + Sauce | New ID — 5 ingredients |

> `sea_biscuit`, `fish_stew`, and `kraken_roll` must be added to `RecipeCatalog`.

Max order queue = 2 per team (keep pressure focused, not overwhelming).

## Station Layout (10 × 8 grid)

The galley is narrower by design — ingredient crates are pushed to the outer edges, forcing longer carries.

```
Col:  0    1    2    3  |  4    5  |  6    7    8    9
Row 7 [IG] ....  ....  ....  [CT] [CT]  ....  ....  ....  [IG]
Row 6 ....  [PR] ....  ....  ....  ....  ....  ....  [PR]  ....
Row 5 [CK] ....  ....  [CK]  [SL] [SL]  [CK] ....  ....  [CK]
Row 4 ....  ....  ....  ....  ....  ....  ....  ....  ....  ....
Row 3 [PR] ....  ....  [PR]  [SL] [SL]  [PR] ....  ....  [PR]
Row 2 ....  ....  ....  ....  ....  ....  ....  ....  ....  ....
Row 1 [SK] ....  ....  [SV]  [SL] [SL]  [SK] ....  ....  [SV]
Row 0 [IG] ....  ....  ....  ....  ....  ....  ....  ....  [IG]

SL = Sliding counter (aisle-side, shifts ±1 col on roll)
```

**Team A stations (cols 0-3):**

| Station ID | Type | GridX | GridY | Provides / Purpose |
|---|---|---|---|---|
| `a_ing_rice` | Ingredient | 0 | 7 | Rice |
| `a_ing_fish_seaweed` | Ingredient | 0 | 0 | Fish / Seaweed |
| `a_prep_top` | Prep | 1 | 6 | Cutting board |
| `a_prep_mid` | Prep | 0 | 3 | Cutting board |
| `a_prep_bot` | Prep | 3 | 3 | Cutting board |
| `a_cook_top` | Cooking | 0 | 5 | Pot (cooking) |
| `a_cook_bot` | Cooking | 3 | 5 | Pot |
| `a_sink` | Sink | 0 | 1 | |
| `a_serve` | Serving | 3 | 1 | |

**Team B:** mirror (cols 6-9).

## Hazard Specification

**Sliding Counters** (`sliding_counters`)

- 3 aisle counter tiles (rows 1, 3, 5 at cols 4-5) slide ±1 tile left or right every 12 s
- Each slide event is telegraphed 2 s before by a ship-groan audio cue + amber warning light
- All 3 counters slide simultaneously in the same direction
- Players standing on a counter tile during slide: stagger 0.5 s, item not dropped
- Players mid-animation placing/picking on a counter during slide: item placement cancelled, item stays in hand
- Slide directions alternate: right → left → right...
- Fire chance 1.2×: pots can overflow if left unattended, triggering fire at the Cooking station

## Difficulty Tuning

| Parameter | Value |
|---|---|
| Order timer | 45 s (T2), 55 s (T3) |
| Active orders | 2 per team |
| Spawn interval | 22 s |
| Bot difficulty (fill) | `BotDifficulty.Hard` |
| Match duration | 4 min |

## Unlock Condition

Unlocked after Rank ≥ Bronze II or completing 5 matches in `sushi_shuffle`.

---

---

# MAP 5 — Taco Truck

**Scene:** `taco_truck`  
**Game Mode:** 3v3  
**Difficulty:** Medium (1)  
**Kitchen Theme:** `mexican`  
**Fire Chance Multiplier:** 1.0  
**Special Hazard:** `dual_trucks` — two separate food trucks share a narrow 2-tile service window in the centre. One truck handles Tier-1/2 orders, the other handles Tier-3. Serving the wrong truck scores 0 pts.

## Concept

First 3v3 map. Two food trucks are parked side-by-side — Team A works the left truck, Team B works the right, but both trucks share the centre serving window. A third player per team gains real value here: one runs ingredients, one preps, one serves at the window. Recipes are Mexican-themed (new IDs needed) and the dual-truck hazard tests whether teams split responsibilities.

The grid is still 10 × 8 but each team has a longer internal path because the kitchen is arranged in a U-shape per truck.

## Target Recipes

| ID | Tier | Points | Ingredients | Notes |
|---|---|---|---|---|
| `nachos` | 1 | 50 | Bread + Cheese + Sauce | New ID |
| `taco` | 2 | 100 | Bread + Beef + Lettuce + Sauce | New ID |
| `burrito` | 3 | 150 | Bread + Beef + Rice + Cheese + Sauce | New ID |

> `nachos`, `taco`, `burrito` must be added to `RecipeCatalog`.  
> `nachos` uses IngredientType.Bread (tortilla chip stand-in), .Cheese, .Sauce  
> `taco` uses .Bread, .Beef, .Lettuce, .Sauce  
> `burrito` uses .Bread, .Beef, .Rice, .Cheese, .Sauce

Max order queue = 4 per team (3v3 volume).

## Station Layout (10 × 8 grid)

Each truck is a U-shape inside its side. Centre is shared serving window.

```
Col:  0    1    2    3  |  4    5  |  6    7    8    9
Row 7 [IG] ....  [IG] ....  [SW] [SW]  .... [IG]  ....  [IG]
Row 6 ....  [PR] ....  ....  ....  ....  ....  ....  [PR]  ....
Row 5 [CK] ....  [CK] ....  [SW] [SW]  .... [CK]  ....  [CK]
Row 4 ....  [PR] ....  ....  ....  ....  ....  ....  [PR]  ....
Row 3 [CK] ....  [CK] ....  [CT] [CT]  .... [CK]  ....  [CK]
Row 2 ....  ....  ....  ....  ....  ....  ....  ....  ....  ....
Row 1 [SK] ....  [SV·T12] [SV·T3]  .... ....  [SV·T12] [SV·T3] .... [SK]
Row 0 [IG] ....  [IG] ....  ....  ....  .... [IG]  ....  [IG]

SW = Serving Window (shared between trucks — must identify which truck handles which tier)
SV·T12 = Tier 1/2 serve station  SV·T3 = Tier 3 serve station
```

**Team A stations (cols 0-3):**

| Station ID | Type | GridX | GridY | Provides / Purpose |
|---|---|---|---|---|
| `a_ing_bread` | Ingredient | 0 | 7 | Bread / Tortilla |
| `a_ing_beef_rice` | Ingredient | 2 | 7 | Beef / Rice |
| `a_ing_lettuce` | Ingredient | 0 | 0 | Lettuce |
| `a_ing_cheese_sauce` | Ingredient | 2 | 0 | Cheese / Sauce |
| `a_prep_top` | Prep | 1 | 6 | Chopping |
| `a_prep_mid` | Prep | 1 | 4 | Chopping |
| `a_cook_top_l` | Cooking | 0 | 5 | Flat-top grill |
| `a_cook_top_r` | Cooking | 2 | 5 | Flat-top grill |
| `a_cook_mid_l` | Cooking | 0 | 3 | Flat-top grill |
| `a_cook_mid_r` | Cooking | 2 | 3 | Flat-top grill |
| `a_sink` | Sink | 0 | 1 | |
| `a_serve_t12` | Serving | 2 | 1 | T1/T2 window |
| `a_serve_t3` | Serving | 3 | 1 | T3 window |

**Team B:** mirror.

## Hazard Specification

**Dual Trucks** (`dual_trucks`)

- Each team has TWO separate serve stations: one for Tier 1/2 orders and one for Tier-3 orders
- Delivering to the wrong station cancels the dish and returns 0 points (dish is wasted)
- A colour-coded ticket appears on the order ticket UI: yellow = truck A (T1/T2), red = truck B (T3)
- Players can see which truck their order is destined for by looking at the HUD order card
- **3v3 specialisation pressure:** teams who explicitly assign one player to each truck dominate

## Difficulty Tuning

| Parameter | Value |
|---|---|
| Order timer | 50 s (T2), 60 s (T3) |
| Active orders | 4 per team |
| Spawn interval | 15 s |
| Bot difficulty (fill) | `BotDifficulty.Medium` |
| Match duration | 5 min |

## Unlock Condition

Unlocked at Rank ≥ Silver I or by clearing all 2v2 maps at least once.

---

---

# MAP 6 — Clash Kitchen

**Scene:** `clash_kitchen`  
**Game Mode:** 3v3  
**Difficulty:** Hard (2)  
**Kitchen Theme:** `mixed`  
**Fire Chance Multiplier:** 1.5  
**Special Hazard:** `shared_stations` — some stations in the centre aisle are neutral/shared, available to both teams. Teams race to use them first; players can physically block opponents at a shared station.

## Concept

RecipeRage's definitive competitive map. Both teams fight over a single shared kitchen. The layout is **not mirrored** — instead there is one large shared cooking area. Each team has a private ingredient corner, but all prep, cooking, and serving stations are contested. This creates maximum player interaction, strategic station denial, and the highest skill ceiling.

**This is the only map where teams are not separated by an aisle.** Player collisions and station blocking are the primary hazard.

## Target Recipes

| ID | Tier | Points | Notes |
|---|---|---|---|
| `basic_salad` | 1 | 50 | Fast filler |
| `toast` | 1 | 50 | |
| `fried_egg` | 1 | 50 | |
| `sushi_roll` | 2 | 100 | Requires prep |
| `burger` | 2 | 100 | 4 ingredients |
| `pasta_dish` | 2 | 100 | |
| `ramen` | 3 | 150 | |

Full recipe range. Max order queue = 5 per team.

## Station Layout (10 × 8 grid)

No hard team-side boundary. Team A spawns bottom-left, Team B spawns top-right.

```
Col:  0    1    2    3    4    5    6    7    8    9
Row 7 [IG·B] .... [IG·B] .... [PR] [CK] .... [IG·B] .... [IG·B]
Row 6 ....  ....  ....  ....  [PR] [CK] ....  ....  ....  ....
Row 5 [CK] [PR]  ....  [CK]  [CK] [PR] [CK]  ....  [PR]  [CK]
Row 4 ....  ....  ....  ....  ....  ....  ....  ....  ....  ....
Row 3 [CK] [PR]  ....  [CK]  [CK] [PR] [CK]  ....  [PR]  [CK]
Row 2 ....  ....  ....  ....  [PR] [CK] ....  ....  ....  ....
Row 1 [IG·A] .... [IG·A] .... [PR] [CK] .... [IG·A] .... [IG·A]
Row 0 [SV·A] [SK·A] .... .... .... .... .... .... [SK·B] [SV·B]

IG·A = Team A ingredient (locked to A)
IG·B = Team B ingredient (locked to B)
Unlabelled CK/PR = shared, first-come-first-served
```

**Team A private ingredients (rows 0-1, cols 0-3):**
- Lettuce, Tomato, Bread, Butter, Egg (row 1 crates)

**Team B private ingredients (rows 6-7, cols 6-9):**
- Rice, Fish, Seaweed, Pasta, Sauce, Cheese, Noodles, Broth (row 7 crates)

**All cooking + prep stations:** shared (10 stations total, 5 cooking + 5 prep).

**Serving + sink:** Team A has private serve+sink at row 0 cols 0-1. Team B at row 0 cols 8-9.

## Hazard Specification

**Shared Stations** (`shared_stations`)

- Shared stations show a neutral grey indicator when free
- When occupied by Team A, they glow blue; Team B = red
- Opposing team player cannot use a station that is actively in use by an enemy player (blocked)
- Players CAN interrupt an enemy's station use by bumping into them (player-vs-player shove mechanic):
  - Bump from behind: does nothing (must face the station)
  - Bump from the side: 0.8 s stagger on both players, cancels the current interaction
- Fire chance 1.5×: shared cooking stations burn faster and there are no dedicated fire counters mid-kitchen
- **Sink rule:** players must reach their *own team's* sink to extinguish items; cannot use enemy sink

## Difficulty Tuning

| Parameter | Value |
|---|---|
| Order timer | 40 s (T2), 55 s (T3) |
| Active orders | 5 per team |
| Spawn interval | 12 s |
| Bot difficulty (fill) | `BotDifficulty.Hard` |
| Match duration | 5 min |

## Unlock Condition

Unlocked at Rank ≥ Gold I.

---

---

# MAP 7 — Volcano Kitchen

**Scene:** `volcano_kitchen`  
**Game Mode:** 3v3  
**Difficulty:** Hard (2)  
**Kitchen Theme:** `volcanic`  
**Fire Chance Multiplier:** 1.8  
**Special Hazard:** `lava_vent` — periodic lava vent eruptions create 2-tile-wide impassable zones for 3 s. Vents cycle through 4 preset positions. Standing in a vent zone when it erupts deals fire status to the player (station also catches fire).

## Concept

A kitchen built on a volcanic island. The extreme fire multiplier means cooking stations WILL catch fire; sink management is a constant secondary task. The lava vent adds zone-denial — you need to know the vent rotation map (4 positions, 10 s cycle) to route efficiently. A lava-themed recipe set requires the new `IngredientType` additions.

## Target Recipes

| ID | Tier | Points | Ingredients | Notes |
|---|---|---|---|---|
| `ember_bread` | 1 | 50 | Bread + Butter | Baked in lava oven |
| `grilled_steak` | 2 | 100 | Beef + Sauce + Vegetables | New ID |
| `lava_meatball` | 2 | 100 | Beef + Sauce + Cheese | New ID |
| `bbq_ribs` | 3 | 150 | Beef + Sauce + Onion + Vegetables | New ID |
| `volcano_stew` | 3 | 150 | Beef + Broth + Tomato + Onion + Vegetables | New ID |

> `ember_bread`, `grilled_steak`, `lava_meatball`, `bbq_ribs`, `volcano_stew` must be added to `RecipeCatalog`.  
> `grilled_steak` uses .Beef, .Sauce, .Vegetables  
> `lava_meatball` uses .Beef, .Sauce, .Cheese  
> `bbq_ribs` uses .Beef, .Sauce, .Onion, .Vegetables  
> `volcano_stew` uses .Beef, .Broth, .Tomato, .Onion, .Vegetables

Max order queue = 4 per team.

## Station Layout (10 × 8 grid)

The 4 vent positions are fixed: (2,3), (4,6), (7,2), (9,5). Stations are arranged to avoid those tiles while keeping viable routing paths around them.

```
Col:  0    1    2    3  |  4    5  |  6    7    8    9
Row 7 [IG] ....  ....  [IG]  [CT] [CT]  [IG] ....  ....  [IG]
Row 6 ....  [PR] ....  ....  [V4]  ....  ....  ....  [PR]  ....
Row 5 [CK] ....  ....  [CK]  [CT] [CT]  [CK] ....  [V3]  [CK]
Row 4 ....  ....  ....  ....  ....  ....  ....  ....  ....  ....
Row 3 ....  ....  [V1] ....  [CT] [CT]  ....  ....  ....  ....
Row 2 [CK] [PR]  ....  [CK]  ....  ....  [CK] [V2]  ....  [CK]
Row 1 [SK] ....  ....  [SV]  [CT] [CT]  [SK] ....  ....  [SV]
Row 0 [IG] ....  ....  [IG]  ....  ....  [IG] ....  ....  [IG]

V1=(2,3) V2=(7,2) V3=(9,5) V4=(4,6)  — lava vent positions
```

**Team A stations (cols 0-3):**

| Station ID | Type | GridX | GridY | Notes |
|---|---|---|---|---|
| `a_ing_beef_onion` | Ingredient | 0 | 7 | Beef / Onion |
| `a_ing_sauce_broth` | Ingredient | 3 | 7 | Sauce / Broth |
| `a_ing_bread_butter` | Ingredient | 0 | 0 | Bread / Butter |
| `a_ing_tomato_veg` | Ingredient | 3 | 0 | Tomato / Vegetables |
| `a_prep` | Prep | 1 | 2 | Cutting board |
| `a_cook_top_l` | Cooking | 0 | 5 | Lava grill |
| `a_cook_top_r` | Cooking | 3 | 5 | Lava grill |
| `a_cook_bot_l` | Cooking | 0 | 2 | Lava grill |
| `a_cook_bot_r` | Cooking | 3 | 2 | Lava grill |
| `a_sink` | Sink | 0 | 1 | Extinguish |
| `a_serve` | Serving | 3 | 1 | Submit |

**Team B:** mirror.

## Hazard Specification

**Lava Vent** (`lava_vent`)

- 4 vent positions cycle in a fixed order: V1 → V2 → V3 → V4 → V1 ...
- Active vent duration: 3 s on, 7 s off per vent position
- Total cycle: 40 s per full rotation
- **2 s warning:** visual glow + rumble controller haptic before eruption
- **Eruption zone:** 2×2 tiles centred on vent position
- **Player in zone during eruption:** player receives `FireStatus` (slowed to 50% speed, plays fire VFX, recoverable by sink within 10 s)
- **Station in zone:** station catches fire immediately; must be extinguished by sink before use
- **Fire interaction:** base fire chance 1.8× → any cooking station left unattended for more than 15 s has an 18% / 10 s chance to catch fire
- **Cheese ingredient:** melts if left on a cooking station after a vent nearby — adds `Sauce` accidentally (exploitable for recipes that need sauce)

## Difficulty Tuning

| Parameter | Value |
|---|---|
| Order timer | 45 s (T2), 60 s (T3) |
| Active orders | 4 per team |
| Spawn interval | 16 s |
| Bot difficulty (fill) | `BotDifficulty.Hard` |
| Match duration | 5 min |

## Unlock Condition

Unlocked at Rank ≥ Gold III or by winning 3 matches on `clash_kitchen`.

---

---

# MAP 8 — Space Station

**Scene:** `space_station`  
**Game Mode:** 3v3  
**Difficulty:** Hard (2)  
**Kitchen Theme:** `scifi`  
**Fire Chance Multiplier:** 1.3  
**Special Hazard:** `zero_g_throws` — players can throw carried ingredients across the kitchen. Thrown items travel in a straight line and land on the first valid station in their path. This is both a power move and a risk (items can overshoot into an opponent's area).

## Concept

The most mechanically unique map. Zero-gravity throwing turns ingredient passing from a foot-travel problem into an aiming problem. Expert teams will throw across the aisle to teammates with precision; less experienced players will accidentally gift items to the enemy team. The layout has deliberate long lines of sight for throwing. Sci-fi-themed recipes are built from existing ingredient types — no new `IngredientType` needed.

## Target Recipes

| ID | Tier | Points | Ingredients | Notes |
|---|---|---|---|---|
| `protein_cube` | 1 | 50 | Egg + Cheese | Pressed protein block |
| `space_wrap` | 2 | 100 | Bread + Chicken + Vegetables | New ID |
| `orbit_ramen` | 3 | 150 | Noodles + Broth + Egg + Chicken + Vegetables | New ID |

> `protein_cube`, `space_wrap`, `orbit_ramen` must be added to `RecipeCatalog`.  
> `space_wrap` uses IngredientType.Bread, .Chicken, .Vegetables (Chicken must be added to RecipeCatalog usage — it exists in enum as value 4)  
> `orbit_ramen` uses .Noodles, .Broth, .Egg, .Chicken, .Vegetables

Max order queue = 4 per team.

## Station Layout (10 × 8 grid)

Stations are placed with long horizontal corridors to enable throws across the aisle. No obstacles block the throw lanes (cols 4-5 are completely clear on rows 2-6).

```
Col:  0    1    2    3  |  4    5  |  6    7    8    9
Row 7 [IG] ....  ....  [IG]  ....  ....  [IG] ....  ....  [IG]
Row 6 ....  ....  ....  ....  ....  ....  ....  ....  ....  ....
Row 5 [CK] ....  [PR] ....  ← THROW LANE →  .... [PR]  ....  [CK]
Row 4 ....  ....  ....  ....  ....  ....  ....  ....  ....  ....
Row 3 [PR] ....  [CK] ....  ← THROW LANE →  .... [CK]  ....  [PR]
Row 2 ....  ....  ....  ....  ....  ....  ....  ....  ....  ....
Row 1 [SK] ....  [SV] [CK]  ....  ....  [CK] [SV]  ....  [SK]
Row 0 [IG] ....  ....  [IG]  ....  ....  [IG] ....  ....  [IG]

Throw lanes: rows 3 and 5, cols 4-9 (A→B) or 0-5 (B→A)
No stations in cols 4-5, rows 2-6 — all clear for throws
```

**Team A stations (cols 0-3):**

| Station ID | Type | GridX | GridY | Notes |
|---|---|---|---|---|
| `a_ing_egg_cheese` | Ingredient | 0 | 7 | Egg / Cheese |
| `a_ing_bread_chicken` | Ingredient | 3 | 7 | Bread / Chicken |
| `a_ing_noodles_broth` | Ingredient | 0 | 0 | Noodles / Broth |
| `a_ing_veg` | Ingredient | 3 | 0 | Vegetables |
| `a_prep_top` | Prep | 2 | 5 | Cutting board |
| `a_prep_bot` | Prep | 0 | 3 | Cutting board |
| `a_cook_top` | Cooking | 0 | 5 | Space oven |
| `a_cook_mid` | Cooking | 2 | 3 | Space oven |
| `a_cook_bot` | Cooking | 3 | 1 | Space oven |
| `a_sink` | Sink | 0 | 1 | |
| `a_serve` | Serving | 2 | 1 | Submit |

**Team B:** mirror.

## Hazard Specification

**Zero-G Throws** (`zero_g_throws`)

- Hold the throw button for 0.5 s while carrying an ingredient to enter throw mode
- A trajectory arc preview appears (straight line, no gravity, stops at first collidable object)
- Release to throw: item travels at 6 tiles/s
- **Valid landing targets:** any station tile, any counter tile, any floor tile
- **Landing on a station:** item is placed on that station (as if manually placed)
- **Landing on a floor tile:** item lands as a dropped item
- **Landing in opponent's zone:** item is in enemy territory — enemy players can pick it up
- **Collision mid-air:** items thrown from opposite sides cancel each other (both drop at the midpoint)
- **Strategic use:** throw an ingredient over the aisle to a teammate's cooking station to skip the foot-carry
- **Fire interaction:** fire chance 1.3× — space radiation accelerates cooking
- **No-throw zones:** stations in rows 0 and 7 (ingredient crates) cannot be hit by throws — they are shielded (prevents sniping opponent's ingredient supply)

## Difficulty Tuning

| Parameter | Value |
|---|---|
| Order timer | 50 s (T2), 65 s (T3) |
| Active orders | 4 per team |
| Spawn interval | 15 s |
| Bot difficulty (fill) | `BotDifficulty.Hard` — bots use throw AI to pass ingredients |
| Match duration | 5 min |

## Unlock Condition

Unlocked at Rank ≥ Platinum I or by winning 5 matches on `volcano_kitchen`.

---

---

# Implementation Notes for Engineers

## RecipeCatalog additions required

The following recipe IDs are used by maps in this document but do not exist in `RecipeCatalog.BuildDefaultRecipes()`. Add them before populating map assets.

| Recipe ID | Tier | Points | Required IngredientTypes | Map |
|---|---|---|---|---|
| `sea_biscuit` | 1 | 50 | Bread, Butter | pirate_pot |
| `fish_stew` | 2 | 100 | Fish, Broth, Vegetables | pirate_pot |
| `kraken_roll` | 3 | 150 | Rice, Fish, Seaweed, Noodles, Sauce | pirate_pot |
| `nachos` | 1 | 50 | Bread, Cheese, Sauce | taco_truck |
| `taco` | 2 | 100 | Bread, Beef, Lettuce, Sauce | taco_truck |
| `burrito` | 3 | 150 | Bread, Beef, Rice, Cheese, Sauce | taco_truck |
| `ember_bread` | 1 | 50 | Bread, Butter | volcano_kitchen |
| `grilled_steak` | 2 | 100 | Beef, Sauce, Vegetables | volcano_kitchen |
| `lava_meatball` | 2 | 100 | Beef, Sauce, Cheese | volcano_kitchen |
| `bbq_ribs` | 3 | 150 | Beef, Sauce, Onion, Vegetables | volcano_kitchen |
| `volcano_stew` | 3 | 150 | Beef, Broth, Tomato, Onion, Vegetables | volcano_kitchen |
| `protein_cube` | 1 | 50 | Egg, Cheese | space_station |
| `space_wrap` | 2 | 100 | Bread, Chicken, Vegetables | space_station |
| `orbit_ramen` | 3 | 150 | Noodles, Broth, Egg, Chicken, Vegetables | space_station |

Note: `Chicken` (IngredientType.Chicken = 4) already exists in the enum but is not yet used in any recipe. `space_wrap` and `orbit_ramen` are its first users.

## MapDefinitionSO.Stations[] array

Each map's `.asset` file has an empty `Stations` array. Fill it with `StationLayout` entries matching the station tables in this document. The `GridX`/`GridY` values are the tile coordinates from the layout diagrams above.

## Maps.json reconciliation

`Maps.json` (used by the UI map selection screen) contains different map IDs from the `.asset` files. The `.asset` files are authoritative. Update `Maps.json` to reference the 8 map IDs from this document. Suggested structure per category:

| Category | Maps |
|---|---|
| Tutorial | `rookie_kitchen` |
| Quick Play | `burger_boulevard`, `sushi_shuffle` |
| Intermediate | `pirate_pot`, `taco_truck` |
| Ranked | `clash_kitchen`, `volcano_kitchen` |
| Elite | `space_station` |

## New hazard types to implement

These `SpecialHazardType` strings must be handled by the hazard system:

| Type string | Runtime component needed |
|---|---|
| `crosswalk` | NPC spawner + collision handler in `IHazardService` |
| `conveyor_belt` | Belt MonoBehaviour + ingredient transport system |
| `sliding_counters` | Counter shift animation + player stagger trigger |
| `dual_trucks` | Per-tier serve station validator |
| `shared_stations` | Station ownership tracker + player-bump shove mechanic |
| `lava_vent` | Timed zone activator + fire applicator |
| `zero_g_throws` | Input-hold throw mode + projectile physics handler |

These belong in `Assets/_KitchenClash/Infrastructure/Gameplay/Hazards/`.

## Bot AI hints per map

| Map | Bot specialisation note |
|---|---|
| `rookie_kitchen` | Linear pathfinding only — no conflict handling needed |
| `burger_boulevard` | Must wait for crosswalk gap before entering aisle |
| `sushi_shuffle` | Must use belt for cross-team passes; know belt direction |
| `pirate_pot` | Must pre-empt slide by timing counter interactions |
| `taco_truck` | Must read order tier to target correct serve station |
| `clash_kitchen` | Must detect station lock; find alternate routing |
| `volcano_kitchen` | Must know vent rotation schedule; route around active vents |
| `space_station` | Must aim throws; know throw lane clear status |

---

*End of Level Design Specification v1.0*
