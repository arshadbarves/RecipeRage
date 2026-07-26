# Playcenter Client OS — Production Multi-Game Shell Design

**Date:** 2026-07-19  
**Status:** Implemented (Tasks 1–11); wiki laws updated (Task 12)  
**Branch:** `architecture-cleanup`  
**Implementation plan:** `docs/superpowers/plans/2026-07-19-playcenter-client-os.md`  
**Runtime laws:** `wiki/Technical.md` § Playcenter Client OS; `wiki/LLM-Rules.md` REQUIRED/FORBIDDEN  
**Related:**
- `docs/superpowers/specs/2026-07-16-playcenter-shared-stack-design.md` (Approach C pure + Unity-thin DAG — still valid)
- `docs/superpowers/specs/2026-07-14-architecture-hardening-design.md` (dependency laws)
- `docs/superpowers/specs/2026-07-15-playcenter-foundation-extract-design.md`
- `wiki/Technical.md`, `wiki/LLM-Rules.md`, `wiki/Gameplay.md`
- KW reference layout: GunDealerProd `Assets/Submodules/KW*` (comparison only; do not copy fat managers)

**Supersedes / extends:**
- Extends 2026-07-16 shared-stack with **session identity, wallet ledger, net session, settings/input, connectivity-first boot, Brawl-style multiplayer UX, themed UI shell redesign**
- Does **not** reopen GameFlow navigation (`IAppFlow` remains sole product navigator)
- Does **not** move cooking IP, chefs, recipes, maps, or NGO match rules into Playcenter

### Implementation status (Tasks 1–11)

| Task | Commit | Summary |
|------|--------|---------|
| 1 Boot | `52dda74d`, `7366e318` | Connectivity-first BootSequence + CompleteSidePhase from NoConnection |
| 2 Wallet ports | `268bebe8` | IWallet / IWalletLedger / IWalletStore |
| 3 Economy bridge | `9162a3e0` | EconomyService dual-impl; MatchRewardHandler via ledger |
| 4 Session installer | `07896178` | CreateSession requires ISessionScopeInstaller |
| 5 Dual lobby | `c75b4fe6` | Party vs match lobby |
| 6 Net ports | `5dfb2beb` | INetSession ports |
| 7 Net adapter | `8c1a69c9` | NgoEosNetSession + GameStarter + forfeit bridge |
| 8 Shell UI | `68993b1b` | DesignSystem shell + Home |
| 9 Match shell | `61277f65` | Match lobby / VS / results DesignSystem |
| 10 Input/Settings | `864afeff` | IGameplayInput + ISettingsService |
| 11 Live-ops | `70e45110` | Analytics hooks + presentation purity |
| 12 Wiki laws | (this program) | Technical + LLM-Rules + log + memory |
---

## 1. Problem

RecipeRage needs a **Brawl Stars / PUBG Mobile / Fortnite-class client product shell**:

| Need | Today |
|------|--------|
| Multi-game reusable systems behind stable APIs | Playcenter ports exist; many adapters and facades still KitchenClash-shaped or incomplete |
| Completely replaceable auth, economy, IAP, shop, analytics, multiplayer, game mode, settings, input | Partial — ports uneven; economy is `IEconomyService` in Domain; session DI was bare-child until recent fix |
| P2P multiplayer (not dedicated server) | EOS Lobby + NGO + sample `EOSTransport` exist; not unified behind `INetSession`; sample quality uneven |
| Connectivity-first boot | `BootSequence` starts NTP → RC → … with **no** online gate |
| Brawl-style party → PLAY → MM → lobby/VS → match → results | Party/match lobby split exists in `EOSLobbyService`; UX/UI incomplete |
| UI shell that matches new flows | Screens exist; UXML needs redesign for party/MM/lobby; **keep DesignSystem.uss theme** |
| Studio reuse without greenfield rewrite | In-repo `Assets/Playcenter/*`; copy/submodule later; no UPM packaging this program |

**Non-goal of this program:** rewrite cooking gameplay, invent a new navigation OS, or ship a second visual brand.

---

## 2. Goals and non-goals

### 2.1 Goals

