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
