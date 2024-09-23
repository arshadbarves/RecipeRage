using System;
using Core;
using GameSystem.State;
using GameSystem.State.GameStates;
using GameSystem.UI.Base;
using GameSystem.UI.Effects;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace GameSystem.UI.UIPanels
{
    public class MatchmakingPanel : BaseUIPanel
    {
        private VisualElement _loadingElement;
        private VisualElement _topPanel;
        private VisualElement _bottomPanel;
        private Label _matchmakingLabel;
        private Label _matchmakingStatus;
        private Label _matchmakingTime;
        private Label _matchmakingPlayers;
        private Label _tipLabel;
        private Button _cancelButton;

        protected override void SetupUI()
        {
            _loadingElement = Root.Q<VisualElement>("loading-element");
            _topPanel = Root.Q<VisualElement>("top-panel");
            _bottomPanel = Root.Q<VisualElement>("bottom-panel");
            _matchmakingLabel = Root.Q<Label>("matchmaking-label");
            _cancelButton = Root.Q<Button>("cancel-button");
            _matchmakingStatus = Root.Q<Label>("matchmaking-status");
            _matchmakingPlayers = Root.Q<Label>("matchmaking-players");
            _tipLabel = Root.Q<Label>("tip-label");

            _matchmakingLabel.text = "Matchmaking";
            _matchmakingStatus.text = "Searching for players...";
            _matchmakingPlayers.text = "0/4"; // TODO: Get the number of players from the matchmaking system
            _tipLabel.text = "Tip: You can cancel the matchmaking at any time.";

            _cancelButton.clicked += () =>
            {
                GameManager.Instance.GetSystem<StateSystem>().RequestGameStateChange(GameState.MainMenu);
            };
        }

        public override void UpdatePanel()
        {
            // TODO: Update the number of players from the matchmaking system
        }

        public override void Show(Action onComplete = null)
        {
            base.Show(onComplete);

            IUIEffectTransition transition =
                new UISlideEffect(duration: 0.2f, direction: UISlideEffect.SlideDirection.Up);
            transition.ApplyTransitionIn(_topPanel, null);

            transition = new UISlideEffect(duration: 0.2f, direction: UISlideEffect.SlideDirection.Down);
            transition.ApplyTransitionIn(_bottomPanel, onComplete);

            UISpinEffect spinEffect = new UISpinEffect(duration: 2f, rotations: 1f);
            spinEffect.SetContinueSpinning(true);
            spinEffect.ApplyTransitionIn(_loadingElement, null);
        }
    }
}