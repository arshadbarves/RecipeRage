namespace RecipeRage
{
    public sealed class GameStateMachine : IGameStateMachine
    {
        public IGameState CurrentState { get; private set; }

        public void ChangeState(IGameState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        public void Update(float deltaTime)
        {
            CurrentState?.Update(deltaTime);
        }
    }
}
