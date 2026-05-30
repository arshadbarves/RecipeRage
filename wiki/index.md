# Kitchen Clash Wiki Index

Content-oriented catalog of all wiki pages. Updated on every ingest/query.

## Pages

| Page | Category | Summary | Last Updated |
|------|----------|---------|--------------|
| [Gameplay](Gameplay.md) | Core | Scoring system, controls, match flow, daily streak | 2026-05-30 |
| [Characters](Characters.md) | Core | Chef roster (6 chefs), ability system, unlock conditions | 2026-05-30 |
| [Maps](Maps.md) | Core | 8 maps, rotation config, mechanics per map | 2026-05-30 |
| [UI-UX](UI-UX.md) | Presentation | Screens, RouterService, USS transitions, navigation flow | 2026-05-30 |
| [Technical](Technical.md) | Architecture | Clean architecture, VContainer DI, networking, Firebase | 2026-05-30 |
| [Monetization](Monetization.md) | Business | IAP items, ads, battle pass, daily streak rewards | 2026-05-30 |
| [Analytics](Analytics.md) | Telemetry | Firebase events, tracking parameters, metrics | 2026-05-30 |
| [Audio](Audio.md) | Audio | Audio architecture, services, pooling | 2026-05-30 |
| [Art-Direction](Art-Direction.md) | Visual | Visual style, 3D models, animations | 2026-05-30 |

## Key Entities

| Entity | Page | Description |
|--------|------|-------------|
| ScoreService | Technical.md, Gameplay.md | Pure C# scoring with RC keys |
| RouterService | UI-UX.md | Push/pop screen navigation |
| IAbility | Characters.md | Open/Closed ability system |
| IConfigService | Technical.md | Firebase Remote Config wrapper |
| IAuthService | Technical.md | EOS Connect auth (no Firebase Auth) |
| VContainer Scopes | Technical.md | Root → Menu → Match hierarchy |

## Key Concepts

| Concept | Pages | Description |
|---------|-------|-------------|
| Clean Architecture | Technical.md | Domain → Application → Presentation → Infrastructure |
| Open/Closed Principle | Characters.md | New chef = new IAbility class, zero edits elsewhere |
| All Values Externalized | Technical.md, Gameplay.md | Every tunable = IConfigService.Get(key, fallback) |
| USS Transitions | UI-UX.md | CSS-based animations, no tweens |
| EOS P2P Networking | Technical.md | Unity NGO over EOS transport |

## Source Documents

| Document | Location | Content |
|----------|----------|---------|
| KitchenClash_GDD_v3_aspirational.docx | Documentation/ | Full GDD with 18 sections, 34 tables |
