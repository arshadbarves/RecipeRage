# Logging Service - Report System Integration

This document outlines how to integrate the RecipeRage Logging system with a player reporting system, allowing players to submit bug reports with detailed logs.

## Overview

The Logging system can be extended to support player-submitted bug reports by:
1. Maintaining a separate circular buffer of logs specifically for error reporting
2. Providing mechanisms to attach logs to bug reports
3. Integrating with a reporting service (like Firebase Crashlytics or a custom solution)

## Design Approach

### 1. Extend the ILogService Interface

Add report-specific methods to the `ILogService` interface:

```csharp
public interface ILogService
{
    // Existing methods...
    
    /// <summary>
    /// Get logs suitable for inclusion in a bug report
    /// </summary>
    /// <param name="maxAge">Maximum age of logs to include (in minutes)</param>
    /// <param name="minLevel">Minimum log level to include</param>
    /// <returns>Formatted string containing relevant logs</returns>
    string GetLogsForReport(int maxAge = 30, LogLevel minLevel = LogLevel.Warning);
    
    /// <summary>
    /// Capture the current state of the application for reporting
    /// </summary>
    /// <param name="description">User-provided description of the issue</param>
    /// <param name="screenshotData">Optional screenshot data</param>
    /// <returns>Report ID that can be referenced later</returns>
    string CaptureReportState(string description, byte[] screenshotData = null);
    
    /// <summary>
    /// Add custom data to the next report
    /// </summary>
    /// <param name="key">Data key</param>
    /// <param name="value">Data value</param>
    void AddReportMetadata(string key, string value);
}
```

### 2. Create a ReportingService Module

Instead of overloading the LogService, create a dedicated ReportingService module that uses the LogService:

```
Assets/Scripts/Modules/Reporting/
│
├── Interfaces/
│   └── IReportingService.cs
│
├── Core/
│   └── ReportingService.cs
│
├── UI/
│   └── ReportDialog.cs
│
└── ReportingHelper.cs
```

The `IReportingService` interface would look like:

```csharp
public interface IReportingService
{
    /// <summary>
    /// Initialize the reporting service
    /// </summary>
    /// <param name="onComplete">Callback when initialization is complete</param>
    void Initialize(Action<bool> onComplete = null);
    
    /// <summary>
    /// Capture information about the current state for reporting
    /// </summary>
    /// <param name="reportType">Type of report (Bug, Crash, Feedback, etc.)</param>
    /// <returns>Report ID</returns>
    string CaptureState(ReportType reportType);
    
    /// <summary>
    /// Add logs to the report
    /// </summary>
    /// <param name="reportId">Report ID</param>
    /// <param name="maxAge">Maximum age of logs to include (in minutes)</param>
    /// <param name="minLevel">Minimum log level to include</param>
    void AddLogs(string reportId, int maxAge = 30, LogLevel minLevel = LogLevel.Warning);
    
    /// <summary>
    /// Add a screenshot to the report
    /// </summary>
    /// <param name="reportId">Report ID</param>
    /// <param name="screenshotData">Screenshot data</param>
    void AddScreenshot(string reportId, byte[] screenshotData);
    
    /// <summary>
    /// Add metadata to the report
    /// </summary>
    /// <param name="reportId">Report ID</param>
    /// <param name="key">Metadata key</param>
    /// <param name="value">Metadata value</param>
    void AddMetadata(string reportId, string key, string value);
    
    /// <summary>
    /// Add user description to the report
    /// </summary>
    /// <param name="reportId">Report ID</param>
    /// <param name="description">User-provided description</param>
    void AddUserDescription(string reportId, string description);
    
    /// <summary>
    /// Submit the report
    /// </summary>
    /// <param name="reportId">Report ID</param>
    /// <param name="onComplete">Callback when submission is complete</param>
    void SubmitReport(string reportId, Action<bool, string> onComplete = null);
    
    /// <summary>
    /// Register for automatic crash reporting
    /// </summary>
    /// <param name="enabled">Whether to enable automatic crash reporting</param>
    void SetAutomaticCrashReporting(bool enabled);
}
```

### 3. Implement Crash Detection

For automatic crash reporting, implement a system that can detect unhandled exceptions and game crashes:

```csharp
// In ReportingService.cs
private void RegisterForCrashDetection()
{
    // Register for Unity exceptions
    Application.logMessageReceived += HandleLogMessage;
    
    // Register for .NET exceptions
    AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
    
    // Set up a watchdog timer to detect freezes
    StartCoroutine(WatchdogCoroutine());
}

private void HandleLogMessage(string logString, string stackTrace, LogType type)
{
    if (type == LogType.Exception || type == LogType.Assert)
    {
        // Capture crash report
        string reportId = CaptureState(ReportType.Crash);
        AddMetadata(reportId, "exception_type", type.ToString());
        AddMetadata(reportId, "stack_trace", stackTrace);
        AddLogs(reportId);
        
        // Submit automatically if automatic reporting is enabled
        if (_automaticReportingEnabled)
        {
            SubmitReport(reportId);
        }
    }
}
```

### 4. Create a User-Friendly Report Dialog

Implement a UI dialog that allows players to:
- Describe the issue they encountered
- Take a screenshot or use the last automatically captured screenshot
- Review what information will be sent
- Submit the report

### 5. Integration with Backend Service

#### Option A: Firebase Crashlytics Integration

