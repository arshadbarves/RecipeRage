using GameSystem.Gameplay;
using GameSystem.Input;
using GameSystem.State;
using GameSystem.UI;
using GameSystem.UI.UIPanels;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace Core
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // Try to use Interface instead of concrete class
            base.Configure(builder);
            builder.RegisterComponentInHierarchy<GameManager>();

            // Input System
            builder.Register<InputSystem>(Lifetime.Singleton);

            // UI System
            builder.Register<UISystem>(Lifetime.Singleton);
            builder.Register<StateSystem>(Lifetime.Singleton);
            builder.Register<IUIFactory, UIFactory>(Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<UIDocument>();
            builder.RegisterComponentInHierarchy<Canvas>();

            builder.Register<SplashScreenPanel>(Lifetime.Singleton);
            builder.Register<MainMenuPanel>(Lifetime.Singleton);
            builder.Register<MatchmakingPanel>(Lifetime.Singleton);

            // Audio System
            // builder.Register<AudioSystem>(Lifetime.Singleton);
            // builder.Register<IAudioService, AdvancedAudioSystem>(Lifetime.Singleton);

            // Gameplay System
            builder.Register<GameplaySystem>(Lifetime.Singleton);
        }
    }
}
