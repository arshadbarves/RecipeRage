# Kitchen Clash Wiki

**Source of Truth** — This wiki is the authoritative reference for all design decisions. When implementation conflicts with this wiki, the wiki wins unless explicitly updated.

**Stack:** Unity 2022 LTS | NGO + EOS P2P | UI Toolkit MVVM | VContainer | Firebase

## Wiki Navigation

### By Category

| Category | Description | Key Pages |
|----------|-------------|-----------|
| [Gameplay](Gameplay.md) | Core mechanics, scoring, controls | Scoring, Daily Streak |
| [Characters](Characters.md) | Chef roster, abilities | 6 chefs, IAbility system |
| [Maps](Maps.md) | Level design, rotation | 8 maps, RC config |
| [UI-UX](UI-UX.md) | Screens, navigation | RouterService, USS |
| [Technical](Technical.md) | Architecture, DI, networking | VContainer, Firebase, NGO |
| [Monetization](Monetization.md) | IAP, ads, battle pass | Revenue model |
| [Analytics](Analytics.md) | Firebase events, tracking | Event registry |
| [Audio](Audio.md) | Sound design, music | Audio architecture |
| [Art-Direction](Art-Direction.md) | Visual style, 3D models | Art pipeline |

### Quick Reference

| Attribute | Value |
|-----------|-------|
| Genre | Competitive Multiplayer Kitchen Battle |
| Platform | iOS + Android — Unity 2022.3 LTS |
| Match Size | 2v2 and 3v3 |
| Match Length | 3 min quick / 5 min ranked |
| Controls | Brawl Stars fixed dual-joystick |
| Networking | Unity NGO over EOS P2P transport |
| Auth | Google / Facebook / Apple / Guest — EOS Connect direct |
| UI | UI Toolkit + MVVM + RouterService |
| DI | VContainer (Root → Menu → Match scopes) |
| Config | Firebase Remote Config (40+ keys) |
| Analytics | Firebase Analytics + Crashlytics |
| Winning Score | ~80-150pts |

### Key Design Principles

1. **All Values Externalized** — Every tunable = `IConfigService.Get(key, fallback)`. No hardcoded numbers.
2. **Open/Closed** — New chef = new `IAbility` class. Zero edits to existing code.
3. **Clean Architecture** — Domain → Application → Presentation → Infrastructure. MonoBehaviour only in 4 places.
4. **No Firebase Auth** — Google/FB/Apple tokens link directly to EOS Connect.
5. **EOS P2P Only** — No Unity Relay. Free relay via EOS transport.

## Wiki Protocol

This wiki follows the [Karpathy LLM Wiki pattern](https://gist.github.com/karpathy/442a6bf555914893e9891c11519de94f):

- **Raw sources:** GDD docx, code, documentation
- **The wiki:** This directory (LLM-maintained)
- **The schema:** [wiki-schema skill](../.config/opencode/skills/wiki-schema/skill.md)

### Files

| File | Purpose |
|------|---------|
| [index.md](index.md) | Content-oriented catalog of all pages |
| [log.md](log.md) | Chronological activity log |
| README.md | This file — overview and navigation |

### Maintaining the Wiki

- **Ingest:** Add new sources → update relevant pages → update index.md → append to log.md
- **Query:** Search wiki first → synthesize from wiki → cite pages
- **Lint:** Check contradictions, stale claims, orphans, missing cross-refs
