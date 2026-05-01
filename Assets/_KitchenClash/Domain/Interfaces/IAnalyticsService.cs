using System.Collections.Generic;

namespace KitchenClash.Domain
{
    public interface IAnalyticsService
    {
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);
        void SetUserProperty(string name, string value);
    }
}
