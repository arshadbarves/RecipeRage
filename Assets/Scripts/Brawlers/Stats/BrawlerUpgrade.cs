using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace Brawlers.Stats
{
    [System.Serializable]
    public class BrawlerUpgrade
    {
        public int level;
        public int upgradeTokenCost;
        public int upgradeCoinCost;
        public float statIncrease;
    }

    [System.Serializable]
    public static class BrawlerDataSystem
    {
        public static Dictionary<int, BrawlerUpgrade> BrawlerUpgrades = new Dictionary<int, BrawlerUpgrade>
        {
            { 1, new BrawlerUpgrade { level = 1, upgradeTokenCost = 0, upgradeCoinCost = 0, statIncrease = 0.05f } },
            { 2, new BrawlerUpgrade { level = 2, upgradeTokenCost = 20, upgradeCoinCost = 20, statIncrease = 0.06f } },
            { 3, new BrawlerUpgrade { level = 3, upgradeTokenCost = 30, upgradeCoinCost = 50, statIncrease = 0.07f } },
            { 4, new BrawlerUpgrade { level = 4, upgradeTokenCost = 50, upgradeCoinCost = 75, statIncrease = 0.08f } },
            { 5, new BrawlerUpgrade { level = 5, upgradeTokenCost = 80, upgradeCoinCost = 140, statIncrease = 0.09f } },
            { 6, new BrawlerUpgrade { level = 6, upgradeTokenCost = 130, upgradeCoinCost = 290, statIncrease = 0.10f } },
            { 7, new BrawlerUpgrade { level = 7, upgradeTokenCost = 210, upgradeCoinCost = 480, statIncrease = 0.12f } },
            { 8, new BrawlerUpgrade { level = 8, upgradeTokenCost = 340, upgradeCoinCost = 800, statIncrease = 0.14f } },
            { 9, new BrawlerUpgrade { level = 9, upgradeTokenCost = 550, upgradeCoinCost = 1250, statIncrease = 0.15f } },
            { 10, new BrawlerUpgrade { level = 10, upgradeTokenCost = 890, upgradeCoinCost = 1875, statIncrease = 0.20f } }
        };

        public static Dictionary<string, BrawlerData> Brawlers;
        
        public static void LoadBrawlerData()
        {
            Brawlers = Addressables.LoadAssetAsync<Dictionary<string, BrawlerData>>("Assets/Scripts/Brawlers/Stats/BrawlerData.asset").Result;
        }

        public static BrawlerData GetBrawlerData(string brawlerName)
        {
            return Brawlers[brawlerName];
        }
        
        public static BrawlerUpgrade GetBrawlerUpgrade(int level)
        {
            return BrawlerUpgrades[level];
        }

        public static BrawlerUpgrade GetNextBrawlerUpgrade(int currentLevel)
        {
            return BrawlerUpgrades[currentLevel + 1];
        }

        public static bool CanUpgradeBrawler(int currentLevel, int tokens, int cost)
        {
            return BrawlerUpgrades.ContainsKey(currentLevel + 1) &&
                   tokens >= BrawlerUpgrades[currentLevel + 1].upgradeTokenCost && cost >= BrawlerUpgrades[currentLevel + 1].upgradeCoinCost;
        }

        public static void UpgradeBrawler(BrawlerData brawlerData, int currentLevel, int tokens, int cost)
        {
            if (CanUpgradeBrawler(currentLevel, tokens, cost))
            {
                // BrawlerUpgrade upgrade = GetNextBrawlerUpgrade(currentLevel);
                // brawlerData.MaxHealth += brawlerData.MaxHealth * upgrade.statIncrease;
                // brawlerData.AttackPower += brawlerData.AttackPower * upgrade.statIncrease;
                // brawlerData.MoveSpeed += brawlerData.MoveSpeed * upgrade.statIncrease;
                // brawlerData.CarryCapacity += brawlerData.CarryCapacity * upgrade.statIncrease;
                // brawlerData.MultitaskSkill += brawlerData.MultitaskSkill * upgrade.statIncrease;
                // brawlerData.TeamworkSkill += brawlerData.TeamworkSkill * upgrade.statIncrease;
                // brawlerData.Level++;
            }
        }
    }
}