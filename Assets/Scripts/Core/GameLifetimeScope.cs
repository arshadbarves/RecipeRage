using GameSystem;
using GameSystem.State;
using GameSystem.UI;
using GameSystem.UI.Base;
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
            base.Configure(builder);
            builder.RegisterComponentInHierarchy<GameManager>();
            builder.Register<InputSystem>(Lifetime.Singleton);
            builder.Register<UISystem>(Lifetime.Singleton);
            builder.Register<StateSystem>(Lifetime.Singleton);
            builder.Register<IUIFactory, UIFactory>(Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<UIDocument>();
            builder.RegisterComponentInHierarchy<Canvas>();

            builder.Register<SplashScreenPanel>(Lifetime.Singleton);
            builder.Register<MainMenuPanel>(Lifetime.Singleton);
            builder.Register<MatchmakingPanel>(Lifetime.Singleton);
        }
    }
}