```csharp
// In FirebaseCrashlyticsReportingService.cs
public void SubmitReport(string reportId, Action<bool, string> onComplete = null)
{
    ReportData report = _pendingReports[reportId];
    
    // Add all metadata to Crashlytics
    foreach (var item in report.Metadata)
    {
        Firebase.Crashlytics.Crashlytics.SetCustomKey(item.Key, item.Value);
    }
    
    // Add user description
    Firebase.Crashlytics.Crashlytics.SetCustomKey("user_description", report.UserDescription);
    
    // Add logs
    Firebase.Crashlytics.Crashlytics.Log(report.Logs);
    
    // If it's a non-crash report, force a crash report with the custom data
    Exception reportException = new Exception($"User Report: {report.Type}");
    Firebase.Crashlytics.Crashlytics.LogException(reportException);
    
    // Upload screenshot separately to Firebase Storage
    if (report.ScreenshotData != null)
    {
        UploadScreenshot(reportId, report.ScreenshotData);
    }
    
    onComplete?.Invoke(true, reportId);
}
```

#### Option B: Custom Backend Service

```csharp
// In CustomBackendReportingService.cs
public async void SubmitReport(string reportId, Action<bool, string> onComplete = null)
{
    ReportData report = _pendingReports[reportId];
    
    // Create report payload
    var payload = new Dictionary<string, object>
    {
        {"report_id", reportId},
        {"type", report.Type.ToString()},
        {"user_id", AuthHelper.GetCurrentUserId()},
        {"timestamp", DateTime.UtcNow.ToString("o")},
        {"description", report.UserDescription},
        {"metadata", report.Metadata},
        {"logs", report.Logs},
        {"app_version", Application.version},
        {"platform", Application.platform.ToString()},
        {"device_model", SystemInfo.deviceModel},
        {"operating_system", SystemInfo.operatingSystem}
    };
    
    try
    {
        // Submit report to backend
        var response = await HttpClient.PostAsync(
            "https://api.reciperage.com/reports",
            new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
        );
        
        // Upload screenshot if available
        if (report.ScreenshotData != null && response.IsSuccessStatusCode)
        {
            var reportResponse = JsonConvert.DeserializeObject<ReportResponse>(
                await response.Content.ReadAsStringAsync()
            );
            
            await UploadScreenshot(reportResponse.Id, report.ScreenshotData);
        }
        
        bool success = response.IsSuccessStatusCode;
        string resultId = success ? reportId : null;
        onComplete?.Invoke(success, resultId);
    }
    catch (Exception ex)
    {
        LogHelper.Exception("ReportingService", ex, "Failed to submit report");
        onComplete?.Invoke(false, null);
    }
}
```

## Usage Example

```csharp
// In GameManager.cs or similar class
public void ShowReportDialog()
{
    // Capture a screenshot
    StartCoroutine(CaptureScreenshotCoroutine());
}

private IEnumerator CaptureScreenshotCoroutine()
{
    // Wait for the end of frame to capture screenshot
    yield return new WaitForEndOfFrame();
    
    // Capture the screenshot
    Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
    screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
    screenshot.Apply();
    
    // Convert to bytes
    byte[] screenshotBytes = screenshot.EncodeToPNG();
    Destroy(screenshot);
    
    // Create report
    string reportId = ReportingHelper.CaptureState(ReportType.Bug);
    ReportingHelper.AddScreenshot(reportId, screenshotBytes);
    ReportingHelper.AddLogs(reportId);
    
    // Add system info
    ReportingHelper.AddMetadata(reportId, "graphics_device_name", SystemInfo.graphicsDeviceName);
    ReportingHelper.AddMetadata(reportId, "graphics_memory", SystemInfo.graphicsMemorySize.ToString());
    ReportingHelper.AddMetadata(reportId, "processor_type", SystemInfo.processorType);
    ReportingHelper.AddMetadata(reportId, "system_memory", SystemInfo.systemMemorySize.ToString());
    
    // Show dialog
    ReportDialog.Show(reportId, OnReportSubmitted);
}

private void OnReportSubmitted(bool success, string reportId)
{
    if (success)
    {
        UIManager.ShowNotification("Thank you for your report! We'll look into it.");
    }
    else
    {
        UIManager.ShowNotification("Failed to submit report. Please try again later.");
    }
}
```

## Best Practices

1. **Respect Privacy**:
   - Always inform users about what data will be collected
   - Provide options to review data before submission
   - Never automatically collect personally identifiable information

2. **Minimize Data Size**:
   - Filter logs to include only relevant information
   - Compress screenshots and logs
   - Set reasonable time limits for log history

3. **Handle Offline Scenarios**:
   - Store reports locally if they can't be submitted immediately
   - Retry submission when connection is restored
   - Provide users with feedback about submission status

4. **Provide Context**:
   - Collect system information to help reproduce issues
   - Include app state and breadcrumbs
   - Allow users to describe steps to reproduce

5. **Security Considerations**:
   - Encrypt sensitive information in reports
   - Use secure connections for submission
   - Sanitize logs to remove sensitive data

## Integration with Analytics

For a complete picture of application health, integrate the Reporting system with Analytics:

1. Track report submission as an analytics event
2. Monitor frequency of different report types
3. Correlate user flows with crash reports
4. Measure impact of fixes by tracking issue recurrence

## Implementation Timeline

1. Extend LogService to support report-oriented log retrieval
2. Create ReportingService interface and implementation
3. Implement crash detection and automatic reporting
4. Create user-facing report dialog
5. Integrate with backend service
6. Test reporting flow end-to-end
7. Implement offline support and retry logic

## Conclusion

By creating a dedicated Reporting module that leverages the Logging system, we can provide a comprehensive error reporting solution without overloading the Logging module's responsibilities. This approach maintains separation of concerns while enabling powerful reporting capabilities. 