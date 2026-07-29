# Login Screen Redesign — Playcenter Light Theme

Date: 2026-07-29
Status: Approved
Scope: `Assets/Game/UI/` only (screen-level change; no service or SDK changes)

## 1. Goal

Recreate the provided HTML reference login screen exactly in Unity UI Toolkit:
a premium, minimal, light-themed (porcelain white) mobile landscape login screen
with the Playcenter studio branding on the left and floating, cardless auth
buttons on the right, including the HTML's blur-fade-slide entrance animations.

## 2. Reference (HTML mockup — summary)

- Background: porcelain off-white `#FAFAFC`.
- Accent: valve matte pale red `#B33232` (dark `#912424`).
- Text: charcoal `#0D0E11`, muted slate `#8E939E`, border `#E5E7EB`.
- Fonts: Outfit (800 brand, 600 buttons), Space Grotesk (mono labels).
- Landscape row layout:
  - Left: brand `play` + red `center`, subtitle `STUDIOS` (letter-spaced mono).
  - 1px vertical divider (40% height).
  - Right (max-width 320px): mono label "SIGN IN TO PLAY", three 46px buttons
    (Google white bordered, Facebook `#1877F2`, Guest red pill), terms text.
- Entrance animations (CSS `cubic-bezier(0.16,1,0.3,1)` ≈ ease-out-cubic):
  - Brand side: 1.4s, delay 0.2s — opacity 0→1, blur 10→0, translateX −15→0.
  - Divider: 1s fade-in, delay 0.5s.
  - Login side: 1.4s, delay 0.4s — opacity 0→1, blur 10→0, translateX +15→0.
- Button press: `scale(0.97)` + variant-specific pressed background.

## 3. Decisions (from brainstorming)

| Question | Decision |
|---|---|
| Branding | Exactly like HTML — `playcenter STUDIOS`; no game title on login screen |
| Theme | Light theme matching HTML reference |
| Animations | Exact same entrance timings, recreated with `UITween` |
| Icons | High-res PNG sprites in `Assets/Resources/UI/Icons/` |
| Approach | Screen-specific light USS + UXML rewrite + UITween (Option A) |
| Portrait | Not handled — game is landscape-locked per rebuild spec |

## 4. Architecture

### 4.1 Files

| File | Change |
|---|---|
| `Assets/Game/UI/Styles/login.uss` | **New.** Light-theme styles scoped under `.login-screen` so the global dark theme (`playcenter.uss`) is untouched. |
| `Assets/Game/UI/UXML/LoginScreen.uxml` | **Rewritten.** Two-pane row layout (brand \| divider \| login side). |
| `Assets/Game/UI/Screens/LoginScreen.cs` | **Modified.** Drives entrance animation via `UITween`; auth wiring unchanged. |
| `Assets/Resources/UI/Icons/icon_google.png` | **New.** Google 4-color mark, white-free glyph for tinted/untinted use. |
| `Assets/Resources/UI/Icons/icon_facebook.png` | **New.** Facebook "f" mark (white on transparent). |
| `Assets/Resources/UI/Icons/icon_guest.png` | **New.** Person-outline glyph (white stroke on transparent). |

### 4.2 Layout (UXML)

```
.login-screen (row, bg #FAFAFC, padding-left/right 8%)
├── #brand-side (flex 1, column, left-aligned)
│   ├── #brand-title (row): Label "play" (charcoal) + Label "center" (red)
│   └── #brand-subtitle: "STUDIOS" (Space Grotesk, letter-spaced, muted)
├── #divider (1px wide, 40% tall, #E5E7EB, horizontal margins ~4%)
└── #login-side (flex 1, column, max-width 320px)
    ├── #login-label: "SIGN IN TO PLAY" (mono, letter-spaced, muted)
    ├── #button-container (column, gap 12px)
    │   ├── #google-button   (.btn-login .btn-login--google,  icon + "Continue with Google")
    │   ├── #facebook-button (.btn-login .btn-login--facebook, icon + "Continue with Facebook")
    │   └── #guest-button    (.btn-login .btn-login--guest,    icon + "Play as Guest")
    └── #terms: "By continuing, you agree to our Terms & Privacy Policy."
```

