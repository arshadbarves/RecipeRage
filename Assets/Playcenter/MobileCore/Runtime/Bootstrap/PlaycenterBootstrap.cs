using Playcenter.SDK;
using UnityEngine;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Sole scene entry point for the Playcenter stack. One prefab per title.
    /// Owns the SDK client and the MobileCore context; ticks all core systems.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class PlaycenterBootstrap : MonoBehaviour
    {
        public static PlaycenterBootstrap Instance { get; private set; }

        public IPlaycenterServices Services { get; private set; }
        public MobileCoreContext Core { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Core = new MobileCoreContext();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
