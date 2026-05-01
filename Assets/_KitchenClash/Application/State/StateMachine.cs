using System;
using System.Collections.Generic;

namespace KitchenClash.Application.State
{
    public sealed class StateMachine
    {
        private readonly Dictionary<Type, IState> _states = new();
        private IState _current;
        private IState _previous;

        public IState Current => _current;
        public IState CurrentState => _current;
        public IState PreviousState => _previous;

        public event Action<IState, IState> OnStateChanged;

        public void RegisterState<T>(T state) where T : IState
        {
            _states[typeof(T)] = state;
        }

        public void Initialize(IState initialState)
        {
            _previous = null;
            _current = initialState;
            _current?.Enter();
        }

        public void ChangeState(IState newState)
        {
            var prev = _current;
            _current?.Exit();
            _previous = prev;
            _current = newState;
            _current?.Enter();
            OnStateChanged?.Invoke(prev, _current);
        }

        public void TransitionTo<T>() where T : IState
        {
            if (_states.TryGetValue(typeof(T), out var state))
            {
                ChangeState(state);
            }
        }

        public void Update() => _current?.Update();
        public void FixedUpdate() => _current?.FixedUpdate();
    }
}
