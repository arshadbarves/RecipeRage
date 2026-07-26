namespace RecipeRage
{
    public interface IGameState
    {
        void Enter();
        void Exit();
        void Update(float deltaTime);
    }
}
