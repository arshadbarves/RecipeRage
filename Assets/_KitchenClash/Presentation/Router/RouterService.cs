using System;
using System.Collections.Generic;
using KitchenClash.Domain;
using UnityEngine.UIElements;

namespace KitchenClash.Presentation
{
    /// <summary>
    /// GDD Section 4: Push/pop navigation stack with USS transitions.
    /// Supports lazy initialization so it can be registered as a singleton in DI
    /// before the UIDocument root and screen factory are available.
    /// </summary>
    public sealed class RouterService : IRouterService
    {
        private readonly Stack<(ScreenId Id, IScreen Screen)> _stack = new();
        private IReadOnlyDictionary<ScreenId, Func<IScreen>> _factory;
        private VisualElement _root;
        private bool _initialized;

        public event Action<ScreenId> OnScreenChanged;
        public ScreenId Current => _stack.Count > 0 ? _stack.Peek().Id : ScreenId.Splash;

        /// <summary>
        /// Parameterless constructor for DI container registration.
        /// Call <see cref="Initialize"/> before first navigation.
        /// </summary>
        public RouterService() { }

        /// <summary>
        /// Legacy constructor kept for backward compatibility.
        /// </summary>
        public RouterService(VisualElement root, IReadOnlyDictionary<ScreenId, Func<IScreen>> factory)
        {
            Initialize(root, factory);
        }

        /// <summary>
        /// Lazily sets the root VisualElement and screen factory after UIDocument is ready.
        /// </summary>
        public void Initialize(VisualElement root, IReadOnlyDictionary<ScreenId, Func<IScreen>> factory)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _initialized = true;
        }

        public bool IsInitialized => _initialized;

        public void Push(ScreenId id, object param = null)
        {
            EnsureInitialized();
            if (_stack.TryPeek(out var prev))
                prev.Screen.Exit(TransitionDir.Left);

            var screen = _factory[id]();
            screen.Enter(_root, TransitionDir.Right, param);
            _stack.Push((id, screen));
            OnScreenChanged?.Invoke(id);
        }

        public void Pop()
        {
            EnsureInitialized();
            if (_stack.Count <= 1) return;
            _stack.Pop().Screen.Exit(TransitionDir.Right);
            _stack.Peek().Screen.Resume(TransitionDir.Left);
            OnScreenChanged?.Invoke(_stack.Peek().Id);
        }

        public void PopToRoot()
        {
            EnsureInitialized();
            while (_stack.Count > 1)
            {
                _stack.Pop().Screen.Exit(TransitionDir.Right);
            }
            if (_stack.Count > 0)
            {
                _stack.Peek().Screen.Resume(TransitionDir.Left);
                OnScreenChanged?.Invoke(_stack.Peek().Id);
            }
        }

        public void Replace(ScreenId id, object param = null)
        {
            EnsureInitialized();
            if (_stack.Count > 0)
                _stack.Pop().Screen.Exit(TransitionDir.Left);

            var screen = _factory[id]();
            screen.Enter(_root, TransitionDir.Right, param);
            _stack.Push((id, screen));
            OnScreenChanged?.Invoke(id);
        }

        private void EnsureInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException(
                    "RouterService.Initialize(root, factory) must be called before navigation.");
        }
    }
}