1. **Playcenter Client OS** — modular product shell with stable public ports; KitchenClash (and future titles) bind implementations in Composition.
2. **Replaceability** — any vertical (auth, wallet, IAP, ads, analytics, lobby/MM, net transport, settings, input) can be swapped by rebinding DI to a new adapter implementing the same port.
3. **P2P multiplayer** — party + match lobbies (EOS) + NGO over EOS P2P transport; ports never expose NGO/EOS types.
4. **Production boot** — connectivity first, then NTP/RC/force update/maintenance/auth/session.
5. **Brawl-class multiplayer UX** — party on Home, leader PLAY, matchmaking, short match lobby/VS, match, results with wallet grant, return Home with party intact.
6. **UI shell redesign** — UXML + reusable component templates for new flows; **single theme: DesignSystem.uss** (Brawl/Overcooked ink + yellow). No rebrand.
7. **Raise weak modules to top tier** where grade is low; keep strong modules (GameFlow, Shell event bus).
8. **Multi-title path** — in-repo modules now; physical git submodules only when a second title consumes them.

### 2.2 Non-goals

| Out of scope | Why |
|--------------|-----|
| Dedicated game servers / host migration v1 | P2P; reconnect window then forfeit/end |
| Full anti-cheat / authoritative sim | Client-trust P2P cooking; server economy later via ledger seam |
| UPM package publish | Premature before second title |
| Cooking, chefs, recipes, station NetBehaviours in Playcenter | Game IP |
| New art direction / dual theme (Hot Kitchen dark + DesignSystem white) | Lock DesignSystem v3 only |
| Replacing `IAppFlow` | Already production navigator |
| KW-style fat MonoBehaviour managers / static event mesh | Steal vertical module *ideas*, not architecture smells |
| Complete IAP store backend / real ads mediation in wave 1 | Ports + stubs/adapters; real vendors when keys exist |

---

## 3. Locked assumptions

| # | Assumption |
|---|------------|
| L1 | **Stack:** Unity 6 + VContainer + UniTask + UI Toolkit + DOTween + NGO + EOS (PlayEveryWare) |
| L2 | **In-repo Playcenter** under `Assets/Playcenter/*`; multi-game via copy/submodule later |
| L3 | **Two tiers:** pure (`noEngineReferences`) + Unity-thin; restricted DAG only (2026-07-16 Approach C) |
| L4 | **Scopes:** ROOT → SESSION (menu) → MATCH; MATCH never owns wallet writes |
| L5 | **SESSION only after auth**; empty session child without installer is a bug |
| L6 | **P2P:** `INetSession` / `INetTransportConfigurator` in Playcenter; NGO + EOS P2P + match rules in game |
| L7 | **Live-ops shell in Playcenter ports;** game supplies catalogs, prices, chefs, recipes, mode IDs |
| L8 | **Approach A evolve** — raise modules and adapters; no mega `IPlaycenterClient`; no greenfield OS |
| L9 | **DesignSystem.uss** is the locked visual theme for shell redesign |
| L10 | **Connectivity is boot step 1** (before NTP/RC/auth) |
| L11 | **Reconnect v1:** no host migration; reconnect window then forfeit/end match |
| L12 | **Hard cutover** per vertical when promoting — no dual namespaces / obsolete stubs left behind after a phase’s delete gate |
| L13 | **Composition owns DI registration;** pure modules do not reference VContainer |
| L14 | **Presentation must not reference Infrastructure** (end-state law from 2026-07-14) |

---

## 4. Approaches considered

### Approach A — Evolve Playcenter DAG + raise adapters (**chosen**)

Keep GameFlow/Shell/Services/UI.Toolkit/EOS. Add missing ports (player session, wallet ledger, net session, settings, gameplay input). Harden EOS lobby/MM/P2P to sample grade. Redesign UXML on DesignSystem. Incremental phases with compile + delete gates.

| Pros | Cons |
|------|------|
| Reuses shipped navigation and modules | Multi-phase |
| Multi-title ports without second-repo tax | Requires discipline on vendor leaks |
| Matches “replace implementations, keep bus” | |

### Approach B — Greenfield Client OS assembly

New top-level product framework; migrate KitchenClash onto it.

| Pros | Cons |
|------|------|
| Clean slate | Breaks working GameFlow; months of dual systems |
| | Rejected — user wants complete replace of *systems*, not throw away the spine |

### Approach C — KW submodule explosion now

One git submodule per KW-style vertical immediately.

| Pros | Cons |
|------|------|
| Studio packaging early | Overhead before second title; KW fat-manager smell |
| | Rejected for packaging; **steal module map ideas only** |

**Decision: Approach A.**

---

## 5. Architecture

### 5.1 Two-tier Playcenter + game

