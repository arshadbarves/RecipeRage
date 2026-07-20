namespace KitchenClash.Infrastructure.Flow
{
    /// <summary>
    /// Allows <see cref="Handlers.NoConnectionPhase"/> to trigger a full SDK boot retry
    /// without a direct dependency on <see cref="Composition.PlaycenterSdkBootstrap"/>.
    /// Implemented by <c>PlaycenterSdkBootstrap</c> and registered in the root container.
    /// </summary>
    public interface IPlaycenterBootRetry
    {
        void Retry();
    }

    /// <summary>
    /// Mutable reference holder that breaks the circular dependency between
    /// <c>IAppFlow</c> factory (which resolves <see cref="IPlaycenterBootRetry"/>) and
    /// <c>PlaycenterSdkBootstrap</c> (which depends on <c>IAppFlow</c>).
    /// Registered as singleton in root scope; bound by <c>PlaycenterSdkBootstrap.Start()</c>.
    /// </summary>
    public sealed class BootRetryRef : IPlaycenterBootRetry
    {
        private IPlaycenterBootRetry _target;

        public void Bind(IPlaycenterBootRetry target) => _target = target;

        public void Retry() => _target?.Retry();
    }
}
