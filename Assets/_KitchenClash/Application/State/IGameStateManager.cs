using System;

namespace KitchenClash.Application.State
{
    public interface IGameStateManager
    {
        IState CurrentState { get; }
        IState PreviousState { get; }

        void Initialize(IState initialState);
        void ChangeState(IState newState);
        void ChangeState<T>() where T : IState;
        void Update(float deltaTime);
        void FixedUpdate(float fixedDeltaTime);

        event Action<IState, IState> OnStateChanged;
    }
}
