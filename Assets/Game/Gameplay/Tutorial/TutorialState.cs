using Playcenter;
using Playcenter.Services;
using UnityEngine.SceneManagement;

namespace RecipeRage
{
    /// <summary>Published when the tutorial scene finishes loading. RecipeRage.UI shows TutorialHUD.</summary>
    public readonly struct TutorialStartedEvent { }

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

            SceneManager.LoadSceneAsync("Tutorial", LoadSceneMode.Additive)!.completed += _ =>
            {
                // Tell the UI layer to show the tutorial HUD (RecipeRage.UI listens)
                ServiceLocator.Get<IEventBus>().Publish(new TutorialStartedEvent());

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
