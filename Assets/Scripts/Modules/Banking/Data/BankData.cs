using System;
using System.Collections.Generic;

namespace Modules.Core.Banking.Data
{
    [Serializable]
    public class BankData
    {
        // Currency Balances (e.g., "coins" -> 100, "gems" -> 50)
        public Dictionary<string, long> Balances = new Dictionary<string, long>();

        // Owned Items / Inventory (e.g., "skin_blue", "weapon_sword")
        public HashSet<string> Inventory = new HashSet<string>();

        // Generic Key-Value Storage (e.g., "level" -> "5", "tutorial_complete" -> "true")
        public Dictionary<string, string> Data = new Dictionary<string, string>();

        public BankData() { }
    }
}