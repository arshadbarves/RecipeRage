namespace Core.Interfaces
{
    /// <summary>
    /// Interface for services that can be reset without full disposal
    /// Used for services that need to clear user-specific data on logout
    /// but can continue to exist (e.g., CurrencyService)
    /// </summary>
    public interface IResettableService
    {
        /// <summary>
        /// Reset the service to its initial state
        /// Clears user-specific data but keeps the service alive
        /// </summary>
        void Reset();
    }
}