```
┌──────────────────────────────────────────────────────────────────┐
│ TIER 0 — Pure (noEngineReferences)                               │
│  Playcenter.Shell      — IEventBus, ILoggingService,             │
│                          IConnectivityService                      │
│  Playcenter.GameFlow   — IAppFlow, phases, policies              │
│  Playcenter.Services   — all product ports (auth, wallet,        │
│                          lobby, MM, net session, IAP, ads,       │
│                          analytics, settings, input contracts,   │
│                          storage, RC, friends, …)                │
│  Playcenter.UI         — IUIService ports / screen enums         │
└──────────────────────────────────────────────────────────────────┘
                                ▲ references only downward
┌──────────────────────────────────────────────────────────────────┐
│ TIER 1 — Unity-thin (optional per title)                         │
│  Playcenter.UI.Toolkit — BaseUIScreen, stack host                │
│  Playcenter.Animation  — DOTween adapters                        │
│  Playcenter.EOS        — shared EOS auth/lobby glue (no game IP) │
│  (future) Persistence / Networking pool helpers                  │
└──────────────────────────────────────────────────────────────────┘
                                ▲
┌──────────────────────────────────────────────────────────────────┐
│ GAME — KitchenClash                                              │
│  Composition (Root/Menu/Match scopes, installers)                │
│  Flow handlers (BootSequence, phases)                            │
│  EOSLobbyService, EOSMatchmakingService, EOS transport bind      │
│  NGO match, cooking, bots, title UXML/USS, catalogs              │
└──────────────────────────────────────────────────────────────────┘
```

### 5.2 Dependency laws (non-negotiable)

| From → To | Allowed |
|-----------|---------|
| Pure module → pure module | Only down DAG (documented edges) |
| Unity-thin → pure | Yes |
| Unity-thin → peer Unity-thin | **No** (no mesh) |
| Game Presentation → Application, Domain, Playcenter.* ports | Yes |
| Game Presentation → Infrastructure | **No** (end state) |
| Application / Playcenter.Services → Epic / NGO / UnityEngine | **No** |
| Infrastructure → Application, Domain, Playcenter | Yes |
| Composition → all | Yes |

### 5.3 Lifetime scopes

| Scope | Lifetime | Owns |
|-------|----------|------|
| **ROOT** | App | Event bus, logging, connectivity, auth port, UI service, AppFlow, RC, config, network primitives (`IPlayerNetworkManager`, pool, `INetworkGameManager`), `ISessionScopeInstaller`, cold `SessionManager` |
| **SESSION** | Post-auth menu | Lobby, MM, team, friends, economy/wallet, character/skins, shop VMs, IAP/ads bindings, session-scoped loaders |
| **MATCH** | Single match | Score, orders, abilities, hazards, match context, bots, recipe catalog — **read** wallet for display only; **write** rewards via SESSION ledger after match |

**Laws:**
- Never inject MATCH service into ROOT/SESSION.
- Never create SESSION child without `ISessionScopeInstaller` (or equivalent shared registration path).
- `ISessionContext` / future `IPlayerSession` facade resolves SESSION-first, then safe fallbacks — never crash boot on missing optional.

### 5.4 KW-aligned vertical module map (logical)

Logical concerns (may live as folders under `Playcenter.Services` until a second title forces split assemblies):

| Vertical | Port surface | Default adapter (KC) |
|----------|--------------|----------------------|
| Identity / Auth | `IAuthService`, lifecycle hooks | Playcenter.EOS + KC hooks |
| Player session | `IPlayerSession`, `ISessionModuleInstaller` | SessionManager + MenuSessionRegistrations |
| Wallet / economy | `IWallet`, `IWalletLedger`, `IWalletStore` | Evolve from `IEconomyService` |
| IAP | `IIAPService` | Stub → store adapter |
| Ads | `IAdsService` | Stub → mediator |
| Analytics | `IAnalyticsService` + `IFlowAnalyticsPort` | Existing / no-op |
| Remote config | `IRemoteConfigService` | Existing |
| Connectivity | `IConnectivityService` | Existing + boot gate |
| Lobby / party | `ILobbyManager` | `EOSLobbyService` |
| Matchmaking | `IMatchmakingService` | `EOSMatchmakingService` |
| Team | `ITeamManager` | EOS team adapter |
| Net session | `INetSession`, `INetTransportConfigurator` | NGO + EOSTransport |
| Friends | `IFriendsService` | EOS friends |
| Settings | `ISettingsService` | Local + cloud key-value |
| Input | `IGameplayInput` (+ dual-stick binding in game) | New Input System / mobile sticks |
| UI shell | `IUIService`, screens | UI.Toolkit + title UXML |
| Flow | `IAppFlow` | GameFlow (unchanged contract) |

