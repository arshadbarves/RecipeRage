# Playcenter Studio SDK — Design

**Date:** 2026-07-20  
**Status:** Approved design (not implemented)  
**Branch:** `architecture-cleanup`  
**Codename / folder:** `Assets/Playcenter/**`  
**Public product name:** Playcenter Studio SDK  
**Public facade assembly:** `Playcenter.SDK`

**Related:**
- `docs/superpowers/specs/2026-07-19-playcenter-client-os-design.md` (implemented Client OS; boot story superseded here)
- `docs/superpowers/specs/2026-07-16-playcenter-shared-stack-design.md` (pure + Unity-thin DAG)
- `docs/superpowers/specs/2026-07-14-architecture-hardening-design.md`
- Session ownership lock (shipped): CreateSession-only install; Menu/Match parent Root
- Wiki (post-implement): `wiki/Technical.md`, `wiki/LLM-Rules.md`

**Supersedes / keeps:**
- **Supersedes** Client OS cold boot via KitchenClash `BootSequence` and `IAppFlow.StartColdBoot()` as the init path
- **Keeps** Client OS ports and product laws: wallet ledger, net session, party vs match lobby, session ownership, DesignSystem visual language, `IAppFlow` as post-ready navigator
- **Adds** facade SDK, SDK-owned DI (no VContainer), module host + loading shell, vendor firewall, SDK common screens, AI skill

**Cutover policy:** AAA complete replacement. No dual-path boot, no obsolete shims, no feature flags that keep legacy boot alive after its wave delete gate.

---

## 1. Problem

RecipeRage needs a **studio-grade multi-game client SDK** (Brawl Stars / PUBG Mobile / Fortnite client-shell class), not a KitchenClash-shaped boot script:

| Need | Today |
|------|--------|
| Plug-and-play modules any title can host | Logic buried in `BootSequence` + Root VContainer |
| Init with loading screen, then start-game handoff | Boot mixed with AppFlow; no clean `OnReady` |
| SDK modules must not use VContainer | Playcenter still couples to game DI patterns |
| Common screens (splash, loading, settings, gates) in SDK, themable | Duplicated under KitchenClash + Resources |
| Full vendor isolation (EOS and others) | Partial wraps; Epic/NGO still leak toward game shell |
| AI skill so agents/engineers integrate correctly | Missing |
| Multi-game in one Unity project | Composition + scenes swap possible but boot not portable |
| Hard replace, no legacy | Prior programs left parallel paths risk |

**Non-goals of this program:** cooking IP, dedicated servers, UPM publish, replacing post-ready `IAppFlow`, host migration, full IAP/ads backends.

---

## 2. Goals and non-goals

### 2.1 Goals

1. **Playcenter Studio SDK** — public facade `Playcenter.SDK`; internal modules under `Assets/Playcenter/**`.
2. **Builder + ServiceRegistry DI** inside the SDK — zero third-party VContainer references under Playcenter.
3. **Ordered module pipeline** with weighted **Loading** progress and terminal gate screens.
4. **Game handoff** via `IGameEntry.OnPlaycenterReady` / `OnPlaycenterFailed` — SDK never calls `IAppFlow` internally.
5. **SDK-owned shell screens** — Splash, Loading, NoConnection, ForceUpdate, Maintenance, Settings — themable per title.
6. **Vendor firewall** — gameplay and game shell depend on ports only; vendor bumps update adapter layer.
7. **Full boot cutover** — delete `BootSequence` and cold-boot-via-AppFlow; no backward compatibility layer.
8. **AI skill** documenting integrate / extend / forbidden patterns.
9. **Preserve** session ownership lock and Client OS ports already shipped.

### 2.2 Non-goals

| Out of scope | Why |
|--------------|-----|
| UPM / second git repo packaging | Premature before second live title |
| Cooking, chefs, recipes, station NetBehaviours | Game IP |
| Replacing `IAppFlow` for Home/MM/Match/Results | Already production navigator post-ready |
| Dedicated servers / host migration v1 | P2P; reconnect window then forfeit |
| Full IAP store backend / ads mediation | Ports + stubs; real vendors when keys exist |
| Dual visual brands | One theme per title; RecipeRage keeps DesignSystem language |
| Migrating all game Composition off VContainer in v1 | Game may keep VContainer for **game IP only**; must bridge SDK ports, not re-host SDK singletons |

---

## 3. Locked assumptions

