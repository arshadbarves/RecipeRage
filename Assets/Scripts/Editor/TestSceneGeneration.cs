using UnityEditor;
using UnityEngine;

namespace RecipeRage.Editor
{
    public class TestSceneGeneration
    {
        [MenuItem("RecipeRage/Test/Generate Main Menu Scene")]
        public static void TestGenerateMainMenuScene()
        {
            Debug.Log("Starting scene generation...");
            SceneSetupGenerator.GenerateMainMenuScene();
            Debug.Log("Scene generation completed!");
        }

        [MenuItem("RecipeRage/Test/Generate Game Scene")]
        public static void TestGenerateGameScene()
        {
            Debug.Log("Starting game scene generation...");
            SceneSetupGenerator.GenerateGameScene();
            Debug.Log("Game scene generation completed!");
        }
    }
}
