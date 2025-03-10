using System;
using UnityEngine;

namespace RecipeRage.Modules.Friends.Core
{
    /// <summary>
    /// MonoBehaviour responsible for providing Update events to the friends system
    /// 
    /// Complexity Rating: 1
    /// </summary>
    public class FriendsServiceUpdater : MonoBehaviour
    {
        private static FriendsServiceUpdater _instance;
        
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static FriendsServiceUpdater Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Create a new GameObject with the updater
                    GameObject go = new GameObject("FriendsServiceUpdater");
                    _instance = go.AddComponent<FriendsServiceUpdater>();
                    DontDestroyOnLoad(go);
                }
                
                return _instance;
            }
        }
        
        /// <summary>
        /// Event fired on each Update call
        /// </summary>
        public event Action OnUpdate;
        
        /// <summary>
        /// Event fired on application pause
        /// </summary>
        public event Action<bool> OnApplicationPause;
        
        /// <summary>
        /// Event fired on application quit
        /// </summary>
        public event Action OnApplicationQuit;
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Update()
        {
            OnUpdate?.Invoke();
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        private void OnApplicationPause(bool pause)
        {
            OnApplicationPause?.Invoke(pause);
        }
        
        private void OnApplicationQuit()
        {
            OnApplicationQuit?.Invoke();
        }
    }
} 