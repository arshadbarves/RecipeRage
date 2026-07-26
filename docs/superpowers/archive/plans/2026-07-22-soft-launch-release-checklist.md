# Soft-Launch Release Checklist Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Take RecipeRage from “architecture production-shaped” to a **guest-only soft launch** on Dev/Stage with a playable Rush Service 2v2 loop, verified EOS multiplayer, cloud save proof, and an explicit monetization ship/no-ship decision.

**Architecture:** Release work is ordered by risk: lock soft-launch scope → fix EOS environment alignment → prove guest auth + lobby + match + save → finish Rush Service vertical slice on existing Kitchen Brawler scaffold → close results→economy→analytics → optional vendor monetization → device QA Stage→Live. Shared Playcenter services (Ads/Analytics/IAP/RC) already live in the SDK; this plan does **not** rework them—only wires vendors or documents soft-launch without them.

**Tech Stack:** Unity 6.0, NGO 2.11.1, PlayEveryware EOS plugin, VContainer, Playcenter SDK (`Playcenter.Services` + `Playcenter.Services.Unity` + `Playcenter.EOS`), UI Toolkit, optional Firebase / AppLovin MAX / Unity IAP behind defines.

## Global Constraints

- Soft launch default: **guest Device ID auth only**; social login (Google/FB/Apple) stays stubbed unless a later task explicitly implements it.
- All **development and CI device builds** target EOS **Dev** sandbox until Task 10 promotes Stage/Live.
- EOS product ids (do not invent new ones):
  - ProductId: `1fbb10d0979749e2a3eddf74edfb1745`
  - ClientId: `xyza7891a4duvFnuVg7ZJzUGcaf7vgHF`
  - Dev SandboxId: `p-3e949b6n57y7qcjyg5sccpyatyzser` / DeploymentId: `146b53cc89584a8d9586e9dd1f0caf91`
  - Stage SandboxId: `p-uvb48fad3qb2tza5wyetcx2hyxnrpt` / DeploymentId: `b27af9e630504620a05e5794e16ce190`
  - Live SandboxId: `19df6d3517a34ba480c2a65880c8567c` / DeploymentId: `70f48f125a80447688e18cd17aac17db`
- Do **not** commit real ClientSecret rotation in chat logs; edit JSON in-repo only if already present.
- Do **not** push `main` to `origin` unless the human explicitly asks (local main may be far ahead).
- Do **not** re-implement shared Ads/Analytics/IAP/RC facades; they shipped in `docs/superpowers/plans/2026-07-22-playcenter-shared-services.md`.
- Vendor packages stay behind defines: `FIREBASE_ANALYTICS`, `FIREBASE_REMOTE_CONFIG`, `APPLOVIN_MAX`, `UNITY_IAP`, `EOS_AVAILABLE`.
- Gameplay source of truth: `wiki/GameplayDesign.md` (Kitchen Brawler v2). Soft-launch mode is **Rush Service only** (`rush_service` / `Map_RushService`).
- Match never mints wallet; only session `MatchRewardHandler` credits via `IWalletLedger`.
- Prefer `dotnet build` for compile gates; Unity Test Runner for EditMode/PlayMode. `dotnet test` may be a no-op without NUnit adapter.
- Commit style: `type(scope): description` + `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`.
- Drift: if a task contradicts `wiki/`, issue DRIFT WARNING per `wiki/DRIFT-PROTOCOL.md` before coding.

## File map (create / modify)

| Path | Role |
|------|------|
| `docs/release/SOFT_LAUNCH_SCOPE.md` | Locked soft-launch product scope |
| `Assets/StreamingAssets/EOS/eos_*_config.json` | Per-platform sandbox/deployment |
| `Assets/StreamingAssets/EOS/EpicOnlineServicesConfig.json` | PEW legacy config (null today) |
| `Assets/StreamingAssets/EOS/eos_product_config.json` | Product version alignment |
| `ProjectSettings/EditorBuildSettings.asset` | Add `Map_RushService` |
| `ProjectSettings/ProjectSettings.asset` | bundleVersion / defines |
| `Assets/_KitchenClash/Infrastructure/Persistence/PlayerDataService.cs` | Real progress load/save |
| `Assets/_KitchenClash/Infrastructure/Persistence/SaveService.cs` | Cloud key registration |
| `Assets/_KitchenClash/Infrastructure/Network/*` | Brawler match loop |
| `Assets/Scenes/Map_RushService.unity` | Playable 2v2 map |
| `Assets/Resources/ScriptableObjects/GameModes/RushService.asset` | Mode asset |
| `wiki/Technical.md`, `wiki/GameplayDesign.md`, `wiki/log.md` | Status after gates |
| `docs/release/DEVICE_QA_CHECKLIST.md` | Device QA script |
| `docs/release/SOFT_LAUNCH_GATE.md` | Go/no-go sign-off |

