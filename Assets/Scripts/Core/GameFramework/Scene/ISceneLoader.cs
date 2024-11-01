namespace Core.GameFramework.Scene
{
    public interface ISceneLoader
    {
        void LoadSceneAsync(string sceneName, System.Action onSceneLoaded = null);
        void UnloadSceneAsync(string sceneName, System.Action onSceneUnloaded = null);
        void LoadSceneAsync(string sceneName, System.Action<Scene> onSceneLoaded = null);
        void UnloadSceneAsync(string sceneName, System.Action<Scene> onSceneUnloaded = null);
        Scene GetScene(string sceneName);
    }
}