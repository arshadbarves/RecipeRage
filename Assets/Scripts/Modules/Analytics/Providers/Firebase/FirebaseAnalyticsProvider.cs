using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using RecipeRage.Modules.Analytics.Interfaces;
using RecipeRage.Modules.Logging;

#if FIREBASE_ANALYTICS
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
#endif

namespace RecipeRage.Modules.Analytics.Providers.Firebase
{
    /// <summary>
    /// Firebase Analytics implementation of the analytics provider.
    /// Uses Firebase SDK for Unity.
    /// 
    /// Complexity Rating: 4
    /// </summary>
    public class FirebaseAnalyticsProvider : IAnalyticsProvider
    {
        private const string PROVIDER_NAME = "Firebase";
        private bool _isInitialized = false;
        private bool _isEnabled = true;
        private DependencyStatus _dependencyStatus = DependencyStatus.UnavailableMissingDependency;
        
#if FIREBASE_ANALYTICS
        private FirebaseApp _firebaseApp = null;
#endif
        
        /// <summary>
        /// Initialize the Firebase provider
        /// </summary>
        /// <param name="consentRequired">Whether user consent is required before tracking</param>
        /// <param name="onComplete">Callback when initialization is complete</param>
        public void Initialize(bool consentRequired = false, Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                LogHelper.Warning("Firebase Analytics", "Provider already initialized");
                onComplete?.Invoke(true);
                return;
            }
            
            LogHelper.Info("Firebase Analytics", "Initializing Firebase Analytics provider");
            
#if FIREBASE_ANALYTICS
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                _dependencyStatus = task.Result;
                if (_dependencyStatus == DependencyStatus.Available)
                {
                    _firebaseApp = FirebaseApp.DefaultInstance;
                    
                    // Set default collection state based on consent requirements
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(!consentRequired);
                    
                    // Configure global properties
                    SetDefaultUserProperties();
                    
                    _isInitialized = true;
                    LogHelper.Info("Firebase Analytics", "Firebase Analytics initialized successfully");
                    onComplete?.Invoke(true);
                }
                else
                {
                    LogHelper.Error("Firebase Analytics", $"Firebase initialization failed: {_dependencyStatus}");
                    _isInitialized = false;
                    onComplete?.Invoke(false);
                }
            });
#else
            LogHelper.Warning("Firebase Analytics", "Firebase Analytics is not enabled. Add FIREBASE_ANALYTICS define to enable it.");
            _isInitialized = false;
            onComplete?.Invoke(false);
#endif
        }
        
        /// <summary>
        /// Log an event with optional parameters
        /// </summary>
        /// <param name="eventName">Name of the event to log</param>
        /// <param name="parameters">Optional parameters for the event</param>
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!CheckInitialized())
            {
                return;
            }
            
#if FIREBASE_ANALYTICS
            try
            {
                if (parameters == null || parameters.Count == 0)
                {
                    FirebaseAnalytics.LogEvent(eventName);
                    LogHelper.Debug("Firebase Analytics", $"Logged event: {eventName}");
                }
                else
                {
                    // Convert parameters to Firebase parameters
                    List<Parameter> firebaseParams = ConvertToFirebaseParameters(parameters);
                    
                    FirebaseAnalytics.LogEvent(eventName, firebaseParams.ToArray());
                    LogHelper.Debug("Firebase Analytics", $"Logged event: {eventName} with {firebaseParams.Count} parameters");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Exception("Firebase Analytics", ex, $"Failed to log event: {eventName}");
            }
#endif
        }
        
        /// <summary>
        /// Log a purchase event
        /// </summary>
        /// <param name="productId">ID of the product purchased</param>
        /// <param name="currency">Currency code (e.g., USD)</param>
        /// <param name="price">Price of the product</param>
        /// <param name="source">Source of the purchase (e.g., store, offer)</param>
        /// <param name="parameters">Additional parameters</param>
        public void LogPurchase(string productId, string currency, double price, string source = null, Dictionary<string, object> parameters = null)
        {
            if (!CheckInitialized())
            {
                return;
            }
            
#if FIREBASE_ANALYTICS
            try
            {
                // Convert parameters to Firebase parameters
                List<Parameter> firebaseParams = new List<Parameter>();
                
                // Add required parameters
                firebaseParams.Add(new Parameter(FirebaseAnalytics.ParameterItemId, productId));
                firebaseParams.Add(new Parameter(FirebaseAnalytics.ParameterCurrency, currency));
                firebaseParams.Add(new Parameter(FirebaseAnalytics.ParameterValue, price));
                
                if (!string.IsNullOrEmpty(source))
                {
                    firebaseParams.Add(new Parameter("source", source));
                }
                
                // Add additional parameters
                if (parameters != null)
                {
                    foreach (var param in ConvertToFirebaseParameters(parameters))
                    {
                        // Skip parameters we already added
                        if (param.Name != FirebaseAnalytics.ParameterItemId && 
                            param.Name != FirebaseAnalytics.ParameterCurrency && 
                            param.Name != FirebaseAnalytics.ParameterValue)
                        {
                            firebaseParams.Add(param);
                        }
                    }
                }
                
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, firebaseParams.ToArray());
                LogHelper.Debug("Firebase Analytics", $"Logged purchase: {productId} for {price} {currency}");
            }
            catch (Exception ex)
            {
                LogHelper.Exception("Firebase Analytics", ex, $"Failed to log purchase: {productId}");
            }
