using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using RecipeRage.Modules.Logging;
using RecipeRage.Modules.Reporting.Data;
using RecipeRage.Modules.Reporting.Interfaces;

namespace RecipeRage.Modules.Reporting.Core
{
    /// <summary>
    /// Main implementation of the reporting service.
    /// Handles bug reports, crash detection, and integration with the logging system.
    /// 
    /// Complexity Rating: 4
    /// </summary>
    public class ReportingService : MonoBehaviour, IReportingService
    {
        private const string SAVE_DIRECTORY = "Reports";
        private const int MAX_STORED_REPORTS = 10;
        private const int CRASH_DETECTION_INTERVAL = 5; // seconds
        
        private Dictionary<string, ReportData> _pendingReports = new Dictionary<string, ReportData>();
        private List<IReportingProvider> _providers = new List<IReportingProvider>();
        private bool _isInitialized = false;
        private bool _automaticCrashReporting = false;
        private Coroutine _watchdogCoroutine = null;
        private string _lastError = null;
        
        #region IReportingService Events
        
        /// <summary>
        /// Event triggered when a report is created
        /// </summary>
        public event Action<ReportData> OnReportCreated;
        
        /// <summary>
        /// Event triggered when a report is submitted
        /// </summary>
        public event Action<ReportData, bool, string> OnReportSubmitted;
        
        /// <summary>
        /// Event triggered when a crash is detected
        /// </summary>
        public event Action<ReportData> OnCrashDetected;
        
        #endregion
        
        #region Unity Lifecycle
        
        /// <summary>
        /// Awake is called when the script instance is being loaded
        /// </summary>
        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            LoadPendingReports();
        }
        
        /// <summary>
        /// OnDestroy is called when the script instance is being destroyed
        /// </summary>
        private void OnDestroy()
        {
            // Unregister from Unity log messages
            Application.logMessageReceived -= HandleLogMessage;
            
            // Stop the watchdog
            if (_watchdogCoroutine != null)
            {
                StopCoroutine(_watchdogCoroutine);
                _watchdogCoroutine = null;
            }
            
            // Save any pending reports
            SavePendingReports();
        }
        
        #endregion
        
        #region IReportingService Implementation
        
        /// <summary>
        /// Initialize the reporting service
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        public void Initialize(Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                LogHelper.Warning("ReportingService", "Reporting service already initialized");
                onComplete?.Invoke(true);
                return;
            }
            
            LogHelper.Info("ReportingService", "Initializing reporting service");
            
