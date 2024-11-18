// using Core.GameFramework.Scene;
// using VContainer;
//
// namespace Core.GameFramework.State.States
// {
//     public abstract class LoadingState : GameState
//     {
//         private SceneLoadManager _sceneLoadManager;
//         private UIManager _uiManager;
//         private SceneConfig.GameScene _targetScene;
//         private IState _nextState;
//
//         [Inject]
//         public void Construct(SceneLoadManager sceneLoadManager, UIManager uiManager)
//         {
//             this._sceneLoadManager = sceneLoadManager;
//             this._uiManager = uiManager;
//         }
//
//         public void Initialize(SceneConfig.GameScene scene, IState nextState)
//         {
//             this._targetScene = scene;
//             this._nextState = nextState;
//         }
//
//         public override async void Enter()
//         {
//             _uiManager.ShowMenu<LoadingScreenUI>();
//
//             bool success = await _sceneLoadManager.LoadScene(_targetScene);
//
//             if (success)
//             {
//                 StateMachine.SetState(_nextState);
//             }
//             else
//             {
//                 StateMachine.SetState<ErrorState>();
//             }
//         }
//
//         public override void Exit()
//         {
//             _uiManager.HideMenu<LoadingScreenUI>();
//         }
//     }
// }
