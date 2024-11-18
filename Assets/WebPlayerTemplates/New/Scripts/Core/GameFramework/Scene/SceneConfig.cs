using System;
using UnityEngine;

namespace Core.GameFramework.Scene
{
    [CreateAssetMenu(fileName = "SceneConfig", menuName = "RecipeRage/Config/SceneConfig")]
    public class SceneConfig : ScriptableObject
    {

        public enum GameScene
        {
            Bootstrap,
            MainMenu,
            Loading,
            Kitchen1,
            Kitchen2,
            Kitchen3
        }

        public SceneData[] scenes;
        public GameScene initialScene = GameScene.MainMenu;
        public GameScene loadingScene = GameScene.Loading;

        private void OnValidate()
        {
            foreach (SceneData sceneData in scenes)
            {
                if (string.IsNullOrEmpty(sceneData.sceneName))
                {
                    sceneData.sceneName = sceneData.scene.ToString();
                }
            }
        }

        public SceneData GetSceneData(GameScene scene)
        {
            foreach (SceneData sceneData in scenes)
            {
                if (sceneData.scene == scene)
                    return sceneData;
            }
            return null;
        }
        [Serializable]
        public class SceneData
        {
            public GameScene scene;
            public string sceneName;
            public bool requiresLoading = true;
            public bool waitForAllClients = true;
            [Range(0, 100)]
            public int progressWeight = 1;
        }
    }
}