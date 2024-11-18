using Core;
using GameSystem.State;
using GameSystem.State.GameStates;
using GameSystem.UI.Base;
using UnityEngine.UIElements;

namespace GameSystem.UI.UIPanels
{
    public class MainMenuPanel : BaseUIPanel
    {
        private Button _gameModeButton;
        private Button _playButton;
        protected override void SetupUI()
        {
            _playButton = Root.Q<Button>("play-button");
            _gameModeButton = Root.Q<Button>("game-mode-button");

            _playButton.clicked += () =>
            {
                GameManager.Instance.GetSystem<StateSystem>().RequestGameStateChange(GameState.Matchmaking);
            };

            _gameModeButton.clicked += () =>
            {
            };
        }

        public override void UpdatePanel()
        {
        }
    }
}