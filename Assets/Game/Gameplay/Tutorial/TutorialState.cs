using Playcenter;
using Playcenter.Services;
using UnityEngine.SceneManagement;

namespace RecipeRage
{
    /// <summary>
    /// Loads the tutorial scene, waits for completion, marks tutorial_completed,
    /// then returns to the main menu (Boot systems persist — no scene reload needed).
    /// </summary>
    public sealed class TutorialState : IGameState
    {
        public void Enter()
        {
            // Hide menu UI while the tutorial runs (Boot UI persists across scenes)
            if (ServiceLocator.TryGet<Playcenter.UI.IUIService>(out var ui))
            {
                ui.HideAll();
            }

            SceneManager.LoadSceneAsync("Tutorial").completed += _ =>
            {
                // Bridge: tutorial completion → CompleteTutorial
                var controller = UnityEngine.Object.FindFirstObjectByType<TutorialController>();
                if (controller != null)
                {
                    controller.OnTutorialCompleted += CompleteTutorial;
                }
            };
        }

        public void Exit() { }
        public void Update(float deltaTime) { }

        public static void CompleteTutorial()
        {
            ServiceLocator.Get<ISaveService>().Save("tutorial_completed", true);

            // Unload tutorial scene; Boot systems (composition roots, UI) persist.
            var tutorialScene = SceneManager.GetSceneByName("Tutorial");
            if (tutorialScene.isLoaded)
            {
                SceneManager.UnloadSceneAsync(tutorialScene);
            }

            // Straight to main menu (MainMenuState publishes event → shows MainMenuScreen)
            ServiceLocator.Get<IGameStateMachine>().ChangeState(new MainMenuState());
        }
    }
}
