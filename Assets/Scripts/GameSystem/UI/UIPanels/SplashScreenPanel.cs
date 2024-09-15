using System;
using Core;
using GameSystem.State;
using GameSystem.State.GameStates;
using GameSystem.UI.Base;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace GameSystem.UI.UIPanels
{
    public class SplashScreenPanel : BaseUIPanel
    {
        private VisualElement _splashElement;


        protected override void SetupUI()
        {
            _splashElement = Root.Q<VisualElement>("splash-logo");
        }

        public override void Show(Action onComplete = null)
        {
            base.Show(onComplete);
            StartLogoAnimation();
        }

        void StartLogoAnimation()
        {
            _splashElement.style.opacity = 0;
            _splashElement.transform.scale = new Vector3(0.5f, 0.5f, 1);
            
            _splashElement.schedule.Execute(() =>
            {
                _splashElement.experimental.animation
                    .Start(new StyleValues { opacity = 1, height = 200, width = 1500 }, 500)
                    .Ease(Easing.OutCubic);
            }).StartingIn(500);

            // Add a delay before transitioning to the main scene
            _splashElement.schedule.Execute(TransitionToMainScene).StartingIn(2500);
        }

        void TransitionToMainScene()
        {
            // Fade out
            _splashElement.experimental.animation
                .Start(new StyleValues { opacity = 0 }, 500)
                .Ease(Easing.InCubic)
                .OnCompleted(() =>
                {
                    _splashElement.schedule.Execute(() =>
                    {
                        GameManager.Instance.GetSystem<StateSystem>().RequestGameStateChange(GameState.MainMenu);
                    }).StartingIn(2000);
                });
        }

        public override void UpdatePanel()
        {
        }
    }
}