#endif
        }
        
        /// <summary>
        /// Set a user property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Value of the property</param>
        public void SetUserProperty(string propertyName, string value)
        {
            if (!CheckInitialized())
            {
                return;
            }
            
#if FIREBASE_ANALYTICS
            try
            {
                FirebaseAnalytics.SetUserProperty(propertyName, value);
                LogHelper.Debug("Firebase Analytics", $"Set user property: {propertyName} = {value}");
            }
            catch (Exception ex)
            {
                LogHelper.Exception("Firebase Analytics", ex, $"Failed to set user property: {propertyName}");
            }
#endif
        }
        
        /// <summary>
        /// Set the user ID for cross-device tracking
        /// </summary>
        /// <param name="userId">Unique user identifier</param>
        public void SetUserId(string userId)
        {
            if (!CheckInitialized())
            {
                return;
            }
            
#if FIREBASE_ANALYTICS
            try
            {
                FirebaseAnalytics.SetUserId(userId);
                LogHelper.Debug("Firebase Analytics", $"Set user ID: {userId}");
            }
            catch (Exception ex)
            {
                LogHelper.Exception("Firebase Analytics", ex, $"Failed to set user ID: {userId}");
            }
#endif
        }
        
        /// <summary>
        /// Set user consent for data collection
        /// </summary>
        /// <param name="consentType">Type of consent</param>
        /// <param name="granted">Whether consent is granted</param>
        public void SetUserConsent(AnalyticsConsentType consentType, bool granted)
        {
            if (!CheckInitialized())
            {
                return;
            }
            
#if FIREBASE_ANALYTICS
            try
            {
                if (consentType == AnalyticsConsentType.AnalyticsStorage)
                {
                    // For Firebase, we only support analytics consent
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(granted);
                    _isEnabled = granted;
                    LogHelper.Debug("Firebase Analytics", $"Set analytics collection enabled: {granted}");
                }
                // Other consent types are not directly supported by Firebase Analytics
            }
            catch (Exception ex)
            {
                LogHelper.Exception("Firebase Analytics", ex, $"Failed to set consent for {consentType}");
            }
#endif
        }
        
        /// <summary>
        /// Enable or disable analytics collection
        /// </summary>
        /// <param name="enabled">Whether analytics collection should be enabled</param>
        public void SetEnabled(bool enabled)
        {
            if (!CheckInitialized())
            {
                return;
            }
            
#if FIREBASE_ANALYTICS
            try
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(enabled);
                _isEnabled = enabled;
                LogHelper.Debug("Firebase Analytics", $"Set analytics collection enabled: {enabled}");
            }
            catch (Exception ex)
            {
                LogHelper.Exception("Firebase Analytics", ex, $"Failed to set analytics collection enabled: {enabled}");
            }