| # | Assumption |
|---|------------|
| S1 | **Facade:** Games reference **`Playcenter.SDK` only** for shell. Internal asmdefs are not public API. |
| S2 | **No VContainer in SDK:** SDK DI = Builder + `IServiceRegistry`. Zero `VContainer` refs in `Assets/Playcenter/**`. |
| S3 | **Game may keep VContainer** for game services only; resolves SDK ports from `PlaycenterClient.Services`. |
| S4 | **Vendor firewall:** `Epic.*`, raw NGO shell setup, store SDKs only in adapter assemblies. |
| S5 | **Module init:** Ordered `IPlaycenterModule.InitializeAsync` + weighted LoadingScreen. No interactive login mid-bar. |
| S6 | **Handoff:** Success → `IGameEntry.OnPlaycenterReady(client)`. Fail → `OnPlaycenterFailed(error)`. |
| S7 | **Shell screens in SDK:** Splash, Loading, Settings, NoConnection, ForceUpdate, Maintenance — themable. |
| S8 | **Full boot cutover:** `BootSequence` deleted after module port; no dual boot. |
| S9 | **Session ownership unchanged:** SESSION child only via `CreateSession` + installer; Menu/Match parent Root. |
| S10 | **Multi-game:** same Unity project; swap Composition + scenes + `IGameEntry` + theme. |
| S11 | **Stack (game):** Unity 6 + UniTask + UI Toolkit + NGO + EOS adapters; game may use VContainer outside Playcenter. |
| S12 | **Post-ready navigator:** `IAppFlow` remains sole product navigator after ready. |
| S13 | **Hard cutover:** no legacy shims or parallel “old boot” flags after each wave’s delete gate. |
| S14 | **DesignSystem** tokens/USS remain RecipeRage theme input into SDK theming (not a second brand). |

---

## 4. Approaches considered

### Approach A — Facade SDK + ModuleHost + ServiceRegistry (**chosen**)

New public `Playcenter.SDK` assembly. SDK-owned registry DI (no VContainer). Ordered modules replace BootSequence. SDK shell UI pack. Game implements `IGameEntry`. Hard delete legacy boot.

| Pros | Cons |
|------|------|
| AAA-familiar client SDK shape | Multi-wave migration |
| True plug-and-play across titles | Game must bridge ports once |
| Matches “no VContainer in modules” | |
| Clean vendor firewall boundary | |

### Approach B — Keep BootSequence; extract steps only

Move steps to helpers but keep KitchenClash boot + VContainer Root as host.

| Pros | Cons |
|------|------|
| Smaller diff | Not multi-game SDK |
| | Still VContainer-centric |
| | Rejected: user required complete replacement |

### Approach C — Full custom IoC with Root/Session/Match scopes inside SDK

Mirror VContainer tree inside SDK.

| Pros | Cons |
|------|------|
| Familiar scope names | Overbuilt for shell; fights S9 game session ownership |
| | Rejected: user chose Builder + ServiceRegistry |

**Decision:** Approach A. SDK DI style locked: **Builder + ServiceRegistry**.

---

## 5. Architecture

### 5.1 Layer diagram

```
┌─────────────────────────────────────────────────────────┐
│  GAME (RecipeRage / future title)                       │
│  IGameEntry · game screens · match IP                   │
│  optional VContainer for GAME only                      │
└──────────────────────────▲──────────────────────────────┘
                           │ ports only
┌──────────────────────────┴──────────────────────────────┐
│  Playcenter.SDK  (PUBLIC FACADE)                        │
│  PlaycenterClient · ClientOptions · IServiceRegistry    │
│  IPlaycenterModule · ModuleContext · IBootProgress      │
│  IGameEntry · BootFailure · IShellUi · IShellTheme      │
└──────────────────────────▲──────────────────────────────┘
                           │ internal
┌──────────────────────────┴──────────────────────────────┐
│  SDK RUNTIME (no VContainer)                            │
│  ServiceRegistry · ModuleHost · BootProgress · ShellUi  │
│  Playcenter.Shell / GameFlow / Services / UI*           │
└──────────────────────────▲──────────────────────────────┘
                           │ adapters only
┌──────────────────────────┴──────────────────────────────┐
│  VENDOR FIREWALL                                        │
│  Playcenter.EOS · future Ads/IAP · NGO net adapters     │
└─────────────────────────────────────────────────────────┘
```

### 5.2 Public facade types (`Playcenter.SDK`)

