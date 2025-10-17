using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    /// <summary>
    /// Manages player currency display and updates across the UI.
    /// Simple static instance pattern for UI-only manager.
    /// </summary>
    public class CurrencyManager
    {
        private static CurrencyManager _instance;
        public static CurrencyManager Instance => _instance ??= new CurrencyManager();
        private Label _coinsLabel;
        private Label _gemsLabel;
        
        private int _coins;
        private int _gems;
        
        public CurrencyManager()
        {
            LoadCurrency();
        }
        
        public void Initialize(VisualElement root)
        {
            Debug.Log("[CurrencyManager] Initialize called");
            
            _coinsLabel = root.Q<Label>("coins-amount");
            _gemsLabel = root.Q<Label>("gems-amount");
            
            Debug.Log($"[CurrencyManager] Coins label found: {_coinsLabel != null}");
            Debug.Log($"[CurrencyManager] Gems label found: {_gemsLabel != null}");
            
            UpdateUI();
            
            // Setup add currency buttons
            Button addCoinsButton = root.Q<Button>("add-coins-button");
            Button addGemsButton = root.Q<Button>("add-gems-button");
            
            if (addCoinsButton != null)
            {
                addCoinsButton.clicked += () => OnAddCurrencyClicked("coins");
            }
            
            if (addGemsButton != null)
            {
                addGemsButton.clicked += () => OnAddCurrencyClicked("gems");
            }
        }
        
        private void LoadCurrency()
        {
            _coins = PlayerPrefs.GetInt("PlayerCoins", 1250);
            _gems = PlayerPrefs.GetInt("PlayerGems", 85);
        }
        
        private void SaveCurrency()
        {
            PlayerPrefs.SetInt("PlayerCoins", _coins);
            PlayerPrefs.SetInt("PlayerGems", _gems);
            PlayerPrefs.Save();
        }
        
        public void AddCoins(int amount)
        {
            _coins += amount;
            SaveCurrency();
            UpdateUI();
            Debug.Log($"[CurrencyManager] Added {amount} coins. Total: {_coins}");
        }
        
        public void AddGems(int amount)
        {
            _gems += amount;
            SaveCurrency();
            UpdateUI();
            Debug.Log($"[CurrencyManager] Added {amount} gems. Total: {_gems}");
        }
        
        public bool SpendCoins(int amount)
        {
            if (_coins >= amount)
            {
                _coins -= amount;
                SaveCurrency();
                UpdateUI();
                Debug.Log($"[CurrencyManager] Spent {amount} coins. Remaining: {_coins}");
                return true;
            }
            
            Debug.LogWarning($"[CurrencyManager] Not enough coins. Need {amount}, have {_coins}");
            return false;
        }
        
        public bool SpendGems(int amount)
        {
            if (_gems >= amount)
            {
                _gems -= amount;
                SaveCurrency();
                UpdateUI();
                Debug.Log($"[CurrencyManager] Spent {amount} gems. Remaining: {_gems}");
                return true;
            }
            
            Debug.LogWarning($"[CurrencyManager] Not enough gems. Need {amount}, have {_gems}");
            return false;
        }
        
        public int GetCoins() => _coins;
        public int GetGems() => _gems;
        
        private void UpdateUI()
        {
            if (_coinsLabel != null)
            {
                _coinsLabel.text = FormatCurrency(_coins);
                Debug.Log($"[CurrencyManager] Updated coins label to: {_coinsLabel.text}");
            }
            else
            {
                Debug.LogWarning("[CurrencyManager] Coins label is null, cannot update UI");
            }
            
            if (_gemsLabel != null)
            {
                _gemsLabel.text = FormatCurrency(_gems);
                Debug.Log($"[CurrencyManager] Updated gems label to: {_gemsLabel.text}");
            }
            else
            {
                Debug.LogWarning("[CurrencyManager] Gems label is null, cannot update UI");
            }
        }
        
        private string FormatCurrency(int amount)
        {
            if (amount >= 1000000)
            {
                return $"{amount / 1000000f:F1}M";
            }
            else if (amount >= 1000)
            {
                return $"{amount / 1000f:F1}K";
            }
            return amount.ToString();
        }
        
        private void OnAddCurrencyClicked(string currencyType)
        {
            Debug.Log($"[CurrencyManager] Add {currencyType} clicked - Opening store");
            // This would open an in-app purchase dialog or store
            // For now, just add some currency for testing
            #if UNITY_EDITOR
            if (currencyType == "coins")
            {
                AddCoins(500);
            }
            else if (currencyType == "gems")
            {
                AddGems(50);
            }
            #endif
        }
    }
}
