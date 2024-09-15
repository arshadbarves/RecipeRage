using Core;
using GameSystem.UI;
using GameSystem.UI.UIPanels;

namespace GameSystem.State.GameStates
{
    public class MatchmakingState : BaseGameState
    {
        public MatchmakingState(GameManager gameManager) : base(gameManager)
        {
        }

        public override void Enter()
        {
            GameManager.GetSystem<UISystem>().ShowPanel<MatchmakingPanel>();
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
            GameManager.GetSystem<UISystem>().HidePanel<MatchmakingPanel>();
        }
    }
}