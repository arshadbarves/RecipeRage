using Brawlers.Abilities;
using UnityEngine;

namespace Brawlers.Stats
{
    [CreateAssetMenu(fileName = "CharacterStats", menuName = "Characters/CharacterStats")]
    public class BrawlerData : ScriptableObject
    {
        [Header("Base Stats"), SerializeField]
         private string brawlerName;
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float maxHealth = 3500f;
        [SerializeField] private float attackPower = 600f;
        [SerializeField] private float attackSpeed = 0.8f;
        [SerializeField] private float cookingSpeed = 0.7f;
        [SerializeField] private float carryCapacity = 3f;
        [SerializeField] private float multitaskSkill = 1.2f;
        [SerializeField] private float teamworkSkill = 1.1f;
        [SerializeField] private Ability[] abilities;
        [SerializeField] private int unlockCost;

        public string BrawlerName => brawlerName;
        public float MoveSpeed => moveSpeed;
        public float MaxHealth => maxHealth;
        public float AttackPower => attackPower;
        public float AttackSpeed => attackSpeed;
        public float CookingSpeed => cookingSpeed;
        public float CarryCapacity => carryCapacity;
        public float MultitaskSkill => multitaskSkill;
        public float TeamworkSkill => teamworkSkill;
        public Ability[] Abilities => abilities;
        public int UnlockCost => unlockCost;
    }
}