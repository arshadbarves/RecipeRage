using System;
using System.Collections.Generic;
using KitchenClash.Domain;
using UnityEngine;

namespace KitchenClash.Application
{
    public sealed class EconomyService : IEconomyService
    {
        public const int StarterCoins = 100;
        public const int StarterGems = 0;
        public const int MatchWinReward = 50;
        public const int MatchLossReward = 20;
        public const float ScoreBonusCoinRate = 0.1f; // 1 coin per 10 score points

        private readonly IEventBus _eventBus;
        private readonly ISaveService _saveService;
        private int _coins;
        private int _gems;
        private readonly HashSet<string> _ownedItems = new HashSet<string>();

        public event Action<string, long> OnBalanceChanged;

        public EconomyService(IEventBus eventBus, ISaveService saveService)
        {
            _eventBus = eventBus;
            _saveService = saveService;
        }

        public int Coins => _coins;
        public int Gems => _gems;

        public bool TrySpendCoins(int amount)
        {
            if (_coins < amount) return false;
            _coins -= amount;
            NotifyAndSave();
            return true;
        }

        public bool TrySpendGems(int amount)
        {
            if (_gems < amount) return false;
            _gems -= amount;
            NotifyAndSave();
            return true;
        }

        public void AddCoins(int amount)
        {
            _coins += amount;
            NotifyAndSave();
        }

        public void AddGems(int amount)
        {
            _gems += amount;
            NotifyAndSave();
        }

        public void SetCurrency(int coins, int gems)
        {
            _coins = coins;
            _gems = gems;
            NotifyAndSave();
        }

        public int GetBalance(string currencyId)
        {
            if (currencyId == EconomyKeys.CurrencyGems) return _gems;
            return _coins;
        }

        public void AddCurrency(string currencyId, int amount)
        {
            if (currencyId == EconomyKeys.CurrencyGems) AddGems(amount);
            else AddCoins(amount);
        }

        /// <summary>
        /// Award coins based on match result. Win = 50 + score bonus, Loss = 20.
        /// </summary>
        public void AwardMatchReward(bool won, int score)
        {
            int reward = won ? MatchWinReward + Mathf.FloorToInt(score * ScoreBonusCoinRate) : MatchLossReward;
            AddCoins(reward);
            _eventBus.Publish(new MatchRewardEvent { CoinsAwarded = reward, Won = won, Score = score });
        }

        private void NotifyAndSave()
        {
            _eventBus.Publish(new CurrencyChangedEvent { Coins = _coins, Gems = _gems });
            OnBalanceChanged?.Invoke(EconomyKeys.CurrencyCoins, _coins);
            OnBalanceChanged?.Invoke(EconomyKeys.CurrencyGems, _gems);
            Save();
        }

        private void Save()
        {
            var data = new EconomySaveData();
            data.Coins = _coins;
            data.Gems = _gems;
            data.OwnedItems = new List<string>(_ownedItems);
            _saveService.SaveData(EconomyKeys.SaveKey, data);
        }

        /// <summary>
        /// Check if the player owns a specific item.
        /// </summary>
        public bool HasItem(string itemId) => _ownedItems.Contains(itemId);

        /// <summary>
        /// Attempt to purchase an item using the specified currency.
        /// </summary>
        public bool Purchase(string itemId, int cost, string currencyType)
        {
            if (HasItem(itemId)) return false;

            bool spent = currencyType == EconomyKeys.CurrencyGems
                ? TrySpendGems(cost)
                : TrySpendCoins(cost);

            if (spent)
            {
                _ownedItems.Add(itemId);
                _eventBus.Publish(new ItemPurchasedEvent { ItemId = itemId, Cost = cost, CurrencyType = currencyType });
            }

            return spent;
        }

        /// <summary>
        /// Initialize the economy service — loads persisted data or sets starter values.
        /// </summary>
        public void Initialize()
        {
            var data = _saveService.LoadData<EconomySaveData>(EconomyKeys.SaveKey);

            if (data != null && (data.Coins > 0 || data.Gems > 0 || data.OwnedItems?.Count > 0))
            {
                _coins = data.Coins > 0 ? data.Coins : StarterCoins;
                _gems = data.Gems;
                if (data.OwnedItems != null)
                    foreach (var item in data.OwnedItems) _ownedItems.Add(item);
            }
            else
            {
                _coins = StarterCoins;
                _gems = StarterGems;
                Save();
            }

            // Fire initial balance state
            OnBalanceChanged?.Invoke(EconomyKeys.CurrencyCoins, _coins);
            OnBalanceChanged?.Invoke(EconomyKeys.CurrencyGems, _gems);
            _eventBus.Publish(new CurrencyChangedEvent { Coins = _coins, Gems = _gems });
        }
    }

    /// <summary>Simple DTO for economy persistence — no Unity dependencies.</summary>
    public class EconomySaveData
    {
        public int Coins { get; set; }
        public int Gems { get; set; }
        public List<string> OwnedItems { get; set; } = new();
    }
}
