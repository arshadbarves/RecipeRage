using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeRage.Modules.Analytics;
using RecipeRage.Modules.Analytics.Data;
using RecipeRage.Modules.Analytics.Interfaces;
using RecipeRage.Modules.Analytics.Utils;
using RecipeRage.Modules.Logging;

namespace RecipeRage.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the Analytics module.
    /// Shows how to initialize analytics, track events, and manage user consent.
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public class AnalyticsExample : MonoBehaviour
    {
        [Header("Event Tracking Settings")]
        [SerializeField] private float _eventInterval = 5.0f;
        [SerializeField] private bool _autoTrackEvents = true;
        
        [Header("Consent Settings")]
        [SerializeField] private bool _requireConsent = true;
        
        private bool _isInitialized = false;
        private float _timeSinceLastEvent = 0f;
        private int _eventCounter = 0;
        
        /// <summary>
        /// Initialize analytics when the script starts
        /// </summary>
        private void Start()
        {
            // Initialize the logging system first
            LogHelper.SetConsoleOutput(true);
            LogHelper.SetFileOutput(true);
            LogHelper.SetLogLevel(LogLevel.Debug);
            
            if (_requireConsent)
            {
                // In a real application, you would show a consent UI here
                // For this example, we'll just initialize with consent required
                InitializeWithConsent();
            }
            else
            {
                // Initialize without requiring consent
                InitializeAnalytics();
            }
        }
        
        /// <summary>
        /// Update is called once per frame
        /// </summary>
        private void Update()
        {
            if (!_isInitialized || !_autoTrackEvents)
            {
                return;
            }
            
            _timeSinceLastEvent += Time.deltaTime;
            
            if (_timeSinceLastEvent >= _eventInterval)
            {
                _timeSinceLastEvent = 0f;
                _eventCounter++;
                
                // Track a random event type
                TrackRandomEvent();
            }
        }
        
        /// <summary>
        /// Initialize the analytics system and check for consent
        /// </summary>
        private void InitializeWithConsent()
        {
            LogHelper.Info("AnalyticsExample", "Initializing analytics with consent required");
            
            // Initialize analytics with consent management
            AnalyticsConsent.InitializeAnalyticsWithConsent(success =>
            {
                _isInitialized = success;
                
                if (success)
                {
                    LogHelper.Info("AnalyticsExample", "Analytics initialized successfully with consent management");
                    
                    // Check if consent has been provided
                    if (!AnalyticsConsent.HasConsentBeenProvided())
                    {
                        LogHelper.Info("AnalyticsExample", "No consent has been provided yet");
                        
                        // In a real app, you would show a consent UI here
                        // For this example, we'll simulate user granting consent
                        SimulateUserGrantingConsent();
                    }
                    else
                    {
                        LogHelper.Info("AnalyticsExample", "Consent has already been provided");
                        
                        // Check if analytics is enabled
                        bool isEnabled = AnalyticsHelper.IsEnabled();
                        LogHelper.Info("AnalyticsExample", $"Analytics is {(isEnabled ? "enabled" : "disabled")}");
                        
                        // Track session start event if analytics is enabled
                        if (isEnabled)
                        {
                            TrackSessionStart();
                        }
                    }
                }
                else
                {
                    LogHelper.Error("AnalyticsExample", "Failed to initialize analytics");
                }
            });
        }
        
        /// <summary>
        /// Initialize analytics without consent management
        /// </summary>
        private void InitializeAnalytics()
        {
            LogHelper.Info("AnalyticsExample", "Initializing analytics without consent");
            
            AnalyticsHelper.Initialize(false, success =>
            {
                _isInitialized = success;
                
                if (success)
                {
                    LogHelper.Info("AnalyticsExample", "Analytics initialized successfully");
                    
                    // Set user properties
                    SetExampleUserProperties();
                    
                    // Track session start
                    TrackSessionStart();
                }
                else
                {
                    LogHelper.Error("AnalyticsExample", "Failed to initialize analytics");
                }
            });
        }
        
        /// <summary>
        /// Simulate user granting consent
        /// </summary>
        private void SimulateUserGrantingConsent()
        {
            LogHelper.Info("AnalyticsExample", "Simulating user granting consent");
            
            // Simulate user granting consent for all data types
            AnalyticsConsent.SetAllConsent(true);
            
            // Set user properties
            SetExampleUserProperties();
            
            // Track session start
            TrackSessionStart();
        }
        
        /// <summary>
        /// Simulate user declining consent
        /// </summary>
        private void SimulateUserDecliningConsent()
        {
            LogHelper.Info("AnalyticsExample", "Simulating user declining consent");
            
            // Simulate user declining consent for all data types
            AnalyticsConsent.SetAllConsent(false);
        }
        
        /// <summary>
        /// Set example user properties
        /// </summary>
        private void SetExampleUserProperties()
        {
            LogHelper.Info("AnalyticsExample", "Setting user properties");
            
            // Set user ID (in a real app, this would be your user's ID)
            AnalyticsHelper.SetUserId("example_user_123");
            
            // Set user properties
            AnalyticsHelper.SetUserProperty("user_type", "free");
            AnalyticsHelper.SetUserProperty("preferred_theme", "dark");
            AnalyticsHelper.SetUserProperty("language", "en");
            AnalyticsHelper.SetUserProperty("installed_version", Application.version);
        }
        
        /// <summary>
        /// Track session start event
        /// </summary>
        private void TrackSessionStart()
        {
            LogHelper.Info("AnalyticsExample", "Tracking session start event");
            
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { AnalyticsParameters.TIMESTAMP, DateTime.UtcNow.ToString("o") },
                { AnalyticsParameters.PLATFORM, Application.platform.ToString() },
                { AnalyticsParameters.OS_VERSION, SystemInfo.operatingSystem },
                { AnalyticsParameters.APP_VERSION, Application.version },
                { AnalyticsParameters.DEVICE_MODEL, SystemInfo.deviceModel },
                { AnalyticsParameters.SCREEN_NAME, "MainMenu" }
            };
            
            AnalyticsHelper.LogEvent(AnalyticsEventTypes.SESSION_START, parameters);
        }
        
        /// <summary>
        /// Track a random event for demonstration purposes
        /// </summary>
        private void TrackRandomEvent()
        {
            // Pick a random event type
            string[] eventTypes = {
                AnalyticsEventTypes.LEVEL_START,
                AnalyticsEventTypes.RECIPE_CREATED,
                AnalyticsEventTypes.RECIPE_VIEWED,
                AnalyticsEventTypes.MATCH_START,
                AnalyticsEventTypes.INGREDIENT_USED
            };
            
            string eventType = eventTypes[UnityEngine.Random.Range(0, eventTypes.Length)];
            
            // Create parameters based on event type
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            
            switch (eventType)
            {
                case AnalyticsEventTypes.LEVEL_START:
                    parameters.Add(AnalyticsParameters.LEVEL_NAME, $"Level_{UnityEngine.Random.Range(1, 10)}");
                    parameters.Add(AnalyticsParameters.LEVEL_NUMBER, UnityEngine.Random.Range(1, 10));
                    parameters.Add(AnalyticsParameters.DIFFICULTY, UnityEngine.Random.Range(0, 3) == 0 ? "Easy" : UnityEngine.Random.Range(0, 2) == 0 ? "Normal" : "Hard");
                    break;
                    
                case AnalyticsEventTypes.RECIPE_CREATED:
                    parameters.Add(AnalyticsParameters.RECIPE_TYPE, UnityEngine.Random.Range(0, 3) == 0 ? "Appetizer" : UnityEngine.Random.Range(0, 2) == 0 ? "Main Course" : "Dessert");
                    parameters.Add(AnalyticsParameters.INGREDIENTS_COUNT, UnityEngine.Random.Range(3, 10));
                    parameters.Add(AnalyticsParameters.COOKING_TIME, UnityEngine.Random.Range(10, 60));
                    break;
                    
                case AnalyticsEventTypes.RECIPE_VIEWED:
                    parameters.Add(AnalyticsParameters.RECIPE_ID, $"recipe_{UnityEngine.Random.Range(1000, 9999)}");
                    parameters.Add(AnalyticsParameters.RECIPE_NAME, $"Example Recipe {UnityEngine.Random.Range(1, 100)}");
                    parameters.Add(AnalyticsParameters.RECIPE_TYPE, UnityEngine.Random.Range(0, 3) == 0 ? "Appetizer" : UnityEngine.Random.Range(0, 2) == 0 ? "Main Course" : "Dessert");
                    parameters.Add(AnalyticsParameters.DIFFICULTY, UnityEngine.Random.Range(0, 3) == 0 ? "Easy" : UnityEngine.Random.Range(0, 2) == 0 ? "Normal" : "Hard");
                    break;
                    
                case AnalyticsEventTypes.MATCH_START:
                    parameters.Add(AnalyticsParameters.MATCH_ID, $"match_{UnityEngine.Random.Range(1000, 9999)}");
                    parameters.Add(AnalyticsParameters.MATCH_TYPE, UnityEngine.Random.Range(0, 3) == 0 ? "Casual" : UnityEngine.Random.Range(0, 2) == 0 ? "Ranked" : "Tournament");
                    parameters.Add(AnalyticsParameters.PLAYERS_COUNT, UnityEngine.Random.Range(2, 5));
                    parameters.Add(AnalyticsParameters.IS_RANKED, UnityEngine.Random.Range(0, 2) == 0);
                    break;
                    
                case AnalyticsEventTypes.INGREDIENT_USED:
                    parameters.Add("ingredient_id", $"ingredient_{UnityEngine.Random.Range(1, 100)}");
                    parameters.Add("ingredient_name", GetRandomIngredientName());
                    parameters.Add("recipe_id", $"recipe_{UnityEngine.Random.Range(1000, 9999)}");
                    break;
            }
            
            // Add common parameters
            parameters.Add(AnalyticsParameters.TIMESTAMP, DateTime.UtcNow.ToString("o"));
            parameters.Add("event_counter", _eventCounter);
            
            // Log the event
            LogHelper.Info("AnalyticsExample", $"Tracking event: {eventType}");
            AnalyticsHelper.LogEvent(eventType, parameters);
        }
        
        /// <summary>
        /// Simulate a purchase event
        /// </summary>
        public void SimulatePurchase()
        {
            if (!_isInitialized)
            {
                LogHelper.Warning("AnalyticsExample", "Analytics not initialized. Cannot track purchase.");
                return;
            }
            
            LogHelper.Info("AnalyticsExample", "Simulating a purchase event");
            
            string[] productIds = { "coins_1000", "premium_subscription", "chef_hat_skin", "special_recipe_pack" };
            string productId = productIds[UnityEngine.Random.Range(0, productIds.Length)];
            double price = Math.Round(UnityEngine.Random.Range(0.99f, 9.99f), 2);
            string currency = "USD";
            
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { AnalyticsParameters.ITEM_NAME, GetProductName(productId) },
                { AnalyticsParameters.ITEM_TYPE, GetProductType(productId) },
                { AnalyticsParameters.PAYMENT_TYPE, UnityEngine.Random.Range(0, 2) == 0 ? "credit_card" : "apple_pay" },
                { AnalyticsParameters.IS_FIRST_PURCHASE, _eventCounter == 1 }
            };
            
            AnalyticsHelper.LogPurchase(productId, currency, price, "in_app_store", parameters);
        }
        
        /// <summary>
        /// Get a random ingredient name for demo purposes
        /// </summary>
        private string GetRandomIngredientName()
        {
            string[] ingredients = {
                "Tomato", "Onion", "Garlic", "Olive Oil", "Salt", "Pepper",
                "Chicken", "Beef", "Pork", "Fish", "Tofu", "Eggs",
                "Rice", "Pasta", "Potatoes", "Flour", "Sugar", "Butter"
            };
            
            return ingredients[UnityEngine.Random.Range(0, ingredients.Length)];
        }
        
        /// <summary>
        /// Get a product name from a product ID
        /// </summary>
        private string GetProductName(string productId)
        {
            switch (productId)
            {
                case "coins_1000": return "1000 Coins";
                case "premium_subscription": return "Premium Subscription";
                case "chef_hat_skin": return "Chef Hat Skin";
                case "special_recipe_pack": return "Special Recipe Pack";
                default: return productId;
            }
        }
        
        /// <summary>
        /// Get a product type from a product ID
        /// </summary>
        private string GetProductType(string productId)
        {
            if (productId.StartsWith("coins"))
            {
                return "currency";
            }
            else if (productId.Contains("subscription"))
            {
                return "subscription";
            }
            else if (productId.Contains("skin"))
            {
                return "cosmetic";
            }
            else if (productId.Contains("recipe"))
            {
                return "content";
            }
            else
            {
                return "other";
            }
        }
        
        /// <summary>
        /// Clean up when the object is destroyed
        /// </summary>
        private void OnDestroy()
        {
            if (_isInitialized)
            {
                // Track session end event
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { AnalyticsParameters.TIMESTAMP, DateTime.UtcNow.ToString("o") },
                    { AnalyticsParameters.DURATION, Time.time }
                };
                
                AnalyticsHelper.LogEvent(AnalyticsEventTypes.SESSION_END, parameters);
                
                // Flush events to ensure they're sent
                AnalyticsHelper.Flush();
            }
        }
        
        #region UI Callbacks
        
        /// <summary>
        /// Toggle consent for analytics data collection
        /// </summary>
        public void ToggleConsent(bool granted)
        {
            LogHelper.Info("AnalyticsExample", $"Toggling consent to {granted}");
            
            if (granted)
            {
                SimulateUserGrantingConsent();
            }
            else
            {
                SimulateUserDecliningConsent();
            }
        }
        
        /// <summary>
        /// Toggle automatic event tracking
        /// </summary>
        public void ToggleAutoTracking(bool enabled)
        {
            _autoTrackEvents = enabled;
            LogHelper.Info("AnalyticsExample", $"Auto tracking set to {enabled}");
        }
        
        /// <summary>
        /// Reset analytics data
        /// </summary>
        public void ResetAnalyticsData()
        {
            LogHelper.Info("AnalyticsExample", "Resetting analytics data");
            AnalyticsHelper.ResetData();
        }
        
        /// <summary>
        /// Track a custom event
        /// </summary>
        public void TrackCustomEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                eventName = "custom_event";
            }
            
            LogHelper.Info("AnalyticsExample", $"Tracking custom event: {eventName}");
            
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "custom_parameter", "custom_value" },
                { AnalyticsParameters.TIMESTAMP, DateTime.UtcNow.ToString("o") },
                { "event_counter", _eventCounter }
            };
            
            AnalyticsHelper.LogEvent(eventName, parameters);
        }
        
        #endregion
    }
} 