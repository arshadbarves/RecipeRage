using UnityEngine;
using Utilities.Editor.Attributes;

namespace Gameplay.Data
{
    [CreateAssetMenu(fileName = "New Achievement", menuName = "Recipe Rage/Achievement")]
    public class AchievementData : ScriptableObject
    {
        [field: SerializeField, GenerateUniqueId] public string AchievementId { get; private set; }
        [field: SerializeField] public string AchievementName { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public AchievementType Type { get; private set; }
        [field: SerializeField] public int TargetValue { get; private set; }
        [field: SerializeField] public ParticleSystem UnlockEffect { get; private set; }
    }

    public enum AchievementType
    {
        Score,
        Time,
        Action,
        Collectible,
        Custom
    }
}