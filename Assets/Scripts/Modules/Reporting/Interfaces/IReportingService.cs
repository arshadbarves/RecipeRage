using System;
using System.Collections.Generic;
using RecipeRage.Modules.Logging.Interfaces;
using RecipeRage.Modules.Reporting.Data;

namespace RecipeRage.Modules.Reporting.Interfaces
{
    /// <summary>
    /// Interface for the reporting service
    /// Provides unified reporting capabilities throughout the application
    /// Complexity Rating: 3
    /// </summary>
    public interface IReportingService
    {
        /// <summary>
        /// Event triggered when a report is created
        /// </summary>
        event Action<ReportData> OnReportCreated;

        /// <summary>
        /// Event triggered when a report is submitted
        /// </summary>
        event Action<ReportData, bool, string> OnReportSubmitted;

        /// <summary>
        /// Event triggered when a crash is detected
        /// </summary>
        event Action<ReportData> OnCrashDetected;

        /// <summary>
        /// Initialize the reporting service
        /// </summary>
        /// <param name="onComplete"> Callback when initialization is complete </param>
        void Initialize(Action<bool> onComplete = null);

        /// <summary>
        /// Capture information about the current state for reporting
        /// </summary>
        /// <param name="reportType"> Type of report (Bug, Crash, Feedback, etc.) </param>
        /// <returns> Report ID </returns>
        string CaptureState(ReportType reportType);

        /// <summary>
        /// Add logs to the report
        /// </summary>
        /// <param name="reportId"> Report ID </param>
        /// <param name="maxAge"> Maximum age of logs to include (in minutes) </param>
        /// <param name="minLevel"> Minimum log level to include </param>
        void AddLogs(string reportId, int maxAge = 30, LogLevel minLevel = LogLevel.Warning);

        /// <summary>
        /// Add a screenshot to the report
        /// </summary>
        /// <param name="reportId"> Report ID </param>
        /// <param name="screenshotData"> Screenshot data </param>
        void AddScreenshot(string reportId, byte[] screenshotData);

        /// <summary>
        /// Add metadata to the report
        /// </summary>
        /// <param name="reportId"> Report ID </param>
        /// <param name="key"> Metadata key </param>
        /// <param name="value"> Metadata value </param>
        void AddMetadata(string reportId, string key, string value);

        /// <summary>
        /// Add user description to the report
        /// </summary>
        /// <param name="reportId"> Report ID </param>
        /// <param name="description"> User-provided description </param>
        void AddUserDescription(string reportId, string description);

        /// <summary>
        /// Submit the report
        /// </summary>
        /// <param name="reportId"> Report ID </param>
        /// <param name="onComplete"> Callback when submission is complete </param>
        void SubmitReport(string reportId, Action<bool, string> onComplete = null);

        /// <summary>
        /// Register for automatic crash reporting
        /// </summary>
        /// <param name="enabled"> Whether to enable automatic crash reporting </param>
        void SetAutomaticCrashReporting(bool enabled);

        /// <summary>
        /// Get a pending report
        /// </summary>
        /// <param name="reportId"> Report ID </param>
        /// <returns> Report data if found, null otherwise </returns>
        ReportData GetReport(string reportId);

        /// <summary>
        /// Get all pending reports
        /// </summary>
        /// <returns> List of pending reports </returns>
        List<ReportData> GetPendingReports();

        /// <summary>
        /// Get the reporting service status
        /// </summary>
        /// <returns> Service status information </returns>
        ReportingServiceStatus GetStatus();

        /// <summary>
        /// Add a provider to the reporting service
        /// </summary>
        /// <param name="provider"> The provider to add </param>
        void AddProvider(IReportingProvider provider);
    }

    /// <summary>
    /// Interface for provider-specific reporting implementations
    /// </summary>
    public interface IReportingProvider
    {
        /// <summary>
        /// Initialize the provider
        /// </summary>
        /// <param name="onComplete"> Callback when initialization is complete </param>
        void Initialize(Action<bool> onComplete = null);

        /// <summary>
        /// Submit a report
        /// </summary>
        /// <param name="report"> Report data </param>
        /// <param name="onComplete"> Callback when submission is complete </param>
        void SubmitReport(ReportData report, Action<bool, string> onComplete = null);

        /// <summary>
        /// Get the provider name
        /// </summary>
        /// <returns> Provider name </returns>
        string GetProviderName();

        /// <summary>
        /// Get the provider status
        /// </summary>
        /// <returns> Provider status </returns>
        bool IsAvailable();
    }

    /// <summary>
    /// Types of reports
    /// </summary>
    public enum ReportType
    {
        /// <summary>
        /// Bug report
        /// </summary>
        Bug,

        /// <summary>
        /// Crash report
        /// </summary>
        Crash,

        /// <summary>
        /// Feedback report
        /// </summary>
        Feedback,

        /// <summary>
        /// Other report
        /// </summary>
        Other
    }

    /// <summary>
    /// Reporting service status information
    /// </summary>
    public class ReportingServiceStatus
    {
        /// <summary>
        /// Whether the service is initialized
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// Whether automatic crash reporting is enabled
        /// </summary>
        public bool AutomaticCrashReporting { get; set; }

        /// <summary>
        /// Number of available providers
        /// </summary>
        public int ProviderCount { get; set; }

        /// <summary>
        /// Number of pending reports
        /// </summary>
        public int PendingReportCount { get; set; }

        /// <summary>
        /// Last error message (if any)
        /// </summary>
        public string LastError { get; set; }
    }
}