using System.Collections.Generic;

namespace Playcenter.Services
{
    public interface IAnalyticsService
    {
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);
        void SetUserProperty(string name, string value);
    }
}
