using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KitchenClash.Application
{
    public sealed class StateMachine
    {
        private readonly Dictionary<Type, IState> _states = new();
        private IState _current;

        public IState Current => _current;

        public void RegisterState<T>(T state) where T : IState
        {
            _states[typeof(T)] = state;
        }

        public async Task TransitionToAsync<T>() where T : IState
        {
            if (_current != null)
                await _current.ExitAsync();

            _current = _states[typeof(T)];
            await _current.EnterAsync();
        }

        public void Tick() => _current?.Tick();
    }
}
