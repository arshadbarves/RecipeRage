using UnityEngine;
using Utilities.Editor.Attributes;

namespace Gameplay.Data
{
    public enum PowerUpType
    {
        SpeedBoost,
        DoublePrepSpeed,
        FreezeTime,
        InstantCook
    }

    [CreateAssetMenu(fileName = "New PowerUp", menuName = "Recipe Rage/PowerUp")]
    public class PowerUpData : ScriptableObject
    {
        [field: SerializeField,GenerateUniqueId] public string PowerUpId { get; private set; }
        [field: SerializeField] public PowerUpType Type { get; private set; }
        [field: SerializeField] public string PowerUpName { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public GameObject Prefab { get; private set; }
        [field: SerializeField] public float Duration { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public ParticleSystem ActivationEffect { get; private set; }
    }
}