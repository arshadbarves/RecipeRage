using System;
using System.Collections;
using System.Collections.Generic;

namespace Playcenter.Services
{
    /// <summary>
    /// AdMob provider. Until the ad SDK is wired (Slice 5), rewarded ads
    /// immediately succeed so the reward flow is testable end-to-end.
    /// </summary>
    public sealed class AdMobService : IAdsService
    {
        private readonly ILoggingService _log;
        private readonly IAnalyticsService _analytics;

        public bool IsReady { get; private set; }
        public bool IsRewardedReady => true; // stub: always ready

        public AdMobService(ILoggingService log, IAnalyticsService analytics)
        {
            _log = log;
            _analytics = analytics;
        }

        public IEnumerator Initialize()
        {
            IsReady = true;
            _log.Log("[Ads] Initialized (stub mode, AdMob pending)");
            yield break;
        }

        public void ShowRewardedAd(string placement, Action<bool> onComplete)
        {
            _log.Log($"[Ads] Rewarded ad requested: {placement} (stub — auto-success)");
            _analytics.TrackEvent("ad_rewarded_shown", new Dictionary<string, object> { { "placement", placement } });
            onComplete?.Invoke(true);
        }

        public void ShowInterstitial()
        {
            _log.Log("[Ads] Interstitial requested (stub — no-op)");
            _analytics.TrackEvent("ad_interstitial_shown");
        }
    }
}