#endif
        }
        
        /// <summary>
        /// Reset all analytics data
        /// </summary>
        public void ResetData()
        {
            if (!CheckInitialized())
            {
                return;
            }
            
#if FIREBASE_ANALYTICS
            try
            {
                FirebaseAnalytics.ResetAnalyticsData();
                LogHelper.Debug("Firebase Analytics", "Reset analytics data");
            }
            catch (Exception ex)
            {
                LogHelper.Exception("Firebase Analytics", ex, "Failed to reset analytics data");
            }
#endif
        }
        
        /// <summary>
        /// Flush any pending analytics events
        /// </summary>
        public void Flush()
        {
            // Firebase Analytics automatically flushes events, but we could force it with additional logic if needed
            if (!CheckInitialized())
            {
                return;
            }
            
            LogHelper.Debug("Firebase Analytics", "Flushing analytics events (automatic in Firebase)");
        }
        
        /// <summary>
        /// Get the provider name
        /// </summary>
        /// <returns>Provider name</returns>
        public string GetProviderName()
        {
            return PROVIDER_NAME;
        }
        
        /// <summary>
        /// Check if the provider is initialized
        /// </summary>
        /// <returns>True if initialized, false otherwise</returns>
        private bool CheckInitialized()
        {
            if (!_isInitialized)
            {
                LogHelper.Warning("Firebase Analytics", "Provider not initialized");
                return false;
            }
            
            if (!_isEnabled)
            {
                LogHelper.Debug("Firebase Analytics", "Provider is disabled");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Set default user properties that will be sent with all events
        /// </summary>
        private void SetDefaultUserProperties()
        {
#if FIREBASE_ANALYTICS
            try
            {
                // Set common user properties that are useful for segmentation
                SetUserProperty("device_model", SystemInfo.deviceModel);
                SetUserProperty("os_version", SystemInfo.operatingSystem);
                SetUserProperty("graphics_device", SystemInfo.graphicsDeviceName);
                SetUserProperty("memory_size", SystemInfo.systemMemorySize.ToString());
                SetUserProperty("processor_count", SystemInfo.processorCount.ToString());
                SetUserProperty("app_version", Application.version);
                SetUserProperty("unity_version", Application.unityVersion);
                SetUserProperty("install_mode", Application.installMode.ToString());
                SetUserProperty("language", Application.systemLanguage.ToString());
                
                LogHelper.Debug("Firebase Analytics", "Set default user properties");
            }
            catch (Exception ex)
            {
                LogHelper.Exception("Firebase Analytics", ex, "Failed to set default user properties");
            }
#endif
        }
        
        /// <summary>
        /// Convert a dictionary of parameters to Firebase parameters
        /// </summary>
        /// <param name="parameters">Dictionary of parameters</param>
        /// <returns>List of Firebase parameters</returns>
        private List<Parameter> ConvertToFirebaseParameters(Dictionary<string, object> parameters)
        {
            List<Parameter> firebaseParams = new List<Parameter>();
            
#if FIREBASE_ANALYTICS
            foreach (var param in parameters)
            {
                // Skip null values
                if (param.Value == null)
                {
                    continue;
                }
                
                // Convert the parameter based on its type
                if (param.Value is string stringValue)
                {
                    firebaseParams.Add(new Parameter(param.Key, stringValue));
                }
                else if (param.Value is long longValue)
                {
                    firebaseParams.Add(new Parameter(param.Key, longValue));
                }
                else if (param.Value is int intValue)
                {
                    firebaseParams.Add(new Parameter(param.Key, (long)intValue));
                }
                else if (param.Value is double doubleValue)
                {
                    firebaseParams.Add(new Parameter(param.Key, doubleValue));
                }
                else if (param.Value is float floatValue)
                {
                    firebaseParams.Add(new Parameter(param.Key, (double)floatValue));
                }
                else if (param.Value is bool boolValue)
                {
                    // Firebase doesn't support boolean parameters, so convert to int
                    firebaseParams.Add(new Parameter(param.Key, boolValue ? 1L : 0L));
                }
                else
                {
                    // For other types, convert to string
                    firebaseParams.Add(new Parameter(param.Key, param.Value.ToString()));
                }
            }
#endif
            
            return firebaseParams;
        }
        
        // Firebase enums for reference (in case FIREBASE_ANALYTICS is not defined)
        private enum DependencyStatus
        {
            Available = 0,
            UnavailableMissingDependency = 1,
            UnavailableNotSupported = 2,
            UnavailableOther = 3
        }
    }
} 