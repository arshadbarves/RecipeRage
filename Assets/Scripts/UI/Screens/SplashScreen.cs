using System.Collections;
using Core.GameFramework.State;
using Core.GameFramework.State.States;
using Core.UI.Animation;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Screens
{
    /// <summary>
    /// Splash screen shown when the game starts
    /// </summary>
    public class SplashScreen : UIScreen
    {
        /// <summary>
        /// Logo element
        /// </summary>
        private VisualElement _logo;
        
        /// <summary>
        /// Game title label
        /// </summary>
        private Label _gameTitle;
        
        /// <summary>
        /// Loading bar fill element
        /// </summary>
        private VisualElement _loadingBarFill;
        
        /// <summary>
        /// Loading text label
        /// </summary>
        private Label _loadingText;
        
        /// <summary>
        /// Loading tip label
        /// </summary>
        private Label _loadingTip;
        
        /// <summary>
        /// Version text label
        /// </summary>
        private Label _versionText;
        
        /// <summary>
        /// Tips to display during loading
        /// </summary>
        private string[] _tips = new string[]
        {
            "Tip: Combine ingredients faster for bonus points!",
            "Tip: Different characters have unique abilities!",
            "Tip: Team up with friends for the best scores!",
            "Tip: Keep your kitchen clean for speed bonuses!",
            "Tip: Special recipes give more points but are harder to make!",
            "Tip: Watch out for kitchen hazards!",
            "Tip: Use power-ups wisely to maximize your score!",
            "Tip: Practice makes perfect - try different recipes!",
            "Tip: Communication is key in team modes!",
            "Tip: Some ingredients need to be cooked before serving!"
        };
        
        /// <summary>
        /// Initialize the splash screen
        /// </summary>
        protected override void InitializeScreen()
        {
            // Get references to UI elements
            _logo = _root.Q<VisualElement>("logo");
            _gameTitle = _root.Q<Label>("game-title");
            _loadingBarFill = _root.Q<VisualElement>("loading-bar-fill");
            _loadingText = _root.Q<Label>("loading-text");
            _loadingTip = _root.Q<Label>("loading-tip");
            _versionText = _root.Q<Label>("version-text");
            
            // Set version text
            _versionText.text = $"v{Application.version}";
            
            // Set initial loading tip
            _loadingTip.text = _tips[UnityEngine.Random.Range(0, _tips.Length)];
        }
        
        /// <summary>
        /// Show the splash screen with animations
        /// </summary>
        /// <param name="animate">Whether to animate the transition</param>
        public override void Show(bool animate = true)
        {
            base.Show(animate);
            
            if (animate && _container != null)
            {
                // Reset elements
                _logo.style.opacity = 0;
                _logo.transform.scale = new Vector2(0.5f, 0.5f);
                
                _gameTitle.style.opacity = 0;
                _gameTitle.transform.position = new Vector2(0, 50);
                
                _loadingBarFill.style.width = new StyleLength(0.0);
                
                // Animate logo
                UIAnimationSystem.Instance.Animate(
                    _logo,
                    UIAnimationSystem.AnimationType.FadeIn,
                    0.8f,
                    0.2f,
                    UIEasing.EaseOutCubic
                );
                
                UIAnimationSystem.Instance.Animate(
                    _logo,
                    UIAnimationSystem.AnimationType.ScaleIn,
                    0.8f,
                    0.2f,
                    UIEasing.EaseOutCubic
                );
                
                // Animate game title
                UIAnimationSystem.Instance.Animate(
                    _gameTitle,
                    UIAnimationSystem.AnimationType.FadeIn,
                    0.8f,
                    0.6f,
                    UIEasing.EaseOutCubic
                );
                
                // Start loading simulation
                StartCoroutine(SimulateLoading());
            }
        }
        
        /// <summary>
        /// Simulate loading progress
        /// </summary>
        private IEnumerator SimulateLoading()
        {
            float progress = 0f;
            float loadingTime = 3f; // Total loading time in seconds
            float tipChangeInterval = 1.5f; // Time between tip changes
            float tipTimer = 0f;
            
            while (progress < 1f)
            {
                // Update progress
                progress += Time.deltaTime / loadingTime;
                progress = Mathf.Clamp01(progress);
                
                // Update loading bar
                _loadingBarFill.style.width = new StyleLength(new Length(progress * 100, LengthUnit.Percent));
                
                // Update loading text
                _loadingText.text = $"Loading... {Mathf.FloorToInt(progress * 100)}%";
                
                // Change tip periodically
                tipTimer += Time.deltaTime;
                if (tipTimer >= tipChangeInterval)
                {
                    tipTimer = 0f;
                    _loadingTip.text = _tips[UnityEngine.Random.Range(0, _tips.Length)];
                }
                
                yield return null;
            }
            
            // Loading complete, transition to main menu
            yield return new WaitForSeconds(0.5f);
            
            // Hide splash screen
            Hide(true);
            
            // Transition to main menu state
            GameStateManager.Instance.ChangeState<MainMenuState>();
        }
    }
}
