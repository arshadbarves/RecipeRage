using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "BattleRoyaleCooking/GameConfig")]
    public class ScriptableGameConfig : ScriptableObject
    {
        public float battleDuration = 300f;
        public int maxPlayers = 8;
        public float mapShrinkInterval = 60f;
        public Vector2 mapSize = new Vector2(1000, 1000);
        public int maxIngredients = 50;
        public float cookingStationSpawnInterval = 30f;
    }
}