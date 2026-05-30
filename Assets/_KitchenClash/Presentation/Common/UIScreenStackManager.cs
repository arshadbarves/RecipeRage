using System;
using System.Collections.Generic;
using KitchenClash.Domain;

namespace KitchenClash.Presentation.Common
{
    /// <summary>
    /// Manages UI screen stacks per category.
    /// </summary>
    public class UIScreenStackManager : IUIScreenStackManager
    {
        private readonly Dictionary<UIScreenCategory, Stack<Type>> _stacks = new();

        public void Push(Type screenType, UIScreenCategory category)
        {
            if (!_stacks.TryGetValue(category, out Stack<Type> stack))
            {
                stack = new Stack<Type>();
                _stacks[category] = stack;
            }
            stack.Push(screenType);
        }

        public Type Pop(UIScreenCategory category)
        {
            if (_stacks.TryGetValue(category, out Stack<Type> stack) && stack.Count > 0)
                return stack.Pop();
            return null;
        }

        public Type Peek(UIScreenCategory category)
        {
            if (_stacks.TryGetValue(category, out Stack<Type> stack) && stack.Count > 0)
                return stack.Peek();
            return null;
        }

        public void PopSpecific(Type screenType, UIScreenCategory category)
        {
            if (!_stacks.TryGetValue(category, out Stack<Type> stack)) return;

            Stack<Type> temp = new Stack<Type>();
            while (stack.Count > 0)
            {
                Type current = stack.Pop();
                if (current != screenType)
                    temp.Push(current);
            }
            while (temp.Count > 0)
                stack.Push(temp.Pop());
        }

        public void ClearCategory(UIScreenCategory category)
        {
            if (_stacks.TryGetValue(category, out Stack<Type> stack))
                stack.Clear();
        }

        public void ClearAll()
        {
            _stacks.Clear();
        }

        public bool IsInHistory(Type screenType)
        {
            foreach (Stack<Type> stack in _stacks.Values)
            {
                if (stack.Contains(screenType))
                    return true;
            }
            return false;
        }

        public int GetStackDepth(UIScreenCategory category)
        {
            return _stacks.TryGetValue(category, out Stack<Type> stack) ? stack.Count : 0;
        }
    }
}
