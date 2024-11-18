using System;
using Core;

namespace GameSystem.State.GameStates
{
    public class LoadingState : BaseGameState
    {
        public LoadingState(GameManager gameManager) : base(gameManager)
        {
        }

        public override void Enter()
        {
            // GameManager.GetSystem<UISystem>().ShowPanel<LoadingPanel>();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Exit()
        {
            throw new NotImplementedException();
        }
    }
}