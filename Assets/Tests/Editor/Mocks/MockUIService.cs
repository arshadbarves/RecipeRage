using System;
using System.Collections.Generic;
using Core.RemoteConfig;
using Core.RemoteConfig.Models;
using Cysharp.Threading.Tasks;
using UI;
using UI.Core;
using UI.Screens;
using UnityEngine.UIElements;

namespace Tests.Editor.Mocks
{
    public class MockUIService : IUIService
    {
        public List<UIScreenType> ShownScreens = new List<UIScreenType>();
        public List<UIScreenType> HiddenScreens = new List<UIScreenType>();
        public ConfigHealthStatus HealthStatus => ConfigHealthStatus.Healthy;
        public DateTime LastUpdateTime => DateTime.MinValue;
        public bool IsInitialized => true;

        public event Action<IConfigModel> OnConfigUpdated;
        public event Action<Type, IConfigModel> OnSpecificConfigUpdated;
        public event Action<ConfigHealthStatus> OnHealthStatusChanged;
        public event Action<UIScreenType> OnScreenShown;
        public event Action<UIScreenType> OnScreenHidden;
        public event Action OnAllScreensHidden;

        public void Initialize() { }
        public void Initialize(UIDocument uiDocument) { }
        public void InitializeScreens() { }
        public void Dispose() { }

        public void ShowScreen(UIScreenType screenType, bool animate = true, bool addToHistory = true)
        {
            ShownScreens.Add(screenType);
            OnScreenShown?.Invoke(screenType);
        }

        public void HideScreen(UIScreenType screenType, bool animate = true)
        {
            HiddenScreens.Add(screenType);
            OnScreenHidden?.Invoke(screenType);
        }

        public T GetScreen<T>() where T : BaseUIScreen => null;
        public BaseUIScreen GetScreen(UIScreenType screenType) => null;
        public T GetScreen<T>(UIScreenType screenType) where T : BaseUIScreen => null;
        
        public bool TryGetConfig<T>(out T config) where T : class, IConfigModel { config = null; return false; }
        public T GetConfig<T>() where T : class, IConfigModel => null;
        public UniTask<bool> RefreshConfig() => UniTask.FromResult(true);
        public UniTask<bool> RefreshConfig<T>() where T : class, IConfigModel => UniTask.FromResult(true);
        
        public void HideScreensOfType(UIScreenType screenType, bool animate = true) { }
        public void HideAllPopups(bool animate = true) { }
        public void HideAllModals(bool animate = true) { }
        public void HideAllGameScreens(bool animate = true) { }
        public void HideAllScreens(bool animate = false) { }
        public bool GoBack(bool animate = true) => false;
        public bool IsScreenVisible(UIScreenType screenType) => false;
        public IReadOnlyList<BaseUIScreen> GetVisibleScreens() => new List<BaseUIScreen>();
        public IReadOnlyList<BaseUIScreen> GetScreensByPriority() => new List<BaseUIScreen>();
        public void ClearHistory() { }
        public UIScreenCategory GetScreenCategory(UIScreenType screenType) => UIScreenCategory.Screen;
        public bool IsInteractionBlocked(UIScreenType screenType) => false;
        public UniTask ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f) => UniTask.CompletedTask;
        public UniTask ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f) => UniTask.CompletedTask;

        public void Update(float deltaTime) { }
    }
}
