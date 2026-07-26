using System.Collections.Generic;

namespace Playcenter.Services
{
    /// <summary>Backend sink for analytics. Games/adapters implement (e.g. Firebase).</summary>
    public interface IAnalyticsSink
    {
        void LogEvent(string eventName, Dictionary<string, object> parameters);
        void SetUserProperty(string name, string value);
    }
}
