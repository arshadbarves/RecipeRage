# Kitchen Clash Wiki Index

Content-oriented catalog of all wiki pages. Updated on every ingest/query.

## Pages

| Page | Category | Summary | Last Updated |
|------|----------|---------|--------------|
| [Gameplay](Gameplay.md) | Core | Scoring system (legacy), controls, match flow, daily streak | 2026-05-30 |
| [GameplayDesign](GameplayDesign.md) | **Design** | Kitchen Brawler v2: combat-first, autonomous stations, KO-loot economy, Triplet A modes (Rush Service / Hell's Kitchen / Last Plate Standing) | 2026-06-02 |
| [Characters](Characters.md) | Core | Chef roster (6 chefs), ability system, unlock conditions | 2026-05-30 |
| [Maps](Maps.md) | Core | 8 maps, rotation config, mechanics per map | 2026-05-30 |
| [UI-UX](UI-UX.md) | Presentation | Screens, UIService, USS transitions, navigation flow | 2026-05-30 |
| [Technical](Technical.md) | Architecture | Clean architecture, VContainer DI, networking, Firebase | 2026-05-30 |
| [Monetization](Monetization.md) | Business | IAP items, ads, battle pass, daily streak rewards | 2026-05-30 |
| [Analytics](Analytics.md) | Telemetry | Firebase events, tracking parameters, metrics | 2026-05-30 |
| [Audio](Audio.md) | Audio | Audio architecture, services, pooling | 2026-05-30 |
| [Art-Direction](Art-Direction.md) | Visual | Visual style, 3D models, animations | 2026-05-30 |
| [LLM-Rules](LLM-Rules.md) | **Protocol** | LLM dev agent rule sheet — forbidden patterns, arch laws, checklists | 2026-06-02 |
| [DRIFT-PROTOCOL](DRIFT-PROTOCOL.md) | **Protocol** | Drift warning format, severity levels, update procedure | 2026-06-02 |

## Key Entities

| Entity | Page | Description |
|--------|------|-------------|
| ScoreService | Technical.md, Gameplay.md | Pure C# scoring with RC keys |
| UIService | Technical.md | UIScreenStackManager-based navigation (production) |
| IAbility | Characters.md | Open/Closed ability system |
| IConfigService | Technical.md | Firebase Remote Config wrapper |
| IAuthService | Technical.md | EOS Connect auth (production path) |
| VContainer Scopes | Technical.md | Root → Menu → Match hierarchy |
| Station State Machine | GameplayDesign.md | IDLE → PRIMED → COOKING → READY → BURNT loop; cooking is autonomous |
| Prime Input | GameplayDesign.md | Right-stick rapid multi-tap to start a station; fire-and-forget |
| KO Loot Drop | GameplayDesign.md | KO drops 100% of carried items as floor pickups — sole comeback engine |
| Tug-of-War Bar | GameplayDesign.md | Rush Service score model; 0–100 relative score |
| Station Burn / Sabotage | GameplayDesign.md | Controller-archetype denial of enemy economy |
| Drift Warning | DRIFT-PROTOCOL.md | Format for flagging wiki vs. code divergence |

## Key Concepts

| Concept | Pages | Description |
|---------|-------|-------------|
| Clean Architecture | Technical.md | Domain → Application → Presentation → Infrastructure |
| Open/Closed Principle | Characters.md | New chef = new IAbility class, zero edits elsewhere |
| All Values Externalized | Technical.md, Gameplay.md | Every tunable = IConfigService.Get(key, fallback) |
| USS Transitions | UI-UX.md | CSS-based animations, no tweens |
| EOS P2P Networking | Technical.md | Unity NGO over EOS transport |
| Combat-First Kitchen | GameplayDesign.md | Combat dominates moment-to-moment; cooking is the scoring verb |
| Autonomous Cooking | GameplayDesign.md | Players prime stations and leave; stations cook on a timer |
| Emergent Comeback | GameplayDesign.md | KO-loot economy — no rubber-banding or scripted catch-up |
| Flat Intensity | GameplayDesign.md | No scripted escalation; tension constant from start to finish |
| Drift Warning | DRIFT-PROTOCOL.md | Issue before any change that contradicts documented design |

## Source Documents

| Document | Location | Content | Ingested |
|----------|----------|---------|---------|
| KitchenClash_GDD_v3_aspirational.docx | Documentation/ | Full GDD — 18 sections, 34 tables | 2026-05-30 |
| LEVEL_DESIGN_SPEC.md | Documentation/LevelDesign/ | 8 maps, station layouts, recipe catalog | 2026-05-30 |
| Deep research report | wf_62fe7f03-60b | Competitive mobile design research — 103 agents, 12 verified findings | 2026-06-02 |
