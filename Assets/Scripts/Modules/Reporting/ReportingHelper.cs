using System;
using System.Collections.Generic;
using UnityEngine;
using RecipeRage.Modules.Logging;
using RecipeRage.Modules.Reporting.Core;
using RecipeRage.Modules.Reporting.Data;
using RecipeRage.Modules.Reporting.Interfaces;

namespace RecipeRage.Modules.Reporting
{
    /// <summary>
    /// Static helper class for easy access to reporting functionality
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public static class ReportingHelper
    {
        private static IReportingService _reportingService;
        private static bool _isInitialized = false;
        
        /// <summary>
        /// Event triggered when a report is created
        /// </summary>
        public static event Action<ReportData> OnReportCreated
        {
            add
            {
                EnsureServiceCreated();
                _reportingService.OnReportCreated += value;
            }
            remove
            {
                if (_reportingService != null)
                {
                    _reportingService.OnReportCreated -= value;
                }
            }
        }
        
        /// <summary>
        /// Event triggered when a report is submitted
        /// </summary>
        public static event Action<ReportData, bool, string> OnReportSubmitted
        {
            add
            {
                EnsureServiceCreated();
                _reportingService.OnReportSubmitted += value;
            }
            remove
            {
                if (_reportingService != null)
                {
                    _reportingService.OnReportSubmitted -= value;
                }
            }
        }
        
        /// <summary>
        /// Event triggered when a crash is detected
        /// </summary>
        public static event Action<ReportData> OnCrashDetected
        {
            add
            {
                EnsureServiceCreated();
                _reportingService.OnCrashDetected += value;
            }
            remove
            {
                if (_reportingService != null)
                {
                    _reportingService.OnCrashDetected -= value;
                }
            }
        }
        
        /// <summary>
        /// Initialize the reporting service
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        public static void Initialize(Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                LogHelper.Warning("ReportingHelper", "Reporting helper already initialized");
                onComplete?.Invoke(true);
                return;
            }
            
            LogHelper.Info("ReportingHelper", "Initializing reporting helper");
            
            // Ensure the service is created
            EnsureServiceCreated();
            
            // Initialize the service
            _reportingService.Initialize(success =>
            {
                _isInitialized = success;
                LogHelper.Info("ReportingHelper", $"Reporting helper initialization {(success ? "successful" : "failed")}");
                onComplete?.Invoke(success);
            });
        }
        
        /// <summary>
        /// Capture information about the current state for reporting
        /// </summary>
        /// <param name="reportType">Type of report (Bug, Crash, Feedback, etc.)</param>
        /// <returns>Report ID</returns>
        public static string CaptureState(ReportType reportType)
        {
            if (!CheckInitialized("CaptureState"))
            {
                return null;
            }
            
            return _reportingService.CaptureState(reportType);
        }
        
        /// <summary>
        /// Add logs to the report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="maxAge">Maximum age of logs to include (in minutes)</param>
        /// <param name="minLevel">Minimum log level to include</param>
        public static void AddLogs(string reportId, int maxAge = 30, LogLevel minLevel = LogLevel.Warning)
        {
            if (!CheckInitialized("AddLogs"))
            {
                return;
            }
            
            _reportingService.AddLogs(reportId, maxAge, minLevel);
        }
        
        /// <summary>
        /// Add a screenshot to the report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="screenshotData">Screenshot data</param>
        public static void AddScreenshot(string reportId, byte[] screenshotData)
        {
            if (!CheckInitialized("AddScreenshot"))
            {
                return;
            }
            
            _reportingService.AddScreenshot(reportId, screenshotData);
        }
        
        /// <summary>
        /// Add metadata to the report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="key">Metadata key</param>
        /// <param name="value">Metadata value</param>
        public static void AddMetadata(string reportId, string key, string value)
        {
            if (!CheckInitialized("AddMetadata"))
            {
                return;
            }
            
            _reportingService.AddMetadata(reportId, key, value);
        }
        
