using System;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage.Store
{
    /// <summary>
    /// Represents a virtual currency in the game
    /// </summary>
    [Serializable]
    public class Currency
    {
        /// <summary>
        /// Unique code for the currency (e.g., "GOLD", "GEMS")
        /// </summary>
        public string CurrencyCode { get; set; }
        
        /// <summary>
        /// Display name of the currency
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Description of the currency
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Current balance of the currency
        /// </summary>
        public decimal Balance { get; set; }
        
        /// <summary>
        /// Icon for the currency
        /// </summary>
        public Sprite Icon { get; set; }
        
        /// <summary>
        /// URL to the icon image (if not loaded)
        /// </summary>
        public string IconUrl { get; set; }
        
        /// <summary>
        /// Exchange rate to real money (if applicable)
        /// </summary>
        public decimal ExchangeRate { get; set; }
        
        /// <summary>
        /// Real currency code for exchange rate (e.g., "USD")
        /// </summary>
        public string RealCurrencyCode { get; set; }
        
        /// <summary>
        /// Whether this currency can be purchased
        /// </summary>
        public bool IsPurchasable { get; set; }
        
        /// <summary>
        /// Whether this currency can be earned through gameplay
        /// </summary>
        public bool IsEarnable { get; set; }
        
        /// <summary>
        /// Name of the provider this currency came from
        /// </summary>
        public string ProviderName { get; set; }
        
        /// <summary>
        /// Provider-specific data
        /// </summary>
        public object ProviderData { get; set; }
        
        /// <summary>
        /// Custom properties for the currency
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public Currency()
        {
            Balance = 0;
            ExchangeRate = 0;
        }

        /// <summary>
        /// Constructor with basic properties
        /// </summary>
        /// <param name="currencyCode">Unique code for the currency</param>
        /// <param name="displayName">Display name of the currency</param>
        /// <param name="balance">Initial balance</param>
        /// <param name="providerName">Name of the provider</param>
        public Currency(string currencyCode, string displayName, decimal balance, string providerName)
        {
            CurrencyCode = currencyCode;
            DisplayName = displayName;
            Balance = balance;
            ProviderName = providerName;
            IsEarnable = true;
        }

        /// <summary>
        /// Formats the currency balance for display
        /// </summary>
        /// <returns>Formatted balance string</returns>
        public string FormatBalance()
        {
            // If the balance is a whole number, don't show decimal places
            if (Balance == Math.Floor(Balance))
            {
                return Balance.ToString("N0");
            }
            
            // Otherwise, show up to 2 decimal places
            return Balance.ToString("N2");
        }

        /// <summary>
        /// Formats the currency balance with currency symbol for display
        /// </summary>
        /// <param name="symbol">Symbol to use (if null, uses currency code)</param>
        /// <returns>Formatted balance string with symbol</returns>
        public string FormatBalanceWithSymbol(string symbol = null)
        {
            string formattedBalance = FormatBalance();
            string currencySymbol = symbol ?? CurrencyCode;
            
            return $"{formattedBalance} {currencySymbol}";
        }

        /// <summary>
        /// Gets the real money value of the currency balance
        /// </summary>
        /// <returns>Real money value</returns>
        public decimal GetRealMoneyValue()
        {
            return Balance * ExchangeRate;
        }

        /// <summary>
        /// Formats the real money value for display
        /// </summary>
        /// <returns>Formatted real money value string</returns>
        public string FormatRealMoneyValue()
        {
            decimal realValue = GetRealMoneyValue();
            
            if (string.IsNullOrEmpty(RealCurrencyCode))
                return realValue.ToString("N2");
                
            switch (RealCurrencyCode.ToUpper())
            {
                case "USD":
                    return $"${realValue:N2}";
                case "EUR":
                    return $"€{realValue:N2}";
                case "GBP":
                    return $"£{realValue:N2}";
                case "JPY":
                    return $"¥{realValue:N0}";
                default:
                    return $"{realValue:N2} {RealCurrencyCode}";
            }
        }

        /// <summary>
        /// Gets a property as a string
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="defaultValue">Default value if property doesn't exist</param>
        /// <returns>The property value or default value</returns>
        public string GetProperty(string key, string defaultValue = "")
        {
            if (Properties.TryGetValue(key, out string value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets a property as an integer
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="defaultValue">Default value if property doesn't exist or isn't a valid integer</param>
        /// <returns>The property value as an integer or default value</returns>
        public int GetPropertyAsInt(string key, int defaultValue = 0)
        {
            if (Properties.TryGetValue(key, out string value) && int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets a property as a decimal
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="defaultValue">Default value if property doesn't exist or isn't a valid decimal</param>
        /// <returns>The property value as a decimal or default value</returns>
        public decimal GetPropertyAsDecimal(string key, decimal defaultValue = 0)
        {
            if (Properties.TryGetValue(key, out string value) && decimal.TryParse(value, out decimal result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets a property as a boolean
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="defaultValue">Default value if property doesn't exist or isn't a valid boolean</param>
        /// <returns>The property value as a boolean or default value</returns>
        public bool GetPropertyAsBool(string key, bool defaultValue = false)
        {
            if (Properties.TryGetValue(key, out string value) && bool.TryParse(value, out bool result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Sets a property
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        public void SetProperty(string key, string value)
        {
            Properties[key] = value;
        }

        /// <summary>
        /// Sets a property from an integer
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        public void SetProperty(string key, int value)
        {
            Properties[key] = value.ToString();
        }

        /// <summary>
        /// Sets a property from a decimal
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        public void SetProperty(string key, decimal value)
        {
            Properties[key] = value.ToString();
        }

        /// <summary>
        /// Sets a property from a boolean
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value</param>
        public void SetProperty(string key, bool value)
        {
            Properties[key] = value.ToString();
        }
    }
} 