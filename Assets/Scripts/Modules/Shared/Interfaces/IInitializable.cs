namespace Core.Bootstrap
{
    /// <summary>
    /// Interface for services that require two-phase initialization.
    /// Similar to Unity's Awake/Start pattern:
    /// - Constructor: Set up internal state, store dependencies
    /// - Initialize: Safe to access other services, perform cross-service setup
    /// </summary>
    public interface IInitializable
    {
        /// <summary>
        /// Called after all services are constructed.
        /// Safe to access other services through VContainer here.
        /// </summary>
        void Initialize();
    }
}
