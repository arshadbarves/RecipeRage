using System;
using System.Collections;
using Core.Logging;
using UnityEngine;

namespace Examples
{
    /// <summary>
    /// Example demonstrating the logging system features
    /// Attach this to a GameObject to see logging in action
    /// </summary>
    public class LoggingExample : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool _runOnStart = true;
        [SerializeField] private float _testInterval = 2f;

        private void Start()
        {
            if (_runOnStart)
            {
                StartCoroutine(RunLoggingExamples());
            }
        }

        private IEnumerator RunLoggingExamples()
        {
            yield return new WaitForSeconds(1f);

            // Example 1: Basic logging
            GameLogger.Log("=== Logging System Examples ===");
            yield return new WaitForSeconds(_testInterval);

            // Example 2: Different log levels
            GameLogger.LogInfo("This is an info message");
            yield return new WaitForSeconds(_testInterval);

            GameLogger.LogWarning("This is a warning message");
            yield return new WaitForSeconds(_testInterval);

            GameLogger.LogError("This is an error message");
            yield return new WaitForSeconds(_testInterval);

            // Example 3: Category-specific logging
            GameLogger.Network.Log("Connected to server at 192.168.1.1");
            yield return new WaitForSeconds(_testInterval);

            GameLogger.Audio.LogWarning("Audio clip 'explosion.wav' not found");
            yield return new WaitForSeconds(_testInterval);

            GameLogger.Save.LogError("Failed to save game state");
            yield return new WaitForSeconds(_testInterval);

            GameLogger.Auth.Log("User authenticated successfully");
            yield return new WaitForSeconds(_testInterval);

            GameLogger.UI.Log("Main menu opened");
            yield return new WaitForSeconds(_testInterval);

            // Example 4: Custom categories
            GameLogger.LogInfo("Player spawned at position (0, 0, 0)", "Gameplay");
            yield return new WaitForSeconds(_testInterval);

            GameLogger.LogWarning("Low FPS detected: 25 fps", "Performance");
            yield return new WaitForSeconds(_testInterval);

            // Example 5: Exception logging
            try
            {
                throw new InvalidOperationException("This is a test exception");
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex, "ExampleSystem");
            }
            yield return new WaitForSeconds(_testInterval);

            // Example 6: Unity's built-in logs (automatically captured)
            Debug.Log("Unity Debug.Log - automatically captured");
            Debug.LogWarning("Unity Debug.LogWarning - automatically captured");
            Debug.LogError("Unity Debug.LogError - automatically captured");
            yield return new WaitForSeconds(_testInterval);

            GameLogger.Log("=== Examples Complete! Press ` to open console ===");
        }

        [ContextMenu("Run Examples")]
        public void RunExamples()
        {
            StartCoroutine(RunLoggingExamples());
        }

        [ContextMenu("Test Export")]
        public void TestExport()
        {
            var logger = Core.Bootstrap.GameBootstrap.Services?.LoggingService;
            if (logger == null)
            {
                Debug.LogError("Logging service not available!");
                return;
            }

            var path = System.IO.Path.Combine(Application.persistentDataPath, "test_export.txt");
            logger.SaveLogsToFile(path);
            Debug.Log($"Logs exported to: {path}");
        }

        [ContextMenu("Clear Logs")]
        public void ClearLogs()
        {
            var logger = Core.Bootstrap.GameBootstrap.Services?.LoggingService;
            if (logger == null)
            {
                Debug.LogError("Logging service not available!");
                return;
            }

            logger.ClearLogs();
            Debug.Log("Logs cleared!");
        }
    }
}