        /// <summary>
        /// Add user description to the report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="description">User-provided description</param>
        public static void AddUserDescription(string reportId, string description)
        {
            if (!CheckInitialized("AddUserDescription"))
            {
                return;
            }
            
            _reportingService.AddUserDescription(reportId, description);
        }
        
        /// <summary>
        /// Submit the report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="onComplete">Callback when submission is complete</param>
        public static void SubmitReport(string reportId, Action<bool, string> onComplete = null)
        {
            if (!CheckInitialized("SubmitReport"))
            {
                onComplete?.Invoke(false, "Reporting helper not initialized");
                return;
            }
            
            _reportingService.SubmitReport(reportId, onComplete);
        }
        
        /// <summary>
        /// Register for automatic crash reporting
        /// </summary>
        /// <param name="enabled">Whether to enable automatic crash reporting</param>
        public static void SetAutomaticCrashReporting(bool enabled)
        {
            if (!CheckInitialized("SetAutomaticCrashReporting"))
            {
                return;
            }
            
            _reportingService.SetAutomaticCrashReporting(enabled);
        }
        
        /// <summary>
        /// Get a pending report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <returns>Report data if found, null otherwise</returns>
        public static ReportData GetReport(string reportId)
        {
            if (!CheckInitialized("GetReport"))
            {
                return null;
            }
            
            return _reportingService.GetReport(reportId);
        }
        
        /// <summary>
        /// Get all pending reports
        /// </summary>
        /// <returns>List of pending reports</returns>
        public static List<ReportData> GetPendingReports()
        {
            if (!CheckInitialized("GetPendingReports"))
            {
                return new List<ReportData>();
            }
            
            return _reportingService.GetPendingReports();
        }
        
        /// <summary>
        /// Get the reporting service status
        /// </summary>
        /// <returns>Service status information</returns>
        public static ReportingServiceStatus GetStatus()
        {
            if (!CheckInitialized("GetStatus", false))
            {
                return new ReportingServiceStatus
                {
                    IsInitialized = false,
                    AutomaticCrashReporting = false,
                    ProviderCount = 0,
                    PendingReportCount = 0,
                    LastError = "Reporting helper not initialized"
                };
            }
            
            return _reportingService.GetStatus();
        }
        
        /// <summary>
        /// Add a provider to the reporting service
        /// </summary>
        /// <param name="provider">The provider to add</param>
        public static void AddProvider(IReportingProvider provider)
        {
            if (provider == null)
            {
                LogHelper.Error("ReportingHelper", "Cannot add null provider");
                return;
            }
            
            // Ensure the service is created
            EnsureServiceCreated();
            
            _reportingService.AddProvider(provider);
        }
        
        /// <summary>
        /// Capture a screenshot of the current screen
        /// </summary>
        /// <param name="callback">Callback with the screenshot data</param>
        public static void CaptureScreenshot(Action<byte[]> callback)
        {
            if (callback == null)
            {
                LogHelper.Error("ReportingHelper", "Screenshot callback cannot be null");
                return;
            }
            
            // This must be called at the end of a frame
            if (Application.isPlaying)
            {
                GameObject captureObject = new GameObject("ScreenshotCapture");
                ScreenshotCapture captureComponent = captureObject.AddComponent<ScreenshotCapture>();
                captureComponent.CaptureScreenshot(callback);
            }
            else
            {
                LogHelper.Error("ReportingHelper", "Screenshot capture is only available in play mode");
                callback(null);
            }
        }
        
