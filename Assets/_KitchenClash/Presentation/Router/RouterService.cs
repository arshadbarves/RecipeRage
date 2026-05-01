using System;
using System.Collections.Generic;
using KitchenClash.Domain;
using UnityEngine.UIElements;

namespace KitchenClash.Presentation
{
    /// <summary>
    /// GDD Section 4: Push/pop navigation stack with USS transitions.
    /// </summary>
    public sealed class RouterService : IRouterService
    {
        private readonly Stack<(ScreenId Id, IScreen Screen)> _stack = new();
        private readonly IReadOnlyDictionary<ScreenId, Func<IScreen>> _factory;
        private readonly VisualElement _root;

        public event Action<ScreenId> OnScreenChanged;
        public ScreenId Current => _stack.Count > 0 ? _stack.Peek().Id : ScreenId.Splash;

        public RouterService(VisualElement root, IReadOnlyDictionary<ScreenId, Func<IScreen>> factory)
        {
            _root = root;
            _factory = factory;
        }

        public void Push(ScreenId id, object param = null)
        {
            if (_stack.TryPeek(out var prev))
                prev.Screen.Exit(TransitionDir.Left);

            var screen = _factory[id]();
            screen.Enter(_root, TransitionDir.Right, param);
            _stack.Push((id, screen));
            OnScreenChanged?.Invoke(id);
        }

        public void Pop()
        {
            if (_stack.Count <= 1) return;
            _stack.Pop().Screen.Exit(TransitionDir.Right);
            _stack.Peek().Screen.Resume(TransitionDir.Left);
            OnScreenChanged?.Invoke(_stack.Peek().Id);
        }

        public void PopToRoot()
        {
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
            if (_stack.Count > 0)
                _stack.Pop().Screen.Exit(TransitionDir.Left);

            var screen = _factory[id]();
            screen.Enter(_root, TransitionDir.Right, param);
            _stack.Push((id, screen));
            OnScreenChanged?.Invoke(id);
        }
    }
}
