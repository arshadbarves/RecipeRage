namespace KitchenClash.Application.State
{
    public interface IState
    {
        string StateName { get; }
        void Enter();
        void Exit();
        void Update();
        void FixedUpdate();
    }
}
