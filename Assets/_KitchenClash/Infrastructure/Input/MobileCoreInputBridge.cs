using Playcenter.MobileCore;

namespace KitchenClash.Infrastructure.Input
{
    /// <summary>
    /// Game-facing seam over Playcenter.MobileCore input. Prefers the bootstrap's
    /// live model; falls back to a local model so EditMode tests and headless runs
    /// work without a scene bootstrap.
    /// </summary>
    public sealed class MobileCoreInputBridge
    {
        private readonly DualStickModel _fallback;
        private readonly ManualClock _fallbackClock;

        public MobileCoreInputBridge(DualStickConfig config)
        {
            _fallbackClock = new ManualClock();
            _fallback = new DualStickModel(config, _fallbackClock);
        }

        public DualStickModel Model =>
            PlaycenterBootstrap.Instance != null && PlaycenterBootstrap.Instance.Core.Input != null
                ? PlaycenterBootstrap.Instance.Core.Input
                : _fallback;

        public InputFrame LatestFrame =>
            PlaycenterBootstrap.Instance != null
                ? PlaycenterBootstrap.Instance.Core.LatestFrame
                : _fallback.Tick();
    }
}
