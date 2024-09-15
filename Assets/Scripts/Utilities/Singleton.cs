using Unity.Netcode;
using UnityEngine;

namespace Utilities
{
    // Mono Singleton
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        private static readonly object Lock = new object();
        private static bool _applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning(
                        $"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindFirstObjectByType(typeof(T));

                        if (FindObjectsByType(typeof(T), FindObjectsSortMode.None).Length > 1)
                        {
                            Debug.LogError(
                                $"[Singleton] Something went really wrong - there should never be more than 1 singleton! Reopening the scene might fix it.");
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"{typeof(T)} (Singleton)";

                            DontDestroyOnLoad(singletonObject);

                            Debug.Log(
                                $"[Singleton] An instance of {typeof(T)} is needed in the scene, so '{singletonObject}' was created with DontDestroyOnLoad.");
                        }
                        else
                        {
                            Debug.Log($"[Singleton] Using instance already created: {_instance.gameObject.name}");
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = (T)this;
                DontDestroyOnLoad(this.gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _applicationIsQuitting = true;
            }
        }
    }

// Network Singleton
    public class NetworkSingleton<T> : NetworkBehaviour where T : NetworkSingleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                    if (_instance == null)
                    {
                        Debug.LogError($"[NetworkSingleton] No instance of {typeof(T)} found in the scene!");
                    }
                }

                return _instance;
            }
        }

        public override void OnNetworkSpawn()
        {
            if (_instance == null)
            {
                _instance = (T)this;
            }
            else if (_instance != this)
            {
                // If an instance already exists and it's not this one, destroy this instance
                Destroy(gameObject);
            }

            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            base.OnNetworkDespawn();
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = (T)this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}