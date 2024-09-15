using System;
using System.Threading.Tasks;
using GameSystem.UI.Effects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace GameSystem.UI.Base
{
    public abstract class BaseUIPanel : IUIPanel
    {
        protected IUITransition Transition;
        protected UIPanelConfig Config;
        public VisualElement Root { get; set; }

        public virtual async Task InitializeAsync(UIPanelConfig config)
        {
            Config = config;
            AsyncOperationHandle<VisualTreeAsset> uxmlHandle =
                Addressables.LoadAssetAsync<VisualTreeAsset>(Config.PanelUxmlAsset);
            await uxmlHandle.Task;
            if (uxmlHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Root = uxmlHandle.Result.Instantiate();
                Root.style.display = DisplayStyle.None;
                Root.style.position = Position.Absolute;
                Root.style.left = 0;
                Root.style.right = 0;
                Root.style.top = 0;
                Root.style.bottom = 0;
            }
            else
            {
                Debug.LogError($"Failed to load UXML for {Config.PanelUxmlAsset}");
            }

            ConfigureTransitions();
            SetupUI();
        }

        protected abstract void SetupUI();

        private void ConfigureTransitions()
        {
            // Configure transition based on UIPanelConfig settings
            switch (Config.TransitionType)
            {
                case TransitionType.Fade:
                    Transition = new FadeTransition(Config.TransitionDuration);
                    break;
                case TransitionType.Slide:
                    Transition = new SlideTransition(Config.TransitionDuration, Config.TransitionDistance);
                    break;
                case TransitionType.Scale:
                    Transition = new ScaleTransition(Config.TransitionDuration, Config.TransitionScaleIn,
                        Config.TransitionScaleOut);
                    break;
                default:
                    Transition = new FadeTransition(Config.TransitionDuration);
                    break;
            }
        }

        public virtual void Show(Action onComplete = null)
        {
            if (Config.UseTransition && Config.UseTransitionOnShow)
            {
                Transition.TransitionIn(Root, onComplete);
            }
            else
            {
                // Ensure the panel is fully visible even if no transition is applied
                Root.style.display = DisplayStyle.Flex;
                Root.style.opacity = 1; // Set opacity to 1 in case it was set to 0 previously
                onComplete?.Invoke();
            }
        }

        public virtual void Hide(Action onComplete = null)
        {
            if (Config.UseTransition && Config.UseTransitionOnHide)
            {
                // Ensure the panel is visible before applying hide transition
                Root.style.display = DisplayStyle.Flex; // Ensure the element is visible first
                Root.style.opacity = 1; // Ensure opacity is 1 before animating the hide transition
                Transition.TransitionOut(Root, onComplete);
            }
            else
            {
                // Directly hide the panel without transition
                Root.style.display = DisplayStyle.None;
                onComplete?.Invoke();
            }
        }

        public abstract void UpdatePanel();

        public virtual Task CleanupAsync()
        {
            Root?.RemoveFromHierarchy();
            return Task.CompletedTask;
        }
    }
}