---

### Task 1: Lock soft-launch scope document

**Files:**
- Create: `docs/release/SOFT_LAUNCH_SCOPE.md`
- Modify: `wiki/log.md` (one line pointer)

**Interfaces:**
- Consumes: none
- Produces: written scope all later tasks must honor

- [ ] **Step 1: Create scope doc**

Write exactly:

```markdown
# Soft-Launch Scope (locked)

**Status:** LOCKED  
**Target:** Guest-only closed/soft launch on EOS Dev → Stage  
**Out of scope for v0.1 soft launch:**
- Google / Facebook / Apple login (UI may show disabled or hidden)
- Hell's Kitchen and Last Plate Standing as ship modes
- Anti-cheat, leaderboards, friends-required flows
- Paid UA scale; store listing may be unlisted/internal

**In scope:**
1. Cold boot → guest login → home
2. Queue Rush Service 2v2 (bots fill if needed for solo smoke)
3. Playable match: prime → fight → deliver → tug-of-war win/loss
4. Results → coin reward → optional rewarded ad (if MAX wired; else hidden)
5. Progress/settings survive relaunch via local + EOS Player Data Storage when online
6. Analytics events fire to Firebase **or** debug sink (document which)
7. No crash on Android + one desktop platform for internal testers

**Version labels:**
- Unity `bundleVersion`: start `0.1.0-soft`
- EOS `ProductVersion`: match Unity marketing version major.minor (`0.1.0`)

**Monetization default:** Ship **without** IAP and **without** interstitial ads unless Task 8 completes. Rewarded post-match is optional nicety, not a gate.
```

- [ ] **Step 2: Append wiki log**

Append to `wiki/log.md`:

```markdown
- YYYY-MM-DD: Soft-launch scope locked → `docs/release/SOFT_LAUNCH_SCOPE.md`
```

- [ ] **Step 3: Commit**

```bash
git add docs/release/SOFT_LAUNCH_SCOPE.md wiki/log.md
git commit -m "$(cat <<'EOF'
docs(release): lock guest-only soft-launch scope

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

**Gate:** File exists; out-of-scope list includes social auth and non-Rush modes.

---

### Task 2: Align all platform EOS configs to Dev for development builds

**Files:**
- Modify: `Assets/StreamingAssets/EOS/eos_android_config.json`
- Modify: `Assets/StreamingAssets/EOS/eos_ios_config.json`
- Modify: `Assets/StreamingAssets/EOS/eos_macos_config.json`
- Modify: `Assets/StreamingAssets/EOS/eos_windows_config.json` (verify already Dev)
- Modify: `Assets/StreamingAssets/EOS/eos_linux_config.json` (verify already Dev)
- Create: `docs/release/EOS_ENVIRONMENTS.md` (how to flip Stage/Live)

**Interfaces:**
- Consumes: product deployment table in Global Constraints
- Produces: every mobile/desktop client config on Dev sandbox for internal builds

**Current bug:** Windows/Linux = Dev; Android/iOS/macOS = Live. Cross-platform lobbies fail or hit wrong environment.

- [ ] **Step 1: Document environment flip procedure**

Create `docs/release/EOS_ENVIRONMENTS.md`:

```markdown
# EOS Environments

| Name  | SandboxId                          | DeploymentId                         |
|-------|------------------------------------|--------------------------------------|
| Dev   | p-3e949b6n57y7qcjyg5sccpyatyzser   | 146b53cc89584a8d9586e9dd1f0caf91     |
| Stage | p-uvb48fad3qb2tza5wyetcx2hyxnrpt   | b27af9e630504620a05e5794e16ce190     |
| Live  | 19df6d3517a34ba480c2a65880c8567c   | 70f48f125a80447688e18cd17aac17db     |

## Development builds
All `Assets/StreamingAssets/EOS/eos_*_config.json` `deployment.SandboxId` + `deployment.DeploymentId` MUST match **Dev**.

## Promotion
Before Stage/Live store builds, set every platform file to Stage then Live as a single commit pair. Never mix sandboxes across platforms in one build train.
```

- [ ] **Step 2: Patch Android/iOS/macOS to Dev**

For each of `eos_android_config.json`, `eos_ios_config.json`, `eos_macos_config.json`, set:

```json
"deployment": {
  "SandboxId": {
    "Value": "p-3e949b6n57y7qcjyg5sccpyatyzser"
  },
  "DeploymentId": "146b53cc89584a8d9586e9dd1f0caf91"
}
```

Preserve surrounding keys (`clientCredentials`, flags, etc.). If `SandboxId` is a bare string in a file, use the same shape that file already uses—do not change schema, only values.

- [ ] **Step 3: Verify all platforms**

```bash
python3 - <<'PY'
import json
from pathlib import Path
DEV_SB='p-3e949b6n57y7qcjyg5sccpyatyzser'
DEV_DEP='146b53cc89584a8d9586e9dd1f0caf91'
base=Path('Assets/StreamingAssets/EOS')
for name in ['eos_windows_config.json','eos_linux_config.json','eos_android_config.json','eos_ios_config.json','eos_macos_config.json']:
    d=json.loads((base/name).read_text())
    sb=d['deployment']['SandboxId']
    if isinstance(sb,dict): sb=sb.get('Value',sb)
    dep=d['deployment']['DeploymentId']
    ok = sb==DEV_SB and dep==DEV_DEP
    print(('OK' if ok else 'FAIL'), name, sb, dep)
    if not ok: raise SystemExit(1)
