using System;

namespace KitchenClash.Infrastructure.Persistence
{
    [Serializable]
    public class PlayerStateData
    {
        public string PlayerId;
        public string DisplayName;
        public int Level;
        public int Experience;
        public int Coins;
        public int Gems;
        public string LastLoginDate;
    }
}