            // Initialize providers
            if (_providers.Count > 0)
            {
                int providersInitialized = 0;
                bool anyProviderInitialized = false;
                
                foreach (var provider in _providers)
                {
                    provider.Initialize(success =>
                    {
                        if (success)
                        {
                            anyProviderInitialized = true;
                            LogHelper.Info("ReportingService", $"Provider initialized: {provider.GetProviderName()}");
                        }
                        else
                        {
                            LogHelper.Warning("ReportingService", $"Failed to initialize provider: {provider.GetProviderName()}");
                        }
                        
                        providersInitialized++;
                        
                        if (providersInitialized >= _providers.Count)
                        {
                            // Even if some providers fail, we can still use the service
                            _isInitialized = true;
                            LogHelper.Info("ReportingService", "Reporting service initialized" + (anyProviderInitialized ? "" : " (no providers available)"));
                            onComplete?.Invoke(true);
                        }
                    });
                }
            }
            else
            {
                // No providers, but service can still be used for local reports
                _isInitialized = true;
                LogHelper.Info("ReportingService", "Reporting service initialized (no providers)");
                onComplete?.Invoke(true);
            }
        }
        
        /// <summary>
        /// Capture information about the current state for reporting
        /// </summary>
        /// <param name="reportType">Type of report (Bug, Crash, Feedback, etc.)</param>
        /// <returns>Report ID</returns>
        public string CaptureState(ReportType reportType)
        {
            if (!_isInitialized)
            {
                LogHelper.Warning("ReportingService", "Reporting service not initialized");
                return null;
            }
            
            // Generate a unique ID for the report
            string reportId = GenerateReportId();
            
            // Create a new report
            ReportData report = new ReportData(reportId, reportType);
            
            // Add it to the pending reports
            _pendingReports[reportId] = report;
            
            LogHelper.Info("ReportingService", $"Created new {reportType} report with ID: {reportId}");
            
            // Trigger the event
            OnReportCreated?.Invoke(report);
            
            // Save pending reports
            SavePendingReports();
            
            return reportId;
        }
        
        /// <summary>
        /// Add logs to the report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="maxAge">Maximum age of logs to include (in minutes)</param>
        /// <param name="minLevel">Minimum log level to include</param>
        public void AddLogs(string reportId, int maxAge = 30, LogLevel minLevel = LogLevel.Warning)
        {
            if (!_isInitialized)
            {
                LogHelper.Warning("ReportingService", "Reporting service not initialized");
                return;
            }
            
            if (!_pendingReports.TryGetValue(reportId, out ReportData report))
            {
                LogHelper.Warning("ReportingService", $"Report not found: {reportId}");
                return;
            }
            
            // Get logs from the logging service
            try
            {
                // Convert to Logging.LogLevel
                Logging.LogLevel loggingLevel = (Logging.LogLevel)(int)minLevel;
                
                // Get recent logs that match the criteria
                var logs = LogHelper.GetRecentLogs(1000) // Get a large number of logs to start with
                    .Where(log => (int)log.Level >= (int)loggingLevel) // Filter by level
                    .Where(log => (DateTime.UtcNow - log.Timestamp).TotalMinutes <= maxAge); // Filter by age
                
                // Convert logs to a string
                string logText = string.Join(Environment.NewLine, logs.Select(log => 
                    $"[{log.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{log.Level}] [{log.Module}] {log.Message}" + 
                    (log.Exception != null ? $"{Environment.NewLine}{log.Exception}" : "")));
                
                // Add logs to the report
                report.Logs = logText;
                
                LogHelper.Debug("ReportingService", $"Added logs to report {reportId}");
                
                // Save pending reports
                SavePendingReports();
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ReportingService", ex, $"Failed to add logs to report {reportId}");
                _lastError = ex.Message;
            }
        }
        
        /// <summary>
        /// Add a screenshot to the report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="screenshotData">Screenshot data</param>
        public void AddScreenshot(string reportId, byte[] screenshotData)
        {
            if (!_isInitialized)
            {
                LogHelper.Warning("ReportingService", "Reporting service not initialized");
                return;
            }
            
            if (!_pendingReports.TryGetValue(reportId, out ReportData report))
            {
                LogHelper.Warning("ReportingService", $"Report not found: {reportId}");
                return;
            }
            
            // Add screenshot to the report
            report.ScreenshotData = screenshotData;
            
            LogHelper.Debug("ReportingService", $"Added screenshot to report {reportId} ({screenshotData.Length} bytes)");
            
            // Save pending reports (without screenshot to avoid large files)
            SavePendingReportsWithoutScreenshot();
        }
        
        /// <summary>
        /// Add metadata to the report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="key">Metadata key</param>
        /// <param name="value">Metadata value</param>
        public void AddMetadata(string reportId, string key, string value)
        {
            if (!_isInitialized)
            {
                LogHelper.Warning("ReportingService", "Reporting service not initialized");
                return;
            }
            
            if (!_pendingReports.TryGetValue(reportId, out ReportData report))
            {
                LogHelper.Warning("ReportingService", $"Report not found: {reportId}");
                return;
            }
            
            // Add metadata to the report
            report.AddMetadata(key, value);
            
            LogHelper.Debug("ReportingService", $"Added metadata to report {reportId}: {key}={value}");
            
            // Save pending reports
            SavePendingReports();
        }
        
        /// <summary>
        /// Add user description to the report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="description">User-provided description</param>
        public void AddUserDescription(string reportId, string description)
        {
            if (!_isInitialized)
            {
                LogHelper.Warning("ReportingService", "Reporting service not initialized");
                return;
            }
            
            if (!_pendingReports.TryGetValue(reportId, out ReportData report))
            {
                LogHelper.Warning("ReportingService", $"Report not found: {reportId}");
                return;
            }
            
            // Add description to the report
            report.UserDescription = description;
            
            LogHelper.Debug("ReportingService", $"Added description to report {reportId}");
            
            // Save pending reports
            SavePendingReports();
        }
        
        /// <summary>
        /// Submit the report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="onComplete">Callback when submission is complete</param>
        public void SubmitReport(string reportId, Action<bool, string> onComplete = null)
        {
            if (!_isInitialized)
            {
                LogHelper.Warning("ReportingService", "Reporting service not initialized");
                onComplete?.Invoke(false, "Reporting service not initialized");
                return;
            }
            
            if (!_pendingReports.TryGetValue(reportId, out ReportData report))
            {
                LogHelper.Warning("ReportingService", $"Report not found: {reportId}");
                onComplete?.Invoke(false, "Report not found");
                return;
            }
            
            // If there are no providers, we can't submit the report
            if (_providers.Count == 0)
            {
                LogHelper.Warning("ReportingService", "No reporting providers available");
                report.MarkAsSubmitted(false, "No reporting providers available");
                OnReportSubmitted?.Invoke(report, false, "No reporting providers available");
                onComplete?.Invoke(false, "No reporting providers available");
                _lastError = "No reporting providers available";
                SavePendingReports();
                return;
            }
            
            // Find an available provider
            IReportingProvider provider = _providers.FirstOrDefault(p => p.IsAvailable());
            
            if (provider == null)
            {
                LogHelper.Warning("ReportingService", "No available reporting providers");
                report.MarkAsSubmitted(false, "No available reporting providers");
                OnReportSubmitted?.Invoke(report, false, "No available reporting providers");
                onComplete?.Invoke(false, "No available reporting providers");
                _lastError = "No available reporting providers";
                SavePendingReports();
                return;
            }
            
            LogHelper.Info("ReportingService", $"Submitting report {reportId} using provider {provider.GetProviderName()}");
            
            // Also send to analytics
            try
            {
                SendReportToAnalytics(report);
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ReportingService", ex, "Failed to send report to analytics");
            }
            
            // Submit the report
            provider.SubmitReport(report, (success, externalReportId) =>
            {
                string error = null;
                
                if (success)
                {
                    LogHelper.Info("ReportingService", $"Report {reportId} submitted successfully, external ID: {externalReportId}");
                }
                else
                {
                    error = "Failed to submit report";
                    LogHelper.Error("ReportingService", $"Failed to submit report {reportId}");
                    _lastError = error;
                }
                
                // Update the report
                report.MarkAsSubmitted(success, error, externalReportId);
                
                // Trigger the event
                OnReportSubmitted?.Invoke(report, success, externalReportId);
                
                // Save pending reports
                SavePendingReports();
                
                // Invoke the callback
                onComplete?.Invoke(success, externalReportId);
            });
        }
        
        /// <summary>
        /// Register for automatic crash reporting
        /// </summary>
        /// <param name="enabled">Whether to enable automatic crash reporting</param>
        public void SetAutomaticCrashReporting(bool enabled)
        {
            _automaticCrashReporting = enabled;
            
            if (enabled)
            {
                // Register for Unity log messages
                Application.logMessageReceived -= HandleLogMessage; // Avoid duplicate registrations
                Application.logMessageReceived += HandleLogMessage;
                
                // Start the watchdog
                if (_watchdogCoroutine == null)
                {
                    _watchdogCoroutine = StartCoroutine(WatchdogCoroutine());
                }
                
                LogHelper.Info("ReportingService", "Automatic crash reporting enabled");
            }
            else
            {
                // Unregister from Unity log messages
                Application.logMessageReceived -= HandleLogMessage;
                
                // Stop the watchdog
                if (_watchdogCoroutine != null)
                {
                    StopCoroutine(_watchdogCoroutine);
                    _watchdogCoroutine = null;
                }
                
                LogHelper.Info("ReportingService", "Automatic crash reporting disabled");
            }
        }
        
        /// <summary>
        /// Get a pending report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <returns>Report data if found, null otherwise</returns>
        public ReportData GetReport(string reportId)
        {
            if (!_isInitialized)
            {
                LogHelper.Warning("ReportingService", "Reporting service not initialized");
                return null;
            }
            
            if (!_pendingReports.TryGetValue(reportId, out ReportData report))
            {
                LogHelper.Warning("ReportingService", $"Report not found: {reportId}");
                return null;
            }
            
            return report;
        }
        
        /// <summary>
        /// Get all pending reports
        /// </summary>
        /// <returns>List of pending reports</returns>
        public List<ReportData> GetPendingReports()
        {
            if (!_isInitialized)
            {
                LogHelper.Warning("ReportingService", "Reporting service not initialized");
                return new List<ReportData>();
            }
            
            return _pendingReports.Values.ToList();
        }
        
        /// <summary>
        /// Get the reporting service status
        /// </summary>
        /// <returns>Service status information</returns>
        public ReportingServiceStatus GetStatus()
        {
            return new ReportingServiceStatus
            {
                IsInitialized = _isInitialized,
                AutomaticCrashReporting = _automaticCrashReporting,
                ProviderCount = _providers.Count,
                PendingReportCount = _pendingReports.Count,
                LastError = _lastError
            };
        }
        
        /// <summary>
        /// Add a provider to the reporting service
        /// </summary>
        /// <param name="provider">The provider to add</param>
        public void AddProvider(IReportingProvider provider)
        {
            if (provider == null)
            {
                LogHelper.Error("ReportingService", "Cannot add null provider");
                return;
            }
            
            if (!_providers.Contains(provider))
            {
                _providers.Add(provider);
                LogHelper.Info("ReportingService", $"Added provider: {provider.GetProviderName()}");
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Generate a unique ID for a report
        /// </summary>
        /// <returns>Unique ID</returns>
        private string GenerateReportId()
        {
            return $"{SystemInfo.deviceUniqueIdentifier.Substring(0, 8)}-{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}-{UnityEngine.Random.Range(1000, 9999)}";
        }
        
        /// <summary>
        /// Handle a log message from Unity
        /// </summary>
        /// <param name="logString">Log message</param>
        /// <param name="stackTrace">Stack trace</param>
        /// <param name="type">Log type</param>
        private void HandleLogMessage(string logString, string stackTrace, LogType type)
        {
            if (!_automaticCrashReporting)
            {
                return;
            }
            
            // Only create a crash report for exceptions and assertions
            if (type == LogType.Exception || type == LogType.Assert)
            {
                LogHelper.Error("ReportingService", $"Crash detected: {logString}");
                
                // Create a crash report
                string reportId = CaptureState(ReportType.Crash);
                
                if (reportId != null)
                {
                    // Add metadata
                    AddMetadata(reportId, "exception_message", logString);
                    AddMetadata(reportId, "stack_trace", stackTrace);
                    AddMetadata(reportId, "log_type", type.ToString());
                    
                    // Add logs
                    AddLogs(reportId, 30, LogLevel.Warning);
                    
                    // Get the report
                    ReportData report = GetReport(reportId);
                    
                    // Trigger the event
                    OnCrashDetected?.Invoke(report);
                    
                    // If automatic submission is enabled, submit the report
                    if (_automaticCrashReporting)
                    {
                        SubmitReport(reportId);
                    }
                }
            }
        }
        
        /// <summary>
        /// Watchdog coroutine to detect freezes
        /// </summary>
        private IEnumerator WatchdogCoroutine()
        {
            DateTime lastUpdateTime = DateTime.UtcNow;
            
            while (true)
            {
                yield return new WaitForSeconds(CRASH_DETECTION_INTERVAL);
                
                // Check for freezes (if game is not responding for a long time)
                // In a real implementation, this would involve a separate thread or process
                
                // For this example, we're just checking that the coroutine is still running
                lastUpdateTime = DateTime.UtcNow;
            }
        }
        
        /// <summary>
        /// Send report data to analytics
        /// </summary>
        /// <param name="report">Report data</param>
        private void SendReportToAnalytics(ReportData report)
        {
            try
            {
                // Check if analytics is available
                var analyticsType = Type.GetType("RecipeRage.Modules.Analytics.AnalyticsHelper, Assembly-CSharp");
                if (analyticsType == null)
                {
                    LogHelper.Warning("ReportingService", "Analytics module not found");
                    return;
                }
                
                // Get the LogEvent method
                var logEventMethod = analyticsType.GetMethod("LogEvent", new[] { typeof(string), typeof(Dictionary<string, object>) });
                if (logEventMethod == null)
                {
                    LogHelper.Warning("ReportingService", "LogEvent method not found in Analytics module");
                    return;
                }
                
                // Create event parameters
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "report_id", report.Id },
                    { "report_type", report.Type.ToString() },
                    { "has_screenshot", report.ScreenshotData != null },
                    { "has_logs", !string.IsNullOrEmpty(report.Logs) },
                    { "has_description", !string.IsNullOrEmpty(report.UserDescription) },
                    { "metadata_count", report.Metadata.Count }
                };
                
                // Add selected metadata (avoid sending everything)
                if (report.Metadata.ContainsKey("app_version"))
                {
                    parameters["app_version"] = report.Metadata["app_version"];
                }
                
                if (report.Metadata.ContainsKey("device_model"))
                {
                    parameters["device_model"] = report.Metadata["device_model"];
                }
                
                if (report.Metadata.ContainsKey("os"))
                {
                    parameters["os"] = report.Metadata["os"];
                }
                
                if (report.Type == ReportType.Crash && report.Metadata.ContainsKey("exception_message"))
                {
                    parameters["exception_message"] = report.Metadata["exception_message"];
                }
                
                // Invoke the LogEvent method
                logEventMethod.Invoke(null, new object[] { "report_created", parameters });
                
                LogHelper.Debug("ReportingService", "Sent report data to analytics");
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ReportingService", ex, "Failed to send report to analytics");
            }
        }
        
        /// <summary>
        /// Save pending reports to disk
        /// </summary>
        private void SavePendingReports()
        {
            try
            {
                // Create the directory if it doesn't exist
                string directory = Path.Combine(Application.persistentDataPath, SAVE_DIRECTORY);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Convert to a serializable format
                var reports = _pendingReports.Values.ToList();
                
                // Limit to the maximum number of stored reports
                if (reports.Count > MAX_STORED_REPORTS)
                {
                    // Sort by timestamp (newest first)
                    reports = reports.OrderByDescending(r => r.Timestamp).Take(MAX_STORED_REPORTS).ToList();
                }
                
                // Save the reports
                string json = JsonConvert.SerializeObject(reports, Formatting.Indented);
                string path = Path.Combine(directory, "pending_reports.json");
                File.WriteAllText(path, json);
                
                LogHelper.Debug("ReportingService", $"Saved {reports.Count} pending reports");
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ReportingService", ex, "Failed to save pending reports");
                _lastError = ex.Message;
            }
        }
        
        /// <summary>
        /// Save pending reports to disk, but without screenshots to reduce file size
        /// </summary>
        private void SavePendingReportsWithoutScreenshot()
        {
            try
            {
                // Create the directory if it doesn't exist
                string directory = Path.Combine(Application.persistentDataPath, SAVE_DIRECTORY);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Convert to a serializable format, without screenshots
                var reportsWithoutScreenshots = _pendingReports.Values.Select(r =>
                {
                    var reportCopy = new ReportData(r.Id, r.Type)
                    {
                        Timestamp = r.Timestamp,
                        UserDescription = r.UserDescription,
                        Logs = r.Logs,
                        IsSubmitted = r.IsSubmitted,
                        IsSubmissionSuccessful = r.IsSubmissionSuccessful,
                        SubmissionError = r.SubmissionError,
                        ExternalReportId = r.ExternalReportId
                    };
                    
                    // Copy metadata
                    foreach (var meta in r.Metadata)
                    {
                        reportCopy.AddMetadata(meta.Key, meta.Value);
                    }
                    
                    // Add a flag indicating there's a screenshot
                    if (r.ScreenshotData != null)
                    {
                        reportCopy.AddMetadata("has_screenshot", "true");
                    }
                    
                    return reportCopy;
                }).ToList();
                
                // Limit to the maximum number of stored reports
                if (reportsWithoutScreenshots.Count > MAX_STORED_REPORTS)
                {
                    // Sort by timestamp (newest first)
                    reportsWithoutScreenshots = reportsWithoutScreenshots.OrderByDescending(r => r.Timestamp).Take(MAX_STORED_REPORTS).ToList();
                }
                
                // Save the reports
                string json = JsonConvert.SerializeObject(reportsWithoutScreenshots, Formatting.Indented);
                string path = Path.Combine(directory, "pending_reports.json");
                File.WriteAllText(path, json);
                
                LogHelper.Debug("ReportingService", $"Saved {reportsWithoutScreenshots.Count} pending reports (without screenshots)");
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ReportingService", ex, "Failed to save pending reports");
                _lastError = ex.Message;
            }
        }
        
        /// <summary>
        /// Load pending reports from disk
        /// </summary>
        private void LoadPendingReports()
        {
            try
            {
                // Create the directory if it doesn't exist
                string directory = Path.Combine(Application.persistentDataPath, SAVE_DIRECTORY);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Load the reports
                string path = Path.Combine(directory, "pending_reports.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var reports = JsonConvert.DeserializeObject<List<ReportData>>(json);
                    
                    // Add to the dictionary
                    _pendingReports.Clear();
                    foreach (var report in reports)
                    {
                        _pendingReports[report.Id] = report;
                    }
                    
                    LogHelper.Info("ReportingService", $"Loaded {reports.Count} pending reports");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ReportingService", ex, "Failed to load pending reports");
                _lastError = ex.Message;
            }
        }
        
        #endregion
    }
} 