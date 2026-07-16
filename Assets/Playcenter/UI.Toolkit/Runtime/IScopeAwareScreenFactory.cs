namespace Playcenter.UI.Toolkit
{
    /// <summary>
    /// Optional extension for <see cref="IScreenInstanceFactory"/> that allows the
    /// active DI scope to be swapped at runtime (e.g. when a session scope opens).
    /// </summary>
    public interface IScopeAwareScreenFactory
    {
        void SetScope(object scope);
    }
}
