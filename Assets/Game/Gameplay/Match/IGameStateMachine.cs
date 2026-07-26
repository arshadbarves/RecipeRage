namespace RecipeRage
{
    public interface IGameStateMachine
    {
        IGameState CurrentState { get; }
        void ChangeState(IGameState newState);
        void Update(float deltaTime);
    }
}
