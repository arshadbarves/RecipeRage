namespace Playcenter.MobileCore
{
    /// <summary>
    /// Container-neutral registration surface for session installers. The game's
    /// ISessionScopeFactory implementation wraps its real container builder
    /// (RecipeRage: VContainer IContainerBuilder) behind this port.
    /// </summary>
    public interface ISessionContainerBuilder
    {
        void AddSingleton<TService>(TService instance) where TService : class;
        void AddSingleton<TService, TImpl>() where TService : class where TImpl : class, TService, new();
    }
}
