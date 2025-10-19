using System;
using UnityEngine;

namespace Core.State
{
    /// <summary>
    /// Game state manager - pure C# class, no MonoBehaviour
    /// Implements IDisposable for proper cleanup on logout
    /// </summary>
    public class GameStateManager : IGameStateManager, IDisposable
    {
        private readonly IStateMachine _stateMachine;

        public event Action<IState, IState> OnStateChanged;

        public IState CurrentState => _stateMachine.CurrentState;
        public IState PreviousState => _stateMachine.PreviousState;

        public GameStateManager()
        {
            _stateMachine = new StateMachine();
            _stateMachine.OnStateChanged += (prev, curr) => OnStateChanged?.Invoke(prev, curr);
        }

        public void Initialize(IState initialState)
        {
            _stateMachine.Initialize(initialState);
        }

        public void ChangeState(IState newState)
        {
            _stateMachine.ChangeState(newState);
        }

        public void ChangeState<T>() where T : IState, new()
        {
            ChangeState(new T());
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
            Debug.Log("[GameStateManager] Disposing");

            // Exit current state
            CurrentState?.Exit();

            // Clear state machine
            // Note: StateMachine doesn't have a Dispose method, so we just clear references
        }
    }
}
