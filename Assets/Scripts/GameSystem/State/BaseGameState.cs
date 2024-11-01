using Core;
using GameSystem.State.GameStates;

namespace GameSystem.State
{
    public abstract class BaseGameState : IState<GameState>
    {
        protected readonly GameManager GameManager;

        protected BaseGameState(GameManager gameManager)
        {
            GameManager = gameManager;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
    }
}