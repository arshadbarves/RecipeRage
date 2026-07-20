---
name: playcenter-sdk
description: Integrate and extend Playcenter Studio SDK (modules, shell, vendor firewall). Auto-loads when touching Assets/Playcenter, boot sequences, IGameEntry, IShellUi, or module init.
---

# Playcenter Studio SDK

Spec: `docs/superpowers/specs/2026-07-20-playcenter-studio-sdk-design.md`  
Wiki: `wiki/Technical.md` § Playcenter Studio SDK  
Laws: `wiki/LLM-Rules.md` § Playcenter Studio SDK

---

## When to use

Invoke this skill when you are:

- Integrating a new game title into the SDK (new `IGameEntry` + Composition wiring)
- Adding or modifying a boot **module** (`IPlaycenterModule`)
- Working with SDK **shell screens** (Splash, Loading, NoConnection, ForceUpdate, Maintenance, Settings)
- Auditing **vendor firewall** compliance (Epic / NGO in Presentation / Application)
- Touching `PlaycenterClient`, `IShellUi`, `IShellTheme`, or `IBootProgress`
- Deleting legacy boot artefacts (`BootSequence`, `GameBootstrapper`, cold-boot `IAppFlow`)

Do **not** invoke for post-ready product navigation (Home, Matchmaking, Match) — that is `IAppFlow` (see `wiki/GameFlow-SDK.md`).

---

## Glossary

| Term | Meaning |
|------|---------|
| **PlaycenterClient** | Process-wide SDK host. Owns registry, modules, shell. Created once at app start. |
| **Module** | One ordered boot capability implementing `IPlaycenterModule`. Has an `Id`, `Weight` (0–100 relative %), and `InitializeAsync`. |
| **Registry** | SDK DI container (`IServiceRegistry`). `AddSingleton<TInt,TImpl>()`, `Get<T>()`, `TryGet<T>()`. No VContainer inside. |
| **Shell** | SDK-owned boot / gate / settings UI. Shown via `IShellUi`. UXML lives in `Assets/Playcenter/SDK.Unity/Resources/UI/Shell`. |
| **GameEntry** | Title callback. `OnPlaycenterReady(PlaycenterClient)` = happy path; `OnPlaycenterFailed(BootFailure)` = terminal fail. |
| **BootFailure** | `Code` (`Offline`, `ForceUpdate`, `Maintenance`, `RemoteConfig`, `Cancelled`, `Unknown`) + message + optional retry metadata. |
| **ShellRef / BootRetryRef** | Thin holder registered in `RootLifetimeScope` that breaks the AppFlow↔bootstrap DI cycle. |
| **Vendor firewall** | Adapters isolate `Epic.*`, NGO setup, store SDKs from game Presentation / Application. |
| **IAppFlow** | Post-ready navigator only. SDK never calls it internally. |

---

## Integrate a game

### 1. Bootstrap (Composition/PlaycenterSdkBootstrap.cs)

```csharp
// IStartable — called by VContainer Root; replaces legacy GameBootstrapper
public sealed class PlaycenterSdkBootstrap : IStartable
{
    public void Start()
    {
        var client = PlaycenterClient.Create(o =>
        {
            o.UseDefaultModules();                               // logging → shell_ready
            o.Theme.FromResources("UI/Themes/DesignSystem");    // RecipeRage theme overlay
            o.SetGameEntry(new RecipeRageGameEntry());
        });
        _ = client.RunAsync(destroyCancellationToken);
    }
}
```

Register in `RootLifetimeScope`:
```csharp
builder.RegisterEntryPoint<PlaycenterSdkBootstrap>();
```

### 2. Game entry (Composition/RecipeRageGameEntry.cs)

```csharp
public sealed class RecipeRageGameEntry : IGameEntry
{
    public async UniTask OnPlaycenterReadyAsync(PlaycenterClient client)
    {
        // optional auth side phase
        // CreateSession + ISessionScopeInstaller
        // IAppFlow → Home
    }

    public void OnPlaycenterFailed(BootFailure failure)
    {
        // log and optionally quit
    }
}
```

### 3. Session after ready

```
OnPlaycenterReady
  → auth side phase (game Login screen) if needed
  → SessionManager.CreateSession(ISessionScopeInstaller)   ← session DI law unchanged
  → IAppFlow → Home
```

Do **not** register SDK singletons a second time in VContainer. Bridge via `client.Services.Get<T>()`.

### 4. Provide IShellUi / IBootRetry to game systems