| Type | Role |
|------|------|
| `PlaycenterClient` | Process-wide SDK host; create once at app start |
| `ClientOptions` | Builder config: modules, theme, logging, game entry |
| `IServiceRegistry` | Register: `AddSingleton<TInt, TImpl>()`, etc. |
| `IServiceProvider` (or registry resolve API) | `Get<T>()` / `TryGet<T>()` |
| `IPlaycenterModule` | `Id`, `Weight`, `InitializeAsync(ModuleContext, ct)` |
| `ModuleContext` | Registry + progress + CT + logger |
| `IBootProgress` | `Report(moduleId, 0–1)`; weighted overall progress |
| `IGameEntry` | `OnPlaycenterReady(PlaycenterClient)`, `OnPlaycenterFailed(BootFailure)` |
| `BootFailure` | `Code` + message + optional retry metadata |
| `BootFailureCode` | `Offline`, `ForceUpdate`, `Maintenance`, `RemoteConfig`, `Cancelled`, `Unknown` |
| `IShellUi` | Show/hide SDK screens by id; apply theme |
| `ShellScreenId` | `Splash`, `Loading`, `Settings`, `NoConnection`, `ForceUpdate`, `Maintenance` |
| `IShellTheme` | Tokens + optional USS override resource path |

### 5.3 Entry snippet (normative)

```csharp
var client = PlaycenterClient.Create(o =>
{
    o.UseDefaultModules();
    o.Theme.FromResources("UI/Themes/DesignSystem");
    o.SetGameEntry(new RecipeRageGameEntry());
});
await client.RunAsync(destroyCancellationToken);
```

`RunAsync` = show shell → run modules → ready/fail handoff.  
**Forbidden after cutover:** `IAppFlow.StartColdBoot()` as init; any remaining `BootSequence`.

### 5.4 Default module pack

| Order | Module id | Weight | Responsibility |
|------:|-----------|-------:|----------------|
| 1 | `logging` | 5% | Logger / GameLogger bind |
| 2 | `connectivity` | 15% | Online gate |
| 3 | `ntp` | 10% | Clock sync |
| 4 | `remote_config` | 15% | RC fetch |
| 5 | `force_update` | 10% | Version gate |
| 6 | `maintenance` | 10% | Maintenance gate |
| 7 | `auth_warmup` | 15% | Platform/auth SDK warm only — **not** interactive login |
| 8 | `analytics` | 10% | Analytics init |
| 9 | `shell_ready` | 10% | Shell UI bind + theme apply |

Interactive login, `CreateSession`, and Home occur **after** `OnPlaycenterReady`, owned by the game.

### 5.5 Boot timeline

**Happy path**

1. Unity starts → thin bootstrap constructs `PlaycenterClient` via builder.  
2. SDK shows Splash (optional short) → Loading.  
3. Modules run in order; progress bar advances by weight.  
4. Ready → hide Loading → `IGameEntry.OnPlaycenterReady(client)`.  
5. Game: auth UI if needed → `CreateSession` + installer → `IAppFlow` → Home.

**Failure path**

1. Module fails → map to `BootFailureCode`.  
2. `IShellUi` shows NoConnection / ForceUpdate / Maintenance.  
3. Retry → `ModuleHost` retries from failed module (or from connectivity when policy says full restart).  
4. Quit / abandon → `OnPlaycenterFailed`.

### 5.6 Game Composition after cutover

```
Unity Awake
  → PlaycenterClient.Create(...).RunAsync()
  → OnPlaycenterReady(client)
       → optional VContainer Root for game IP only
       → bridge: resolve IAuthService, IEventBus, … from client.Services
       → auth side phase if needed (game Login screen)
       → CreateSession + ISessionScopeInstaller
       → IAppFlow → Home
```

**Law:** Game VContainer must **not** register a second copy of SDK singletons. Bridge = resolve from registry or thin facades that forward to it.

### 5.7 Session ownership (unchanged, still law)

- Root (DDOL) holds process gateways and cold primitives as today where still required by game.  
- SESSION services install **only** via `CreateSession` + installer.  
- Menu/Match lifetime scopes parent Root and must **not** orphan-install session graphs without parent bus/ports.  
- MATCH never owns wallet writes (Client OS ledger law).

---

## 6. Shell screens and theming

### 6.1 Ownership

| Screen | Owner |
|--------|--------|
| Splash, Loading, NoConnection, ForceUpdate, Maintenance, Settings | **SDK** |
| Login, Home, Party, MM, Match lobby/VS, HUD, Results, Store, Chefs, … | **Game** |

### 6.2 Pack layout (target)

```
Assets/Playcenter/SDK/   (facade + host)
Assets/Playcenter/...    (existing Shell/Services/GameFlow/UI*/EOS)
  UI/Shell/
    Screens/*.uxml
    Styles/shell_*.uss
    Themes/DefaultShell.uss
```

