using System;
using Modules.Shared.Interfaces;

namespace App.State
{
    public interface IGameStateManager : IInitializable
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
