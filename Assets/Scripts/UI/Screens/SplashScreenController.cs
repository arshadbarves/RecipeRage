image.pngusing System.Threading.Tasks;
using UnityEngine.UIElements;
using RecipeRage.UI.Core;
using RecipeRage.Modules.Logging;
using RecipeRage.UI.Animation;
using UnityEngine;

namespace RecipeRage.UI.Screens
{
    public class SplashScreenController : BaseScreenController
    {
        private Label _statusLabel;
        private Label _studioNameLabel;
        private Label _titleLabel;

        // Animation timings (milliseconds)
        private const uint BG_FADE_IN_DURATION = 600;
        private const uint STUDIO_NAME_MOVE_DURATION = 700;
        private const uint STUDIO_NAME_FADE_DURATION = 500;
        private const uint TITLE_DELAY = 200; // Delay after studio name starts animating
        private const uint TITLE_FADE_DURATION = 600;
        private const uint TITLE_SCALE_DURATION = 700;
        private const uint HOLD_DURATION = 1200; // How long to hold the final look
        private const uint FINAL_FADE_OUT_DURATION = 400;

        public override void Initialize(UIScreenManager manager, ScreenDefinition definition, VisualElement rootElement)
        {
            base.Initialize(manager, definition, rootElement);

            // Get references to the labels
            _studioNameLabel = RootElement.Q<Label>("StudioNameLabel");
            _titleLabel = RootElement.Q<Label>("TitleLabel");
            _statusLabel = RootElement.Q<Label>("StatusLabel"); // Keep reference if needed later

            // Basic null checks
            if (_studioNameLabel == null) LogHelper.Warning("SplashScreenController", "StudioNameLabel not found in UXML.");
            if (_titleLabel == null) LogHelper.Warning("SplashScreenController", "TitleLabel not found in UXML.");
            if (_statusLabel == null) LogHelper.Warning("SplashScreenController", "StatusLabel not found in UXML.");
        }

        public override void OnAfterShow()
        {
            LogHelper.Info("SplashScreenController", "Splash screen shown, starting cinematic intro.");
            // Set initial background color (redundant if USS starts black, but safe)
            RootElement.style.backgroundColor = new StyleColor(Color.black);
            // Start the animation sequence
            _ = PlayIntroAnimationSequence();
        }

        private async Task PlayIntroAnimationSequence()
        {
            if (RootElement == null || _studioNameLabel == null || _titleLabel == null)
            {
                LogHelper.Error("SplashScreenController", "Cannot play animation, required elements not found.");
                await Task.Delay(1000); // Fallback delay
                TransitionToLoadingScreen();
                return;
            }

            // --- CINEMATIC SEQUENCE --- 

            // 1. Fade In Background Color (from black set above)
            Color targetBgColor = RootElement.resolvedStyle.backgroundColor; // Get target from USS var if set, or parse
            // Simple fallback if var isn't resolved/parsed easily - set explicit target color
            targetBgColor = new Color(0.0f, 180 / 255f, 210 / 255f); // Approx --color-secondary

            RootElement.Animate()
                .Custom(0f, 1f, BG_FADE_IN_DURATION, (ve, t) =>
                {
                    ve.style.backgroundColor = Color.Lerp(Color.black, targetBgColor, t); // Lerp color
                }, UIEasingFunctions.EaseInOutQuad)
                .Play();

            // Wait slightly less than BG fade duration before starting elements
            await Task.Delay((int)(BG_FADE_IN_DURATION * 0.5f));

            // 2. Animate Studio Name In (Run move, scale, and fade concurrently)
            _studioNameLabel.Animate()
                .Move(0, 0, STUDIO_NAME_MOVE_DURATION, UIEasingFunctions.EaseOutCubic)
                .Play(); // Play the move animation
            _studioNameLabel.Animate()
                .Scale(1.0f, STUDIO_NAME_MOVE_DURATION, UIEasingFunctions.EaseOutCubic)
                .Play(); // Play the scale animation concurrently
            _studioNameLabel.Animate()
                .Fade(1, STUDIO_NAME_FADE_DURATION)
                .Play(); // Play the fade animation concurrently

            // 3. Wait a bit, then Animate Game Title In (Run fade and scale concurrently)
            await Task.Delay((int)TITLE_DELAY);

            _titleLabel.Animate()
                 .Fade(1, TITLE_FADE_DURATION, UIEasingFunctions.EaseInQuad)
                 .Play(); // Play fade
            _titleLabel.Animate()
                 .Scale(1.0f, TITLE_SCALE_DURATION, UIEasingFunctions.EaseOutBack)
                 .Play(); // Play scale concurrently

            // 4. Hold the final look
            // Calculate hold time remaining after animations finish
            // (Approximate based on longest running animation part)
            uint maxAnimTime = System.Math.Max(STUDIO_NAME_MOVE_DURATION, TITLE_DELAY + TITLE_SCALE_DURATION);
            int holdWait = (int)HOLD_DURATION;
            await Task.Delay(holdWait + (int)maxAnimTime - (int)(BG_FADE_IN_DURATION * 0.5f));

            // 5. Fade Out Everything 
            // (Fade outs can be chained as they are separate animations starting now)
            _studioNameLabel.Animate().Fade(0, FINAL_FADE_OUT_DURATION).Play();
            _titleLabel.Animate().Fade(0, FINAL_FADE_OUT_DURATION).Play();
            RootElement.Animate()
                 .Custom(1f, 0f, FINAL_FADE_OUT_DURATION, (ve, t) =>
                 {
                     ve.style.backgroundColor = Color.Lerp(targetBgColor, Color.black, t);
                 })
                .Play();

            // Wait for fade out to complete
            await Task.Delay((int)FINAL_FADE_OUT_DURATION + 50);

            // 6. Transition to Loading Screen
            TransitionToLoadingScreen();
        }

        private void TransitionToLoadingScreen()
        {
            if (ScreenManager != null)
            {
                LogHelper.Info("SplashScreenController", "Transitioning to Loading Screen.");
                _ = ScreenManager.ShowScreenAsync(ScreenId.LoadingScreen);
            }
            else
            {
                LogHelper.Error("SplashScreenController", "ScreenManager is null, cannot transition.");
            }
        }

        public override Task OnBeforeHideAsync()
        {
            LogHelper.Info("SplashScreenController", "Splash screen hiding.");
            // Stop any lingering animations on elements in case transition was interrupted
            RootElement?.StopAnimations(); // Stop background anim
            _studioNameLabel?.StopAnimations();
            _titleLabel?.StopAnimations();
            _statusLabel?.StopAnimations();
            return Task.CompletedTask;
        }
    }
}