**Do not** create a KWCore junk drawer. **Do** keep server-facing seams (`IWalletLedger` grant validation hook) ready for future trusted backend.

---

## 6. Public API (ports)

All new ports live in `Playcenter.Services` (pure) unless noted. Names are normative for the implementation plan.

### 6.1 Player session

Pure `Playcenter.Services` is `noEngineReferences` with **no UniTask reference** (matches current asmdef). Async ports use `System.Threading.Tasks.Task` + `CancellationToken`. Game adapters may wrap with UniTask at the boundary.

```csharp
namespace Playcenter.Services
{
    public interface IPlayerSession
    {
        bool IsEstablished { get; }
        string PlayerId { get; }          // product user / account id
        string DisplayName { get; }

        /// <summary>Create SESSION scope, install modules, hydrate wallet/profile.</summary>
        Task EstablishAsync(CancellationToken ct);

        /// <summary>Tear down SESSION (logout / account switch).</summary>
        void Teardown();
    }

    /// <summary>
    /// Marker/capability installed by game Composition when SESSION is created.
    /// VContainer-typed install stays on game bridge (ISessionScopeInstaller today)
    /// so pure Playcenter never references VContainer.
    /// </summary>
    public interface ISessionModuleInstaller
    {
        // Intentionally empty capability marker OR game-local typed bridge only.
        // Do not put IContainerBuilder here.
    }
}
```

**Migration note:** KitchenClash already has `ISessionScopeInstaller` + `MenuSessionRegistrations`. Keep the **VContainer-typed bridge in KC Composition/Application**. Optional empty `ISessionModuleInstaller` marker in Playcenter is documentation-only until a second title needs a shared non-VContainer install protocol.

Until full promote, **`ISessionScopeInstaller` remains the in-tree law** for session child registration.

### 6.2 Wallet (replaces god-economy over time)

```csharp
namespace Playcenter.Services
{
    public interface IWallet
    {
        int GetBalance(string currencyId);
        bool CanAfford(string currencyId, int amount);
    }

    public interface IWalletLedger
    {
        /// <summary>Soft currency grant (match reward, streak, IAP delivery).</summary>
        bool TryCredit(string currencyId, int amount, string reasonCode, string correlationId);

        /// <summary>Spend with fail-closed insufficient funds.</summary>
        bool TryDebit(string currencyId, int amount, string reasonCode, string correlationId);
    }

    public interface IWalletStore
    {
        Task LoadAsync(CancellationToken ct);
        Task SaveAsync(CancellationToken ct);
    }
}
```

**KitchenClash bridge:** `IEconomyService` becomes a façade over `IWallet` + `IWalletLedger` + inventory helpers during migration; delete façade when all callers moved (delete gate).

**Laws:**
- MATCH must not call `TryCredit`/`TryDebit` directly; Results/SESSION `MatchRewardHandler` posts ledger entries.
- IAP success → ledger credit with reason `iap:{productId}`.
- No silent coin mint outside ledger.

### 6.3 Net session (P2P, vendor-free)

```csharp
namespace Playcenter.Services
{
    public enum NetSessionRole { None, Host, Client }

    public interface INetSession
    {
        NetSessionRole Role { get; }
        bool IsActive { get; }
        string HostPlayerId { get; }

        event Action OnStarted;
        event Action<string /* reason */> OnStopped;
        event Action<string /* playerId */> OnMemberJoined;
        event Action<string /* playerId */> OnMemberLeft;
        event Action OnHostLost;

        /// <summary>Configure transport then start host or client.</summary>
        Task StartAsync(NetSessionStartRequest request, CancellationToken ct);

        void Stop(string reason);
    }

    public sealed class NetSessionStartRequest
    {
        public NetSessionRole Role;
        public string HostPlayerId;           // PUID string
        public IReadOnlyList<string> MemberPlayerIds;
        public string SessionCorrelationId;   // match lobby id
    }

    /// <summary>Game/Unity-thin binds NGO transport (e.g. EOS P2P) without leaking types upward.</summary>
    public interface INetTransportConfigurator
    {
        void ConfigureForHost(string localPlayerId);
        void ConfigureForClient(string localPlayerId, string hostPlayerId);
        void Reset();
    }
}
```

**No** `Unity.Netcode` or `Epic.OnlineServices` types on these interfaces.

