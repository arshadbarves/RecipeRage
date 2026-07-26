using System.Collections.Generic;

namespace Playcenter.Services
{
    /// <summary>Shared analytics: sanitizes params and forwards to an <see cref="IAnalyticsSink"/>.</summary>
    public sealed class AnalyticsService : IAnalyticsService
    {
        private static readonly Dictionary<string, object> Empty = new();
        private readonly IAnalyticsSink _sink;

        public AnalyticsService(IAnalyticsSink sink)
        {
            _sink = sink;
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }
            _sink?.LogEvent(eventName, parameters ?? Empty);
        }

        public void SetUserProperty(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            _sink?.SetUserProperty(name, value);
        }
    }
}