```csharp
// RootLifetimeScope
builder.RegisterInstance(new ShellRef());     // game systems inject ShellRef, read .Value
builder.RegisterInstance(new BootRetryRef()); // same for IPlaycenterBootRetry
```
The bootstrap sets `.Value` after `PlaycenterClient` is created, breaking the DI cycle.

---

## Add a module

```csharp
public sealed class MyModule : IPlaycenterModule
{
    public string Id     => "my_module";
    public float  Weight => 10f;           // percentage share of loading bar

    public async Task InitializeAsync(ModuleContext ctx, CancellationToken ct)
    {
        ctx.Progress.Report(Id, 0f);
        // ... work ...
        ctx.Progress.Report(Id, 1f);
    }
}
```

Register in builder:
```csharp
o.AddModule(new MyModule());
```

**Module order** (default pack):

| # | Id | Weight |
|--:|-----|-------:|
| 1 | `logging` | 5% |
| 2 | `connectivity` | 15% |
| 3 | `ntp` | 10% |
| 4 | `remote_config` | 15% |
| 5 | `force_update` | 10% |
| 6 | `maintenance` | 10% |
| 7 | `auth_warmup` | 15% |
| 8 | `analytics` | 10% |
| 9 | `shell_ready` | 10% |

Insert before `shell_ready`. Interactive login happens **after** `OnPlaycenterReady` — never inside a module.

**Failure mapping:**

| Condition | Throw / return | Shell shown |
|-----------|---------------|-------------|
| No internet | `BootFailureCode.Offline` | NoConnection + Retry |
| Force update required | `BootFailureCode.ForceUpdate` | ForceUpdate + store URL |
| Maintenance flag | `BootFailureCode.Maintenance` | Maintenance + Retry |
| RC hard fail | `BootFailureCode.RemoteConfig` | Retry gate |
| User cancel | `BootFailureCode.Cancelled` | (quit) |
| Any other | `BootFailureCode.Unknown` | Log + Retry/Quit |

---

## FORBIDDEN

| Forbidden | Why |
|-----------|-----|
| `using VContainer` inside `Assets/Playcenter/**` | SDK DI = Builder + ServiceRegistry (S2). Grep gate W4. |
| `Epic.*` / `EOS.*` / raw NGO in game **Presentation** or **Application** | Vendor firewall (S4). Grep gate W5. Use ports: `IAuthService`, `INetSession`, etc. |
| Orphan session DI (bare `CreateChild` without installer) | Missing economy/wallet/net → VContainerException (session DI law). |
| Dual boot / keeping `BootSequence` alive | Hard cutover (S8/S13). No feature flag parallel boot. |
| Copying SDK UXML (`Shell/Screens/*.uxml`) into the game | Game themes via tokens/USS override; never fork per-title UXML. |
| `IAppFlow.StartColdBoot()` as init path | Replaced by `PlaycenterClient.RunAsync`. |
| Interactive login inside a module | Login occurs **after** `OnPlaycenterReady`; modules only warm auth SDK. |
| Re-registering SDK singletons in VContainer | Bridge via `client.Services.Get<T>()` only. |

---

## Delete gates

| Wave | What to delete | Verify with |
|------|---------------|-------------|
| W2 | `BootSequence` class + boot-only tests tied solely to it; `IAppFlow.StartColdBoot()` init path | `rg -n "BootSequence" Assets --glob '*.cs'` → 0 code hits (comments OK) |
| W3 | Game duplicate Splash / Loading / NoInternet / Maintenance / Settings shell screen classes and UXML that only served boot | Review `Assets/_KitchenClash/Presentation/Screens/` — keep Login, Home, Match, Store, Chefs |
| W4 | All `using VContainer` in `Assets/Playcenter/**` | `rg -n "using VContainer" Assets/Playcenter --glob '*.cs'` → 0 |
| W5 | Any `Epic.` / `EOS.` usings in `Assets/_KitchenClash/Presentation` or `.../Application` | `rg -n "using Epic\.\|Epic\.OnlineServices" Assets/_KitchenClash/Presentation Assets/_KitchenClash/Application --glob '*.cs'` → 0 |
| W6 | Confirm zero legacy boot symbols; `ShellRef.Value != null` at app start | Compile + manual smoke |

---

## See also

- Spec: `docs/superpowers/specs/2026-07-20-playcenter-studio-sdk-design.md`
- Client OS spec: `docs/superpowers/specs/2026-07-19-playcenter-client-os-design.md`
- `wiki/Technical.md` § Playcenter Studio SDK
- `wiki/LLM-Rules.md` § Playcenter Studio SDK
- `wiki/GameFlow-SDK.md` — post-ready `IAppFlow` navigator