Exact folder names may follow existing Playcenter asmdef layout; **public types** live in `Playcenter.SDK`.

### 6.3 Theming (`IShellTheme`)

| Layer | Content |
|-------|---------|
| Tokens | bg, surface, accent, danger, text, radii, spacing, fonts |
| Base USS | SDK structure + `pc-*` components |
| Game USS | Optional override (e.g. DesignSystem.uss) loaded after base |
| Assets | Logo path, optional splash media; store URLs from config |

Rules:

- Games style via tokens/overrides; do not fork SDK UXML per title.  
- One visual system per title at runtime.  
- UITK fixes (e.g. `cursor: arrow`) live in SDK base theme.

### 6.4 Loading contract

- Weighted overall 0–100% from module weights.  
- Status line = localized module label.  
- Optional cancel → `BootFailureCode.Cancelled`.  
- Minimum display floor (~300–500ms) to avoid flicker.  
- **No** interactive auth on Loading.

### 6.5 Gate screens

| Screen | Actions | Behavior |
|--------|---------|----------|
| NoConnection | Retry, Quit | Retry re-runs from connectivity (or failed module per host policy) |
| ForceUpdate | Update, Quit | Opens store URL from RC; no bypass |
| Maintenance | Retry, Quit | Re-fetch maintenance; message from RC |

Runtime disconnect after ready may reuse the same NoConnection implementation via `IShellUi` or a game toast — **one** UXML implementation.

### 6.6 Settings (SDK)

v1 sections via ports:

- Audio → `ISettingsService`  
- Language (if locale pack exists)  
- Account hooks via `IAuthService` (logout / platform overlay)  
- Legal URLs from config  
- Build version (read-only)

Game opens settings: `client.Shell.Show(ShellScreenId.Settings)`.  
Optional `ISettingsSectionProvider` is **v1.1**, not required for v1.

### 6.7 UI stacks

| Concern | Owner |
|---------|--------|
| Boot shell (splash/loading/gates/settings) | SDK `IShellUi` — works with **zero** game VContainer |
| Post-ready product stack | Game `IUIService` / existing Toolkit stack |
| Shared widgets | SDK `pc-*`; game may compose in game UXML |

### 6.8 UI delete gate

After move: delete KitchenClash Splash/Loading/NoInternet/Maintenance/Settings screen classes and duplicate templates that only served shell/boot. Keep Login, Home, match, store, chefs in game.

---

## 7. Vendor firewall

| Vertical | Port (game sees) | Adapter | v1 requirement |
|----------|------------------|---------|----------------|
| Auth | `IAuthService` | `Playcenter.EOS` | Full wrap |
| Connectivity | `IConnectivityService` | SDK Unity-thin impl | Full |
| Remote config | `IRemoteConfigService` | EOS/RC or file adapter | Full |
| Analytics | `IAnalyticsService` | no-op + real adapter | Full port |
| Lobby / MM / P2P | `INetSession` / lobby ports | EOS + NGO adapters | Ports mandatory; raw Epic/NGO out of game shell code |
| Wallet | `IWallet` / ledger / store | game or EOS PDS | Keep ledger seam |

**Upgrade path:** vendor SDK bump → update adapter assembly + release notes → games keep compiling against ports.

**Grep gate (W5):** no `Epic.` usings in game Presentation/Application.

---

## 8. Error model

| Code | When | UX |
|------|------|-----|
| `Offline` | connectivity module | NoConnection + Retry |
| `ForceUpdate` | version gate | ForceUpdate + store |
| `Maintenance` | maintenance flag | Maintenance + Retry |
| `RemoteConfig` | RC hard fail | Retry-oriented gate |
| `Cancelled` | user cancel | Quit / leave boot |
| `Unknown` | uncaught | Log + Retry/Quit |

No silent continue past force-update or maintenance.

---

## 9. Testing strategy

| Layer | What | How |
|-------|------|-----|
| Registry | register/resolve/replace | EditMode pure tests |
| ModuleHost | order, weights, cancel, fail-fast, retry-from | Fake modules + progress spy |
| BootFailure mapping | result/exception → code | Unit |
| Progress math | weighted % | Unit (no UITK) |
| Game entry handoff | ready/fail once | Fake `IGameEntry` |
| Vendor adapters | ports with fakes | No Epic in CI unit |
| Cutover | zero `BootSequence` references | Compile + grep gate |
| Session law | CreateSession-only install | Keep ownership tests |

Optional PlayMode smoke: fake modules → ready → entry called.

---

