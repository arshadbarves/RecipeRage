# UI/UX

## Architecture

uGUI entirely replaced. RouterService owns a push/pop navigation stack. USS transitions handle all animations. No MonoBehaviour in screen logic.

## Component Roles

| Component | Type | Responsibility |
|-----------|------|----------------|
| RouterService | Pure C# Singleton | Push/pop screen stack, run USS enter/exit transitions, hardware back |
| IScreen | Interface | Contract: Enter(parent,dir,param), Exit(dir), Resume(dir) |
| ScreenPresenter<TVM> | Abstract pure C# | Base class binding UXML to ViewModel. IStartable via VContainer. |
| ScreenViewModel | Pure C# | ObservableProperty<T> fields. View binds to these. |
| UIDocumentRoot | MonoBehaviour (1 in scene) | Holds UIDocument. Passes rootVisualElement to RouterService. |
| UXML files | Markup, no code | Screen layout. Loaded via Resources/UI/{ScreenId} |
| USS files | Stylesheets | All styles + transition classes |

## RouterService

```csharp
public interface IRouterService {
    void Push(ScreenId id, object param = null);
    void Pop();
    void PopToRoot();
    void Replace(ScreenId id, object param = null);
    ScreenId Current { get; }
    event Action<ScreenId> OnScreenChanged;
}

public enum ScreenId {
    Splash, Home, ChefSelect, Store, Settings,
    MatchLobby, MatchHUD, Results, DailyStreak
}
```

## USS Transitions (No Tweens)

```css
.screen { opacity:1; translate:0 0;
  transition: opacity .25s ease, translate .25s cubic-bezier(.4,0,.2,1); }
.screen--enter-right  { opacity:0; translate: 100% 0; }
.screen--enter-left   { opacity:0; translate:-100% 0; }
.screen--enter-bottom { opacity:0; translate:0  100%; }
.screen--exit-left    { opacity:0; translate:-100% 0; }
.screen--exit-right   { opacity:0; translate: 100% 0; }
.overlay  { opacity:0; scale:.9; transition:opacity .2s ease, scale .2s ease; }
.overlay--open  { opacity:1; scale:1; }
.overlay--close { opacity:0; scale:.9; }
```

## Screen Navigation Flow

```
Splash → Login → Home → ChefSelect
                    ↓
                  Store
                    ↓
                MatchLobby → MatchHUD → Results
                    ↓
                DailyStreak
                    ↓
                  Profile
                    ↓
                 Settings
```

## Home Screen Layout

```
+------------------------------------------------------------------+
| [Settings]  KITCHEN CLASH  [Streak Fire]  [Friends]  [Gems:250] |
+------------------------------------------------------------------+
|       +----------------------------------+                        |
|       |  Chef Rosa  * 850 trophies       |  [+slot] [+slot]      |
|       |  [animated 3D chef model]        |  (party panel)        |
|       +----------------------------------+                        |
|                                                                    |
|  <  [SUSHI SHUFFLE -- 2v2 -- Quick Match -- 5h 23m left]  >       |
|                                                                    |
|               [ ########  PLAY  ######## ]                        |
|                                                                    |
|    [Duo Queue]   [Events]   [Rankings]   [Watch]                  |
+------------------------------------------------------------------+
|  [Chefs]   [Store]   [Home]   [Season Pass]   [Profile]          |
+------------------------------------------------------------------+
```
