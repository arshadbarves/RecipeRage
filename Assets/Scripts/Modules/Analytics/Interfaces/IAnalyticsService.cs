using System;
using System.Collections.Generic;

namespace RecipeRage.Modules.Analytics.Interfaces
{
    /// <summary>
    /// Interface for analytics services
    /// Provides unified analytics capabilities throughout the application
    /// Complexity Rating: 3
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Initialize the analytics service
        /// </summary>
        /// <param name="consentRequired"> Whether user consent is required before tracking </param>
        /// <param name="onComplete"> Callback when initialization is complete </param>
        void Initialize(bool consentRequired = false, Action<bool> onComplete = null);

        /// <summary>
        /// Log an event with optional parameters
        /// </summary>
        /// <param name="eventName"> Name of the event to log </param>
        /// <param name="parameters"> Optional parameters for the event </param>
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);

        /// <summary>
        /// Log a purchase event
        /// </summary>
        /// <param name="productId"> ID of the product purchased </param>
        /// <param name="currency"> Currency code (e.g., USD) </param>
        /// <param name="price"> Price of the product </param>
        /// <param name="source"> Source of the purchase (e.g., store, offer) </param>
        /// <param name="parameters"> Additional parameters </param>
        void LogPurchase(string productId, string currency, double price, string source = null, Dictionary<string, object> parameters = null);

        /// <summary>
        /// Set a user property
        /// </summary>
        /// <param name="propertyName"> Name of the property </param>
        /// <param name="value"> Value of the property </param>
        void SetUserProperty(string propertyName, string value);

        /// <summary>
        /// Set the user ID for cross-device tracking
        /// </summary>
        /// <param name="userId"> Unique user identifier </param>
        void SetUserId(string userId);

        /// <summary>
        /// Set user consent for data collection
        /// </summary>
        /// <param name="consentType"> Type of consent </param>
        /// <param name="granted"> Whether consent is granted </param>
        void SetUserConsent(AnalyticsConsentType consentType, bool granted);

        /// <summary>
        /// Check if user consent is required
        /// </summary>
        /// <returns> True if consent is required, false otherwise </returns>
        bool RequiresConsent();

        /// <summary>
        /// Enable or disable analytics collection
        /// </summary>
        /// <param name="enabled"> Whether analytics collection should be enabled </param>
        void SetEnabled(bool enabled);

        /// <summary>
        /// Check if analytics collection is enabled
        /// </summary>
        /// <returns> True if analytics is enabled, false otherwise </returns>
        bool IsEnabled();

        /// <summary>
        /// Reset all analytics data
        /// </summary>
        void ResetData();

        /// <summary>
        /// Get the session identifier
        /// </summary>
        /// <returns> Current session ID </returns>
        string GetSessionId();

        /// <summary>
        /// Flush any pending analytics events
        /// </summary>
        void Flush();
    }

    /// <summary>
    /// Interface for provider-specific analytics implementations
    /// </summary>
    public interface IAnalyticsProvider
    {
        /// <summary>
        /// Initialize the provider
        /// </summary>
        /// <param name="consentRequired"> Whether user consent is required before tracking </param>
        /// <param name="onComplete"> Callback when initialization is complete </param>
        void Initialize(bool consentRequired = false, Action<bool> onComplete = null);

        /// <summary>
        /// Log an event with optional parameters
        /// </summary>
        /// <param name="eventName"> Name of the event to log </param>
        /// <param name="parameters"> Optional parameters for the event </param>
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);

        /// <summary>
        /// Log a purchase event
        /// </summary>
        /// <param name="productId"> ID of the product purchased </param>
        /// <param name="currency"> Currency code (e.g., USD) </param>
        /// <param name="price"> Price of the product </param>
        /// <param name="source"> Source of the purchase (e.g., store, offer) </param>
        /// <param name="parameters"> Additional parameters </param>
        void LogPurchase(string productId, string currency, double price, string source = null, Dictionary<string, object> parameters = null);

        /// <summary>
        /// Set a user property
        /// </summary>
        /// <param name="propertyName"> Name of the property </param>
        /// <param name="value"> Value of the property </param>
        void SetUserProperty(string propertyName, string value);

        /// <summary>
        /// Set the user ID for cross-device tracking
        /// </summary>
        /// <param name="userId"> Unique user identifier </param>
        void SetUserId(string userId);

        /// <summary>
        /// Set user consent for data collection
        /// </summary>
        /// <param name="consentType"> Type of consent </param>
        /// <param name="granted"> Whether consent is granted </param>
        void SetUserConsent(AnalyticsConsentType consentType, bool granted);

        /// <summary>
        /// Enable or disable analytics collection
        /// </summary>
        /// <param name="enabled"> Whether analytics collection should be enabled </param>
        void SetEnabled(bool enabled);

        /// <summary>
        /// Flush any pending analytics events
        /// </summary>
        void Flush();

        /// <summary>
        /// Reset all analytics data
        /// </summary>
        void ResetData();

        /// <summary>
        /// Get the provider name
        /// </summary>
        /// <returns> Provider name </returns>
        string GetProviderName();
    }

    /// <summary>
    /// Types of consent for analytics data collection
    /// </summary>
    public enum AnalyticsConsentType
    {
        /// <summary>
        /// Consent for analytics data storage and processing
        /// </summary>
        AnalyticsStorage,

        /// <summary>
        /// Consent for advertisement data storage and processing
        /// </summary>
        AdStorage,

        /// <summary>
        /// Consent for personalization
        /// </summary>
        Personalization
    }
}