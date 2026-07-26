# Playcenter.UI.Toolkit

**Tier:** Unity-thin (engine + DOTween + UniTask allowed; VContainer not allowed)

Unity UI Toolkit screen host for multi-title reuse. Contains the shared implementation of the UI screen stack: `UIService`, `BaseUIScreen`, controllers, registry, transitions, and factory ports.

## DAG position

```
Playcenter.UI.Toolkit
  → Playcenter.UI      (IUIService, IUIScreenStackManager, UIScreenCategory, NotificationType)
  → Playcenter.Shell   (GameLogger)
  → Playcenter.Animation (DOTween / UniTask wrappers via UITransitionHandler)
  → UniTask
```

No VContainer, Services, GameFlow, EOS, or KitchenClash references.

## Key types

| Type | Purpose |
|------|---------|
| `UIService` | Screen host: document setup, layer roots, resolve/show/hide |
| `BaseUIScreen` | Base class for all title screens; IUIService injected via `SetUIService` |
| `UIScreenController` | Visual element lifecycle per screen |
| `UIScreenRegistry` | Reflection-based auto-registration of `[UIScreen]`-annotated classes |
| `UIScreenAttribute` | Marks a class as a screen with category + template path |
| `UITransitionHandler` | DOTween-backed fade/slide/scale transitions |
| `IScreenInstanceFactory` | Port: game supplies a VContainer-backed implementation |
| `IScopeAwareScreenFactory` | Optional port: allows scope swap on session open |
| `INotificationScreen` | Toast screen contract |

## Game-side wiring (KitchenClash)

```csharp
// Presentation.Common:
VContainerScreenInstanceFactory  — IScreenInstanceFactory + IScopeAwareScreenFactory
UIServiceEntryPoint              — IStartable + ITickable wrapper for UIService

// RootLifetimeScope:
builder.Register<UIScreenStackManager>(Lifetime.Singleton).As<IUIScreenStackManager>();
builder.Register<VContainerScreenInstanceFactory>(Lifetime.Singleton)
    .As<IScreenInstanceFactory>().As<IScopeAwareScreenFactory>();
builder.Register<UIService>(Lifetime.Singleton).As<IUIService>().AsSelf();
builder.RegisterEntryPoint<UIServiceEntryPoint>();
```
