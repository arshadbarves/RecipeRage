using System;

namespace Core.State
{
    public interface IGameStateManager
    {
        IState CurrentState { get; }
        IState PreviousState { get; }
        
        void Initialize(IState initialState);
        void ChangeState(IState newState);
        void ChangeState<T>() where T : IState, new();
        void Update(float deltaTime);
        void FixedUpdate(float fixedDeltaTime);
        
        event Action<IState, IState> OnStateChanged;
    }
}
