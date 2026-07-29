# Login Screen Light Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Recreate the HTML reference login screen in Unity UI Toolkit — light porcelain theme, playcenter branding, floating cardless buttons, blur-fade-slide entrance animations.

**Architecture:** Screen-scoped light USS (`login.uss`) + rewritten `LoginScreen.uxml` two-pane layout + `UITween`-driven entrance animation in `LoginScreen.cs`. Global dark theme untouched; auth flow unchanged.

**Tech Stack:** Unity 6 UI Toolkit (UXML/USS), C#, `UITween` (existing), PNG sprite icons.

**Spec:** `docs/superpowers/specs/2026-07-29-login-screen-light-redesign-design.md`

## Global Constraints

- Flat colors only, **no gradients** (project spec)
- Landscape mobile layout; portrait fallback out of scope
- Auth providers: Facebook, Google, Guest — **no Epic login**
- UXML/USS own layout; C# only queries and binds
- Namespace `RecipeRage.UI`; 4-space indent; explicit accessibility modifiers; `var` only for obvious types
- No test suite — verification = `dotnet build` + in-editor run
- Element names `google-button`, `facebook-button`, `guest-button`, `button-container` must be preserved (scene/code wiring depends on them)

---

### Task 1: Icon PNG Sprites

**Files:**
- Create: `tools/generate_login_icons.py`
- Create: `Assets/Resources/UI/Icons/icon_google.png`
- Create: `Assets/Resources/UI/Icons/icon_facebook.png`
- Create: `Assets/Resources/UI/Icons/icon_guest.png`

**Interfaces:**
- Consumes: nothing (standalone asset generation)
- Produces: `Assets/Resources/UI/Icons/icon_{google,facebook,guest}.png` — referenced by USS classes `.icon-google`, `.icon-facebook`, `.icon-guest` in Task 2 via `resource('UI/Icons/icon_google')` etc.

- [ ] **Step 1: Write the generator script**

Create `tools/generate_login_icons.py` (requires `pip install pillow`; run from repo root). It renders the three 256×256 PNGs: Google 4-color "G" (approximated with arcs), Facebook white "f" on transparent (drawn from the official path proportions — rendered as a bold rounded rect "f" glyph using a font-free polygon), guest white person-outline on transparent.

