using UnityEngine;

namespace RecipeRage.Core.Patterns
{
    /// <summary>
    /// Generic singleton implementation for MonoBehaviour classes.
    /// This singleton will persist between scenes if DontDestroyOnLoad is set to true.
    /// 
    /// Complexity Rating: 2
    /// </summary>
    /// <typeparam name="T">The type of the MonoBehaviour singleton</typeparam>
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// The static reference to the instance.
        /// </summary>
        private static T _instance;
        
        /// <summary>
        /// Lock object for thread safety
        /// </summary>
        private static readonly object _lock = new object();
        
        /// <summary>
        /// Flag to mark if we're destroying the application
        /// </summary>
        private static bool _applicationIsQuitting = false;
        
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[{typeof(T).Name}] Instance will not be returned because the application is quitting.");
                    return null;
                }
                
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));
                        
                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError($"[{typeof(T).Name}] Something went really wrong - there should never be more than 1 singleton!");
                            return _instance;
                        }
                        
                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"{typeof(T).Name} (Singleton)";
                            
                            Debug.Log($"[{typeof(T).Name}] An instance was created with DontDestroyOnLoad.");
                        }
                    }
                    
                    return _instance;
                }
            }
        }
        
        /// <summary>
        /// Override to determine if this singleton should persist between scenes.
        /// </summary>
        protected virtual bool DontDestroyOnLoad => true;
        
        /// <summary>
        /// Initialize the singleton on Awake.
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                
                if (DontDestroyOnLoad)
                {
                    transform.parent = null;
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[{typeof(T).Name}] Another instance was destroyed because one already exists.");
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it has been destroyed, 
        /// it will create a buggy ghost object that will stay in the Editor scene
        /// even after stopping playing the Application. To prevent this, we set the singleton to null.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _applicationIsQuitting = true;
            }
        }
    }
} 