using System.Collections;
using Core.Utilities;
using UnityEngine;

namespace Core.UI.SplashScreen
{
    /// <summary>
    /// Simple test script for the SplashScreenManager.
    /// </summary>
    public class SplashScreenTester : MonoBehaviour
    {
        [SerializeField] private SplashScreenManager _splashScreenManager;
        [SerializeField] private float _testDelay = 1f;
        [SerializeField] private bool _autoTest = true;

        /// <summary>
        /// Set the SplashScreenManager reference.
        /// </summary>
        /// <param name="manager">The SplashScreenManager to use</param>
        public void SetSplashScreenManager(SplashScreenManager manager)
        {
            _splashScreenManager = manager;
        }

        private void Start()
        {
            if (_autoTest && _splashScreenManager != null)
            {
                StartCoroutine(RunSplashScreenTest());
            }
        }

        /// <summary>
        /// Run a test of the splash screen sequence.
        /// </summary>
        private IEnumerator RunSplashScreenTest()
        {
            Debug.Log("[SplashScreenTester] Starting splash screen test...");

            // Wait a moment before starting
            yield return new WaitForSeconds(_testDelay);

            // Show company splash
            Debug.Log("[SplashScreenTester] Showing company splash...");
            yield return _splashScreenManager.ShowCompanySplash().AsCoroutine();

            // Show game logo splash
            Debug.Log("[SplashScreenTester] Showing game logo splash...");
            yield return _splashScreenManager.ShowGameLogoSplash().AsCoroutine();

            // Show loading screen
            Debug.Log("[SplashScreenTester] Showing loading screen...");
            _splashScreenManager.ShowLoadingScreen();

            // Simulate loading progress
            for (float progress = 0f; progress <= 1f; progress += 0.1f)
            {
                _splashScreenManager.UpdateLoadingProgress($"Loading... {progress * 100:0}%", progress);
                yield return new WaitForSeconds(0.5f);
            }

            // Hide loading screen
            Debug.Log("[SplashScreenTester] Hiding loading screen...");
            yield return _splashScreenManager.HideLoadingScreen().AsCoroutine();

            Debug.Log("[SplashScreenTester] Splash screen test complete!");
        }

        /// <summary>
        /// Manually trigger the splash screen test.
        /// </summary>
        [ContextMenu("Run Splash Screen Test")]
        public void ManualTest()
        {
            if (_splashScreenManager != null)
            {
                StartCoroutine(RunSplashScreenTest());
            }
            else
            {
                Debug.LogError("[SplashScreenTester] SplashScreenManager reference is missing!");
            }
        }
    }
}
