using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Skins.Data
{
    [CreateAssetMenu(fileName = "SkinsData", menuName = "RecipeRage/SkinsData")]
    public class SkinsData : ScriptableObject
    {
        public List<SkinItem> skins;
    }

    [Serializable]
    public class SkinItem
    {   public string id;
        public string name;
        public Sprite icon;
        public int price;
        public bool isDefault;
    }
}
