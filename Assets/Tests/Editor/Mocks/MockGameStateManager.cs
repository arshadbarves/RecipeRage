using System;
using Core.State;

namespace Tests.Editor.Mocks
{
    public class MockGameStateManager : IGameStateManager
    {
        public IState CurrentState { get; private set; }
        public IState PreviousState { get; private set; }

        public event Action<IState, IState> OnStateChanged;

        public void Initialize(IState initialState) { CurrentState = initialState; }
        public void Initialize() { }
        public void ChangeState(IState newState) { PreviousState = CurrentState; CurrentState = newState; OnStateChanged?.Invoke(PreviousState, CurrentState); }
        public void ChangeState<T>() where T : IState, new() { ChangeState(new T()); }
        public void Update(float deltaTime) { }
        public void FixedUpdate(float fixedDeltaTime) { }
    }
}