### 6.4 Lobby / matchmaking (existing — harden)

Keep `ILobbyManager`, `IMatchmakingService`, `ITeamManager` in `Playcenter.Services`.

**Hardening requirements:**
- Public DTOs (`LobbyInfo`, `LobbyConfig`, `LobbyOpResult`, `PlayerInfo`) must not expose Epic `Result` or PlayEveryWare `Lobby` types (strip remaining leaks — 2026-07-14 law).
- Party lobby vs match lobby semantics mandatory (see §7).
- Member attributes (opaque string map or typed fields): ready, chefId, teamSlot, displayName.

### 6.5 Settings

```csharp
namespace Playcenter.Services
{
    public interface ISettingsService
    {
        T Get<T>(string key, T defaultValue);
        void Set<T>(string key, T value);
        Task LoadAsync(CancellationToken ct);
        Task SaveAsync(CancellationToken ct);

        event Action<string /* key */> OnSettingChanged;
    }
}
```

Keys are stringly with a documented catalog in game (audio, graphics, controls, privacy). Playcenter does not hardcode title keys.

### 6.6 Gameplay input (pure shapes)

```csharp
namespace Playcenter.Services
{
    public readonly struct InputAxis2
    {
        public readonly float X;
        public readonly float Y;
        public InputAxis2(float x, float y) { X = x; Y = y; }
    }

    public interface IGameplayInput
    {
        InputAxis2 Move { get; }
        InputAxis2 Aim { get; }       // dual-stick; aim may equal move on keyboard fallback
        bool InteractPressed { get; }
        bool CancelPressed { get; }
        bool AbilityPressed { get; }

        void Enable();
        void Disable();
    }
}
```

**No UnityEngine types.** Game adapter maps New Input System / on-screen sticks (Brawl dual-stick per `wiki/Gameplay.md`). Keyboard/gamepad fallback in same adapter.

### 6.7 Existing ports retained

Unchanged contracts (implementations may be raised):  
`IAuthService`, `IAdsService`, `IIAPService`, `IAnalyticsService`, `IRemoteConfigService`, `IConfigService`, `IFriendsService`, `IMaintenanceService`, `INTPTimeService`, `IConnectivityService`, `IUIService`, `IAppFlow`, storage/encryption/audio volume as today.

### 6.8 Session façade (game Application)

`ISessionContext` remains the UI-facing aggregate during migration:

```text
ISessionContext
  → Economy/Wallet, Character, Friends, Lobby, … via TryResolve
```

Long-term: thin wrapper over `IPlayerSession` + resolved SESSION services; Presentation never touches Infrastructure `SessionContext` type (move interface fully to Application — already started).

---

## 7. Runtime flows

### 7.1 Boot (connectivity first)

```
Splash (NotifySplashComplete)
  → BootSequence:
      1. IConnectivityService.IsOnline?
           NO  → EnterSidePhase(NoConnection); wait retry/quit; on online CompleteSidePhase → re-enter from step 1
           YES → continue
      2. NTP sync (best-effort, 5s timeout) — never blocks boot failure alone
      3. RemoteConfig Initialize + Refresh (soft fail → cache/defaults)
      4. Force update check → ForceUpdate side (hard) if required
      5. Maintenance check → Maintenance side (hard) if active
      6. Auth silent restore
           fail → Login side; on LoginSuccess → continue 7
      7. Establish SESSION (installer + wallet LoadAsync + profile)
           fail → Login/retry path; never NotifyBootComplete with empty SESSION
      8. NotifyBootComplete → Home
```

**Continuous gates:**
- Home / before PLAY: if offline → NoConnection or disable PLAY.
- Matchmaking start: re-check online.
- Match: `NotifyMatchStarted` / `NotifyMatchEnded` / `NotifyHostDropped` on connectivity service.

**Code gap:** `BootSequence` today omits step 1 — implementation must add it.

### 7.2 Happy path — Brawl multiplayer shell

