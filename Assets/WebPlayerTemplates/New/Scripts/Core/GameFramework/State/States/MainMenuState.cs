// using Core.GameFramework.Scene;
// using VContainer;
//
// namespace Core.GameFramework.State.States
// {
//     public abstract class MainMenuState : GameState
//     {
//         private UIManager _uiManager;
//         private SceneLoadManager _sceneLoadManager;
//
//         [Inject]
//         public void Construct(UIManager uiManager, SceneLoadManager sceneLoadManager)
//         {
//             this._uiManager = uiManager;
//             this._sceneLoadManager = sceneLoadManager;
//         }
//
//         public override async void Enter()
//         {
//             // Load main menu scene if not already loaded
//             await _sceneLoadManager.LoadScene(SceneConfig.GameScene.MainMenu);
//
//             // Show main menu UI
//             _uiManager.ShowMenu<MainMenuUI>();
//
//             // Subscribe to events
//             EventManager.Subscribe<HostGameEvent>(this, OnHostGame);
//             EventManager.Subscribe<JoinGameEvent>(this, OnJoinGame);
//         }
//
//         public override void Exit()
//         {
//             // Hide main menu UI
//             _uiManager.HideMenu<MainMenuUI>();
//
//             // Unsubscribe from events
//             EventManager.Unsubscribe(this);
//         }
//
//         private void OnHostGame(HostGameEvent evt)
//         {
//             StateMachine.SetState<HostingState>();
//         }
//
//         private void OnJoinGame(JoinGameEvent evt)
//         {
//             StateMachine.SetState<JoiningState>();
//         }
//     }
// }
