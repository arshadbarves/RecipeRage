using Core;
using GameSystem.UI;
using GameSystem.UI.UIPanels;
using UnityEngine;

namespace GameSystem.State.GameStates
{
    public class MainMenuState : BaseGameState
    {
        private UnityEngine.UI.Image _backgroundImage;
        private Canvas _canvas;
        private Material _backgroundMaterial;
        public MainMenuState(GameManager gameManager) : base(gameManager)
        {
            _backgroundMaterial = new Material(Shader.Find("Custom/UI Background Shader"));
        }

        public override void Enter()
        {
            GameManager.GetSystem<UISystem>().ShowPanel<MainMenuPanel>();
            
            _canvas = GameManager.GetSystem<UISystem>().GetCanvas();
            _backgroundImage = _canvas.gameObject.AddComponent<UnityEngine.UI.Image>();
            _backgroundImage.material = _backgroundMaterial;
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
            GameManager.GetSystem<UISystem>().HidePanel<MainMenuPanel>();
            Object.Destroy(_backgroundImage);
        }
    }
}