using Core.Input;
using Gameplay.Character.Controller;
using VContainer;
using VContainer.Unity;

namespace Core
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterComponentInHierarchy<InputManager>();
            builder.RegisterComponentInHierarchy<PlayerController>();
        }
    }
}