using System;
using System.Collections.Generic;
using Core.Animation;
using Core.Logging;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components.Tabs
{
    /// <summary>
    /// Reusable Tab System component.
    /// Handles navigation logic and visual transitions between tabs.
    /// Follows SOLID: Single Responsibility and Open/Closed principles.
    /// </summary>
    public class TabSystem : IDisposable
    {
        private readonly Dictionary<string, TabData> _tabs = new Dictionary<string, TabData>();
        private string _activeTabId;
        private readonly IUIAnimator _animator;
        private readonly VisualElement _contentRoot;

        public event Action<string> OnTabChanged;

        public TabSystem(VisualElement contentRoot, IUIAnimator animator)
        {
            _contentRoot = contentRoot ?? throw new ArgumentNullException(nameof(contentRoot));
            _animator = animator ?? throw new ArgumentNullException(nameof(animator));
        }

        public void AddTab(string tabId, Button button, ITabComponent content)
        {
            if (string.IsNullOrEmpty(tabId)) throw new ArgumentException("Tab ID cannot be null or empty");
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (content == null) throw new ArgumentNullException(nameof(content));

            var data = new TabData
            {
                Id = tabId,
                Button = button,
                Content = content
            };

            _tabs[tabId] = data;

            // Register click handler
            button.clicked += () => SwitchToTab(tabId);
            
            GameLogger.Log($"Tab added: {tabId}");
        }

        public void SwitchToTab(string tabId, bool immediate = false)
        {
            if (!_tabs.ContainsKey(tabId))
            {
                GameLogger.LogError($"Tab ID '{tabId}' not found in system");
                return;
            }

            if (_activeTabId == tabId) return;

            string previousTabId = _activeTabId;
            _activeTabId = tabId;

            // 1. Handle Visual Button states
            foreach (var kvp in _tabs)
            {
                UpdateTabButtonVisual(kvp.Value, kvp.Key == tabId, immediate);
            }

            // 2. Handle Content visibility
            if (!string.IsNullOrEmpty(previousTabId))
            {
                _tabs[previousTabId].Content.OnHide();
            }

            _tabs[tabId].Content.OnShow();

            GameLogger.Log($"Switched from {previousTabId} to {tabId}");
            OnTabChanged?.Invoke(tabId);
        }

        private void UpdateTabButtonVisual(TabData tab, bool isActive, bool immediate)
        {
            if (tab.Button == null) return;

            if (isActive)
            {
                tab.Button.AddToClassList("active");
                
                if (!immediate && _animator != null)
                {
                    // Brawl Stars style: Active tab slides slightly right
                    // The CSS uses translateX(10px) on .active
                    // We can animate this using the UI animator if needed, 
                    // but for now we rely on USS classes for static properties
                }
            }
            else
            {
                tab.Button.RemoveFromClassList("active");
            }
        }

        public void Update(float deltaTime)
        {
            if (!string.IsNullOrEmpty(_activeTabId))
            {
                _tabs[_activeTabId].Content.Update(deltaTime);
            }
        }

        public void Dispose()
        {
            foreach (var tab in _tabs.Values)
            {
                tab.Content.Dispose();
                // Button click handlers are cleaned up by UI Toolkit when destroyed
            }
            _tabs.Clear();
        }

        private class TabData
        {
            public string Id;
            public Button Button;
            public ITabComponent Content;
        }
    }
}
