using System;
using KitchenClash.Infrastructure.Logging;
using VContainer;
using VContainer.Unity;

namespace KitchenClash.Application.State
{
    public class GameStateManager : IGameStateManager, IDisposable, ITickable
    {
        private readonly StateMachine _stateMachine;
        private readonly IStateFactory _stateFactory;

        public event Action<IState, IState> OnStateChanged;

        public IState CurrentState => _stateMachine.CurrentState;
        public IState PreviousState => _stateMachine.PreviousState;

        [Inject]
        public GameStateManager(IStateFactory stateFactory)
        {
            _stateMachine = new StateMachine();
            _stateFactory = stateFactory;
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
            ChangeState(_stateFactory.Create<T>());
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
            CurrentState?.Exit();
        }

        public void Tick()
        {
            Update(UnityEngine.Time.deltaTime);
        }
    }
}
