using System.Collections.Generic;

namespace Playcenter.Services
{
    /// <summary>Default sink that formats events to the debug log. Engine-free; games may swap.</summary>
    public sealed class DebugAnalyticsSink : IAnalyticsSink
    {
        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            // Engine-free: no UnityEngine.Debug. Games wire a real sink; this is a safe no-op default.
            System.Diagnostics.Debug.WriteLine(Format(eventName, parameters));
        }

        public void SetUserProperty(string name, string value)
        {
            System.Diagnostics.Debug.WriteLine($"[Analytics] prop {name}={value}");
        }

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
    }
}
