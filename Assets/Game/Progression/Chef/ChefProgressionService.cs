using System;
using System.Collections.Generic;
using Playcenter.Services;

namespace RecipeRage
{
    public interface IChefProgressionService
    {
        event Action<ChefId> OnChefUnlocked;
        event Action<ChefId> OnChefUpgraded;
        event Action<ChefId> OnChefSelected;

        bool IsUnlocked(ChefId id);
        bool TryUnlock(ChefId id);
        int GetLevel(ChefId id);
        int GetXp(ChefId id);
        int GetUpgradeCost(ChefId id);
        bool TryUpgrade(ChefId id);
        void AddXp(ChefId id, int xp);
        ChefId GetSelectedChef();
        void SelectChef(ChefId id);
        ChefAbilityModifier GetSelectedModifier();
    }

    /// <summary>
    /// Game-specific chef progression. Uses SDK wallet/save/analytics — the SDK
    /// knows coins exist, but knows nothing about chefs.
    /// </summary>
    public sealed class ChefProgressionService : IChefProgressionService
    {
        private const string SaveKey = "chef_progress";

        private readonly IChefCatalog _catalog;
        private readonly IWalletService _wallet;
        private readonly ISaveService _save;
        private readonly IAnalyticsService _analytics;
        private readonly ChefProgressData _data;

        public event Action<ChefId> OnChefUnlocked;
        public event Action<ChefId> OnChefUpgraded;
        public event Action<ChefId> OnChefSelected;

        public ChefProgressionService(
            IChefCatalog catalog,
            IWalletService wallet,
            ISaveService save,
            IAnalyticsService analytics)
        {
            _catalog = catalog;
            _wallet = wallet;
            _save = save;
            _analytics = analytics;
            _data = _save.Load(SaveKey, new ChefProgressData());

            // Starters are always unlocked
            EnsureUnlocked(ChefId.Gordon);
            EnsureUnlocked(ChefId.Julia);
        }

        public bool IsUnlocked(ChefId id) => GetEntry(id)?.Unlocked ?? false;

        public bool TryUnlock(ChefId id)
        {
            var chef = _catalog.Get(id);
            if (chef == null || IsUnlocked(id))
            {
                return false;
            }

            if (!_wallet.TrySpendCoins(chef.UnlockCost))
            {
                return false;
            }

            var entry = GetOrCreateEntry(id);
            entry.Unlocked = true;
            Persist();

            _analytics.TrackEvent("chef_unlocked", new Dictionary<string, object>
            {
                { "chefId", id.ToString() },
                { "cost", chef.UnlockCost }
            });
            OnChefUnlocked?.Invoke(id);
            return true;
        }

        public int GetLevel(ChefId id) => GetEntry(id)?.Level ?? 1;
        public int GetXp(ChefId id) => GetEntry(id)?.Xp ?? 0;

        public int GetUpgradeCost(ChefId id) => ChefUpgradeCosts.ForLevel(GetLevel(id));

        public bool TryUpgrade(ChefId id)
        {
            if (!IsUnlocked(id))
            {
                return false;
            }

            var level = GetLevel(id);
            if (level >= ChefUpgradeCosts.MaxLevel)
            {
                return false;
            }

            var cost = ChefUpgradeCosts.ForLevel(level);
            if (!_wallet.TrySpendCoins(cost))
            {
                return false;
            }

            GetOrCreateEntry(id).Level = level + 1;
            Persist();

            _analytics.TrackEvent("chef_upgraded", new Dictionary<string, object>
            {
                { "chefId", id.ToString() },
                { "level", level + 1 },
                { "cost", cost }
            });
            OnChefUpgraded?.Invoke(id);
            return true;
        }

        public void AddXp(ChefId id, int xp)
        {
            if (xp <= 0 || !IsUnlocked(id))
            {
                return;
            }
            GetOrCreateEntry(id).Xp += xp;
            Persist();
        }

        public ChefId GetSelectedChef() => (ChefId)_data.SelectedChefId;

        public void SelectChef(ChefId id)
        {
            if (!IsUnlocked(id))
            {
                return;
            }
            _data.SelectedChefId = (int)id;
            Persist();
            OnChefSelected?.Invoke(id);
        }

        public ChefAbilityModifier GetSelectedModifier()
        {
            var id = GetSelectedChef();
            return _catalog.BuildModifier(id, GetLevel(id));
        }

        private void EnsureUnlocked(ChefId id)
        {
            var entry = GetOrCreateEntry(id);
            if (!entry.Unlocked)
            {
                entry.Unlocked = true;
                Persist();
            }
        }

        private ChefProgressEntry GetEntry(ChefId id)
        {
            foreach (var entry in _data.Chefs)
            {
                if (entry.ChefId == (int)id)
                {
                    return entry;
                }
            }
            return null;
        }

        private ChefProgressEntry GetOrCreateEntry(ChefId id)
        {
            var entry = GetEntry(id);
            if (entry == null)
            {
                entry = new ChefProgressEntry { ChefId = (int)id };
                _data.Chefs.Add(entry);
            }
            return entry;
        }

        private void Persist()
        {
            _save.Save(SaveKey, _data);
        }
    }
}
