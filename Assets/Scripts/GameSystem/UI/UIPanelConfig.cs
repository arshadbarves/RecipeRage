using System;
using System.Collections.Generic;
using GameSystem.UI.Base;
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
        [Header("Panel")]
        [SerializeField] private ScreenPanel screenPanel;
        [SerializeField] private AssetReferenceT<VisualTreeAsset> panelUxmlAsset;
        
        [Header("Transition Settings")] [SerializeField]
        private bool useTransition = true;

        [SerializeField] private TransitionType transitionType = TransitionType.Fade;
        [SerializeField] private bool useTransitionOnShow = true;
        [SerializeField] private bool useTransitionOnHide = true;

        [Header("Transition Parameters")] [SerializeField]
        private float transitionDuration = 0.5f;

        [SerializeField] private float transitionDistance = 100f;
        [SerializeField] private float transitionScaleIn = 1.2f;
        [SerializeField] private float transitionScaleOut = 0.8f;
        
        public AssetReference PanelUxmlAsset => panelUxmlAsset;
        public Type PanelType => GetPanelType();
        public bool UseTransition => useTransition;
        public TransitionType TransitionType => transitionType;
        public bool UseTransitionOnShow => useTransitionOnShow;
        public bool UseTransitionOnHide => useTransitionOnHide;
        public float TransitionDuration => transitionDuration;
        public float TransitionDistance => transitionDistance;
        public float TransitionScaleIn => transitionScaleIn;
        public float TransitionScaleOut => transitionScaleOut;
        
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}