```python
"""Generates login button icons (Google G, Facebook f, guest person) as 256x256 PNGs."""
import math
import os
from PIL import Image, ImageDraw

SIZE = 256
OUT_DIR = "Assets/Resources/UI/Icons"


def google():
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    cx, cy, r = SIZE / 2, SIZE / 2, 108
    w = 52  # stroke width
    bbox = [cx - r, cy - r, cx + r, cy + r]
    # Four arcs (degrees, PIL measures clockwise from 3 o'clock going down)
    d.arc(bbox, start=305, end=45, fill=(66, 133, 244, 255), width=w)    # blue (right/top-right)
    d.arc(bbox, start=45, end=155, fill=(234, 67, 53, 255), width=w)     # red (top)
    d.arc(bbox, start=155, end=235, fill=(251, 188, 5, 255), width=w)    # yellow (left/bottom-left)
    d.arc(bbox, start=235, end=305, fill=(52, 168, 83, 255), width=w)    # green (bottom)
    # Blue horizontal bar of the G
    bar_h = w
    d.rectangle([cx, cy - bar_h / 2, cx + r, cy + bar_h / 2], fill=(66, 133, 244, 255))
    return img


def facebook():
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    white = (255, 255, 255, 255)
    # Stylized lowercase f drawn with rectangles (matches silhouette closely at icon size)
    # Vertical stem
    d.rounded_rectangle([96, 52, 160, 236], radius=18, fill=white)
    # Top hook curve cutout trick: draw circle then erase left half with transparent
    d.ellipse([52, 20, 180, 148], fill=white)
    d.ellipse([96, 60, 196, 168], fill=(0, 0, 0, 0))  # inner cutout (composited later)
    # Crossbar
    d.rectangle([64, 108, 192, 156], fill=white)
    # Re-erase: build with masks instead for correctness
    img2 = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    d2 = ImageDraw.Draw(img2)
    stem_w = 56
    x0 = 104
    # stem
    d2.rectangle([x0, 60, x0 + stem_w, 236], fill=white)
    # hook: outer circle top-right
    d2.ellipse([x0, 28, x0 + 132, 160], fill=white)
    # cut inner circle to form the hook curve
    d2.ellipse([x0 + 52, 80, x0 + 140, 168], fill=(0, 0, 0, 0))
    # crossbar
    d2.rectangle([x0 - 36, 116, x0 + 108, 160], fill=white)
    # punch the inner circle properly using alpha compositing
    inner = Image.new("L", (SIZE, SIZE), 0)
    di = ImageDraw.Draw(inner)
    di.ellipse([x0 + 52, 80, x0 + 140, 168], fill=255)
    px = img2.load()
    for y in range(SIZE):
        for x in range(SIZE):
            if inner.getpixel((x, y)) > 0 and x > x0:
                r_, g_, b_, a_ = px[x, y]
                # keep only where the stem/crossbar is (erase hook interior right of stem)
                if not (x0 <= x <= x0 + stem_w):
                    px[x, y] = (255, 255, 255, 0)
    return img2


def guest():
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    white = (255, 255, 255, 255)
    stroke = 22
    cx = SIZE / 2
    # Head circle (outline)
    hr = 42
    hy = 66
    d.ellipse([cx - hr, hy - hr, cx + hr, hy + hr], outline=white, width=stroke)
    # Shoulders arc (outline) — half-annulus from 200deg to -20deg
    sr = 74
    sy = 196
    d.arc([cx - sr, sy - sr, cx + sr, sy + sr], start=180, end=360, fill=white, width=stroke)
    return img


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    google().save(os.path.join(OUT_DIR, "icon_google.png"))
    facebook().save(os.path.join(OUT_DIR, "icon_facebook.png"))
    guest().save(os.path.join(OUT_DIR, "icon_guest.png"))
    print("Generated icon_google.png, icon_facebook.png, icon_guest.png in", OUT_DIR)


if __name__ == "__main__":
    main()
```

- [ ] **Step 2: Run the generator**

Run: `cd <repo root> && pip install pillow --quiet && python tools/generate_login_icons.py`
Expected: prints `Generated icon_google.png, icon_facebook.png, icon_guest.png in Assets/Resources/UI/Icons`; three PNGs exist.

- [ ] **Step 3: Add icon meta files (Unity sprite import settings)**

Unity generates `.meta` files on next Editor open, but commit them now so the GUIDs are stable for USS `resource()` references. If the Editor is available, open the project once and let Unity generate them; otherwise create minimal meta files matching the existing icon meta format. Check an existing one first:

Run: `cat Assets/Resources/UI/Icons/icon_coin.png.meta`
Copy that structure for each new icon with a fresh GUID (`uuidgen | tr -d '-' | tr 'A-F' 'a-f'`).

- [ ] **Step 4: Commit**

```bash
git add tools/generate_login_icons.py Assets/Resources/UI/Icons/icon_google.png Assets/Resources/UI/Icons/icon_facebook.png Assets/Resources/UI/Icons/icon_guest.png Assets/Resources/UI/Icons/icon_google.png.meta Assets/Resources/UI/Icons/icon_facebook.png.meta Assets/Resources/UI/Icons/icon_guest.png.meta
git commit -m "feat(ui): login button icons (google, facebook, guest)"
```

---

### Task 2: Light-Theme Stylesheet (login.uss)

**Files:**
- Create: `Assets/Game/UI/Styles/login.uss`

