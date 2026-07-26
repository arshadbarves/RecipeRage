# Playcenter.Services Extract — Design

**Date:** 2026-07-14  
**Branch:** `architecture-cleanup`  
**Status:** Shipped (module + hard cutover)  
**Related:** `Assets/Playcenter/Services/`, `Assets/Playcenter/Shell/`, `Assets/Playcenter/GameFlow/`, `wiki/Technical.md`

---

## 1. Problem

After GameFlow and Shell, multi-title **service contracts** still lived in `KitchenClash.Domain` / `Application`:

| Type | Was | Problem |
|------|-----|---------|
| `IConfigService`, `IConfigModel`, `ConfigHealthStatus` | Domain | Second title cannot reuse without KitchenClash Domain |
| `IAnalyticsService` | Domain | Portable analytics port buried in game Domain |
| `IAdsService`, `AdRewardResult` | Domain | Same |
| `IIAPService`, `IAPResult` | Domain | Same |
| `IAuthService`, `AuthResult` | Application / Domain | Auth port split across layers |
| `IEncryptionService`, `IMaintenanceService` | Domain | Generic product ports |

Leaf assemblies (Configuration, EOS, …) are **KitchenClash** walls, not portable Playcenter modules.

**Goal:** Extract engine-free **Playcenter.Services** contracts so another Brawl-class title can depend on them without KitchenClash. Full cutover — **delete** originals; **no** dual namespaces, aliases, or legacy fallbacks.

---

## 2. Locked decisions

1. **Scope:** Contracts only — config, analytics, ads, IAP, auth, encryption, maintenance (+ result DTOs).
2. **Not in module:** Audio (`AudioClip`), UI Toolkit stack, save/storage with game DTOs, Platform/Async Unity helpers, UniTask remote-config orchestration (`IRemoteConfigService` / `IConfigProvider` stay Application and may compose Services types), cooking/economy/EOS/NGO/bots.
3. **Pattern:** Same as GameFlow/Shell — `noEngineReferences`, zero KitchenClash refs; adapters stay in KitchenClash Infrastructure.
4. **Hard cutover:** No shims, type aliases, obsolete wrappers, dual APIs.
5. **Independence:** `Playcenter.GameFlow` and `Playcenter.Shell` do **not** reference Services.
6. **Domain may reference Services** — cooking Domain keeps game models; portable ports leave Domain.

---

## 3. Target layout

```
Assets/Playcenter/
  GameFlow/     (unchanged — zero Services refs)
  Shell/        (unchanged — zero Services refs)
  Services/     (NEW)
    Runtime/
      Playcenter.Services.asmdef   # noEngineReferences: true, references: []
      Config/       IConfigService, IConfigModel, ConfigHealthStatus
      Analytics/    IAnalyticsService
      Ads/          IAdsService, AdRewardResult
      IAP/          IIAPService, IAPResult
      Auth/         IAuthService, AuthResult
      Encryption/   IEncryptionService
      Maintenance/  IMaintenanceService
    README.md

Assets/_KitchenClash/
  Domain/          # DELETE moved interfaces/enums/models; asmdef refs Playcenter.Services
  Application/     # DELETE IAuthService; asmdef refs Playcenter.Services
  Infrastructure/* # implement Playcenter.Services ports (Firebase, EOS, stubs)
  Composition/     # usings → Playcenter.Services; DI shape unchanged
```

---

## 4. Hard cutover checklist

- [x] Module + README + asmdef committed (`3de46c19`, `ae71aa67`)
- [x] Domain/Application originals deleted
- [x] Consumers: `using Playcenter.Services`
- [x] Asmdefs reference Services (Domain → tests)
- [x] CLI builds: Domain → Application → Infrastructure leaves → Composition → Editor
- [x] Gates: no dual aliases; GameFlow/Shell zero Services refs
- [x] Hard cutover commit (`06006678`)

---

## 5. What stays game-side

| Area | Why |
|------|-----|
| `IRemoteConfigService` / `IConfigProvider` | UniTask + Firebase orchestration |
| Audio / UI / Platform / Async | Unity-bound |
| SaveService + game DTOs | Title-specific persistence shape |
| EOS/Firebase concrete adapters | Backend choice per title |
| Cooking / economy / match | Game IP |

---

## 6. Verification commands

```bash
dotnet build Playcenter.Services.csproj -nologo   # or Temp/Bin DLL for CLI
dotnet build KitchenClash.Domain.csproj -nologo
dotnet build KitchenClash.Application.csproj -nologo
dotnet build KitchenClash.Infrastructure.csproj -nologo
dotnet build KitchenClash.Composition.csproj -nologo
dotnet build RecipeRage.Editor.csproj -nologo

# Gates
rg -n 'Playcenter\.Services' Assets/Playcenter/GameFlow Assets/Playcenter/Shell  # expect empty
test ! -f Assets/_KitchenClash/Domain/Interfaces/IConfigService.cs
test ! -f Assets/_KitchenClash/Application/Interfaces/IAuthService.cs
```

Unity regenerates `.csproj` from asmdefs; CLI may need a local Services DLL `Reference` until Unity refresh (csproj is gitignored).
