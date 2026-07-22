using System.Collections.Generic;
using Playcenter.Services;
using UnityEngine;

namespace Playcenter.Services.Unity
{
    /// <summary>Firebase Analytics sink. Compiles to a debug-log sink when Firebase is absent.</summary>
    public sealed class FirebaseAnalyticsSink : IAnalyticsSink
    {
        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
#if FIREBASE_ANALYTICS
            var firebaseParams = new Firebase.Analytics.Parameter[parameters?.Count ?? 0];
            if (parameters != null)
            {
                int i = 0;
                foreach (KeyValuePair<string, object> kvp in parameters)
                {
                    firebaseParams[i++] = kvp.Value switch
                    {
                        int v => new Firebase.Analytics.Parameter(kvp.Key, v),
                        long v => new Firebase.Analytics.Parameter(kvp.Key, v),
                        float v => new Firebase.Analytics.Parameter(kvp.Key, v),
                        double v => new Firebase.Analytics.Parameter(kvp.Key, v),
                        _ => new Firebase.Analytics.Parameter(kvp.Key, kvp.Value?.ToString() ?? string.Empty)
                    };
                }
            }
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, firebaseParams);
#else
            Debug.Log(Format(eventName, parameters));
#endif
        }

        public void SetUserProperty(string name, string value)
        {
#if FIREBASE_ANALYTICS
            Firebase.Analytics.FirebaseAnalytics.SetUserProperty(name, value);
#else
            Debug.Log($"[Analytics] prop {name}={value}");
#endif
        }

#if !FIREBASE_ANALYTICS
        private static string Format(string eventName, Dictionary<string, object> ps)
        {
            if (ps == null || ps.Count == 0)
            {
                return $"[Analytics] {eventName}";
            }
            var sb = new System.Text.StringBuilder($"[Analytics] {eventName} {{ ");
            foreach (KeyValuePair<string, object> kvp in ps)
            {
                sb.Append($"{kvp.Key}={kvp.Value}, ");
            }
            sb.Append('}');
            return sb.ToString();
        }
#endif
    }
}