**Interfaces:**
- Consumes: icon resources from Task 1 (`UI/Icons/icon_google`, `icon_facebook`, `icon_guest`); fonts `Assets/Fonts/Outfit-ExtraBold.ttf`, `Fonts/SpaceGrotesk` (Resources), `Assets/Resources/Fonts/Outfit-SemiBold.ttf` if present else `Outfit-Regular.ttf`
- Produces: USS classes `.login-screen`, `.btn-login`, `.btn-login--google`, `.btn-login--facebook`, `.btn-login--guest`, `.login-brand-title`, `.login-brand-accent`, `.login-brand-subtitle`, `.login-label`, `.login-terms`, `.icon-google`, `.icon-facebook`, `.icon-guest` — consumed by Task 3 UXML

- [ ] **Step 1: Verify which Outfit weights exist**

Run: `ls Assets/Resources/Fonts/`
Expected: see available Outfit TTFs. Use `Outfit-SemiBold.ttf` for buttons if present; otherwise `Outfit-Regular.ttf` (record the choice for Step 2).

- [ ] **Step 2: Create login.uss**

```css
/* ═══════════════════════════════════════════════════════════════════════════
   LOGIN SCREEN — Light porcelain theme (HTML reference recreation).
   Scoped under .login-screen so the global dark theme is untouched.
   Flat colors only — no gradients.
   ═══════════════════════════════════════════════════════════════════════════ */

.login-screen {
    --login-bg: rgb(250, 250, 252);
    --login-red: rgb(179, 50, 50);
    --login-red-dark: rgb(145, 36, 36);
    --login-text: rgb(13, 14, 17);
    --login-muted: rgb(142, 147, 158);
    --login-border: rgb(229, 231, 235);
    --login-white: rgb(255, 255, 255);
    --login-google-active: rgb(243, 244, 246);
    --login-facebook: rgb(24, 119, 242);
    --login-facebook-dark: rgb(16, 92, 196);

    flex-grow: 1;
    width: 100%;
    height: 100%;
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
    background-color: var(--login-bg);
    padding-left: 8%;
    padding-right: 8%;
}

/* ── Left: studio branding ───────────────────────────────────────────────── */

#brand-side {
    flex-grow: 1;
    justify-content: center;
    align-items: flex-start;
}

#brand-title {
    flex-direction: row;
}

.login-brand-title {
    font-size: 56px;
    color: var(--login-text);
    -unity-font-definition: url("project://database/Assets/Fonts/Outfit-ExtraBold.ttf");
}

.login-brand-accent {
    color: var(--login-red);
}

.login-brand-subtitle {
    font-size: 14px;
    color: var(--login-muted);
    margin-top: 12px;
    -unity-font-definition: resource("Fonts/SpaceGrotesk");
    /* letter-spacing emulated with spaces in UXML text (S T U D I O S) */
}

/* ── Divider ─────────────────────────────────────────────────────────────── */

#login-divider {
    width: 1px;
    height: 40%;
    background-color: var(--login-border);
    margin-left: 4%;
    margin-right: 4%;
}

/* ── Right: floating login buttons (cardless) ────────────────────────────── */

#login-side {
    flex-grow: 1;
    justify-content: center;
    align-items: flex-start;
    max-width: 320px;
}

.login-label {
    font-size: 12px;
    color: var(--login-muted);
    margin-bottom: 16px;
    -unity-font-definition: resource("Fonts/SpaceGrotesk");
    /* letter-spacing emulated with spaces in UXML text */
}

#button-container {
    width: 100%;
}

/* ── Buttons ─────────────────────────────────────────────────────────────── */

.btn-login {
    width: 100%;
    height: 46px;
    border-radius: 12px;
    flex-direction: row;
    align-items: center;
    justify-content: center;
    margin-bottom: 12px;
    padding-left: 16px;
    padding-right: 16px;

    font-size: 15px;
    color: var(--login-text);
    -unity-text-align: middle-center;
    -unity-font-definition: resource("Fonts/Outfit-Regular");

    /* soft flat shadow: 1px bottom "shade" border, no gradient */
    border-left-width: 0;
    border-right-width: 0;
    border-top-width: 0;
    border-bottom-width: 1px;
    border-bottom-color: rgba(0, 0, 0, 0.05);

    scale: 1 1;
    transition-property: scale, background-color;
    transition-duration: 0.15s;
    transition-timing-function: ease-out;
}

.btn-login:active {
    scale: 0.97 0.97;
}

.btn-login__icon {
    width: 18px;
    height: 18px;
    margin-right: 12px;
    flex-shrink: 0;
}

/* Google: white with light border */
.btn-login--google {
    background-color: var(--login-white);
    border-left-width: 1px;
    border-right-width: 1px;
    border-top-width: 1px;
    border-left-color: var(--login-border);
    border-right-color: var(--login-border);
    border-top-color: var(--login-border);
    border-bottom-color: var(--login-border);
}
.btn-login--google:active { background-color: var(--login-google-active); }

/* Facebook: brand blue, white text */
.btn-login--facebook {
    background-color: var(--login-facebook);
    color: var(--login-white);
    border-bottom-color: var(--login-facebook-dark);
}
.btn-login--facebook:active { background-color: var(--login-facebook-dark); }

/* Guest: valve red pill, white text, last in stack (no bottom margin) */
.btn-login--guest {
    background-color: var(--login-red);
    color: var(--login-white);
    border-bottom-color: var(--login-red-dark);
    margin-bottom: 0;
}
.btn-login--guest:active { background-color: var(--login-red-dark); }

/* ── Icons ───────────────────────────────────────────────────────────────── */

.icon-google { background-image: resource('UI/Icons/icon_google'); }
.icon-facebook { background-image: resource('UI/Icons/icon_facebook'); }
.icon-guest { background-image: resource('UI/Icons/icon_guest'); }

/* ── Terms ───────────────────────────────────────────────────────────────── */

.login-terms {
    font-size: 11px;
    color: var(--login-muted);
    margin-top: 16px;
    max-width: 320px;
    white-space: normal;
}
```

