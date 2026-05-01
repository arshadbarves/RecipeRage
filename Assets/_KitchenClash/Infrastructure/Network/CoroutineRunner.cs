using System.Collections;
using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

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

        public static Coroutine Run(IEnumerator coroutine) => Instance.StartCoroutine(coroutine);

        public static void Stop(Coroutine coroutine)
        {
            if (_instance != null && coroutine != null) Instance.StopCoroutine(coroutine);
        }

        public static void StopAll()
        {
            if (_instance != null) Instance.StopAllCoroutines();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
