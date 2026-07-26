using UnityEngine;
using KitchenClash.Domain;

namespace KitchenClash.Application.Models
{
    [CreateAssetMenu(fileName = "NewChef", menuName = "KitchenClash/Chef Definition")]
    public class ChefDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        public ChefId Id;
        public string DisplayName;
        [TextArea(2, 5)]
        public string Description;

        [Header("Stats")]
        public float MoveSpeed = 1.0f;
        public float CookSpeedMult = 1.0f;
        public float BurnResistance = 0.0f;
        public int CarryCapacity = 1;
        public float ScoreMultiplier = 1.0f;

        [Header("Unlock")]
        public UnlockType UnlockType = UnlockType.Starter;
        public int UnlockValue;
        public string UnlockSeason;
        public int UnlockBattlePassTier;

        [Header("Abilities")]
        public AbilityType PassiveAbility;
        public AbilityType ActiveAbility;
        public AbilityType SuperAbility = AbilityType.None;
        public AbilityType GadgetAbility = AbilityType.None;

        public ChefDefinition ToDomain()
        {
            return new ChefDefinition(
                Id,
                DisplayName,
                Description,
                new ChefStatBlock(MoveSpeed, CookSpeedMult, BurnResistance, CarryCapacity, ScoreMultiplier),
                new UnlockRequirement(UnlockType, UnlockValue, UnlockSeason),
                PassiveAbility,
                ActiveAbility,
                SuperAbility,
                GadgetAbility);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = name;
        }
    }
}
