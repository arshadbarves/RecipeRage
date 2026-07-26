using System.Threading.Tasks;

namespace RecipeRage
{
    public interface ISceneLoader
    {
        Task LoadSceneAdditive(string sceneName);
        Task UnloadScene(string sceneName);
    }
}
