using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.State
{
    public interface IState<T> where T : Enum
    {
        void Enter();
        void Update();
        void Exit();
    }
    
    public class StateMachine<T> where T : Enum
    {
        private readonly Dictionary<T, IState<T>> _states = new Dictionary<T, IState<T>>();
        private IState<T> _currentState;

        public event Action<T> OnStateChanged;

        public void AddState(T stateKey, IState<T> state)
        {
            _states[stateKey] = state;
        }

        public void TransitionTo(T stateKey)
        {
            if (!_states.TryGetValue(stateKey, out var newState))
            {
                Debug.LogError($"State {stateKey} not found in the state machine.");
                return;
            }

            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();

            OnStateChanged?.Invoke(stateKey);
        }

        public void Update()
        {
            _currentState?.Update();
        }
    }
}