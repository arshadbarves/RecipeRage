using System;
using UnityEngine;
using RecipeRage.Modules.Analytics.Interfaces;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Modules.Analytics.Utils
{
    /// <summary>
    /// Utility class for managing user consent for analytics data collection.
    /// Handles persistence of consent settings and initialization of analytics based on consent.
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public static class AnalyticsConsent
    {
        private const string CONSENT_PREF_PREFIX = "AnalyticsConsent_";
        private const string CONSENT_VERSION_KEY = "ConsentVersion";
        private const int CURRENT_CONSENT_VERSION = 1; // Increment when consent terms change
        
        /// <summary>
        /// Check if consent has been provided by the user
        /// </summary>
        /// <returns>True if consent has been provided, false otherwise</returns>
        public static bool HasConsentBeenProvided()
        {
            return PlayerPrefs.HasKey(GetConsentKey(AnalyticsConsentType.AnalyticsStorage));
        }
        
        /// <summary>
        /// Check if current consent is up to date with latest version
        /// </summary>
        /// <returns>True if consent is up to date, false otherwise</returns>
        public static bool IsConsentUpToDate()
        {
            if (!HasConsentBeenProvided())
            {
                return false;
            }
            
            int savedVersion = PlayerPrefs.GetInt(CONSENT_VERSION_KEY, 0);
            return savedVersion >= CURRENT_CONSENT_VERSION;
        }
        
        /// <summary>
        /// Get the current consent status for a specific type
        /// </summary>
        /// <param name="consentType">Type of consent</param>
        /// <returns>True if consent is granted, false otherwise</returns>
        public static bool GetConsentStatus(AnalyticsConsentType consentType)
        {
            string key = GetConsentKey(consentType);
            return PlayerPrefs.GetInt(key, 0) == 1;
        }
        
        /// <summary>
        /// Set the consent status for a specific type
        /// </summary>
        /// <param name="consentType">Type of consent</param>
        /// <param name="granted">Whether consent is granted</param>
        public static void SetConsentStatus(AnalyticsConsentType consentType, bool granted)
        {
            string key = GetConsentKey(consentType);
            PlayerPrefs.SetInt(key, granted ? 1 : 0);
            PlayerPrefs.SetInt(CONSENT_VERSION_KEY, CURRENT_CONSENT_VERSION);
            PlayerPrefs.Save();
            
            LogHelper.Info("AnalyticsConsent", $"Set consent for {consentType} to {granted}");
            
            // Update the analytics service
            AnalyticsHelper.SetUserConsent(consentType, granted);
        }
        
        /// <summary>
        /// Set the consent status for all consent types at once
        /// </summary>
        /// <param name="granted">Whether consent is granted</param>
        public static void SetAllConsent(bool granted)
        {
            foreach (AnalyticsConsentType consentType in Enum.GetValues(typeof(AnalyticsConsentType)))
            {
                SetConsentStatus(consentType, granted);
            }
            
            // Enable or disable analytics collection based on analytics storage consent
            AnalyticsHelper.SetEnabled(granted);
        }
        
        /// <summary>
        /// Clear all consent settings
        /// </summary>
        public static void ClearConsent()
        {
            foreach (AnalyticsConsentType consentType in Enum.GetValues(typeof(AnalyticsConsentType)))
            {
                string key = GetConsentKey(consentType);
                PlayerPrefs.DeleteKey(key);
            }
            
            PlayerPrefs.DeleteKey(CONSENT_VERSION_KEY);
            PlayerPrefs.Save();
            
            LogHelper.Info("AnalyticsConsent", "Cleared all consent settings");
            
            // Disable analytics collection
            AnalyticsHelper.SetEnabled(false);
        }
        
        /// <summary>
        /// Initialize analytics based on saved consent settings
        /// </summary>
        /// <param name="onComplete">Callback when initialization is complete</param>
        public static void InitializeAnalyticsWithConsent(Action<bool> onComplete = null)
        {
            bool consentRequired = ShouldRequireConsent();
            
            // Always initialize with consent required first
            AnalyticsHelper.Initialize(consentRequired, success =>
            {
                if (!success)
                {
                    LogHelper.Error("AnalyticsConsent", "Failed to initialize analytics");
                    onComplete?.Invoke(false);
                    return;
                }
                
                // If consent has been provided before, apply the saved settings
                if (HasConsentBeenProvided() && IsConsentUpToDate())
                {
                    ApplySavedConsentSettings();
                }
                
                LogHelper.Info("AnalyticsConsent", $"Initialized analytics with consent required: {consentRequired}");
                onComplete?.Invoke(true);
            });
        }
        
        /// <summary>
        /// Apply saved consent settings to the analytics service
        /// </summary>
        private static void ApplySavedConsentSettings()
        {
            foreach (AnalyticsConsentType consentType in Enum.GetValues(typeof(AnalyticsConsentType)))
            {
                bool granted = GetConsentStatus(consentType);
                AnalyticsHelper.SetUserConsent(consentType, granted);
                
                LogHelper.Debug("AnalyticsConsent", $"Applied saved consent for {consentType}: {granted}");
            }
            
            // Enable or disable analytics collection based on analytics storage consent
            bool analyticsEnabled = GetConsentStatus(AnalyticsConsentType.AnalyticsStorage);
            AnalyticsHelper.SetEnabled(analyticsEnabled);
        }
        
        /// <summary>
        /// Check if consent should be required based on region
        /// </summary>
        /// <returns>True if consent should be required, false otherwise</returns>
        private static bool ShouldRequireConsent()
        {
            // For simplicity, we'll use a simple check that always requires consent
            // In a real application, you would check the user's region (GDPR, CCPA, etc.)
            
            // Advanced implementation would use geolocation or IP-based detection 
            // to determine if the user is in a region that requires consent (EU, California, etc.)
            
            return true;
        }
        
        /// <summary>
        /// Get the PlayerPrefs key for a specific consent type
        /// </summary>
        /// <param name="consentType">Type of consent</param>
        /// <returns>PlayerPrefs key</returns>
        private static string GetConsentKey(AnalyticsConsentType consentType)
        {
            return CONSENT_PREF_PREFIX + consentType.ToString();
        }
    }
} 