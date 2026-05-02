using System.Collections.Generic;
using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Infrastructure.Analytics
{
    public sealed class FirebaseAnalyticsService : IAnalyticsService
    {
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
#if FIREBASE_ANALYTICS
            var firebaseParams = new Firebase.Analytics.Parameter[parameters?.Count ?? 0];
            if (parameters != null)
            {
                int i = 0;
                foreach (var kvp in parameters)
                {
                    firebaseParams[i++] = kvp.Value switch
                    {
                        int v => new Firebase.Analytics.Parameter(kvp.Key, v),
                        long v => new Firebase.Analytics.Parameter(kvp.Key, v),
                        float v => new Firebase.Analytics.Parameter(kvp.Key, v),
                        double v => new Firebase.Analytics.Parameter(kvp.Key, v),
                        _ => new Firebase.Analytics.Parameter(kvp.Key, kvp.Value?.ToString() ?? "")
                    };
                }
            }
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, firebaseParams);
#else
            if (parameters != null && parameters.Count > 0)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append($"[Analytics] {eventName} {{ ");
                foreach (var kvp in parameters)
                    sb.Append($"{kvp.Key}={kvp.Value}, ");
                sb.Append("}");
                Debug.Log(sb.ToString());
            }
            else
            {
                Debug.Log($"[Analytics] {eventName}");
            }
#endif
        }

        public void SetUserProperty(string name, string value)
        {
#if FIREBASE_ANALYTICS
            Firebase.Analytics.FirebaseAnalytics.SetUserProperty(name, value);
#else
            Debug.Log($"[Analytics] UserProperty {name}={value}");
#endif
        }
    }
}
