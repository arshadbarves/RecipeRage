using System;
using System.Collections.Generic;

namespace Modules.Core.Banking.Data
{
    [Serializable]
    public class BankData
    {
        public int Coins;
        public int Gems;
        public List<string> UnlockedSkinIds = new List<string>();
        public Dictionary<int, string> EquippedSkinIds = new Dictionary<int, string>();

        public BankData() { }

        public BankData(int coins, int gems)
        {
            Coins = coins;
            Gems = gems;
        }
    }
}