```
Home (SESSION)
  · Party strip (solo = party of 1; CreatePartyLobby lazy or on first invite)
  · Invite friends → ILobbyManager.InviteToParty
  · Mode select (2v2 / 3v3 / …) — queue-driven team size only
  · PLAY (party leader only if party size > 1)
       → IAppFlow.RequestPlay(PlayRequest)
Matchmaking
  · IMatchmakingService.FindMatch
  · Search / create match lobby; bot fill per existing always-resolve policy
  · OnMatchFound → NotifyMatchResolved
Match lobby / VS (short)
  · Rosters, ready flags, selected chefs
  · Owner starts when policy satisfied (or auto after timer)
Match Intro
  · Load map scene
  · INetSession.StartAsync:
        Host = match lobby owner PUID
        Role Host or Client
        INetTransportConfigurator → EOS P2P NGO transport (sample-aligned)
  · NotifyMatchIntroReady
Countdown → MatchRuntime
  · Gameplay via IGameplayInput
  · Connectivity match mode
Results
  · NotifyMatchCompleted
  · IWalletLedger.TryCredit match rewards (SESSION)
  · LeaveMatchLobby / INetSession.Stop
  · Party lobby retained
  → Home or RequestPlayAgain
```

### 7.3 Party vs match lobby

| | Party | Match |
|---|-------|-------|
| Lifetime | Home until leave/logout | MM success → match end |
| Members | Squad (≤ team size) | Full match roster |
| Leader | Party leader starts PLAY | Match owner is P2P host |
| After match | **Keep** | **Destroy** + stop net |

Solo PLAY = implicit party of 1.

### 7.4 P2P transport (EOS sample-aligned)

**References in repo (do not ship sample UI):**
- `EOSLobbyManager` / lobby member attributes / invites
- `EOSPeer2PeerManager` connection lifecycle
- `EOSTransport` + `EOSTransportManager` (P2P Netcode sample): NGO `NetworkTransport`, `ServerUserIdToConnectTo`, socket name pattern

**Binding rules:**
- `NetworkObjectPool` / `NetworkGameManager` use injected `NetworkManager`, **not** `NetworkManager.Singleton`
- Bots are network objects but **not** NGO player objects
- `PlayerController` registers with `PlayerNetworkManager` only when `NetworkObject.IsPlayerObject == true`
- Transport reset on `INetSession.Stop` and return Home

### 7.5 Failure matrix

| Failure | Behavior |
|---------|----------|
| Offline at boot | NoConnection side; retry loop |
| Offline at PLAY | Block queue; NoConnection or toast + disable |
| Auth fail | Login side |
| Session establish fail | No Home; retry/login |
| MM timeout | Always-resolve policy (bots) or cancel → Home |
| MM cancel | Home; party kept |
| Host leaves mid-match | `OnHostLost` → end match; no host migration v1 |
| Client disconnect | Reconnect window; else forfeit/remove |
| Wallet debit fail | Fail closed; no negative balance |
| IAP fail | No credit; user-visible error |
| RC fail | Cached/defaults; log |

### 7.6 Wallet / rewards

```
Match end → ResultsPhase / MatchRewardHandler (SESSION)
  → IWalletLedger.TryCredit(coins/gems, reason: match_reward, correlation: matchId)
  → IWalletStore.SaveAsync
  → UI reads IWallet balances
```

Shop/IAP:
```
Purchase success → ledger credit → save → UI refresh (event bus)
```

### 7.7 Input

- Menu: UI Toolkit focus / pointer; gameplay input **disabled**
- Match: `IGameplayInput.Enable` on countdown complete or runtime enter; Disable on results/exit
- Dual-stick mobile + keyboard/gamepad fallback (wiki controls)

---

## 8. UI shell redesign

### 8.1 Theme lock

| Asset | Role |
|-------|------|
| `Assets/_KitchenClash/UI/Styles/DesignSystem.uss` | **Source of truth** tokens + components language |
| `components.uss` | Shared component classes |
| Screen-specific USS | Layout only; import DesignSystem |
| `theme.uss` (“Hot Kitchen” dark) | **Do not** extend as parallel brand; deprecate/merge references over UI phase |

Visual language: Brawl Stars / Overcooked mashup — white canvas, Anton type, yellow primary, ink borders, mobile `:active` press, DOTween for motion.

### 8.2 In scope

- Redesign UXML structure for: Home (party + currencies + PLAY), Matchmaking, Match lobby/VS, Results, NoInternet, Settings entry points
- Extract **reusable component UXML**: `PartySlot`, `CurrencyBadge`, `PlayerChip`, `ModePill`, `PrimaryCTA`, `TopBar`, `SearchSpinner`, `RosterRow`, etc.
- Single template path (resolve `Resources/UI/Templates` vs `_KitchenClash/UI` duplication)
- Presenters/ViewModels bind ports only (`IWallet`, lobby, `IAppFlow`)

### 8.3 Out of scope