print('all Dev')
PY
```

Expected: `all Dev`

- [ ] **Step 4: Commit**

```bash
git add Assets/StreamingAssets/EOS/eos_android_config.json \
  Assets/StreamingAssets/EOS/eos_ios_config.json \
  Assets/StreamingAssets/EOS/eos_macos_config.json \
  docs/release/EOS_ENVIRONMENTS.md
git commit -m "$(cat <<'EOF'
fix(eos): point all platform configs at Dev sandbox

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

**Gate:** Script exits 0; no platform remains on Live for internal train.

---

### Task 3: Repair EpicOnlineServicesConfig + version labels

**Files:**
- Modify: `Assets/StreamingAssets/EOS/EpicOnlineServicesConfig.json`
- Modify: `Assets/StreamingAssets/EOS/eos_product_config.json` (`ProductVersion`)
- Modify: `ProjectSettings/ProjectSettings.asset` (`bundleVersion`)

**Interfaces:**
- Consumes: ClientId / Dev DeploymentId from Global Constraints
- Produces: non-null PEW config fields; version strings aligned to scope doc

- [ ] **Step 1: Fill EpicOnlineServicesConfig.json**

Replace nulls (keep other fields):

```json
{
  "deploymentID": "146b53cc89584a8d9586e9dd1f0caf91",
  "clientID": "xyza7891a4duvFnuVg7ZJzUGcaf7vgHF",
  "tickBudgetInMilliseconds": 0,
  "taskNetworkTimeoutSeconds": 0.0,
  "platformOptionsFlags": "None",
  "authScopeOptionsFlags": "NoFlags",
  "integratedPlatformManagementFlags": 0,
  "alwaysSendInputToOverlay": false,
  "schemaVersion": "1.0"
}
```

- [ ] **Step 2: Align versions**

In `eos_product_config.json` set `"ProductVersion": "0.1.0"`.  
In `ProjectSettings/ProjectSettings.asset` set `bundleVersion: 0.1.0-soft`.

- [ ] **Step 3: Commit**

```bash
git add Assets/StreamingAssets/EOS/EpicOnlineServicesConfig.json \
  Assets/StreamingAssets/EOS/eos_product_config.json \
  ProjectSettings/ProjectSettings.asset
git commit -m "$(cat <<'EOF'
chore(eos): fill PEW config ids and align 0.1.0-soft version

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

**Gate:** `deploymentID` and `clientID` are non-null JSON strings.

---

### Task 4: Guest login → session smoke harness (Editor + log gates)

**Files:**
- Modify (only if gaps): `Assets/Playcenter/EOS/Runtime/AuthenticationService.cs`
- Modify (only if gaps): `Assets/_KitchenClash/Presentation/ViewModels/LoginViewModel.cs`
- Create: `docs/release/SMOKE_GUEST_LOGIN.md`
- Test (manual): Editor Play Mode Bootstrap

**Interfaces:**
- Consumes: `IAuthService.LoginAsGuestAsync()` → `AuthResult`
- Produces: documented smoke steps + any bugfixes required for guest success on Dev

- [ ] **Step 1: Write smoke procedure**

Create `docs/release/SMOKE_GUEST_LOGIN.md`:

```markdown
# Smoke: Guest login (EOS Dev)

## Preconditions
- EOS configs all Dev (Task 2)
- `EOS_AVAILABLE` define on target platform
- Network online; Epic product active

## Steps
1. Enter Play Mode on `Assets/Scenes/Bootstrap.unity` (or player build).
2. Reach Login; tap **Continue as Guest** / guest button.
3. Expect: no error modal; transition toward Home within 15s.
4. Log markers (filter Console):
   - EOS platform init success
   - DeviceId create or already exists
   - Connect login success
   - `IAuthService` reports authenticated
5. Kill app; relaunch; guest session restores or re-auths without crash.

