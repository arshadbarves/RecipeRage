# Reporting System

A modular reporting system for RecipeRage that enables bug reports, crash detection, and user feedback.

## Overview

The Reporting system provides a comprehensive solution for collecting, managing, and submitting bug reports and crash reports. It integrates with the Logging module to include relevant logs and automatically captures system information to help with debugging.

Key features:
- Bug report creation with user descriptions
- Automatic crash detection and reporting
- Screenshot capture
- System information collection
- Integration with the Logging module
- Multiple reporting providers support
- Offline reporting with automatic retry
- Event-based reporting workflow
- UI components for report creation

## Architecture

The Reporting system follows a modular design with clear separation of concerns:

```
Assets/Scripts/Modules/Reporting/
│
├── Interfaces/           # Interface definitions
│   └── IReportingService.cs  # Main reporting interface
│
├── Core/                 # Core implementations
│   └── ReportingService.cs   # Default reporting service implementation
│
├── Data/                 # Data models
│   └── ReportData.cs     # Report data model
│
├── UI/                   # UI components
│   └── ReportDialog.cs   # Reporting dialog
│
└── ReportingHelper.cs    # Static helper for easy access
```

## Getting Started

### Initialization

Initialize the reporting system early in your application:

```csharp
// Initialize reporting with automatic crash detection
ReportingHelper.Initialize(success =>
{
    if (success)
    {
        Debug.Log("Reporting system initialized successfully");
        
        // Enable automatic crash reporting (optional)
        ReportingHelper.SetAutomaticCrashReporting(true);
    }
});
```

### Basic Usage

#### Creating a Bug Report

```csharp
// Create a simple bug report
string reportId = ReportingHelper.CaptureState(ReportType.Bug);

// Add user description
ReportingHelper.AddUserDescription(reportId, "The game crashed when I tried to start a new level.");

// Add logs (last 30 minutes, warning level and above)
ReportingHelper.AddLogs(reportId, 30, LogLevel.Warning);

// Submit the report
ReportingHelper.SubmitReport(reportId, (success, externalReportId) =>
{
    if (success)
    {
        Debug.Log($"Report submitted successfully. ID: {externalReportId}");
    }
    else
    {
        Debug.LogError("Failed to submit report.");
    }
});
```

#### Capturing a Screenshot

```csharp
// Capture a screenshot and add it to the report
ReportingHelper.CaptureScreenshot(screenshotData =>
{
    if (screenshotData != null)
    {
        ReportingHelper.AddScreenshot(reportId, screenshotData);
    }
    
    // Continue with the report submission
    ReportingHelper.SubmitReport(reportId);
});
```

#### Using the Convenience Method

```csharp
// Create a report with screenshot and logs in one call
ReportingHelper.CreateReport(ReportType.Bug, includeScreenshot: true, reportId =>
{
    if (reportId != null)
    {
        // Add user description
        ReportingHelper.AddUserDescription(reportId, "The game crashed when I tried to start a new level.");
        
        // Submit the report
        ReportingHelper.SubmitReport(reportId);
    }
});
```

### Handling Crashes

The Reporting system can automatically detect crashes and create reports for them:

```csharp
// Enable automatic crash reporting
ReportingHelper.SetAutomaticCrashReporting(true);

// Register for crash events
ReportingHelper.OnCrashDetected += (report) =>
{
    Debug.LogError($"Crash detected! Report ID: {report.Id}");
    
    // You can add additional information to the crash report here
    ReportingHelper.AddMetadata(report.Id, "current_level", currentLevel.ToString());
    
    // The report will be automatically submitted if automatic reporting is enabled
};
```

### Manual Exception Handling

You can also create reports for caught exceptions:

```csharp
try
{
    // Some code that might throw
    DoSomethingRisky();
}
catch (Exception ex)
{
    // Log the exception
    LogHelper.Exception("MyModule", ex, "An error occurred during risky operation");
    
    // Create a crash report
    string reportId = ReportingHelper.CaptureState(ReportType.Crash);
    
    // Add exception details
    ReportingHelper.AddMetadata(reportId, "exception_type", ex.GetType().Name);
    ReportingHelper.AddMetadata(reportId, "exception_message", ex.Message);
    ReportingHelper.AddMetadata(reportId, "stack_trace", ex.StackTrace);
    
    // Add logs
    ReportingHelper.AddLogs(reportId);
    
    // Submit the report
    ReportingHelper.SubmitReport(reportId);
}
```

## Event Handling

The Reporting system provides events for tracking the reporting workflow:

```csharp
// Register for reporting events
ReportingHelper.OnReportCreated += (report) =>
{
    Debug.Log($"Report created: {report.Id} ({report.Type})");
};

ReportingHelper.OnReportSubmitted += (report, success, externalReportId) =>
{
    if (success)
    {
        Debug.Log($"Report submitted: {report.Id} -> {externalReportId}");
    }
    else
    {
        Debug.LogWarning($"Report submission failed: {report.Id}");
    }
};

ReportingHelper.OnCrashDetected += (report) =>
{
    Debug.LogError($"Crash detected! Report ID: {report.Id}");
};
```

## Report Data

Each report includes a wealth of information to help with debugging:

- **System Information**: Device model, OS version, CPU, GPU, memory, etc.
- **Application Information**: App version, Unity version, build GUID, etc.
- **Screen Information**: Resolution, DPI, orientation, etc.
- **Quality Settings**: Quality level, VSync, etc.
- **User Description**: The user's description of the issue
- **Logs**: Recent logs from the Logging module
- **Screenshot**: An optional screenshot of the application
- **Custom Metadata**: Any additional information you add to the report

## Custom Reporting Providers

You can implement your own reporting provider by implementing the `IReportingProvider` interface:

```csharp
public class CustomReportingProvider : IReportingProvider
{
    public void Initialize(Action<bool> onComplete = null)
    {
        // Initialize the provider
        onComplete?.Invoke(true);
    }
    
    public void SubmitReport(ReportData report, Action<bool, string> onComplete = null)
    {
        // Submit the report to your backend
        // ...
        
        onComplete?.Invoke(true, "external-report-id");
    }
    
    public string GetProviderName()
    {
        return "CustomProvider";
    }
    
    public bool IsAvailable()
    {
        // Check if the provider is available
        return true;
    }
}

// Add the provider to the reporting service
ReportingHelper.AddProvider(new CustomReportingProvider());
```

## Integration with Analytics

The Reporting system automatically integrates with the Analytics module (if available) to track report creation and submission:

```csharp
// In ReportingService.cs
private void SendReportToAnalytics(ReportData report)
{
    // Create event parameters
    Dictionary<string, object> parameters = new Dictionary<string, object>
    {
        { "report_id", report.Id },
        { "report_type", report.Type.ToString() },
        { "has_screenshot", report.ScreenshotData != null },
        { "has_logs", !string.IsNullOrEmpty(report.Logs) },
        // ... other parameters
    };
    
    // Log the event
    AnalyticsHelper.LogEvent("report_created", parameters);
}
```

## Performance Considerations

- Screenshots are stored in memory only, not on disk (to reduce file I/O)
- Logs are filtered by age and level to minimize data size
- Reports are sent asynchronously to avoid blocking the main thread
- Report data is serialized only when needed to minimize overhead

## Thread Safety

The Reporting system is designed to be used from the main thread only.

## License

Copyright © 2024 RecipeRage. All rights reserved. 