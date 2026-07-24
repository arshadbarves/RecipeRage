# Device QA — Soft launch

| # | Case | Android | iOS | Desktop | Pass? |
|---|------|---------|-----|---------|-------|
| 1 | Cold install boot < 30s to login | | | | |
| 2 | Guest login | | | | |
| 3 | Kill resume / relaunch | | | | |
| 4 | Rush queue + match start | | | | |
| 5 | Full slice loop (prime/fight/deliver) | | | | |
| 6 | Disconnect mid-match recovery | | | | |
| 7 | Results + single coin grant | | | | |
| 8 | Name persists relaunch | | | | |
| 9 | No IAP hard crash if shop opened | | | | |
| 10 | Memory/crash free 5 sequential matches | | | | |

## Sign-off
- Dev train QA owner: ____ date: ____
- Stage build QA owner: ____ date: ____

## Minimum matrix (soft launch)
Android + one of (Windows standalone / macOS). iOS only if signing assets exist.

## Stage promotion (after Dev QA pass)
Single commit flipping all `Assets/StreamingAssets/EOS/eos_*_config.json` to Stage ids
(see `docs/release/EOS_ENVIRONMENTS.md`), tag `0.1.0-soft-stage`, re-run rows 2–7 on Stage.
Do not promote before Dev rows 1–9 pass.
