# Playcenter.UI

Engine-free **UI stack contracts** for multi-title Brawl shells.

## Purpose

- Type-based screen navigation (root/push/system/overlay/modal/popup/HUD)
- Toast/notification surface
- Screen category enum for layering

## Rules

1. `noEngineReferences` — no UI Toolkit / Unity / VContainer / UniTask types.
2. Async methods return `Task`.
3. `SetCurrentScope(object)` — game casts to its DI resolver.
4. GameFlow/Shell/Services do not reference UI.
5. Adapters (`UIService`, screens) stay in KitchenClash.Presentation.

## Layout

```
Runtime/
  Playcenter.UI.asmdef
  IUIService.cs
  NotificationType.cs
  UIScreenCategory.cs
```
