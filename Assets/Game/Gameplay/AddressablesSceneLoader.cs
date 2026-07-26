using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace RecipeRage
{
    /// <summary>
    /// Scene loading. Uses SceneManager today; Addressables scene keys swap in
    /// when map assets are built (Polish phase) without changing call sites.
    /// </summary>
    public sealed class AddressablesSceneLoader : ISceneLoader
    {
        public async Task LoadSceneAdditive(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!op.isDone)
            {
                await Task.Yield();
            }
        }

        public async Task UnloadScene(string sceneName)
        {
            var op = SceneManager.UnloadSceneAsync(sceneName);
            while (op != null && !op.isDone)
            {
                await Task.Yield();
            }
        }
    }
}
