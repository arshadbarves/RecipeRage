using System;
using System.Collections.Generic;
using UnityEngine;
using RecipeRage.Modules.Analytics.Core;
using RecipeRage.Modules.Analytics.Interfaces;
using RecipeRage.Modules.Analytics.Providers.Firebase;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Modules.Analytics
{
    /// <summary>
    /// Static helper class for easy access to analytics functionality
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public static class AnalyticsHelper
    {
        private static IAnalyticsService _analyticsService;
        private static bool _isInitialized = false;
        
        /// <summary>
        /// Static constructor
        /// </summary>
        static AnalyticsHelper()
        {
            // Create the analytics service
            _analyticsService = new AnalyticsService();
            
            // Add the Firebase provider
            try
            {
                AddFirebaseProvider();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to add Firebase provider: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Initialize the analytics service
        /// </summary>
        /// <param name="consentRequired">Whether user consent is required before tracking</param>
        /// <param name="onComplete">Callback when initialization is complete</param>
        public static void Initialize(bool consentRequired = false, Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                LogHelper.Warning("Analytics", "Analytics helper already initialized");
                onComplete?.Invoke(true);
                return;
            }
            
            LogHelper.Info("Analytics", "Initializing analytics helper");
            
            _analyticsService.Initialize(consentRequired, success =>
            {
                _isInitialized = success;
                LogHelper.Info("Analytics", $"Analytics helper initialization {(success ? "successful" : "failed")}");
                onComplete?.Invoke(success);
            });
        }
        
        /// <summary>
        /// Log an event with optional parameters
        /// </summary>
        /// <param name="eventName">Name of the event to log</param>
        /// <param name="parameters">Optional parameters for the event</param>
        public static void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!CheckInitialized("LogEvent"))
            {
                return;
            }
            
            _analyticsService.LogEvent(eventName, parameters);
        }
        
        /// <summary>
        /// Log a purchase event
        /// </summary>
        /// <param name="productId">ID of the product purchased</param>
        /// <param name="currency">Currency code (e.g., USD)</param>
        /// <param name="price">Price of the product</param>
        /// <param name="source">Source of the purchase (e.g., store, offer)</param>
        /// <param name="parameters">Additional parameters</param>
        public static void LogPurchase(string productId, string currency, double price, string source = null, Dictionary<string, object> parameters = null)
        {
            if (!CheckInitialized("LogPurchase"))
            {
                return;
            }
            
            _analyticsService.LogPurchase(productId, currency, price, source, parameters);
        }
        
        /// <summary>
        /// Set a user property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Value of the property</param>
        public static void SetUserProperty(string propertyName, string value)
        {
            if (!CheckInitialized("SetUserProperty"))
            {
                return;
            }
            
            _analyticsService.SetUserProperty(propertyName, value);
        }
        
        /// <summary>
        /// Set the user ID for cross-device tracking
        /// </summary>
        /// <param name="userId">Unique user identifier</param>
        public static void SetUserId(string userId)
        {
            if (!CheckInitialized("SetUserId"))
            {
                return;
            }
            
            _analyticsService.SetUserId(userId);
        }
        
        /// <summary>
        /// Set user consent for data collection
        /// </summary>
        /// <param name="consentType">Type of consent</param>
        /// <param name="granted">Whether consent is granted</param>
        public static void SetUserConsent(AnalyticsConsentType consentType, bool granted)
        {
            if (!CheckInitialized("SetUserConsent", false))
            {
                return;
            }
            
            _analyticsService.SetUserConsent(consentType, granted);
        }
        
        /// <summary>
        /// Check if user consent is required
        /// </summary>
        /// <returns>True if consent is required, false otherwise</returns>
        public static bool RequiresConsent()
        {
            if (!CheckInitialized("RequiresConsent", false))
            {
                return false;
            }
            
            return _analyticsService.RequiresConsent();
        }
        
        /// <summary>
        /// Enable or disable analytics collection
        /// </summary>
        /// <param name="enabled">Whether analytics collection should be enabled</param>
        public static void SetEnabled(bool enabled)
        {
            if (!CheckInitialized("SetEnabled", false))
            {
                return;
            }
            
            _analyticsService.SetEnabled(enabled);
        }
        
        /// <summary>
        /// Check if analytics collection is enabled
        /// </summary>
        /// <returns>True if analytics is enabled, false otherwise</returns>
        public static bool IsEnabled()
        {
            if (!CheckInitialized("IsEnabled", false))
            {
                return false;
            }
            
            return _analyticsService.IsEnabled();
        }
        
        /// <summary>
        /// Reset all analytics data
        /// </summary>
        public static void ResetData()
        {
            if (!CheckInitialized("ResetData"))
            {
                return;
            }
            
            _analyticsService.ResetData();
        }
        
        /// <summary>
        /// Get the session identifier
        /// </summary>
        /// <returns>Current session ID</returns>
        public static string GetSessionId()
        {
            if (!CheckInitialized("GetSessionId"))
            {
                return string.Empty;
            }
            
            return _analyticsService.GetSessionId();
        }
        
        /// <summary>
        /// Flush any pending analytics events
        /// </summary>
        public static void Flush()
        {
            if (!CheckInitialized("Flush", false))
            {
                return;
            }
            
            _analyticsService.Flush();
        }
        
        /// <summary>
        /// Set a custom analytics service implementation
        /// </summary>
        /// <param name="analyticsService">The analytics service to use</param>
        public static void SetAnalyticsService(IAnalyticsService analyticsService)
        {
            if (analyticsService == null)
            {
                LogHelper.Error("Analytics", "Cannot set null analytics service");
                return;
            }
            
            _analyticsService = analyticsService;
            _isInitialized = false;
            
            LogHelper.Info("Analytics", $"Set custom analytics service: {analyticsService.GetType().Name}");
        }
        
        /// <summary>
        /// Add a provider to the analytics service
        /// </summary>
        /// <param name="provider">The provider to add</param>
        public static void AddProvider(IAnalyticsProvider provider)
        {
            if (provider == null)
            {
                LogHelper.Error("Analytics", "Cannot add null provider");
                return;
            }
            
            if (_analyticsService is AnalyticsService service)
            {
                service.AddProvider(provider);
                LogHelper.Info("Analytics", $"Added provider: {provider.GetType().Name}");
            }
            else
            {
                LogHelper.Error("Analytics", "Current analytics service does not support adding providers");
            }
        }
        
        /// <summary>
        /// Add the Firebase provider to the analytics service
        /// </summary>
        private static void AddFirebaseProvider()
        {
            if (_analyticsService is AnalyticsService service)
            {
                service.AddProvider(new FirebaseAnalyticsProvider());
            }
        }
        
        /// <summary>
        /// Check if the analytics service is initialized
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
                    LogHelper.Warning("Analytics", $"Analytics helper not initialized. Call Initialize() before {methodName}().");
                }
                
                return false;
            }
            
            return true;
        }
    }
} 