If Step 1 found `Outfit-SemiBold.ttf` in `Assets/Resources/Fonts/`, replace both `resource("Fonts/Outfit-Regular")` occurrences with `resource("Fonts/Outfit-SemiBold")`.

- [ ] **Step 3: Commit**

```bash
git add Assets/Game/UI/Styles/login.uss Assets/Game/UI/Styles/login.uss.meta
git commit -m "feat(ui): light-theme login stylesheet (scoped, flat colors)"
```

---

### Task 3: Rewrite LoginScreen.uxml (two-pane layout)

**Files:**
- Modify: `Assets/Game/UI/UXML/LoginScreen.uxml` (full rewrite)

**Interfaces:**
- Consumes: USS classes from Task 2 (`.login-screen`, `.btn-login*`, icon classes, brand classes)
- Produces: element names consumed by Task 4 C#: `brand-side`, `login-divider`, `login-side`, `google-button`, `facebook-button`, `guest-button`, `button-container`

- [ ] **Step 1: Rewrite the UXML**

Replace the entire contents of `Assets/Game/UI/UXML/LoginScreen.uxml` with:

```xml
<UXML xmlns="UnityEngine.UIElements">
    <Style src="../Styles/playcenter.uss" />
    <Style src="../Styles/login.uss" />
    <VisualElement name="login-screen" class="login-screen">

        <!-- Left: studio branding -->
        <VisualElement name="brand-side">
            <VisualElement name="brand-title">
                <Label name="brand-play" class="login-brand-title" text="play" />
                <Label name="brand-center" class="login-brand-title login-brand-accent" text="center" />
            </VisualElement>
            <Label name="brand-subtitle" class="login-brand-subtitle" text="S T U D I O S" />
        </VisualElement>

        <!-- Subtle vertical divider -->
        <VisualElement name="login-divider" />

        <!-- Right: floating login buttons (no card) -->
        <VisualElement name="login-side">
            <Label name="login-label" class="login-label" text="S I G N   I N   T O   P L A Y" />

            <VisualElement name="button-container">
                <Button name="google-button" class="btn-login btn-login--google">
                    <VisualElement name="icon" class="btn-login__icon icon-google" />
                    <Label name="label" text="Continue with Google" />
                </Button>
                <Button name="facebook-button" class="btn-login btn-login--facebook">
                    <VisualElement name="icon" class="btn-login__icon icon-facebook" />
                    <Label name="label" text="Continue with Facebook" />
                </Button>
                <Button name="guest-button" class="btn-login btn-login--guest">
                    <VisualElement name="icon" class="btn-login__icon icon-guest" />
                    <Label name="label" text="Play as Guest" />
                </Button>
            </VisualElement>

            <Label name="terms" class="login-terms" text="By continuing, you agree to our Terms &amp; Privacy Policy." />
        </VisualElement>

    </VisualElement>
</UXML>
```

