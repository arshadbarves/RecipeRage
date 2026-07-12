# Kitchen Clash Wiki

**Source of Truth** — This wiki is the authoritative reference for all design decisions.
When implementation conflicts with this wiki, a **[Drift Warning](DRIFT-PROTOCOL.md)** must be issued before proceeding.

**Stack:** Unity 6.0 (6000.3.0f1) | NGO + EOS P2P | UI Toolkit MVVM | VContainer | Firebase

## Wiki Navigation

### By Category

| Category | Description | Key Pages |
|----------|-------------|-----------|
| [Gameplay](Gameplay.md) | Base mechanics, scoring (legacy), controls, daily streak | Scoring, RC keys |
| [GameplayDesign](GameplayDesign.md) | **NEW** Competitive redesign — shared arena, modes, combos | ⚠️ Redesign in progress |
| [Characters](Characters.md) | Chef roster, abilities | 6 chefs, IAbility system |
| [Maps](Maps.md) | Level design, rotation | 8 maps, RC config |
| [UI-UX](UI-UX.md) | Screens, navigation | UIService, USS |
| [Technical](Technical.md) | Architecture, DI, networking | VContainer, Firebase, NGO |
| [Monetization](Monetization.md) | IAP, ads, battle pass | Revenue model |
| [Analytics](Analytics.md) | Firebase events, tracking | Event registry |
| [Audio](Audio.md) | Sound design, music | Audio architecture |
| [Art-Direction](Art-Direction.md) | Visual style, 3D models | Art pipeline |
| [LLM-Rules](LLM-Rules.md) | **LLM dev agent rules** — forbidden patterns, architecture laws | SKILL.md v3 |
| [DRIFT-PROTOCOL](DRIFT-PROTOCOL.md) | **Drift warning procedure** — when wiki and code diverge | Protocol |

### Quick Reference

| Attribute | Value |
|-----------|-------|
| Genre | Competitive Multiplayer Kitchen Battle |
| Platform | iOS + Android — Unity 6.0 (6000.3.0f1) |
| Match Size | 2v2 and 3v3 |
| Match Length | 3 min quick / 5 min ranked |
| Controls | Brawl Stars fixed dual-joystick |
| Networking | Unity NGO over EOS P2P transport |
| Auth | Google / Facebook / Apple / Guest — EOS Connect direct |
| UI | UI Toolkit + MVVM + UIService / UIScreenStackManager (production) |
| DI | VContainer (Root → Menu → Match scopes) |
| Config | Firebase Remote Config (40+ keys) |
| Analytics | Firebase Analytics + Crashlytics |

### Key Design Principles

1. **All Values Externalized** — Every tunable = `IConfigService.Get(key, fallback)`. No hardcoded numbers.
2. **Open/Closed** — New chef = new `IAbility` class. Zero edits to existing code.
3. **Clean Architecture** — Domain → Application → Presentation → Infrastructure. MonoBehaviour only in 4 places.
4. **Auth via EOS Connect** — Google/FB/Apple tokens link directly to EOS Connect (production path).
5. **EOS P2P Only** — No Unity Relay. Free relay via EOS transport.
6. **Wiki-First** — Any change that contradicts this wiki requires a [Drift Warning](DRIFT-PROTOCOL.md) first.

## Wiki Protocol

- **Ingest:** Add new sources → update relevant pages → update index.md → append to log.md
- **Query:** Search wiki first → synthesize from wiki → cite pages
- **Drift:** If code or a proposal contradicts the wiki → issue a [Drift Warning](DRIFT-PROTOCOL.md) → wait for user confirmation before proceeding
- **Lint:** Check contradictions, stale claims, orphans, missing cross-refs

### Files

| File | Purpose |
|------|---------|
| [index.md](index.md) | Content-oriented catalog of all pages |
| [log.md](log.md) | Chronological activity log |
| README.md | This file — overview and navigation |
| [DRIFT-PROTOCOL.md](DRIFT-PROTOCOL.md) | Drift warning format and procedure |
| [LLM-Rules.md](LLM-Rules.md) | LLM developer agent rule sheet |
