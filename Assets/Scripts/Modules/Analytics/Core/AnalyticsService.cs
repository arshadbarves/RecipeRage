using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RecipeRage.Modules.Analytics.Interfaces;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Modules.Analytics.Core
{
    /// <summary>
    /// Main implementation of the analytics service.
    /// Supports multiple providers for analytics data collection.
    /// 
    /// Complexity Rating: 3
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly List<IAnalyticsProvider> _providers = new List<IAnalyticsProvider>();
        private bool _isInitialized = false;
        private bool _isEnabled = true;
        private bool _consentRequired = false;
        private readonly Dictionary<AnalyticsConsentType, bool> _consentStatus = new Dictionary<AnalyticsConsentType, bool>();
        private string _sessionId = string.Empty;
        private DateTime _sessionStartTime;
        
        /// <summary>
        /// Constructor for the analytics service
        /// </summary>
        public AnalyticsService()
        {
            // Initialize default consent status
            foreach (AnalyticsConsentType consentType in Enum.GetValues(typeof(AnalyticsConsentType)))
            {
                _consentStatus[consentType] = false;
            }
            
            // Generate a new session ID
            _sessionId = GenerateSessionId();
            _sessionStartTime = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Add a provider to the analytics service
        /// </summary>
        /// <param name="provider">The provider to add</param>
        public void AddProvider(IAnalyticsProvider provider)
        {
            if (!_providers.Any(p => p.GetProviderName() == provider.GetProviderName()))
            {
                _providers.Add(provider);
                LogHelper.Info("Analytics", $"Added provider: {provider.GetProviderName()}");
            }
        }
        
        /// <summary>
        /// Initialize the analytics service
        /// </summary>
        /// <param name="consentRequired">Whether user consent is required before tracking</param>
        /// <param name="onComplete">Callback when initialization is complete</param>
        public void Initialize(bool consentRequired = false, Action<bool> onComplete = null)
        {
            if (_isInitialized)
            {
                LogHelper.Warning("Analytics", "Analytics service already initialized");
                onComplete?.Invoke(true);
                return;
            }
            
            LogHelper.Info("Analytics", $"Initializing analytics service with consent required: {consentRequired}");
            
            _consentRequired = consentRequired;
            
            if (_providers.Count == 0)
            {
                LogHelper.Warning("Analytics", "No analytics providers registered");
                _isInitialized = true;
                onComplete?.Invoke(true);
                return;
            }
            
            int providersInitialized = 0;
            int providersToInitialize = _providers.Count;
            bool allSuccessful = true;
            
            foreach (var provider in _providers)
            {
                provider.Initialize(consentRequired, success =>
                {
                    if (!success)
                    {
                        LogHelper.Error("Analytics", $"Failed to initialize provider: {provider.GetProviderName()}");
                        allSuccessful = false;
                    }
                    else
                    {
                        LogHelper.Info("Analytics", $"Initialized provider: {provider.GetProviderName()}");
                    }
                    
                    providersInitialized++;
                    
                    if (providersInitialized >= providersToInitialize)
                    {
                        _isInitialized = allSuccessful;
                        LogHelper.Info("Analytics", $"Analytics service initialization {(allSuccessful ? "successful" : "failed")}");
                        onComplete?.Invoke(allSuccessful);
                    }
                });
            }
        }
        
        /// <summary>
        /// Log an event with optional parameters
        /// </summary>
        /// <param name="eventName">Name of the event to log</param>
        /// <param name="parameters">Optional parameters for the event</param>
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!CanSendEvents())
            {
                return;
            }
            
            // Add session ID to parameters if it doesn't exist
            Dictionary<string, object> enrichedParams = EnrichParameters(parameters);
            
            LogHelper.Debug("Analytics", $"Logging event: {eventName}");
            
            foreach (var provider in _providers)
            {
                try
                {
                    provider.LogEvent(eventName, enrichedParams);
                }
                catch (Exception ex)
                {
                    LogHelper.Exception("Analytics", ex, $"Error logging event {eventName} with provider {provider.GetProviderName()}");
                }
            }
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
            if (!CanSendEvents())
            {
                return;
            }
            
            LogHelper.Debug("Analytics", $"Logging purchase: {productId} for {price} {currency}");
            
            // Add purchase-specific parameters
            Dictionary<string, object> purchaseParams = parameters != null 
                ? new Dictionary<string, object>(parameters) 
                : new Dictionary<string, object>();
                
            purchaseParams["product_id"] = productId;
            purchaseParams["currency"] = currency;
            purchaseParams["price"] = price;
            
            if (!string.IsNullOrEmpty(source))
            {
                purchaseParams["source"] = source;
            }
            
            // Enrich with standard parameters
            Dictionary<string, object> enrichedParams = EnrichParameters(purchaseParams);
            
            foreach (var provider in _providers)
            {
                try
                {
                    provider.LogPurchase(productId, currency, price, source, enrichedParams);
                }
                catch (Exception ex)
                {
                    LogHelper.Exception("Analytics", ex, $"Error logging purchase {productId} with provider {provider.GetProviderName()}");
                }
            }
        }
        
        /// <summary>
        /// Set a user property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Value of the property</param>
        public void SetUserProperty(string propertyName, string value)
        {
            if (!CanSendEvents())
            {
                return;
            }
            
            LogHelper.Debug("Analytics", $"Setting user property: {propertyName} = {value}");
            
            foreach (var provider in _providers)
            {
                try
                {
                    provider.SetUserProperty(propertyName, value);
                }
                catch (Exception ex)
                {
                    LogHelper.Exception("Analytics", ex, $"Error setting user property {propertyName} with provider {provider.GetProviderName()}");
                }
            }
        }
        
        /// <summary>
        /// Set the user ID for cross-device tracking
        /// </summary>
        /// <param name="userId">Unique user identifier</param>
        public void SetUserId(string userId)
        {
            if (!CanSendEvents())
            {
                return;
            }
            
            LogHelper.Debug("Analytics", $"Setting user ID: {userId}");
            
            foreach (var provider in _providers)
            {
                try
                {
                    provider.SetUserId(userId);
                }
                catch (Exception ex)
                {
                    LogHelper.Exception("Analytics", ex, $"Error setting user ID with provider {provider.GetProviderName()}");
                }
            }
        }
        
        /// <summary>
        /// Set user consent for data collection
        /// </summary>
        /// <param name="consentType">Type of consent</param>
        /// <param name="granted">Whether consent is granted</param>
        public void SetUserConsent(AnalyticsConsentType consentType, bool granted)
        {
            LogHelper.Info("Analytics", $"Setting user consent for {consentType}: {granted}");
            
            _consentStatus[consentType] = granted;
            
            foreach (var provider in _providers)
            {
                try
                {
                    provider.SetUserConsent(consentType, granted);
                }
                catch (Exception ex)
                {
                    LogHelper.Exception("Analytics", ex, $"Error setting consent for {consentType} with provider {provider.GetProviderName()}");
                }
            }
        }
        
        /// <summary>
        /// Check if user consent is required
        /// </summary>
        /// <returns>True if consent is required, false otherwise</returns>
        public bool RequiresConsent()
        {
            return _consentRequired;
        }
        
        /// <summary>
        /// Enable or disable analytics collection
        /// </summary>
        /// <param name="enabled">Whether analytics collection should be enabled</param>
        public void SetEnabled(bool enabled)
        {
            LogHelper.Info("Analytics", $"Setting analytics enabled: {enabled}");
            
            _isEnabled = enabled;
            
            foreach (var provider in _providers)
            {
                try
                {
                    provider.SetEnabled(enabled);
                }
                catch (Exception ex)
                {
                    LogHelper.Exception("Analytics", ex, $"Error setting enabled state with provider {provider.GetProviderName()}");
                }
            }
        }
        
        /// <summary>
        /// Check if analytics collection is enabled
        /// </summary>
        /// <returns>True if analytics is enabled, false otherwise</returns>
        public bool IsEnabled()
        {
            return _isEnabled;
        }
        
        /// <summary>
        /// Reset all analytics data
        /// </summary>
        public void ResetData()
        {
            LogHelper.Info("Analytics", "Resetting analytics data");
            
            // Generate a new session
            _sessionId = GenerateSessionId();
            _sessionStartTime = DateTime.UtcNow;
            
            foreach (var provider in _providers)
            {
                try
                {
                    provider.ResetData();
                }
                catch (Exception ex)
                {
                    LogHelper.Exception("Analytics", ex, $"Error resetting data with provider {provider.GetProviderName()}");
                }
            }
        }
        
        /// <summary>
        /// Get the session identifier
        /// </summary>
        /// <returns>Current session ID</returns>
        public string GetSessionId()
        {
            return _sessionId;
        }
        
        /// <summary>
        /// Flush any pending analytics events
        /// </summary>
        public void Flush()
        {
            if (!_isInitialized)
            {
                return;
            }
            
            LogHelper.Debug("Analytics", "Flushing analytics events");
            
            foreach (var provider in _providers)
            {
                try
                {
                    provider.Flush();
                }
                catch (Exception ex)
                {
                    LogHelper.Exception("Analytics", ex, $"Error flushing events with provider {provider.GetProviderName()}");
                }
            }
        }
        
        /// <summary>
        /// Check if events can be sent based on initialization, enabled state, and consent
        /// </summary>
        /// <returns>True if events can be sent, false otherwise</returns>
        private bool CanSendEvents()
        {
            if (!_isInitialized)
            {
                LogHelper.Warning("Analytics", "Analytics service not initialized");
                return false;
            }
            
            if (!_isEnabled)
            {
                LogHelper.Debug("Analytics", "Analytics is disabled");
                return false;
            }
            
            if (_consentRequired && !_consentStatus.ContainsKey(AnalyticsConsentType.AnalyticsStorage))
            {
                LogHelper.Warning("Analytics", "Analytics consent required but not set");
                return false;
            }
            
            if (_consentRequired && !_consentStatus[AnalyticsConsentType.AnalyticsStorage])
            {
                LogHelper.Debug("Analytics", "Analytics consent denied");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Generate a unique session ID
        /// </summary>
        /// <returns>Session ID</returns>
        private string GenerateSessionId()
        {
            return $"{SystemInfo.deviceUniqueIdentifier.Substring(0, 8)}-{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}-{UnityEngine.Random.Range(1000, 9999)}";
        }
        
        /// <summary>
        /// Enrich parameters with standard system information
        /// </summary>
        /// <param name="parameters">Original parameters</param>
        /// <returns>Enriched parameters</returns>
        private Dictionary<string, object> EnrichParameters(Dictionary<string, object> parameters)
        {
            Dictionary<string, object> enrichedParams = parameters != null 
                ? new Dictionary<string, object>(parameters) 
                : new Dictionary<string, object>();
            
            // Always add these parameters
            if (!enrichedParams.ContainsKey("session_id"))
            {
                enrichedParams["session_id"] = _sessionId;
            }
            
            if (!enrichedParams.ContainsKey("platform"))
            {
                enrichedParams["platform"] = Application.platform.ToString();
            }
            
            if (!enrichedParams.ContainsKey("app_version"))
            {
                enrichedParams["app_version"] = Application.version;
            }
            
            if (!enrichedParams.ContainsKey("timestamp"))
            {
                enrichedParams["timestamp"] = DateTime.UtcNow.ToString("o");
            }
            
            return enrichedParams;
        }
    }
} 