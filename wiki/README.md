# Kitchen Clash Wiki

Game Design Document v3.0 — Organized Reference

**Stack:** Unity 2022 LTS | NGO + EOS P2P | UI Toolkit MVVM | VContainer | Firebase

## Categories

| Category | Description |
|----------|-------------|
| [Gameplay](Gameplay.md) | Core mechanics, scoring, controls, abilities |
| [Characters](Characters.md) | Chef roster, abilities, unlock conditions |
| [Maps](Maps.md) | Level design, map rotation, mechanics |
| [UI-UX](UI-UX.md) | Screens, navigation, RouterService, USS transitions |
| [Technical](Technical.md) | Architecture, DI, networking, Firebase |
| [Monetization](Monetization.md) | IAP, ads, battle pass, daily streak |
| [Audio](Audio.md) | Sound design, music, SFX |
| [Art-Direction](Art-Direction.md) | Visual style, 3D models, animations |

## Quick Reference

- **Match Size:** 2v2 and 3v3
- **Match Length:** 3 min quick / 5 min ranked
- **Controls:** Brawl Stars fixed dual-joystick
- **Auth:** Google / Facebook / Apple / Guest — EOS Connect direct
- **Config:** Firebase Remote Config (40+ keys)
- **Winning Score Range:** ~80-150pts
