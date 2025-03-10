using System;
using System.Collections.Generic;
using UnityEngine;
using RecipeRage.Modules.Reporting.Interfaces;

namespace RecipeRage.Modules.Reporting.Data
{
    /// <summary>
    /// Data model for bug and crash reports
    /// 
    /// Complexity Rating: 2
    /// </summary>
    [Serializable]
    public class ReportData
    {
        /// <summary>
        /// Unique ID of the report
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Type of report
        /// </summary>
        public ReportType Type { get; set; }
        
        /// <summary>
        /// When the report was created
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// User-provided description of the issue
        /// </summary>
        public string UserDescription { get; set; }
        
        /// <summary>
        /// Log entries included in the report
        /// </summary>
        public string Logs { get; set; }
        
        /// <summary>
        /// Screenshot data (PNG format)
        /// </summary>
        public byte[] ScreenshotData { get; set; }
        
        /// <summary>
        /// Additional metadata for the report
        /// </summary>
        public Dictionary<string, string> Metadata { get; private set; }
        
        /// <summary>
        /// Whether the report has been submitted
        /// </summary>
        public bool IsSubmitted { get; set; }
        
        /// <summary>
        /// Result of submission (if submitted)
        /// </summary>
        public bool IsSubmissionSuccessful { get; set; }
        
        /// <summary>
        /// Error message from submission (if failed)
        /// </summary>
        public string SubmissionError { get; set; }
        
        /// <summary>
        /// External report ID (if submitted)
        /// </summary>
        public string ExternalReportId { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Unique ID for the report</param>
        /// <param name="type">Type of report</param>
        public ReportData(string id, ReportType type)
        {
            Id = id;
            Type = type;
            Timestamp = DateTime.UtcNow;
            Metadata = new Dictionary<string, string>();
            
            // Add default metadata
            AddSystemInfo();
        }
        
        /// <summary>
        /// Add a metadata value to the report
        /// </summary>
        /// <param name="key">Metadata key</param>
        /// <param name="value">Metadata value</param>
        public void AddMetadata(string key, string value)
        {
            if (Metadata == null)
            {
                Metadata = new Dictionary<string, string>();
            }
            
            Metadata[key] = value;
        }
        
        /// <summary>
        /// Get metadata value from the report
        /// </summary>
        /// <param name="key">Metadata key</param>
        /// <param name="defaultValue">Default value if key not found</param>
        /// <returns>Metadata value</returns>
        public string GetMetadata(string key, string defaultValue = null)
        {
            if (Metadata == null || !Metadata.ContainsKey(key))
            {
                return defaultValue;
            }
            
            return Metadata[key];
        }
        
        /// <summary>
        /// Mark the report as submitted
        /// </summary>
        /// <param name="success">Whether submission was successful</param>
        /// <param name="error">Error message (if failed)</param>
        /// <param name="externalReportId">External report ID (if successful)</param>
        public void MarkAsSubmitted(bool success, string error = null, string externalReportId = null)
        {
            IsSubmitted = true;
            IsSubmissionSuccessful = success;
            SubmissionError = error;
            ExternalReportId = externalReportId;
        }
        
        /// <summary>
        /// Add system information to the report metadata
        /// </summary>
        private void AddSystemInfo()
        {
            // Device information
            AddMetadata("device_model", SystemInfo.deviceModel);
            AddMetadata("device_name", SystemInfo.deviceName);
            AddMetadata("device_type", SystemInfo.deviceType.ToString());
            
            // Operating system information
            AddMetadata("os", SystemInfo.operatingSystem);
            AddMetadata("os_family", SystemInfo.operatingSystemFamily.ToString());
            
            // Memory information
            AddMetadata("system_memory", SystemInfo.systemMemorySize.ToString());
            
            // Graphics information
            AddMetadata("graphics_device_name", SystemInfo.graphicsDeviceName);
            AddMetadata("graphics_device_type", SystemInfo.graphicsDeviceType.ToString());
            AddMetadata("graphics_memory", SystemInfo.graphicsMemorySize.ToString());
            AddMetadata("graphics_shader_level", SystemInfo.graphicsShaderLevel.ToString());
            
            // CPU information
            AddMetadata("processor_type", SystemInfo.processorType);
            AddMetadata("processor_count", SystemInfo.processorCount.ToString());
            AddMetadata("processor_frequency", SystemInfo.processorFrequency.ToString());
            
            // Application information
            AddMetadata("app_version", Application.version);
            AddMetadata("unity_version", Application.unityVersion);
            AddMetadata("platform", Application.platform.ToString());
            AddMetadata("build_guid", Application.buildGUID);
            AddMetadata("install_mode", Application.installMode.ToString());
            AddMetadata("system_language", Application.systemLanguage.ToString());
            
            // Time information
            AddMetadata("timestamp_utc", DateTime.UtcNow.ToString("o"));
            AddMetadata("uptime_seconds", Time.realtimeSinceStartup.ToString());
            AddMetadata("time_scale", Time.timeScale.ToString());
            
            // Screen information
            AddMetadata("screen_width", Screen.width.ToString());
            AddMetadata("screen_height", Screen.height.ToString());
            AddMetadata("screen_dpi", Screen.dpi.ToString());
            AddMetadata("screen_orientation", Screen.orientation.ToString());
            
            // Quality settings
            AddMetadata("quality_level", QualitySettings.GetQualityLevel().ToString());
            AddMetadata("vsync_count", QualitySettings.vSyncCount.ToString());
        }
    }
} 