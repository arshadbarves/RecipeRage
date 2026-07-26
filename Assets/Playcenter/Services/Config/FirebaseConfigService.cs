using System.Collections;
using System.Collections.Generic;

namespace Playcenter.Services
{
    /// <summary>
    /// Firebase Remote Config provider. Until Firebase is wired (Slice 2), serves
    /// the built-in defaults table — the same defaults every Get() call passes.
    /// </summary>
    public sealed class FirebaseConfigService : IConfigService
    {
        private readonly ILoggingService _log;
        private readonly Dictionary<string, object> _overrides = new Dictionary<string, object>();

        public bool IsReady { get; private set; }

        public FirebaseConfigService(ILoggingService log)
        {
            _log = log;
        }

        public IEnumerator Initialize()
        {
            // Firebase Remote Config fetch goes here in Slice 2.
            IsReady = true;
            _log.Log("[Config] Initialized (defaults mode, Firebase pending)");
            yield break;
        }

        public T Get<T>(string key, T fallback)
        {
            if (_overrides.TryGetValue(key, out var value) && value is T typed)
            {
                return typed;
            }
            return fallback;
        }

        /// <summary>Editor/debug hook for forcing values without Firebase.</summary>
        public void SetOverride(string key, object value)
        {
            _overrides[key] = value;
        }
    }
}
