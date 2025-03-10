using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RecipeRage.Modules.Logging;
using RecipeRage.Modules.Reporting;
using RecipeRage.Modules.Reporting.Data;
using RecipeRage.Modules.Reporting.Interfaces;

namespace RecipeRage.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the Reporting module.
    /// Shows how to create and submit bug reports, handle crashes, and capture screenshots.
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public class ReportingExample : MonoBehaviour
    {
        [Header("Reporting Settings")]
        [SerializeField] private bool _automaticCrashReporting = true;
        
        [Header("UI Elements")]
        [SerializeField] private InputField _descriptionInput;
        [SerializeField] private Toggle _includeScreenshotToggle;
        [SerializeField] private Button _submitButton;
        [SerializeField] private Text _statusText;
        [SerializeField] private Image _screenshotPreview;
        
        private byte[] _currentScreenshot;
        private string _currentReportId;
        private bool _isInitialized = false;
        
        /// <summary>
        /// Initialize the reporting system when the component is enabled
        /// </summary>
        private void OnEnable()
        {
            // Initialize the logging system first
            LogHelper.SetConsoleOutput(true);
            LogHelper.SetFileOutput(true);
            LogHelper.SetLogLevel(LogLevel.Debug);
            
            // Initialize the reporting system
            InitializeReporting();
            
            // Register event handlers
            RegisterEvents();
            
            // Set up UI if available
            SetupUI();
        }
        
        /// <summary>
        /// Clean up when the component is disabled
        /// </summary>
        private void OnDisable()
        {
            // Unregister event handlers
            UnregisterEvents();
        }
        
        /// <summary>
        /// Initialize the reporting system
        /// </summary>
        private void InitializeReporting()
        {
            LogHelper.Info("ReportingExample", "Initializing reporting system");
            
            ReportingHelper.Initialize(success =>
            {
                _isInitialized = success;
                
                if (success)
                {
                    LogHelper.Info("ReportingExample", "Reporting system initialized successfully");
                    
                    // Enable automatic crash reporting if selected
                    if (_automaticCrashReporting)
                    {
                        ReportingHelper.SetAutomaticCrashReporting(true);
                        LogHelper.Info("ReportingExample", "Automatic crash reporting enabled");
                    }
                    
                    // Update UI
                    UpdateStatusText("Reporting system initialized.");
                }
                else
                {
                    LogHelper.Error("ReportingExample", "Failed to initialize reporting system");
                    UpdateStatusText("Failed to initialize reporting system.");
                }
            });
        }
        
        /// <summary>
        /// Register for reporting events
        /// </summary>
        private void RegisterEvents()
        {
            ReportingHelper.OnReportCreated += HandleReportCreated;
            ReportingHelper.OnReportSubmitted += HandleReportSubmitted;
            ReportingHelper.OnCrashDetected += HandleCrashDetected;
        }
        
        /// <summary>
        /// Unregister from reporting events
        /// </summary>
        private void UnregisterEvents()
        {
            ReportingHelper.OnReportCreated -= HandleReportCreated;
            ReportingHelper.OnReportSubmitted -= HandleReportSubmitted;
            ReportingHelper.OnCrashDetected -= HandleCrashDetected;
        }
        
        /// <summary>
        /// Set up UI elements
        /// </summary>
        private void SetupUI()
        {
            if (_submitButton != null)
            {
                _submitButton.onClick.AddListener(OnSubmitButtonClicked);
            }
            
            if (_includeScreenshotToggle != null)
            {
                _includeScreenshotToggle.onValueChanged.AddListener(OnScreenshotToggleChanged);
            }
            
            if (_descriptionInput != null)
            {
                _descriptionInput.placeholder.GetComponent<Text>().text = "Describe the issue...";
            }
            
            if (_statusText != null)
            {
                _statusText.text = "Initializing reporting system...";
            }
            
            // Take an initial screenshot for preview
            if (_includeScreenshotToggle != null && _includeScreenshotToggle.isOn)
            {
                CaptureScreenshotForPreview();
            }
        }
        
        /// <summary>
        /// Handle the submit button being clicked
        /// </summary>
        private void OnSubmitButtonClicked()
        {
            if (!_isInitialized)
            {
                UpdateStatusText("Reporting system not initialized. Please try again later.");
                return;
            }
            
            // Disable the button during submission
            if (_submitButton != null)
            {
                _submitButton.interactable = false;
            }
            
            UpdateStatusText("Creating report...");
            
            // Get the description
            string description = _descriptionInput != null ? _descriptionInput.text : "";
            
            // Check if we should include a screenshot
            bool includeScreenshot = _includeScreenshotToggle != null && _includeScreenshotToggle.isOn;
            
            // Create the report
            CreateReport(description, includeScreenshot);
        }
        
        /// <summary>
        /// Handle the screenshot toggle being changed
        /// </summary>
        /// <param name="isOn">Whether the toggle is on</param>
        private void OnScreenshotToggleChanged(bool isOn)
        {
            if (isOn)
            {
                CaptureScreenshotForPreview();
            }
            else
            {
                if (_screenshotPreview != null)
                {
                    _screenshotPreview.enabled = false;
                }
                
                _currentScreenshot = null;
            }
        }
        
        /// <summary>
        /// Create a bug report
        /// </summary>
        /// <param name="description">User-provided description</param>
        /// <param name="includeScreenshot">Whether to include a screenshot</param>
        private void CreateReport(string description, bool includeScreenshot)
        {
            ReportingHelper.CreateReport(ReportType.Bug, includeScreenshot, reportId =>
            {
                if (reportId != null)
                {
                    _currentReportId = reportId;
                    
                    // Add the description
                    if (!string.IsNullOrEmpty(description))
                    {
                        ReportingHelper.AddUserDescription(reportId, description);
                    }
                    
                    UpdateStatusText("Report created. Ready to submit.");
                    
                    // Re-enable the button
                    if (_submitButton != null)
                    {
                        _submitButton.interactable = true;
                        _submitButton.GetComponentInChildren<Text>().text = "Submit Report";
                    }
                    
                    // Submit the report automatically
                    SubmitReport(reportId);
                }
                else
                {
                    UpdateStatusText("Failed to create report.");
                    
                    // Re-enable the button
                    if (_submitButton != null)
                    {
                        _submitButton.interactable = true;
                    }
                }
            });
        }
        
        /// <summary>
        /// Submit a report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        private void SubmitReport(string reportId)
        {
            UpdateStatusText("Submitting report...");
            
            // Disable the button during submission
            if (_submitButton != null)
            {
                _submitButton.interactable = false;
            }
            
            ReportingHelper.SubmitReport(reportId, (success, externalReportId) =>
            {
                if (success)
                {
                    UpdateStatusText($"Report submitted successfully. ID: {externalReportId}");
                }
                else
                {
                    UpdateStatusText("Failed to submit report. Please try again later.");
                    
                    // Re-enable the button
                    if (_submitButton != null)
                    {
                        _submitButton.interactable = true;
                    }
                }
            });
        }
        
        /// <summary>
        /// Capture a screenshot for preview
        /// </summary>
        private void CaptureScreenshotForPreview()
        {
            ReportingHelper.CaptureScreenshot(screenshotData =>
            {
                _currentScreenshot = screenshotData;
                
                if (screenshotData != null && _screenshotPreview != null)
                {
                    // Create a texture from the screenshot data
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(screenshotData);
                    
                    // Create a sprite from the texture
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    
                    // Set the sprite on the image
                    _screenshotPreview.sprite = sprite;
                    _screenshotPreview.enabled = true;
                }
            });
        }
        
        /// <summary>
        /// Update the status text
        /// </summary>
        /// <param name="status">Status text</param>
        private void UpdateStatusText(string status)
        {
            if (_statusText != null)
            {
                _statusText.text = status;
            }
            
            LogHelper.Info("ReportingExample", status);
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Handle a report being created
        /// </summary>
        /// <param name="report">Report data</param>
        private void HandleReportCreated(ReportData report)
        {
            LogHelper.Info("ReportingExample", $"Report created: {report.Id} ({report.Type})");
        }
        
        /// <summary>
        /// Handle a report being submitted
        /// </summary>
        /// <param name="report">Report data</param>
        /// <param name="success">Whether submission was successful</param>
        /// <param name="externalReportId">External report ID</param>
        private void HandleReportSubmitted(ReportData report, bool success, string externalReportId)
        {
            if (success)
            {
                LogHelper.Info("ReportingExample", $"Report submitted: {report.Id} -> {externalReportId}");
            }
            else
            {
                LogHelper.Warning("ReportingExample", $"Report submission failed: {report.Id}");
            }
        }
        
        /// <summary>
        /// Handle a crash being detected
        /// </summary>
        /// <param name="report">Report data</param>
        private void HandleCrashDetected(ReportData report)
        {
            LogHelper.Error("ReportingExample", $"Crash detected! Report ID: {report.Id}");
        }
        
        #endregion
        
        #region Test Methods
        
        /// <summary>
        /// Simulate a crash for testing
        /// </summary>
        public void SimulateCrash()
        {
            LogHelper.Info("ReportingExample", "Simulating a crash...");
            
            // This will cause a NullReferenceException
            GameObject obj = null;
            obj.transform.position = Vector3.zero;
        }
        
        /// <summary>
        /// Throw a test exception
        /// </summary>
        public void ThrowTestException()
        {
            LogHelper.Info("ReportingExample", "Throwing a test exception...");
            
            try
            {
                throw new InvalidOperationException("This is a test exception!");
            }
            catch (Exception ex)
            {
                LogHelper.Exception("ReportingExample", ex, "Test exception caught");
                
                // Create a report for the exception
                string reportId = ReportingHelper.CaptureState(ReportType.Crash);
                
                if (reportId != null)
                {
                    ReportingHelper.AddMetadata(reportId, "exception_type", ex.GetType().Name);
                    ReportingHelper.AddMetadata(reportId, "exception_message", ex.Message);
                    ReportingHelper.AddMetadata(reportId, "stack_trace", ex.StackTrace);
                    ReportingHelper.AddUserDescription(reportId, "This is a test exception report.");
                    ReportingHelper.AddLogs(reportId);
                    
                    UpdateStatusText($"Created crash report for test exception: {reportId}");
                    
                    // Submit the report
                    ReportingHelper.SubmitReport(reportId);
                }
            }
        }
        
        /// <summary>
        /// Show the reporting service status
        /// </summary>
        public void ShowStatus()
        {
            var status = ReportingHelper.GetStatus();
            
            string statusText = $"Reporting Service Status:\n" +
                               $"Initialized: {status.IsInitialized}\n" +
                               $"Automatic Crash Reporting: {status.AutomaticCrashReporting}\n" +
                               $"Provider Count: {status.ProviderCount}\n" +
                               $"Pending Reports: {status.PendingReportCount}\n";
            
            if (!string.IsNullOrEmpty(status.LastError))
            {
                statusText += $"Last Error: {status.LastError}";
            }
            
            UpdateStatusText(statusText);
        }
        
        #endregion
    }
} 