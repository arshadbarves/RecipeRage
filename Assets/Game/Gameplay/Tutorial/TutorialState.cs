using Playcenter;
using Playcenter.Services;
using UnityEngine.SceneManagement;

namespace RecipeRage
{
    /// <summary>
    /// Loads the tutorial scene, waits for completion, marks tutorial_completed,
    /// then returns to the main menu.
    /// </summary>
    public sealed class TutorialState : IGameState
    {
        public void Enter()
        {
            SceneManager.LoadSceneAsync("Tutorial");
            // TutorialController.OnTutorialCompleted is wired to CompleteTutorial
            // via a scene bridge in the Tutorial scene (TutorialSceneBridge below).
        }

        public void Exit() { }
        public void Update(float deltaTime) { }

        public static void CompleteTutorial()
        {
            ServiceLocator.Get<ISaveService>().Save("tutorial_completed", true);
            SceneManager.LoadSceneAsync("Boot");
            ServiceLocator.Get<IGameStateMachine>().ChangeState(new MainMenuState());
        }
    }
}
