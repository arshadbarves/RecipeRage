using NUnit.Framework;
using Playcenter.Services;

namespace KitchenClash.Tests.EditMode.Playcenter.Wallet
{
    public sealed class WalletPortContractTests
    {
        private sealed class MemStore : IWalletStore
        {
            public WalletSnapshot Snap = new WalletSnapshot { Coins = 100, Gems = 0 };
            public System.Threading.Tasks.Task<WalletSnapshot> LoadAsync(System.Threading.CancellationToken ct = default)
                => System.Threading.Tasks.Task.FromResult(Snap);
            public System.Threading.Tasks.Task SaveAsync(WalletSnapshot snapshot, System.Threading.CancellationToken ct = default)
            {
                Snap = snapshot;
                return System.Threading.Tasks.Task.CompletedTask;
            }
        }

        private sealed class MemWallet : IWallet, IWalletLedger
        {
            private int _coins = 100, _gems;
            private readonly System.Collections.Generic.HashSet<string> _items = new();
            public int GetBalance(CurrencyId c) => c.Equals(CurrencyId.Gems) ? _gems : _coins;
            public bool HasItem(string id) => _items.Contains(id);
            public bool TryDebit(CurrencyId c, int amount, string reason)
            {
                if (amount < 0) return false;
                if (c.Equals(CurrencyId.Gems)) { if (_gems < amount) return false; _gems -= amount; return true; }
                if (_coins < amount) return false; _coins -= amount; return true;
            }
            public void Credit(CurrencyId c, int amount, string reason)
            {
                if (amount <= 0) return;
                if (c.Equals(CurrencyId.Gems)) _gems += amount; else _coins += amount;
            }
            public bool TryPurchase(string itemId, CurrencyId currency, int cost, string reason)
            {
                if (HasItem(itemId)) return false;
                if (!TryDebit(currency, cost, reason)) return false;
                _items.Add(itemId);
                return true;
            }
        }

        [Test]
        public void TryDebit_Insufficient_ReturnsFalse()
        {
            var w = new MemWallet();
            Assert.IsFalse(w.TryDebit(CurrencyId.Coins, 9999, "test"));
            Assert.AreEqual(100, w.GetBalance(CurrencyId.Coins));
        }

        [Test]
        public void Credit_IncreasesBalance()
        {
            var w = new MemWallet();
            w.Credit(CurrencyId.Coins, 50, "reward");
            Assert.AreEqual(150, w.GetBalance(CurrencyId.Coins));
        }

        [Test]
        public void CurrencyId_CoinsAndGems_AreDistinct()
        {
            Assert.AreNotEqual(CurrencyId.Coins, CurrencyId.Gems);
            Assert.AreEqual("coins", CurrencyId.Coins.Value);
        }
    }
}