## 10. AI skill deliverable

**Path:** `.github/skills/playcenter-sdk/SKILL.md` (mirror to repo agent skills path if both are used).

**Must include:**

1. When to use (new game, module, shell, vendor swap)  
2. Glossary (Client, Module, Registry, Shell, GameEntry, BootFailure)  
3. Integrate a game (builder, `IGameEntry`, theme, session after ready)  
4. Add a module (id, weight, order, failure codes)  
5. FORBIDDEN: VContainer in Playcenter; Epic/NGO in game shell; orphan session DI; dual boot; copying SDK UXML into game  
6. Delete gates per wave  
7. Pointers to this spec + wiki laws after implementation  

---

## 11. Migration waves

| Wave | Ship | Delete gate |
|------|------|-------------|
| **W0** | This spec + implementation plan + skill stub | — |
| **W1** | `Playcenter.SDK` facade, `ServiceRegistry`, `ModuleHost`, progress | — |
| **W2** | Default modules from BootSequence logic; Loading/Splash shell; bootstrap = `RunAsync` | **Delete `BootSequence`**; no `StartColdBoot` init |
| **W3** | Gate screens + Settings in SDK; theme tokens | **Delete** game Splash/Loading/NoInternet/Maintenance/Settings duplicates |
| **W4** | Strip all VContainer refs from `Assets/Playcenter/**`; game bridges ports | Grep: no `VContainer` under Playcenter |
| **W5** | Vendor firewall audit; wiki + skill final | No Epic in game Presentation/Application |
| **W6** | RecipeRage `IGameEntry` polish; auth/session after ready stable | Confirm zero legacy boot symbols |

No feature flag keeps old boot alive past W2.

---

## 12. Success criteria

- New title: `IGameEntry` + theme + Composition/scenes → boot works without copying KitchenClash boot code.  
- Loading bar reflects real module weights.  
- Force update / maintenance / offline never reach Home.  
- Zero VContainer inside `Assets/Playcenter/**`.  
- Zero `BootSequence`.  
- Game shell depends on ports, not Epic.  
- AI skill sufficient to add a module without reading all of KitchenClash.  
- Session CreateSession ownership laws still hold.

---

## 13. Delete list (normative after cutover)

| Remove | Replacement |
|--------|-------------|
| `BootSequence` (+ boot-only tests tied solely to it) | Default modules + ModuleHost |
| `GameBootstrapper` → `IAppFlow.StartColdBoot()` init path | `PlaycenterClient.RunAsync` |
| Dual registration of SDK services into VContainer “for modules” | `PlaycenterClient.Services` |
| Any `VContainer` reference under `Assets/Playcenter/**` | ServiceRegistry |
| Parallel old-boot flags | Hard cutover |
| Game duplicate Splash/Loading/NoInternet/Maintenance/Settings shell screens | SDK shell pack |

**Keep:** `IAppFlow` post-ready; session ownership; DesignSystem as RecipeRage theme input; vendor adapters behind ports; Client OS wallet/net/party ports.

---

## 14. Glossary

| Term | Meaning |
|------|---------|
| **Playcenter** | Folder/codename for the studio client stack |
| **Studio SDK** | Product name for the reusable client SDK |
| **PlaycenterClient** | Public host entry; owns registry, modules, shell |
| **Module** | One ordered boot capability (`IPlaycenterModule`) |
| **Registry** | SDK DI container (`IServiceRegistry`) |
| **Shell** | SDK-owned boot/settings/gate UI |
| **GameEntry** | Title callback after SDK ready/fail |
| **Vendor firewall** | Adapters isolate third-party SDKs from game code |
| **MM** | Matchmaking (party id ≠ match id) |

---

## 15. Open items resolved in design

| Question | Resolution |
|----------|------------|
| SDK DI style | Builder + ServiceRegistry (not manual-only, not full scope IoC) |
| VContainer in SDK | Forbidden |
| VContainer in game | Allowed for game IP; bridge SDK ports |
| Login during loading bar | No — after ready |
| Boot cutover | Complete replacement; no legacy |
| Common screens | Inside SDK, themable |
| AI skill | Required deliverable |
| Facade name | `Playcenter.SDK` / `PlaycenterClient` |

---

## 16. Implementation next step

After user reviews this committed spec:

1. Invoke **writing-plans** → `docs/superpowers/plans/2026-07-20-playcenter-studio-sdk.md`  
2. Execute waves W1–W6 with delete gates  
3. Update wiki laws when implementation lands  

**Do not implement until the implementation plan exists and execution is explicitly started.**
