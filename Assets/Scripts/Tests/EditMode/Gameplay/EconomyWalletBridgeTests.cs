using System;
using System.Collections.Generic;
using KitchenClash.Application;
using KitchenClash.Application.Models;
using KitchenClash.Domain;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Playcenter.Services;
using Playcenter.Shell;
using UnityEngine;

namespace RecipeRage.Tests.EditMode.Gameplay
{
    public sealed class EconomyWalletBridgeTests
    {
        private sealed class RecordingEventBus : IEventBus
        {
            private readonly Dictionary<Type, List<Delegate>> _handlers = new();
            public readonly List<object> Published = new();

            public void Publish<T>(T evt) where T : class
            {
                Published.Add(evt);
                if (_handlers.TryGetValue(typeof(T), out List<Delegate> list))
                    foreach (Delegate d in list)
                        ((Action<T>)d)?.Invoke(evt);
            }

            public void Subscribe<T>(Action<T> handler) where T : class
            {
                if (!_handlers.TryGetValue(typeof(T), out List<Delegate> list))
                    _handlers[typeof(T)] = list = new List<Delegate>();
                list.Add(handler);
            }

            public void Unsubscribe<T>(Action<T> handler) where T : class
            {
                if (_handlers.TryGetValue(typeof(T), out List<Delegate> list))
                    list.Remove(handler);
            }

            public void ClearAllSubscriptions() => _handlers.Clear();
        }

        private sealed class NullSaveService : ISaveService
        {
            private readonly Dictionary<string, object> _store = new();

            public void RegisterStorageConfig(string key, StorageStrategy strategy, bool encrypt) { }
            public void OnUserLoggedIn() { }
            public void OnUserLoggedOut() { }
            public GameSettingsData GetSettings() => new GameSettingsData();
            public void SaveSettings(GameSettingsData settings) { }
            public void UpdateSettings(Action<GameSettingsData> updateAction) { }
            public SyncStatus GetSyncStatus(string key) => new SyncStatus();
            public UniTask SyncAllCloudDataAsync() => UniTask.CompletedTask;

            public T LoadData<T>(string key) where T : class, new()
            {
                _store.TryGetValue(key, out object val);
                return val as T;
            }

            public void SaveData<T>(string key, T data) where T : class, new()
            {
                _store[key] = data;
            }

            public T Load<T>(string key, T defaultValue)
            {
                _store.TryGetValue(key, out object val);
                return val is T t ? t : defaultValue;
            }

            public void Save(string key, object data) => _store[key] = data;
        }

        private EconomyService BuildEconomy(out RecordingEventBus bus)
        {
            bus = new RecordingEventBus();
            var economy = new EconomyService(bus, new NullSaveService());
            economy.Initialize();
            return economy;
        }

        [Test]
        public void TryDebit_MapsToTrySpendCoins()
        {
            var economy = BuildEconomy(out _);
            int before = economy.Coins;
            Assert.IsTrue(((IWalletLedger)economy).TryDebit(CurrencyId.Coins, 10, "shop"));
            Assert.AreEqual(before - 10, economy.Coins);
        }

        [Test]
        public void TryDebit_InsufficientCoins_ReturnsFalse()
        {
            var economy = BuildEconomy(out _);
            Assert.IsFalse(((IWalletLedger)economy).TryDebit(CurrencyId.Coins, 9999, "shop"));
            Assert.AreEqual(EconomyService.StarterCoins, economy.Coins);
        }

        [Test]
        public void Credit_MatchReward_IncreasesBalance()
        {
            var economy = BuildEconomy(out _);
            ((IWalletLedger)economy).Credit(CurrencyId.Coins, 50, "match_win");
            Assert.AreEqual(EconomyService.StarterCoins + 50, ((IWallet)economy).GetBalance(CurrencyId.Coins));
        }

        [Test]
        public void Credit_Zero_IsNoOp()
        {
            var economy = BuildEconomy(out _);
            int before = economy.Coins;
            ((IWalletLedger)economy).Credit(CurrencyId.Coins, 0, "noop");
            Assert.AreEqual(before, economy.Coins);
        }

        [Test]
        public void GetBalance_Gems_ReturnGems()
        {
            var economy = BuildEconomy(out _);
            ((IWalletLedger)economy).Credit(CurrencyId.Gems, 5, "gem_reward");
            Assert.AreEqual(5, ((IWallet)economy).GetBalance(CurrencyId.Gems));
        }

        [Test]
        public void MatchRewardHandler_CreditsViaLedger_OnWin()
        {
            var bus = new RecordingEventBus();
            var economy = new EconomyService(bus, new NullSaveService());
            economy.Initialize();

            var handler = new KitchenClash.Infrastructure.Services.MatchRewardHandler(
                (IWalletLedger)economy, bus);
            handler.Initialize();

            bus.Publish(new MatchEndedEvent(won: true, localTeamScore: 0));

            int expected = EconomyService.StarterCoins + EconomyService.MatchWinReward;
            Assert.AreEqual(expected, economy.Coins);
        }

        [Test]
        public void MatchRewardHandler_CreditsViaLedger_OnLoss()
        {
            var bus = new RecordingEventBus();
            var economy = new EconomyService(bus, new NullSaveService());
            economy.Initialize();

            var handler = new KitchenClash.Infrastructure.Services.MatchRewardHandler(
                (IWalletLedger)economy, bus);
            handler.Initialize();

            bus.Publish(new MatchEndedEvent(won: false, localTeamScore: 0));

            Assert.AreEqual(EconomyService.StarterCoins + EconomyService.MatchLossReward, economy.Coins);
        }

        [Test]
        public void MatchRewardHandler_PublishesMatchRewardEvent()
        {
            var bus = new RecordingEventBus();
            var economy = new EconomyService(bus, new NullSaveService());
            economy.Initialize();

            var handler = new KitchenClash.Infrastructure.Services.MatchRewardHandler(
                (IWalletLedger)economy, bus);
            handler.Initialize();

            bus.Published.Clear();
            bus.Publish(new MatchEndedEvent(won: true, localTeamScore: 100));

            bool found = false;
            foreach (object evt in bus.Published)
                if (evt is MatchRewardEvent r && r.Won && r.CoinsAwarded > 0)
                    found = true;
            Assert.IsTrue(found, "Expected MatchRewardEvent to be published");
        }

        [Test]
        public void MatchRewardHandler_ScoreBonusApplied()
        {
            var bus = new RecordingEventBus();
            var economy = new EconomyService(bus, new NullSaveService());
            economy.Initialize();

            var handler = new KitchenClash.Infrastructure.Services.MatchRewardHandler(
                (IWalletLedger)economy, bus);
            handler.Initialize();

            int score = 100;
            bus.Publish(new MatchEndedEvent(won: true, localTeamScore: score));

            int expected = EconomyService.StarterCoins
                + EconomyService.MatchWinReward
                + Mathf.FloorToInt(score * EconomyService.ScoreBonusCoinRate);
            Assert.AreEqual(expected, economy.Coins);
        }
    }
}
