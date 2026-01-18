using System;
using Core.Logging;
using VContainer;

namespace Gameplay.App.State
{
    public class GameStateManager : IGameStateManager, IDisposable
    {
        private readonly StateMachine _stateMachine;
        private readonly IObjectResolver _container;

        public event Action<IState, IState> OnStateChanged;

        public IState CurrentState => _stateMachine.CurrentState;
        public IState PreviousState => _stateMachine.PreviousState;

        [Inject]
        public GameStateManager(IObjectResolver container)
        {
            _stateMachine = new StateMachine();
            _container = container;
            _stateMachine.OnStateChanged += (prev, curr) => OnStateChanged?.Invoke(prev, curr);
        }

        public void Initialize(IState initialState)
        {
            GameLogger.Log($"Initializing with state: {initialState?.GetType().Name}");
            _stateMachine.Initialize(initialState);
        }

        public void ChangeState(IState newState)
        {
            GameLogger.Log($"Changing state to: {newState?.GetType().Name}");
            _stateMachine.ChangeState(newState);
        }

        public void ChangeState<T>() where T : IState
        {
            ChangeState(_container.Resolve<T>());
        }

        public void Update(float deltaTime)
        {
            _stateMachine.Update();
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            _stateMachine.FixedUpdate();
        }

        public void Dispose()
        {
            GameLogger.Log("Disposing GameStateManager");

            // Exit current state
            CurrentState?.Exit();
        }
    }
}