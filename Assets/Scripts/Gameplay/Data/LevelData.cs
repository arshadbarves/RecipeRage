using System.Collections.Generic;
using UnityEngine;
using Utilities.Editor.Attributes;

namespace Gameplay.Data
{
    [CreateAssetMenu(fileName = "New Level", menuName = "Recipe Rage/Level")]
    public class LevelData : ScriptableObject
    {
        [field: SerializeField, GenerateUniqueId] public string LevelId { get; private set; }
        [field: SerializeField] public string LevelName { get; private set; }
        [field: SerializeField] public float GameDuration { get; private set; }
        [field: SerializeField] public List<TeamSpawnPoints> PlayerSpawnPoints { get; private set; }
        [field: SerializeField] public List<RecipeData> AvailableRecipes { get; private set; }
        [field: SerializeField] public List<PowerUpType> AvailablePowerUps { get; private set; }
        [field: SerializeField] public float MinOrderDelay { get; private set; }
        [field: SerializeField] public float MaxOrderDelay { get; private set; }
        [field: SerializeField] public int TargetScore { get; private set; }
        [field: SerializeField] public Sprite LevelThumbnail { get; private set; }
        [field: SerializeField] public AudioClip LevelMusic { get; private set; }
    }

    [System.Serializable]
    public struct TeamSpawnPoints
    {
        [field: SerializeField] public TeamID Team { get; private set; }
        [field: SerializeField] public List<Vector3> SpawnPoints { get; private set; }
    }
    
    public enum TeamID
    {
        Team1,
        Team2,
        Team3,
        Team4,
        Team5,
        Team6,
        Team7,
        Team8,
        Team9,
        Team10
    }
}