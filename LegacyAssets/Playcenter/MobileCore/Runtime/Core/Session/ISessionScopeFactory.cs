namespace Playcenter.MobileCore
{
    /// <summary>Implemented game-side (RecipeRage: wraps VContainer LifetimeScope child).</summary>
    public interface ISessionScopeFactory
    {
        ISessionScopeHandle Create(ISessionScopeInstaller installer);
    }
}
