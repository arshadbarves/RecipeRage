using UnityEngine;
using UnityEditor;
using RecipeRage.Core;
using RecipeRage.Core.Networking;
using RecipeRage.Core.GameFramework.State;
using RecipeRage.Core.GameModes;
using RecipeRage.Core.Characters;
using RecipeRage.Core.Input;
using RecipeRage.Gameplay.Cooking;
using RecipeRage.Gameplay.Scoring;
using RecipeRage.UI;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Editor utility for creating the GameBootstrap prefab.
    /// </summary>
    public class GameBootstrapPrefabCreator
    {
        [MenuItem("RecipeRage/Create/GameBootstrap Prefab")]
        public static void CreateGameBootstrapPrefab()
        {
            // Create the GameBootstrap GameObject
            var gameBootstrapObj = new GameObject("GameBootstrap");
            var gameBootstrap = gameBootstrapObj.AddComponent<GameBootstrap>();
            
            // Create the NetworkManager prefab
            var networkManagerObj = new GameObject("NetworkManager");
            networkManagerObj.AddComponent<NetworkBootstrap>();
            var networkManagerPrefab = CreatePrefab(networkManagerObj, "Assets/Prefabs/Managers/NetworkManager.prefab");
            
            // Create the GameStateManager prefab
            var gameStateManagerObj = new GameObject("GameStateManager");
            gameStateManagerObj.AddComponent<GameStateManager>();
            var gameStateManagerPrefab = CreatePrefab(gameStateManagerObj, "Assets/Prefabs/Managers/GameStateManager.prefab");
            
            // Create the UIManager prefab
            var uiManagerObj = new GameObject("UIManager");
            uiManagerObj.AddComponent<UIManager>();
            var uiManagerPrefab = CreatePrefab(uiManagerObj, "Assets/Prefabs/Managers/UIManager.prefab");
            
            // Create the InputManager prefab
            var inputManagerObj = new GameObject("InputManager");
            inputManagerObj.AddComponent<InputManager>();
            var inputManagerPrefab = CreatePrefab(inputManagerObj, "Assets/Prefabs/Managers/InputManager.prefab");
            
            // Create the GameModeManager prefab
            var gameModeManagerObj = new GameObject("GameModeManager");
            gameModeManagerObj.AddComponent<GameModeManager>();
            var gameModeManagerPrefab = CreatePrefab(gameModeManagerObj, "Assets/Prefabs/Managers/GameModeManager.prefab");
            
            // Create the CharacterManager prefab
            var characterManagerObj = new GameObject("CharacterManager");
            characterManagerObj.AddComponent<CharacterManager>();
            var characterManagerPrefab = CreatePrefab(characterManagerObj, "Assets/Prefabs/Managers/CharacterManager.prefab");
            
            // Create the ScoreManager prefab
            var scoreManagerObj = new GameObject("ScoreManager");
            scoreManagerObj.AddComponent<ScoreManager>();
            var scoreManagerPrefab = CreatePrefab(scoreManagerObj, "Assets/Prefabs/Managers/ScoreManager.prefab");
            
            // Create the OrderManager prefab
            var orderManagerObj = new GameObject("OrderManager");
            orderManagerObj.AddComponent<OrderManager>();
            var orderManagerPrefab = CreatePrefab(orderManagerObj, "Assets/Prefabs/Managers/OrderManager.prefab");
            
            // Assign the prefabs to the GameBootstrap
            gameBootstrap._networkManagerPrefab = networkManagerPrefab;
            gameBootstrap._gameStateManagerPrefab = gameStateManagerPrefab;
            gameBootstrap._uiManagerPrefab = uiManagerPrefab;
            gameBootstrap._inputManagerPrefab = inputManagerPrefab;
            gameBootstrap._gameModeManagerPrefab = gameModeManagerPrefab;
            gameBootstrap._characterManagerPrefab = characterManagerPrefab;
            gameBootstrap._scoreManagerPrefab = scoreManagerPrefab;
            gameBootstrap._orderManagerPrefab = orderManagerPrefab;
            
            // Create the GameBootstrap prefab
            var gameBootstrapPrefab = CreatePrefab(gameBootstrapObj, "Assets/Prefabs/GameBootstrap.prefab");
            
            // Clean up the temporary GameObjects
            Object.DestroyImmediate(gameBootstrapObj);
            Object.DestroyImmediate(networkManagerObj);
            Object.DestroyImmediate(gameStateManagerObj);
            Object.DestroyImmediate(uiManagerObj);
            Object.DestroyImmediate(inputManagerObj);
            Object.DestroyImmediate(gameModeManagerObj);
            Object.DestroyImmediate(characterManagerObj);
            Object.DestroyImmediate(scoreManagerObj);
            Object.DestroyImmediate(orderManagerObj);
            
            Debug.Log("GameBootstrap prefab created successfully!");
        }
        
        /// <summary>
        /// Create a prefab from a GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to create a prefab from</param>
        /// <param name="path">The path to save the prefab to</param>
        /// <returns>The created prefab</returns>
        private static GameObject CreatePrefab(GameObject gameObject, string path)
        {
            // Create the directory if it doesn't exist
            var directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            // Create the prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, path);
            
            return prefab;
        }
    }
}