Note: `playcenter.uss` is still imported first for shared fonts/utilities, but `.login-screen` overrides the background. The `screen`/`center` classes are intentionally not used.

- [ ] **Step 2: Verify UXML parses in Unity**

Open the UXML in Unity's UI Builder (or open the project and check Console for UXML parse errors). Expected: no errors; screen shows the two-pane layout (unstyled positions may shift until C# runs — styles come from USS).

- [ ] **Step 3: Commit**

```bash
git add Assets/Game/UI/UXML/LoginScreen.uxml
git commit -m "feat(ui): rewrite login screen layout to two-pane light design"
```

---

### Task 4: Entrance animation + auth wiring in LoginScreen.cs

**Files:**
- Modify: `Assets/Game/UI/Screens/LoginScreen.cs`

**Interfaces:**
- Consumes: UXML names from Task 3 (`brand-side`, `login-divider`, `login-side`, `google-button`, `facebook-button`, `guest-button`); `UITween.Animate(duration, delay, ease, apply)` and `UITween.EaseOutCubic` from `Assets/Game/UI/Animations/UITween.cs`; existing `ServiceLocator.Get<IAuthService>()` / `ISaveService` / `IGameStateMachine` / `IUIService` flow
- Produces: none (terminal task — screen behavior)

- [ ] **Step 1: Rewrite LoginScreen.cs**

Replace the entire contents of `Assets/Game/UI/Screens/LoginScreen.cs` with:

