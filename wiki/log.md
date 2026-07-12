# Kitchen Clash Wiki Log

Chronological record of wiki activity. Each entry starts with timestamp for parseability.

## [2026-05-30] ingest | KitchenClash_GDD_v3_aspirational.docx

- Extracted 18 sections, 34 tables, 895 text blocks from docx
- Created 10 wiki pages covering all GDD content
- Pages: README, Gameplay, Characters, Maps, UI-UX, Technical, Monetization, Analytics, Audio, Art-Direction
- All content preserved in markdown format with tables and code blocks

## [2026-05-30] cleanup | Empty folder removal

- Removed 5 empty directories: Tests/EditMode, Application/ViewModels, Application/State/States, Application/UseCases, Infrastructure/Interfaces
- Removed corresponding .meta files

## [2026-05-30] implementation | VContainer DI fixes

- Made UIService scope-aware (resolves screens from active LifetimeScope)
- Added SetCurrentScope to IUIService interface
- Updated SessionManager to track scope lifecycle
- Auto-registered all screens via reflection (no manual registration)
- Refactored RootLifetimeScope into focused domain methods
- Fixed UXML namespace mismatch for SkewedBoxElement
- Fixed EncryptionService missing passphrase parameter

## [2026-05-30] implementation | UIScreenStackManager

- Created UIScreenStackManager implementing IUIScreenStackManager
- Per-category Stack<Type> dictionary for screen navigation
- Registered as singleton in RootLifetimeScope

## [2026-06-02] ingest | KitchenClash_GDD_v3_aspirational.docx (complete re-extraction)

- Re-extracted full docx with table content (prior ingest missed all table data)
- Verified all 18 sections + 34 tables extracted correctly
- Confirmed all content now covered in wiki pages
- Notable additions recovered: full auth flow code, RouterService code, USS transition code,
  InputReceiver code, ScoreService code, VContainer scope tree details, full RC key registry,
  full chef ability roster with gadgets, SKILL.md v3 rules, monetization constraints

## [2026-06-02] new-page | wiki/LLM-Rules.md

- Created from SKILL.md v3 (GDD Section 18) + PROJECT_MEMORY.md
- Covers: stack reference, architecture rules, forbidden patterns, controls, auth flow,
  connectivity handling, feature-addition checklists
- Source authority for LLM agents working in this codebase

## [2026-06-02] new-page | wiki/DRIFT-PROTOCOL.md

- Established drift warning protocol for wiki vs. code divergence
- Defines warning format, severity levels (Critical/Major/Minor/Additive),
  when to issue warnings, how to update wiki after confirmation
- Required reading for all LLM agents before making architecture or design changes

## [2026-06-02] new-page | wiki/GameplayDesign.md

- Created from competitive redesign initiative (deep-research wf_62fe7f03-60b + design sessions)
- Covers: shared arena design, serving zone commitment window + knockback,
  teamwork enforcement mechanics, combo scoring system, three distinct game modes
  (Rush Hour / Contested Counter / Iron Chef), character archetypes, comeback mechanics,
  Heat Challenges replacing Star Player (research-backed), playtesting targets,
  new RC keys for all tunable design values
- Status: REDESIGN IN PROGRESS — not yet implemented

## [2026-06-02] update | wiki/README.md + wiki/index.md

- Added GameplayDesign, LLM-Rules, DRIFT-PROTOCOL to navigation
- Added drift-warning protocol to wiki maintenance rules
- Corrected engine version: Unity 6.0 (6000.3.0f1), not Unity 2022 LTS
- Updated DI scope name: Root → Session → Match (was Root → Menu → Match)

## [2026-06-02] drift-resolved | gameplay redesign v2 (Kitchen Brawler)

- Drift detected in: wiki/GameplayDesign.md (full page) + wiki/Gameplay.md (score events)
                     + wiki/Characters.md (archetype intent) + wiki/index.md (entities)
- Original spec (v1): Cooking-primary shared-arena game with combo chain, knockback shoves,
                      hand-off stations, two-ingredient carry limit, 0.8–1.2s commit window,
                      heat challenges, desperation aura, environmental disruptions
- New decision (v2): Combat-primary Kitchen Brawler. Cooking is autonomous (prime → walk →
                     return → deliver). HP+KO+respawn combat. KO drops 100% of carried items
                     as floor pickups (sole comeback engine). Three modes: Rush Service (2v2
                     tug-of-war), Hell's Kitchen (3v3 race-to-score), Last Plate Standing
                     (2v2 BO3, no respawn within round). Flat intensity — no scripted
                     escalation. Bots fill only at <300 trophies.
- Severity: 🟠 Major (10 documented mechanics replaced or removed)
- Confirmed by: user (option B) after 5-round design interview on 2026-06-02
- Updated pages: GameplayDesign.md (full rewrite), Gameplay.md (score events + match flow),
                 Characters.md (archetypes + chef table), index.md (entities + concepts)
- New RC keys: station_prime_taps, station_cook_duration_sec, station_burn_grace_sec,
               station_sabotage_lockout_sec, ko_respawn_sec, ko_loot_drop_pct,
               ko_loot_despawn_sec, rush_service_target_score, hell_kitchen_target_score,
               last_plate_round_seconds, bot_fill_trophy_threshold (+ archetype tunables)
- Removed RC keys (legacy v1): serve_commit_window_sec, serve_win_countdown_sec,
                                knockback_stagger_sec, shove_knockback_tiles,
                                shove_drop_recover_sec, combo_decay_interval_sec,
                                combo_break_cooldown_sec, proximity_combo_radius,
                                rush_token_target, rush_token_scatter_count,
                                disruption_interval_sec, desperation_aura_pts_gap,
                                iron_chef_ko_hit_count, iron_chef_ingredient_cd,
                                score_combo, score_speed_max, score_rhythm,
                                score_tier2_mult, score_tier3_mult, score_burn_penalty,
                                score_fire_penalty, score_plate_pct
- Pending follow-up: Documentation/Architecture/PROJECT_MEMORY.md gameplay section may
                     still reference v1 concepts; flag for next architecture pass.

## [2026-07-12] drift-resolved | Architecture cleanup truth pass (GameplayDesign)

- Option B: wiki updated to match code reality
- Was: "Implementation has not started"
- Now: scaffold present / implementation in progress / not playable-complete
- Track: docs/superpowers/specs/2026-07-12-architecture-cleanup-design.md
- Keep-v2 anchors listed in docs/superpowers/plans/2026-07-12-architecture-cleanup-inventory.md