- New illustration set / full rebrand
- Hover-first desktop chrome
- Business logic in UXML

### 8.4 Screen map

| Screen | Phase |
|--------|-------|
| Splash, Login, NoInternet, Maintenance, ForceUpdate | Gates |
| Home | SESSION hub |
| Mode / map lightweight sheet | Pre-PLAY or under PLAY |
| Matchmaking | Search |
| MatchLobby / VS | Short ready |
| Match Intro, Countdown, HUD, Results | Match |
| Settings, Store, Friends, Profile, SeasonPass, … | SESSION overlays |

---

## 9. Module grade targets

| Module / vertical | Current (approx) | Target | Raise strategy |
|-------------------|------------------|--------|----------------|
| GameFlow | A | A | Keep; boot policy only |
| Shell (bus, log, connectivity) | A− | A | Connectivity boot + match hooks verified |
| UI.Toolkit | B+ | A− | Stable; screens consume |
| EOS auth | B | A− | Lifecycle hooks clean |
| Lobby/MM | C+ | A− | Sample-aligned, de-vendor DTOs, party/match laws |
| Net / P2P | C | A− | `INetSession` + EOSTransport bind |
| Economy/Wallet | C | A− | Ledger + store + SESSION ownership |
| Session DI | B− (fixed bare child) | A− | Installer law + `IPlayerSession` |
| IAP/Ads | D (stubs) | B | Real adapters when vendors ready; ports stable now |
| Analytics | C | B+ | Flow + economy reason codes |
| Settings | D | B+ | `ISettingsService` + UI |
| Input | C | A− | `IGameplayInput` dual-stick |
| Presentation purity | C | A− | Drop Infra references |

**Top-tier bar:** documented port, one primary adapter, EditMode tests for pure logic, no vendor types on port, DI scope correct, failure matrix covered.

---

## 10. Multi-game reuse model

1. **Now:** develop Playcenter in this repo; KitchenClash is first consumer.
2. **Second title:** copy or git-submodule `Assets/Playcenter`; game supplies Composition bindings + UXML theme tokens if needed (prefer shared DesignSystem patterns).
3. **API stability:** ports version by additive change; breaking port changes require spec revision.
4. **Optional assemblies:** title without EOS omits `Playcenter.EOS` and binds different auth/lobby adapters.
5. **No** requirement that every title use NGO P2P — only that `INetSession` be implementable for that title’s transport.

---

## 11. Migration strategy

### 11.1 Principles

- **Strangler:** new ports first; adapters wrap existing services; callers move; delete old types at phase gate.
- **No big-bang** weekend rewrite of all screens.
- **Each phase:** compile green + targeted EditMode tests + delete gate list.
- **Economy DI fix** (`ISessionScopeInstaller` / `MenuSessionRegistrations`) is **already in tree** and is the session-install pattern to standardize.

### 11.2 Phase outline (plan will detail tasks)

| Phase | Deliverable | Exit gate |
|-------|-------------|-----------|
| **P0** | Spec + plan approved | This doc + plan file |
| **P1** | Connectivity-first boot; session installer law documented/enforced; wallet ports + bridge over `IEconomyService` | Boot offline path works; login session has wallet; EditMode boot/session tests |
| **P2** | De-vendor lobby DTOs; party/match hardening; MM path through `IAppFlow` unchanged externally | No Epic types on Services DTOs; party survives match |
| **P3** | `INetSession` + EOSTransport configurator; intro starts host/client without Singleton | 2-device or host+client editor path |
| **P4** | UI shell redesign (components + Home/MM/Lobby/Results) on DesignSystem | Visual QA checklist; no theme.uss dependency for new screens |
| **P5** | `IGameplayInput` + settings service; HUD/settings bind | Match controls + settings persist |
| **P6** | IAP/ads/analytics raise; Presentation → Infra reference purge | Asmdef law; stubs or real adapters |
| **P7** | Wiki + PROJECT_MEMORY sync; optional submodule prep notes | Drift protocol clean |

Phases may split further in the implementation plan. **UI is in-program (P4), not infinite deferral.**

### 11.3 Delete gates (examples)

- After wallet migration: remove direct mint APIs not going through ledger.
- After net session: remove ad-hoc `NetworkManager.Singleton` start paths in match intro.
- After UI path unify: remove duplicate dead templates.
- After Presentation purity: remove `KitchenClash.Infrastructure` from Presentation asmdef.

---

## 12. Testing strategy

