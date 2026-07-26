using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Playcenter.Services
{
    /// <summary>
    /// Firebase Analytics provider. Until Firebase is wired (Slice 2), logs events
    /// through ILoggingService so funnels can be verified in the editor console.
    /// </summary>
    public sealed class FirebaseAnalyticsService : IAnalyticsService
    {
        private readonly ILoggingService _log;

        public bool IsReady { get; private set; }

        public FirebaseAnalyticsService(ILoggingService log)
        {
            _log = log;
        }

        public IEnumerator Initialize()
        {
            IsReady = true;
            _log.Log("[Analytics] Initialized (log mode, Firebase pending)");
            yield break;
        }

        public void TrackEvent(string eventName, Dictionary<string, object> properties = null)
        {
            var sb = new StringBuilder($"[Analytics] {eventName}");
            if (properties != null)
            {
                foreach (var kvp in properties)
                {
                    sb.Append($" {kvp.Key}={kvp.Value}");
                }
            }
            _log.Log(sb.ToString());
        }
    }
}