## Fail criteria
- AuthResult.Failed with "not yet implemented" on guest path
- Hang >30s with no log
- Exception in AuthenticationService
```

- [ ] **Step 2: Code audit — guest must not hit social stubs**

Confirm `LoginAsGuestAsync` only calls Device ID Connect path. If any guest path returns `"not yet implemented"`, fix to Device ID only:

```csharp
// Pattern already in AuthenticationService — guest must stay on:
// EnsureEosDeviceIdCreated → StartConnectLoginWithOptions(DeviceID)
// Social methods may remain Failed("not yet implemented").
```

- [ ] **Step 3: Run Editor smoke (human or agent with Unity)**

Execute `docs/release/SMOKE_GUEST_LOGIN.md`. Record pass/fail in the same file under `## Last run`.

- [ ] **Step 4: Commit**

```bash
git add docs/release/SMOKE_GUEST_LOGIN.md Assets/Playcenter/EOS/Runtime/AuthenticationService.cs
git commit -m "$(cat <<'EOF'
docs(release): guest login smoke + EOS guest path fixes

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

**Gate:** Last run = PASS on at least one platform (Editor OSX or Windows counts for this task).

---

### Task 5: Lobby + matchmaking + P2P host smoke (2 clients or 1 + bots)

**Files:**
- Create: `docs/release/SMOKE_MATCH_FLOW.md`
- Modify only if broken:
  - `Assets/_KitchenClash/Infrastructure/EOS/*` (LobbyManager, Matchmaking)
  - `Assets/_KitchenClash/Infrastructure/Network/*` transport/spawn
  - `Assets/_KitchenClash/Infrastructure/Flow/Handlers/*` matchmaking phase

**Interfaces:**
- Consumes: authenticated session from Task 4; `ILobbyManager` / `IMatchmakingService` / `IGameStarter`
- Produces: two-peer or bot-filled match enters `Game` / map scene without hard exception

- [ ] **Step 1: Write match smoke doc**

```markdown
# Smoke: Match flow (EOS Dev)

## Steps
1. Two builds/Editor instances, both guest-logged on Dev **or** one client with bot fill enabled.
2. Both select Rush Service 2v2 queue (mode id `rush_service`).
3. Expect lobby form → match start ≤ 60s (or bot fill policy).
4. Both load gameplay scene; NGO `IsConnectedClient` true; player objects spawn.
5. Host migration / disconnect: leaving client returns to menu without freeze.

## Log markers
- Lobby create/join success
- Netcode start host/client
- `MatchLifetimeScope` build
- No repeated transport hard-fail loops
```

- [ ] **Step 2: Fix blockers only**

If smoke fails, fix the **first** hard blocker (auth token missing on lobby, transport not configured, scene not in build). Do not redesign matchmaking.

Known build gap to fix here if match loads wrong map:

`ProjectSettings/EditorBuildSettings.asset` must include:

```yaml
- enabled: 1
  path: Assets/Scenes/Map_RushService.unity
  guid: <existing meta guid from Map_RushService.unity.meta>
```

Read GUID from `Assets/Scenes/Map_RushService.unity.meta` — do not invent.

- [ ] **Step 3: Record PASS/FAIL in smoke doc; commit**

```bash
git add docs/release/SMOKE_MATCH_FLOW.md ProjectSettings/EditorBuildSettings.asset
# plus any infrastructure fixes
git commit -m "$(cat <<'EOF'
fix(release): match flow smoke path and Rush map in build

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

**Gate:** At least one successful match start with networked player spawn on Dev.

---

### Task 6: Cloud save proof — PlayerDataService persists via SaveService + EOS PDS

**Files:**
- Modify: `Assets/_KitchenClash/Infrastructure/Persistence/PlayerDataService.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/Persistence/SaveService.cs` (register cloud keys)
- Modify: `Assets/_KitchenClash/Composition/RootLifetimeScope.cs` only if registration order wrong
- Create: `docs/release/SMOKE_CLOUD_SAVE.md`
- Test: `Assets/Scripts/Tests/EditMode/Persistence/PlayerDataServiceSaveTests.cs` (new)

**Interfaces:**
- Consumes: `ISaveService`, `ICloudStorageProvider` (`EOSCloudStorageProvider` already root-registered)
- Produces: `IPlayerDataService` load/save of display name + progress that round-trips

**Current bug:** `PlayerDataService` constructs empty DTOs and barely uses save; `PlayerDataServiceAdapter` no-ops Load/Save. Root registers `PlayerDataService`, not the adapter.

- [ ] **Step 1: Write failing EditMode test**

```csharp
// Assets/Scripts/Tests/EditMode/Persistence/PlayerDataServiceSaveTests.cs
using System.Collections.Generic;
using KitchenClash.Application;
using KitchenClash.Infrastructure.Persistence;
using NUnit.Framework;

namespace RecipeRage.Tests.EditMode.Persistence
{
    public class PlayerDataServiceSaveTests
    {
        private sealed class MemSave : ISaveService
        {
            private readonly Dictionary<string, object> _data = new();
            public void Save<T>(string key, T value) => _data[key] = value;
            public T Load<T>(string key, T fallback = default) =>
                _data.TryGetValue(key, out var v) && v is T t ? t : fallback;
            // Implement remaining ISaveService members as no-op / default to compile
        }

        [Test]
        public void SetPlayerName_ThenNewService_LoadsName()
        {
            var save = new MemSave();
            var a = new PlayerDataService(save);
            a.Initialize();
            a.SetPlayerName("ChefTest");

            var b = new PlayerDataService(save);
            b.Initialize();
            Assert.AreEqual("ChefTest", b.GetStats().PlayerName);
        }
    }
}
```

Implement `MemSave` fully against real `ISaveService` surface (open interface file and stub every member). Test must compile in `RecipeRage.Tests.EditMode`.

- [ ] **Step 2: Run test — expect FAIL** (name not persisted)

Unity Test Runner or compile+manual until runner available.

- [ ] **Step 3: Implement persistence in PlayerDataService**

Minimal behavior:

```csharp
public class PlayerDataService : IPlayerDataService
{
    private const string ProgressKey = "player_progress.json";
    private const string StatsKey = "player_stats.json";

    private readonly ISaveService _saveService;
    private PlayerProgressData _progress;
    private PlayerStatsData _stats;

    public PlayerDataService(ISaveService saveService)
    {
        _saveService = saveService;
    }

    public void Initialize()
    {
        _progress = _saveService.Load(ProgressKey, new PlayerProgressData()) ?? new PlayerProgressData();
        _stats = _saveService.Load(StatsKey, new PlayerStatsData()) ?? new PlayerStatsData();
    }

    public PlayerProgressData GetProgress() => _progress;
    public PlayerStatsData GetStats() => _stats;

    public void SetPlayerName(string name)
    {
        _stats.PlayerName = name;
        PersistStats();
    }

    public void RecordGamePlayed(bool won, string gameModeId, string characterId, float playTime, int score, int xp)
    {
        _stats.RecordGamePlayed(won, gameModeId, characterId, playTime, score);
        _stats.AddExperience(xp);
        PersistStats();
    }

    // Upgrade/Unlock mutate _progress then PersistProgress()

    private void PersistStats() => _saveService.Save(StatsKey, _stats);
    private void PersistProgress() => _saveService.Save(ProgressKey, _progress);
}
```

- [ ] **Step 4: Register cloud strategy on login path**

In `SaveService` constructor or a small bootstrap called after DI build, register:

```csharp
RegisterStorageConfig("player_progress.json", StorageStrategy.CloudWithLocalFallback, encrypt: true);
RegisterStorageConfig("player_stats.json", StorageStrategy.CloudWithLocalFallback, encrypt: true);
```

Confirm `StorageStrategy` enum has this value; if name differs, use the existing cloud+local enum member.

Ensure `OnUserLoggedIn` is invoked from auth success path (grep; wire if missing):

```csharp
_saveService.OnUserLoggedIn();
```

- [ ] **Step 5: Smoke doc + commit**

```markdown
# Smoke: Cloud save
1. Guest login.
2. Set display name in UI.
3. Force save / return home.
4. Relaunch offline: name present from local.
5. Relaunch online second device same account if available: name present from PDS.
```

```bash
git add Assets/_KitchenClash/Infrastructure/Persistence/PlayerDataService.cs \
  Assets/_KitchenClash/Infrastructure/Persistence/SaveService.cs \
  Assets/Scripts/Tests/EditMode/Persistence/PlayerDataServiceSaveTests.cs \
  docs/release/SMOKE_CLOUD_SAVE.md
git commit -m "$(cat <<'EOF'
feat(persistence): persist player progress/stats via SaveService

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

**Gate:** Unit test green in Unity; smoke local round-trip PASS. Cloud second-device is best-effort if only one device.

---

### Task 7: Rush Service 2v2 playable vertical slice

**Files:**
- Modify: `Assets/Scenes/Map_RushService.unity` (stations, spawns, delivery zone, net objects)
- Modify: `Assets/_KitchenClash/Infrastructure/Network/PlayerCombatController.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/Network/Stations/AutonomousCookingStation.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/Network/LootPickup.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/Network/MatchWinConditionCoordinator.cs`
- Modify: `Assets/_KitchenClash/Infrastructure/Network/MatchEndController.cs`
- Modify: `Assets/Resources/ScriptableObjects/GameModes/RushService.asset` (timing/targets)
- Modify: `wiki/GameplayDesign.md` status line when slice playable
- Create: `docs/release/SMOKE_RUSH_SERVICE.md`

**Interfaces:**
- Consumes: mode id `rush_service` → `TugOfWarWinCondition`; RC keys `rush_service_target` default 100
- Produces: one complete match loop soft-launch quality (not full art polish)

**Design targets (from wiki — do not invent alternate mode rules):**
- 2v2; two team kitchens + middle delivery; tug-of-war bar to 100; ~3 min cap
- Loop: prime station → fight → collect → deliver → KO drops loot → 3s respawn

- [ ] **Step 1: Scene readiness checklist (edit in Unity)**

On `Map_RushService.unity` ensure:
1. NGO `NetworkManager` / match bootstrap objects present (same pattern as `Game.unity` or working map).
2. Team A/B spawn points (2 each).
3. ≥2 `AutonomousCookingStation` per team side.
4. Contested middle delivery zone with trigger/score authority.
5. `MatchWinConditionCoordinator` + `MatchEndController` + `RoundTimer` + score managers in scene or spawned by binder.
6. Scene in Editor Build Settings (Task 5).

- [ ] **Step 2: Wire mode id on match start**

Wherever match runtime starts (grep `SetMode`), ensure Rush queue passes `"rush_service"`:

```csharp
winConditionCoordinator.SetMode("rush_service");
```

- [ ] **Step 3: Implement missing gameplay only for slice**

Minimum playable bar (stop when each works networked):
1. Prime input advances station IDLE→PRIMED→COOKING→READY.
2. Collect READY dish into carry slot.
3. Deliver in zone → `ScoreChangedEvent` with delta → tug bar moves.
4. Melee hit on carrier drops loot (`LootPickup`).
5. KO → respawn 3s.
6. Bar hits target OR timer expiry → `MatchEndController` ends match → Results flow.

Prefer fixing existing classes over new systems. No Hell's Kitchen / Last Plate work.

- [ ] **Step 4: Smoke doc**

```markdown
# Smoke: Rush Service slice
1. Start 2v2 Rush (bots OK).
2. Prime, collect, deliver at least one dish each team.
3. Confirm score UI / tug bar moves.
4. KO a carrier; loot appears; respawn works.
5. Force win (deliver to target or debug); Results screen shows; coins grant (Task 8 dependency OK if rewards already wired).
```

- [ ] **Step 5: Update wiki status**

In `wiki/GameplayDesign.md` header, change implementation line to note:

```markdown
> Rush Service 2v2 soft-launch slice: PLAYABLE (YYYY-MM-DD). Other modes still incomplete.
```

Append `wiki/log.md`.

- [ ] **Step 6: Commit**

```bash
git add Assets/Scenes/Map_RushService.unity \
  Assets/_KitchenClash/Infrastructure/Network \
  Assets/Resources/ScriptableObjects/GameModes/RushService.asset \
  docs/release/SMOKE_RUSH_SERVICE.md wiki/GameplayDesign.md wiki/log.md
git commit -m "$(cat <<'EOF'
feat(gameplay): Rush Service 2v2 soft-launch playable slice

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

**Gate:** `SMOKE_RUSH_SERVICE.md` Last run = PASS. Not a full content complete game—slice only.

---

### Task 8: Results → economy → analytics closed loop

**Files:**
- Verify/fix: `Assets/_KitchenClash/Infrastructure/Flow/Handlers/ResultsPhase.cs`
- Verify/fix: `Assets/_KitchenClash/Infrastructure/Services/MatchRewardHandler.cs`
- Verify/fix: `Assets/_KitchenClash/Presentation/ViewModels/ResultsViewModel.cs`
- Create: `docs/release/SMOKE_RESULTS_ECONOMY.md`
- Test: `Assets/Scripts/Tests/EditMode/Economy/MatchRewardHandlerTests.cs` (extend or add)

**Interfaces:**
- Consumes: `MatchEndedEvent` `{ Won, LocalTeamScore }`; `IWalletLedger.Credit`; `IAnalyticsService.LogEvent`
- Produces: coins credited once per results entry; analytics `WalletCredit` (or project constant)

- [ ] **Step 1: Failing test if missing**

```csharp
[Test]
public void OnMatchEnded_Win_CreditsWinRewardPlusScoreBonus()
{
    // Spy ledger + spy event bus + handler.Initialize()
    // Publish MatchEndedEvent Won=true LocalTeamScore=10
    // Assert ledger credited Coins == MatchWinReward + Floor(10 * ScoreBonusCoinRate)
    // Assert analytics LogEvent called once
}
```

- [ ] **Step 2: Fix double-credit / missing publish bugs only**

Rules:
- `ResultsPhase` publishes `MatchEndedEvent` once per entry.
- `MatchRewardHandler` is sole credit path.
- Results UI must not call `IEconomyService` mint except rewarded ad gems path.

- [ ] **Step 3: Soft-launch ads UX**

If `IAdsService.IsRewardedReady` is always false (no MAX), Results UI hides watch-ad CTA (already gated by `CanShowRewardedAd`). Do **not** block release on ads.

- [ ] **Step 4: Smoke + commit**

```markdown
# Smoke: Results economy
1. Finish Rush match.
2. Results shows win/loss.
3. Wallet coins increase once.
4. Analytics debug sink or Firebase shows wallet_credit / match event.
5. Re-enter results (if possible) does not double pay.
```

```bash
git commit -m "$(cat <<'EOF'
fix(economy): close results reward and analytics loop

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

**Gate:** Test green; smoke no double credit.

---

### Task 9: Monetization ship decision (default: without paid IAP/MAX)

**Files:**
- Create: `docs/release/MONETIZATION_DECISION.md`
- Optional path A (no vendors): Modify UI to hide shop IAP buttons when store uninitialized
- Optional path B (wire vendors): `Packages/manifest.json` + defines + adapter completion under `Assets/Playcenter/Services.Unity/Runtime/**`

**Interfaces:**
- Consumes: `IIAPService`, `IAdsService`, `IAnalyticsService`, `IRemoteConfigService` already in RootLifetimeScope
- Produces: written decision; either hidden IAP or working UNITY_IAP sandbox purchase

- [ ] **Step 1: Write decision file (choose A unless human says B)**

Default content:

```markdown
# Monetization decision — soft launch

**Decision:** Ship **without** Unity IAP and **without** AppLovin MAX.
**Analytics/RC:** Keep Debug/Fallback sinks unless Firebase packages are already resolvable; do not block soft launch on Firebase.

## Rationale
- Shared SDK facades are production-shaped.
- Vendor packages absent from `Packages/manifest.json` (no purchasing, no MAX, no full Firebase modules).
- Soft-launch scope prioritizes multiplayer loop.

## Follow-up (post soft launch)
1. Add `com.unity.purchasing`, implement `UnityIapStoreBackend` `#if UNITY_IAP` purchase callbacks.
2. Add AppLovin MAX Unity plugin; set `APPLOVIN_MAX`; complete `MaxAdNetwork` callbacks.
3. Add Firebase Analytics + Remote Config packages; enable `FIREBASE_ANALYTICS` + `FIREBASE_REMOTE_CONFIG` on Android/iOS.
4. Re-enable Results rewarded CTA and shop SKUs.
```

- [ ] **Step 2A (default): Hide unpaid surfaces**

Grep shop/IAP buttons; when `IIAPService` reports not initialized, disable purchase buttons and show “Coming soon” or hide section. No fake grants in production builds (`#if UNITY_EDITOR` only for EditorFake).

- [ ] **Step 2B (only if human overrides): Vendor wire**

1. Add packages to manifest.  
2. Enable defines via `Assets/Scripts/Editor/ProjectDefinesWindow.cs` recommended sets.  
3. Complete TODO callbacks in `MaxAdNetwork`, `UnityIapStoreBackend`, Firebase sinks.  
4. Device sandbox purchase + one rewarded ad smoke.

- [ ] **Step 3: Commit**

```bash
git add docs/release/MONETIZATION_DECISION.md
# + UI guards or vendor files
git commit -m "$(cat <<'EOF'
docs(release): soft-launch monetization decision and UI guards

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

**Gate:** Decision file exists; production build cannot soft-lock on IAP init failure.

---

### Task 10: Device QA script + Stage promotion dry-run

**Files:**
- Create: `docs/release/DEVICE_QA_CHECKLIST.md`
- Modify (promotion commit only when QA on Dev passed): all `eos_*_config.json` → Stage values from Task 2 table

**Interfaces:**
- Consumes: all prior smokes
- Produces: signed checklist rows

- [ ] **Step 1: Write device QA checklist**

```markdown
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
```

- [ ] **Step 2: Run Dev QA on minimum matrix**

Minimum for soft launch: **Android + one of (Windows Editor player / macOS / Windows standalone)**. iOS if certificates exist.

- [ ] **Step 3: Stage promotion (separate commit after Dev QA pass)**

Set all platform EOS configs to Stage sandbox/deployment from Global Constraints. Tag build `0.1.0-soft-stage`. Re-run rows 2–7 on Stage.

- [ ] **Step 4: Commit checklist results (no secrets)**

```bash
git add docs/release/DEVICE_QA_CHECKLIST.md
git commit -m "$(cat <<'EOF'
docs(release): device QA checklist and Dev results

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

**Gate:** Rows 1–9 Pass on minimum matrix on Dev; Stage optional but required before public store.

---

### Task 11: Soft-launch go/no-go gate + wiki freeze

**Files:**
- Create: `docs/release/SOFT_LAUNCH_GATE.md`
- Modify: `wiki/Technical.md` (EOS env + soft-launch status section)
- Modify: `wiki/log.md`
- Modify: `Documentation/Architecture/PHASE_ROADMAP.md` (mark soft-launch gate)

**Interfaces:**
- Consumes: all smoke docs + QA checklist
- Produces: binary GO / NO-GO with open risks

- [ ] **Step 1: Write gate file**

```markdown
# Soft-Launch Go/No-Go

## Must be GO
- [ ] SOFT_LAUNCH_SCOPE locked
- [ ] All platforms EOS Dev aligned for internal; Stage configs tested if shipping Stage
- [ ] Guest login smoke PASS
- [ ] Match flow smoke PASS
- [ ] Cloud/local save smoke PASS
- [ ] Rush Service slice smoke PASS
- [ ] Results economy smoke PASS (no double grant)
- [ ] Monetization decision filed; no IAP crash
- [ ] Device QA minimum matrix PASS
- [ ] Version 0.1.0-soft on binary
- [ ] Known issues list attached (max 10, none data-loss critical)

## Explicit non-blockers
- Social login stubs
- MAX/IAP/Firebase absent
- Non-Rush modes incomplete
- Friends/leaderboards/anti-cheat

## Decision
- [ ] GO soft launch
- [ ] NO-GO (blockers: ____)

Owner: ____ Date: ____
```

- [ ] **Step 2: Wiki Technical note**

Add short section: soft-launch guest-only; EOS Dev/Stage table pointer to `docs/release/EOS_ENVIRONMENTS.md`; shared services already in SDK.

- [ ] **Step 3: Commit**

```bash
git add docs/release/SOFT_LAUNCH_GATE.md wiki/Technical.md wiki/log.md \
  Documentation/Architecture/PHASE_ROADMAP.md
git commit -m "$(cat <<'EOF'
docs(release): soft-launch go/no-go gate and wiki freeze

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

**Gate:** Human signs GO before store upload. Agents must not claim production Live release without this signature.

---

### Task 12: Live promotion runbook (post soft launch — do not execute until GO + Stage soak)

**Files:**
- Create: `docs/release/LIVE_PROMOTION_RUNBOOK.md`
- Modify (execution time only): all `eos_*_config.json` → Live ids

**Interfaces:**
- Consumes: Stage soak ≥ N days (human sets N; default 3)
- Produces: reversible Live cutover steps

- [ ] **Step 1: Write runbook (no Live cutover in same commit as writing)**

```markdown
# Live promotion runbook

1. Confirm SOFT_LAUNCH_GATE = GO and Stage soak done.
2. Single commit: all eos_*_config.json → Live SandboxId/DeploymentId.
3. bundleVersion bump (e.g. 0.1.0).
4. Build Android/iOS store binaries; smoke guest login + one match on Live.
5. Submit store; monitor crash + EOS error rates 24h.
6. Rollback = revert config commit to Stage and hotfix build.

## Live ids
- SandboxId: 19df6d3517a34ba480c2a65880c8567c
- DeploymentId: 70f48f125a80447688e18cd17aac17db
```

- [ ] **Step 2: Commit runbook only**

```bash
git add docs/release/LIVE_PROMOTION_RUNBOOK.md
git commit -m "$(cat <<'EOF'
docs(release): Live promotion runbook (execution gated)

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
EOF
)"
```

**Gate:** Runbook exists; Live JSON not changed until human orders promotion.

---

## Dependency graph

```text
T1 scope
  → T2 EOS Dev align → T3 PEW/version → T4 guest smoke → T5 match smoke
  → T6 cloud save (parallel after T4)
  → T7 Rush slice (after T5 map-in-build)
  → T8 results economy (after T7 match end events exist)
  → T9 monetization decision (parallel after T1; finish before T10)
  → T10 device QA (after T4–T9)
  → T11 go/no-go
  → T12 Live runbook (doc now; execute later)
```

## Out of scope (do not sneak in)

- Full social auth implementation
- Hell's Kitchen / Last Plate Standing ship polish
- Anti-cheat, party/friends-required UX
- Rebuilding Playcenter shared service facades
- Pushing 100+ local commits to origin without human ask
- Store screenshot/ASO campaign

## Self-review

1. **Spec coverage:** Prior gap analysis items map to tasks: sandbox mismatch→T2; PEW null config→T3; guest/social→T4+scope; lobby/P2P→T5; cloud save hollow→T6; Kitchen Brawler unplayable→T7; rewards→T8; vendors unwired→T9; device/Stage/Live→T10–T12.
2. **Placeholders:** None intentional; Unity scene work is checklist-driven because binary scenes cannot be fully inlined as C#.
3. **Type consistency:** Mode id `rush_service`; Dev/Stage/Live ids repeated from product config; wallet via `MatchRewardHandler` only.

---

## Execution handoff

Plan complete and saved to `docs/superpowers/plans/2026-07-22-soft-launch-release-checklist.md`.

**Two execution options:**

1. **Subagent-Driven (recommended)** — fresh subagent per task, review between tasks  
2. **Inline Execution** — this session with executing-plans checkpoints  

**Which approach?**
