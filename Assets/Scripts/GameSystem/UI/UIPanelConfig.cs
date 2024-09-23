using System;
using GameSystem.UI.Effects;
using GameSystem.UI.UIPanels;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace GameSystem.UI
{
    public enum ScreenPanel
    {
        SplashScreen,
        LoadingScreen,
        MainMenu,
        Matchmaking,
    }

    [CreateAssetMenu(fileName = "UIPanelConfig", menuName = "GameSystem/UIPanelConfig")]
    public class UIPanelConfig : ScriptableObject
    {
        [Header("Panel")] [SerializeField] private ScreenPanel screenPanel;
        [SerializeField] private AssetReferenceT<VisualTreeAsset> panelUxmlAsset;

        [Header("Transition Settings")] [SerializeField]
        private bool useTransition = true;

        [SerializeField] private TransitionType transitionType = TransitionType.Fade;
        [SerializeField] private bool useTransitionOnShow = true;
        [SerializeField] private bool useTransitionOnHide = true;

        [Header("Fade Transition Settings")] [SerializeField]
        private bool transitionFadeIn = true;

        [Header("Scale Transition Settings")] [SerializeField]
        private bool transitionScaleIn = true;

        [Header("Transition Parameters")] [SerializeField]
        private float transitionDuration = 0.5f;

        [SerializeField] private float transitionDistance = 100f;

        public AssetReference PanelUxmlAsset => panelUxmlAsset;
        public Type PanelType => GetPanelType();
        public bool UseTransition => useTransition;
        public bool UseTransitionOnShow => useTransitionOnShow;
        public bool UseTransitionOnHide => useTransitionOnHide;

        private Type GetPanelType()
        {
            switch (screenPanel)
            {
                case ScreenPanel.SplashScreen:
                    return typeof(SplashScreenPanel);
                case ScreenPanel.MainMenu:
                    return typeof(MainMenuPanel);
                case ScreenPanel.Matchmaking:
                    return typeof(MatchmakingPanel);
                case ScreenPanel.LoadingScreen:
                default:
                    Debug.LogError($"Panel type not found for {screenPanel}");
                    return null;
            }
        }

        public IUIEffectTransition GetTransition()
        {
            switch (transitionType)
            {
                case TransitionType.Fade:
                    return new UIFadeEffect(transitionDuration);
                case TransitionType.Slide:
                    return new UISlideEffect(transitionDuration);
                case TransitionType.Scale:
                    return new UIScaleEffect(transitionDuration, transitionScaleIn ? 0f : transitionDistance,
                        transitionScaleIn ? transitionDistance : 0f);
                case TransitionType.Flip:
                case TransitionType.Rotate:
                case TransitionType.Bounce:
                default:
                    Debug.LogError($"Transition type not found for {transitionType}");
                    return null;
            }
        }
    }
}