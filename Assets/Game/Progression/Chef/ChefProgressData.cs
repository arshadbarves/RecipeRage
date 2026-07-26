using System;
using System.Collections.Generic;

namespace RecipeRage
{
    [Serializable]
    public sealed class ChefProgressData
    {
        public List<ChefProgressEntry> Chefs = new List<ChefProgressEntry>();
        public int SelectedChefId;
    }

    [Serializable]
    public sealed class ChefProgressEntry
    {
        public int ChefId;
        public bool Unlocked;
        public int Level = 1;
        public int Xp;
    }
}