| Layer | What |
|-------|------|
| EditMode pure | Wallet ledger rules, lobby state machine fakes, net session state fakes, boot step ordering with fake connectivity |
| EditMode ports | SpyEventBus, fake `IConnectivityService`, fake installer |
| PlayMode / manual | EOS login, party invite, MM, P2P connect, disconnect, rewards |
| CI | Existing `dotnet test RecipeRage.Tests.EditMode.csproj`; do not add low-value clutter tests |

**Test naming:** `MethodName_Condition_ExpectedResult`.  
**No** production code clutter for one-off diagnostics.

---

## 13. Open decisions (resolved in brainstorm)

| Topic | Resolution |
|-------|------------|
| Greenfield vs evolve | Evolve (Approach A) |
| Mega client façade | No |
| Wallet in MATCH | No writes |
| Host migration | Not v1 |
| Connectivity boot order | First |
| UI redesign | In program, DesignSystem theme |
| theme.uss dual brand | No — DesignSystem only |
| KW submodules now | No — logical map only |
| UPM | No this program |

---

## 14. Risks

| Risk | Mitigation |
|------|------------|
| EOS sample code quality / license | Adapt patterns into our adapters; don’t ship sample UI scenes |
| Scope creep (full live ops) | Phases; stubs allowed for IAP/ads |
| Presentation still refs Infra | Explicit P6 gate |
| Dual economy APIs during migrate | Time-boxed façade; delete gate |
| P2P NAT failures | EOS P2P + clear user errors; forfeit path |
| Boot loops on flaky network | Debounce connectivity; manual retry CTA |

---

## 15. Success criteria

1. Cold boot offline → NoConnection before any auth/RC hard dependency failure spam.
2. Login → SESSION always contains economy/wallet (no `VContainerException: IEconomyService`).
3. Party invite → PLAY → MM → match lobby → P2P match → results credit → Home with party still present.
4. `INetSession` starts match without `NetworkManager.Singleton` in new code paths.
5. New/redesigned shell screens use DesignSystem only; reusable component UXML exist for party/currency/CTA.
6. Playcenter ports used by KC have zero Epic/NGO types on public API surfaces touched this program.
7. Second title could bind the same ports with different adapters without forking GameFlow.
8. Wiki/Technical reflects Client OS laws after P7.

---

## 16. Documentation updates (when implementing)

| Doc | Update |
|-----|--------|
| `wiki/Technical.md` | Boot order, scopes, net session, wallet laws |
| `wiki/LLM-Rules.md` | Forbidden: Singleton net start, MATCH wallet write, bare session child |
| `wiki/Gameplay.md` | Input dual-stick binding note if needed |
| `Documentation/Architecture/PROJECT_MEMORY.md` | Client OS pointer |
| This spec | Status → Implemented per phase |

Drift protocol: if implementation contradicts wiki, issue DRIFT WARNING per `wiki/DRIFT-PROTOCOL.md`.

---

## 17. References (code)

| Area | Path |
|------|------|
| Boot | `Assets/_KitchenClash/Infrastructure/Flow/Handlers/BootSequence.cs` |
| Session install | `ISessionScopeInstaller`, `MenuSessionRegistrations`, `SessionManager` |
| Economy | `Domain/Interfaces/IEconomyService.cs`, `Application/Services/EconomyService.cs` |
| Lobby/MM | `EOSLobbyService`, `EOSMatchmakingService` |
| Connectivity | `Playcenter/Shell/Runtime/Connectivity/IConnectivityService.cs` |
| Flow | `Playcenter/GameFlow/Runtime/Core/IAppFlow.cs` |
| EOS samples | `Assets/Samples/...` lobby, P2P, `EOSTransport` |
| UI theme | `Assets/_KitchenClash/UI/Styles/DesignSystem.uss` |
| Home party UI | `Assets/_KitchenClash/UI/Screens/HomeScreen.uxml` |

---

## 18. Approval

| Section | Status |
|---------|--------|
| §1–2 Problem / goals | Locked in brainstorm |
| §3–5 Architecture / approach | Locked |
| §6 Public API | Locked (names normative) |
| §7 Runtime flows (incl. connectivity-first, Brawl MP, P2P) | Locked revised §3 |
| §8 UI redesign on DesignSystem | Locked |
| §9–15 Grades, migration, tests, success | Locked with design write-up |

**Next:** User reviews this file → adjustments if needed → `writing-plans` produces `docs/superpowers/plans/2026-07-19-playcenter-client-os.md`.

---

*End of design spec.*
