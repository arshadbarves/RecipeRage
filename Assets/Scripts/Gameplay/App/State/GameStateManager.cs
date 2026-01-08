using System;
using Modules.Shared.Interfaces;
using Modules.Logging;
using VContainer;

namespace Gameplay.App.State
{
    /// <summary>
    /// Game state manager - pure C# class, no MonoBehaviour
    /// Implements IDisposable for proper cleanup on logout
    /// </summary>
    public class GameStateManager : IGameStateManager, IDisposable
    {
        private readonly IStateMachine _stateMachine;
        private readonly IStateFactory _stateFactory;
        private readonly ILoggingService _logger;

        public event Action<IState, IState> OnStateChanged;

        public IState CurrentState => _stateMachine.CurrentState;
        public IState PreviousState => _stateMachine.PreviousState;

        [Inject]
        public GameStateManager(IStateFactory stateFactory, ILoggingService logger)
        {
            _stateMachine = new StateMachine();
            _stateFactory = stateFactory;
            _logger = logger;
            _stateMachine.OnStateChanged += (prev, curr) => OnStateChanged?.Invoke(prev, curr);
        }

        /// <summary>
        /// Called after all services are constructed (IInitializable).
        /// </summary>
        void IInitializable.Initialize()
        {
            // GameStateManager uses Initialize(IState) for state-specific setup
        }

        public void Initialize(IState initialState)
        {
            _logger.Log($"Initializing with state: {initialState?.GetType().Name}");
            _stateMachine.Initialize(initialState);
        }

        public void ChangeState(IState newState)
        {
            _logger.Log($"Changing state to: {newState?.GetType().Name}");
            _stateMachine.ChangeState(newState);
        }

        public void ChangeState<T>() where T : IState
        {
            ChangeState(_stateFactory.CreateState<T>());
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
            _logger.Log("Disposing GameStateManager");

            // Exit current state
            CurrentState?.Exit();

            // Clear state machine
            // Note: StateMachine doesn't have a Dispose method, so we just clear references
        }
    }
}
