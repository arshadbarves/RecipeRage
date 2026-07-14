namespace KitchenClash.Application
{
    /// <summary>
    /// Application-facing session scope lifecycle (create/destroy child container).
    /// Flow handlers depend on this contract — never on Infrastructure SessionManager.
    /// </summary>
    public interface ISessionLifecycle
    {
        bool IsSessionActive { get; }
        void CreateSession();
        void DestroySession();
    }
}
