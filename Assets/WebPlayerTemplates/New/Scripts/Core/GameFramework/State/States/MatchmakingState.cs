// using Core.GameFramework.Scene;
// using VContainer;
//
// namespace Core.GameFramework.State.States
// {
//     public class MatchmakingState : GameState
//     {
//         private MatchmakingManager _matchmakingManager;
//         private UIManager _uiManager;
//
//         [Inject]
//         public void Construct(MatchmakingManager matchmakingManager, UIManager uiManager)
//         {
//             this._matchmakingManager = matchmakingManager;
//             this._uiManager = uiManager;
//         }
//
//         public override async void Enter()
//         {
//             _uiManager.ShowMenu<MatchmakingUI>();
//
//             EventManager.Subscribe<MatchFoundEvent>(this, OnMatchFound);
//             EventManager.Subscribe<MatchmakingFailedEvent>(this, OnMatchmakingFailed);
//
//             await _matchmakingManager.StartMatchmaking();
//         }
//
//         public override void Exit()
//         {
//             _matchmakingManager.StopMatchmaking();
//             _uiManager.HideMenu<MatchmakingUI>();
//             EventManager.Unsubscribe(this);
//         }
//
//         private void OnMatchFound(MatchFoundEvent evt)
//         {
//             var loadingState = resolver.Resolve<LoadingState>();
//             loadingState.Initialize(SceneConfig.GameScene.Kitchen1, resolver.Resolve<GameplayState>());
//             StateMachine.SetState(loadingState);
//         }
//
//         private void OnMatchmakingFailed(MatchmakingFailedEvent evt)
//         {
//             StateMachine.SetState<MainMenuState>();
//         }
//     }
// }
