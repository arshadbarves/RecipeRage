using System.Collections;
using Core.Bootstrap;
using UnityEngine;

namespace UI.UISystem
{
    /// <summary>
    /// Helper class for accessing UI service from GameBootstrap
    /// Provides backward compatibility for code that used UIServiceAccessor.Instance
    /// </summary>
    public static class UIServiceAccessor
    {
        public static IUIService Instance => GameBootstrap.Services?.UIService;
        
        /// <summary>
        /// Get UIManager as MonoBehaviour for coroutine support
        /// </summary>
        public static MonoBehaviour AsMonoBehaviour => GameBootstrap.Services?.UIService as MonoBehaviour;
        
        /// <summary>
        /// Start a coroutine on the UIManager
        /// </summary>
        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            return AsMonoBehaviour?.StartCoroutine(routine);
        }
        
        /// <summary>
        /// Stop a coroutine on the UIManager
        /// </summary>
        public static void StopCoroutine(Coroutine routine)
        {
            AsMonoBehaviour?.StopCoroutine(routine);
        }
    }
}