        /// <summary>
        /// Create a report with a screenshot
        /// </summary>
        /// <param name="reportType">Type of report</param>
        /// <param name="includeScreenshot">Whether to include a screenshot</param>
        /// <param name="onComplete">Callback when the report is created</param>
        public static void CreateReport(ReportType reportType, bool includeScreenshot, Action<string> onComplete)
        {
            if (!CheckInitialized("CreateReport"))
            {
                onComplete?.Invoke(null);
                return;
            }
            
            // Create the report
            string reportId = CaptureState(reportType);
            
            if (reportId == null)
            {
                LogHelper.Error("ReportingHelper", "Failed to create report");
                onComplete?.Invoke(null);
                return;
            }
            
            // Add logs
            AddLogs(reportId);
            
            if (includeScreenshot)
            {
                // Capture a screenshot
                CaptureScreenshot(screenshotData =>
                {
                    if (screenshotData != null)
                    {
                        AddScreenshot(reportId, screenshotData);
                        LogHelper.Debug("ReportingHelper", "Added screenshot to report");
                    }
                    
                    onComplete?.Invoke(reportId);
                });
            }
            else
            {
                onComplete?.Invoke(reportId);
            }
        }
        
        /// <summary>
        /// Set a custom reporting service implementation
        /// </summary>
        /// <param name="reportingService">The reporting service to use</param>
        public static void SetReportingService(IReportingService reportingService)
        {
            if (reportingService == null)
            {
                LogHelper.Error("ReportingHelper", "Cannot set null reporting service");
                return;
            }
            
            _reportingService = reportingService;
            _isInitialized = false;
            
            LogHelper.Info("ReportingHelper", $"Set custom reporting service: {reportingService.GetType().Name}");
        }
        
        /// <summary>
        /// Ensure the reporting service is created
        /// </summary>
        private static void EnsureServiceCreated()
        {
            if (_reportingService == null)
            {
                // Find existing service
                ReportingService existingService = GameObject.FindObjectOfType<ReportingService>();
                
                if (existingService != null)
                {
                    _reportingService = existingService;
                    LogHelper.Debug("ReportingHelper", "Using existing ReportingService");
                }
                else
                {
                    // Create a new service
                    GameObject serviceObject = new GameObject("ReportingService");
                    _reportingService = serviceObject.AddComponent<ReportingService>();
                    GameObject.DontDestroyOnLoad(serviceObject);
                    LogHelper.Debug("ReportingHelper", "Created new ReportingService");
                }
            }
        }
        
        /// <summary>
        /// Check if the reporting service is initialized
        /// </summary>
        /// <param name="methodName">Name of the calling method</param>
        /// <param name="logWarning">Whether to log a warning if not initialized</param>
        /// <returns>True if initialized, false otherwise</returns>
        private static bool CheckInitialized(string methodName = null, bool logWarning = true)
        {
            if (!_isInitialized)
            {
                if (logWarning && !string.IsNullOrEmpty(methodName))
                {
                    LogHelper.Warning("ReportingHelper", $"Reporting helper not initialized. Call Initialize() before {methodName}().");
                }
                
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Helper component for capturing screenshots at the end of a frame
        /// </summary>
        private class ScreenshotCapture : MonoBehaviour
        {
            private Action<byte[]> _callback;
            
            public void CaptureScreenshot(Action<byte[]> callback)
            {
                _callback = callback;
                StartCoroutine(CaptureCoroutine());
            }
            
            private System.Collections.IEnumerator CaptureCoroutine()
            {
                // Wait for the end of the frame
                yield return new WaitForEndOfFrame();
                
                try
                {
                    // Capture the screenshot
                    Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                    screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                    screenshot.Apply();
                    
                    // Convert to bytes
                    byte[] bytes = screenshot.EncodeToPNG();
                    
                    // Clean up
                    Destroy(screenshot);
                    
                    // Invoke the callback
                    _callback?.Invoke(bytes);
                }
                catch (Exception ex)
                {
                    LogHelper.Exception("ScreenshotCapture", ex, "Failed to capture screenshot");
                    _callback?.Invoke(null);
                }
                
                // Destroy this game object
                Destroy(gameObject);
            }
        }
    }
} 