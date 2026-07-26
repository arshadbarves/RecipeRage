using System.Collections;
using System.Collections.Generic;

namespace Playcenter.Services
{
    public interface IAnalyticsService
    {
        bool IsReady { get; }
        IEnumerator Initialize();
        void TrackEvent(string eventName, Dictionary<string, object> properties = null);
    }
}