Element names `google-button`, `facebook-button`, `guest-button`,
`button-container` are preserved so existing `LoginScreen.cs` queries and any
scene wiring keep working.

### 4.3 Styling (login.uss)

- Tokens defined locally on `.login-screen` (no global token changes):
  `--login-bg #FAFAFC`, `--login-red #B33232`, `--login-red-dark #912424`,
  `--login-text #0D0E11`, `--login-muted #8E939E`, `--login-border #E5E7EB`.
- Brand title: Outfit ExtraBold, ~5.5vw-equivalent fixed size (56px at 1080p
  reference; sized via USS `font-size`), lowercase, tight tracking.
- Brand subtitle: Space Grotesk, letter-spacing emulated with spaces between
  characters (UI Toolkit has no letter-spacing) — text `S T U D I O S`.
- Buttons: height 46px, radius 12px, font Outfit 600-equivalent 15px,
  row layout with 18px icon + label, centered.
  - Soft shadow faked with a 1px bottom border slightly darker than bg
    (flat, no gradients — spec-compliant).
  - `:active` → `scale: 0.97` (USS transition) + pressed bg
    (`#F3F4F6` google, darker red `#912424` guest, darker blue facebook).
- Terms: 11px muted, centered links underlined (single Label; link styling
  not interactive — informational only).

### 4.4 Entrance animation (LoginScreen.cs + UITween)

UI Toolkit has no `filter: blur`; blur is approximated exactly like
`SplashScreen` — opacity + slight scale-up (0.98→1) reads as a blur-reveal
on mobile. Timings match the HTML:

| Element | Delay | Duration | From → To |
|---|---|---|---|
| `#brand-side` | 0.2s | 1.4s | opacity 0→1, translateX −15→0, scale 0.98→1 |
| `#divider` | 0.5s | 1.0s | opacity 0→1 |
| `#login-side` | 0.4s | 1.4s | opacity 0→1, translateX +15→0, scale 0.98→1 |

Easing: `UITween.EaseOutCubic` (matches `cubic-bezier(0.16,1,0.3,1)` closely).

Initial states are set in `OnShow()` before scheduling tweens so re-showing
the screen replays the animation deterministically.

### 4.5 Behavior (unchanged)

`LoginScreen.cs` keeps the existing auth flow:
`google-button → SignInWithGoogle()`, `facebook-button → SignInWithFacebook()`,
`guest-button → SignInAsGuest()`; success → forced tutorial on first launch,
else `MainMenuScreen`. No service, DI, or state-machine changes.

### 4.6 Icons

Three PNGs generated programmatically at 256×256 (down-sampled by Unity at
runtime), imported with default sprite settings, referenced from USS via
`resource('UI/Icons/icon_google')` etc. — same pattern as existing
`.icon-coin` / `.icon-trophy` classes in `components.uss`.

## 5. Error handling

- `ServiceLocator.Get<IAuthService>()` failures: unchanged from current
  behavior (composition root guarantees registration before login screen).
- Missing icon resources: USS `background-image` falls back to nothing —
  buttons still readable (text labels present). Acceptable degradation.
- Re-showing the screen: `OnShow()` resets initial animation states — no
  stale opacity/translate.

## 6. Testing / verification

- No test suite (project decision) — verification = compile + run:
  - `dotnet build RecipeRage.UI.csproj -nologo`
  - Open `Assets/Scenes/Boot.unity` → Play → confirm login screen renders
    light-themed with entrance animation, buttons navigate correctly.

## 7. Spec compliance

- Flat colors, no gradients ✅
- Landscape mobile ✅
- Auth providers: Facebook, Google, Guest (no Epic login) ✅
- UXML/USS own layout; code only queries/binds ✅
- Namespaces: `RecipeRage.UI` ✅
