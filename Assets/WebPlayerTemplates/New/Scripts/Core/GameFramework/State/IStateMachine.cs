namespace Core.GameFramework.State
{
    public interface IStateMachine
    {
        void SetState(IState newState);
        IState GetCurrentState();
        bool IsInState<T>() where T : IState;
    }
}