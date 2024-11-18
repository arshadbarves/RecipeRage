using UnityEngine;
using Utilities.Editor.Attributes;

namespace Core
{
    [CreateAssetMenu(fileName = "GameModeConfig", menuName = "Recipe Rage/GameModeConfig")]
    public class GameModeConfig : ScriptableObject
    {
        [field: SerializeField, GenerateUniqueId] public string GameModeId { get; private set; }
        [field: SerializeField] private string gameModeName;
        [field: SerializeField] private string description;
        [field: SerializeField] private Sprite icon;
        [field: SerializeField] private float lobbyDuration = 60f;
        [field: SerializeField] private float preGameDuration = 30f;
        [field: SerializeField] private float postGameDuration = 30f;
        [field: SerializeField] private float battleDuration = 300f;
        [field: SerializeField] private int maxPlayers = 8;
        [field: SerializeField] private float mapShrinkInterval = 60f;
        [field: SerializeField] private Vector2 mapSize = new Vector2(1000, 1000);
        [field: SerializeField] private int maxIngredients = 50;
        [field: SerializeField] private float cookingStationSpawnInterval = 30f;
    }
}