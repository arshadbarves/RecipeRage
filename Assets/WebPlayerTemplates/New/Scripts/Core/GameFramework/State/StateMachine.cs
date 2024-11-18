using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Core.GameFramework.State
{
    public class StateMachine : IStateMachine
    {
        private readonly IObjectResolver _resolver;
        private readonly Dictionary<Type, IState> _stateCache;
        private IState _currentState;
        private bool _isTransitioning;

        [Inject]
        public StateMachine(IObjectResolver resolver)
        {
            _resolver = resolver;
            _stateCache = new Dictionary<Type, IState>();
        }

        public void SetState(IState newState)
        {
            if (newState == null)
            {
                Debug.LogError("Attempted to set null state");
                return;
            }

            if (_currentState == newState || _isTransitioning) return;

            _isTransitioning = true;
            try
            {
                var prevState = _currentState;
                _currentState?.Exit();
                _currentState = newState;
                _currentState.Enter();

                if (Debug.isDebugBuild)
                {
                    Debug.Log($"State transition: {(prevState != null ? prevState.GetType().Name : "null")} -> {newState.GetType().Name}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during state transition: {e}");
                throw;
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public void SetState<T>() where T : IState
        {
            var newState = GetOrCreateState<T>();
            SetState(newState);
        }

        public IState GetCurrentState() => _currentState;

        public bool IsInState<T>() where T : IState => _currentState is T;

        public void Update()
        {
            if (!_isTransitioning && _currentState != null)
            {
                try
                {
                    _currentState.HandleInput();
                    _currentState.Update();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error in state {_currentState.GetType().Name} update: {e}");
                }
            }
        }

        public void FixedUpdate()
        {
            if (!_isTransitioning && _currentState != null)
            {
                try
                {
                    _currentState.FixedUpdate();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error in state {_currentState.GetType().Name} fixed update: {e}");
                }
            }
        }

        private T GetOrCreateState<T>() where T : IState
        {
            var stateType = typeof(T);
            if (!_stateCache.TryGetValue(stateType, out var state))
            {
                try
                {
                    state = _resolver.Resolve<T>();
                    _stateCache[stateType] = state;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to create state of type {typeof(T).Name}: {e}");
                    throw;
                }
            }
            return (T)state;
        }

        public void Cleanup()
        {
            _currentState?.Exit();
            _currentState = null;
            _stateCache.Clear();
            _isTransitioning = false;
        }
    }
}