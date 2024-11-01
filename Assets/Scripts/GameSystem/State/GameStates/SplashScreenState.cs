using Core;
using GameSystem.UI;
using GameSystem.UI.UIPanels;

namespace GameSystem.State.GameStates
{
    public class SplashScreenState : BaseGameState
    {
        public SplashScreenState(GameManager gameManager) : base(gameManager)
        {
        }

        public override void Enter()
        {
            GameManager.GetSystem<UISystem>().ShowPanel<SplashScreenPanel>();
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
            GameManager.GetSystem<UISystem>().HidePanel<SplashScreenPanel>();
        }
    }
}