```csharp
using Playcenter;
using Playcenter.Services;
using Playcenter.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace RecipeRage.UI
{
    /// <summary>
    /// Light-theme login screen matching the HTML reference: playcenter brand
    /// on the left, cardless floating buttons on the right. Entrance animation
    /// mirrors the HTML blur-fade-slide (blur approximated as opacity+scale,
    /// same technique as SplashScreen):
    ///   brand side  1.4s ease-out-cubic, delay 0.2s, translateX -15 -> 0
    ///   divider     1.0s fade,           delay 0.5s
    ///   login side  1.4s ease-out-cubic, delay 0.4s, translateX +15 -> 0
    /// </summary>
    [UIScreen]
    public sealed class LoginScreen : BaseUIScreen
    {
        private const float SideDurationSec = 1.4f;
        private const float BrandDelaySec = 0.2f;
        private const float LoginDelaySec = 0.4f;
        private const float DividerDurationSec = 1.0f;
        private const float DividerDelaySec = 0.5f;
        private const float SlideOffsetPx = 15f;
        private const float StartScale = 0.98f;

        protected override void OnShow()
        {
            WireButtons();
            PlayEntrance();
        }

        private void WireButtons()
        {
            Root.Q<Button>("facebook-button").clicked += () => SignIn(provider => provider.SignInWithFacebook());
            Root.Q<Button>("google-button").clicked += () => SignIn(provider => provider.SignInWithGoogle());
            Root.Q<Button>("guest-button").clicked += () => SignIn(provider => provider.SignInAsGuest());
        }

        private void PlayEntrance()
        {
            var brandSide = Root.Q<VisualElement>("brand-side");
            var divider = Root.Q<VisualElement>("login-divider");
            var loginSide = Root.Q<VisualElement>("login-side");

            // Initial off-states (re-set every show so re-showing replays cleanly)
            SetSideState(brandSide, opacity: 0f, offsetX: -SlideOffsetPx, scale: StartScale);
            SetSideState(loginSide, opacity: 0f, offsetX: SlideOffsetPx, scale: StartScale);
            if (divider != null)
            {
                divider.style.opacity = 0f;
            }

            // Brand side: blur-fade-slide from left
            UITween.Animate(SideDurationSec, BrandDelaySec, UITween.EaseOutCubic, t =>
            {
                SetSideState(brandSide,
                    opacity: t,
                    offsetX: Mathf.Lerp(-SlideOffsetPx, 0f, t),
                    scale: Mathf.Lerp(StartScale, 1f, t));
            });

            // Divider: simple fade
            UITween.Animate(DividerDurationSec, DividerDelaySec, UITween.EaseOutCubic, t =>
            {
                if (divider != null)
                {
                    divider.style.opacity = t;
                }
            });

            // Login side: blur-fade-slide from right
            UITween.Animate(SideDurationSec, LoginDelaySec, UITween.EaseOutCubic, t =>
            {
                SetSideState(loginSide,
                    opacity: t,
                    offsetX: Mathf.Lerp(SlideOffsetPx, 0f, t),
                    scale: Mathf.Lerp(StartScale, 1f, t));
            });
        }

        private static void SetSideState(VisualElement element, float opacity, float offsetX, float scale)
        {
            if (element == null)
            {
                return;
            }
            element.style.opacity = opacity;
            element.style.translate = new StyleTranslate(new Translate(offsetX, 0f));
            element.style.scale = new StyleScale(new Scale(new Vector2(scale, scale)));
        }

        private async void SignIn(System.Func<IAuthService, System.Threading.Tasks.Task<AuthResult>> signIn)
        {
            var auth = ServiceLocator.Get<IAuthService>();
            var result = await signIn(auth);
            if (result.Success)
            {
                // First launch: tutorial before main menu (per spec, forced tutorial)
                var tutorialDone = ServiceLocator.Get<ISaveService>().Load("tutorial_completed", false);
                if (!tutorialDone)
                {
                    ServiceLocator.Get<IGameStateMachine>().ChangeState(new TutorialState());
                }
                else
                {
                    ServiceLocator.Get<IUIService>().Show<MainMenuScreen>();
                }
            }
        }
    }
}
```

Auth logic is byte-for-byte the previous behavior — only layout queries and animation changed.

- [ ] **Step 2: Compile the UI assembly**

Run: `dotnet build RecipeRage.UI.csproj -nologo` (if the csproj exists after an Editor open) — otherwise open the project in Unity and check Console.
Expected: 0 errors.

- [ ] **Step 3: In-editor verification**

Open `Assets/Scenes/Boot.unity` → Play → navigate to login state. Verify: porcelain background; brand slides in from left; divider fades; buttons slide in from right; Google button is white with the 4-color icon; Facebook blue with white "f"; Guest red pill; buttons scale to 0.97 when pressed; sign-in navigates to tutorial/main menu.

- [ ] **Step 4: Commit**

```bash
git add Assets/Game/UI/Screens/LoginScreen.cs
git commit -m "feat(ui): login entrance animation (blur-fade-slide) + light theme wiring"
```

---

## Self-Review Notes

- **Spec coverage:** layout (Task 3), light styling (Task 2), entrance animation (Task 4), icons (Task 1), unchanged auth (Task 4 Step 1), verification (Task 4 Steps 2-3) — all spec sections map to tasks. Portrait fallback explicitly out of scope per spec §3.
- **Type consistency:** `UITween.Animate(float duration, float delay, Func<float,float> ease, Action<float> apply, Action onComplete = null)` matches `UITween.cs`. `StyleTranslate(new Translate(float, float))`, `StyleScale(new Scale(Vector2))` match SplashScreen.cs usage. UXML element names in Task 4 match Task 3 exactly.
- `UIAnimation.StaggerChildren`/`ScaleBounce` from the old screen are replaced by the HTML-exact `UITween` sequence — intentional, per spec §4.4.
