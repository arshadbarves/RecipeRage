// namespace Core.GameFramework.State.States
// {
//     public class GameplayState : GameState
//     {
//         private GameplayManager _gameplayManager;
//         private UIManager _uiManager;
//
//         [Inject]
//         public void Construct(GameplayManager gameplayManager, UIManager uiManager)
//         {
//             this._gameplayManager = gameplayManager;
//             this._uiManager = uiManager;
//         }
//
//         public override async void Enter()
//         {
//             // Initialize gameplay systems
//             await _gameplayManager.Initialize();
//
//             // Show gameplay UI
//             _uiManager.ShowMenu<GameplayHUD>();
//
//             // Subscribe to gameplay events
//             EventManager.Subscribe<GamePausedEvent>(this, OnGamePaused);
//             EventManager.Subscribe<GameOverEvent>(this, OnGameOver);
//             EventManager.Subscribe<PlayerDisconnectedEvent>(this, OnPlayerDisconnected);
//         }
//
//         public override void Exit()
//         {
//             // Cleanup gameplay systems
//             _gameplayManager.Cleanup();
//
//             // Hide gameplay UI
//             _uiManager.HideMenu<GameplayHUD>();
//
//             // Unsubscribe from events
//             EventManager.Unsubscribe(this);
//         }
//
//         public override void Update()
//         {
//             if (!_gameplayManager.IsGamePaused)
//             {
//                 _gameplayManager.Update();
//             }
//         }
//
//         public override void FixedUpdate()
//         {
//             if (!_gameplayManager.IsGamePaused)
//             {
//                 _gameplayManager.FixedUpdate();
//             }
//         }
//
//         private void OnGamePaused(GamePausedEvent evt)
//         {
//             StateMachine.SetState<PausedState>();
//         }
//
//         private void OnGameOver(GameOverEvent evt)
//         {
//             StateMachine.SetState<GameOverState>();
//         }
//
//         private void OnPlayerDisconnected(PlayerDisconnectedEvent evt)
//         {
//             if (IsHost)
//             {
//                 // Handle host-side disconnection logic
//                 _gameplayManager.HandlePlayerDisconnection(evt.PlayerId);
//             }
//         }
//     }
// }
