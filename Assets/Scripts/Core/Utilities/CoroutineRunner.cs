using System.Collections;
using UnityEngine;

namespace Core.Utilities
{
    /// <summary>
    /// Static helper class to run coroutines from non-MonoBehaviour classes
    /// Follows the service-based architecture pattern
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        /// <summary>
        /// Get or create the singleton instance
        /// </summary>
        private static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("[CoroutineRunner]");
                    _instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>
        /// Start a coroutine from a non-MonoBehaviour class
        /// </summary>
        /// <param name="coroutine">The coroutine to run</param>
        /// <returns>The Coroutine handle</returns>
        public static Coroutine Run(IEnumerator coroutine)
        {
            return Instance.StartCoroutine(coroutine);
        }

        /// <summary>
        /// Stop a running coroutine
        /// </summary>
        /// <param name="coroutine">The coroutine to stop</param>
        public static void Stop(Coroutine coroutine)
        {
            if (_instance != null && coroutine != null)
            {
                Instance.StopCoroutine(coroutine);
            }
        }

        /// <summary>
        /// Stop all running coroutines
        /// </summary>
        public static void StopAll()
        {
            if (_instance != null)
            {
                Instance.StopAllCoroutines();
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
