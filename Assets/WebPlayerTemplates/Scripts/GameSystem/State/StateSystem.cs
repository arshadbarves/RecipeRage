using System.Threading.Tasks;
using Core;
using GameSystem.State.GameStates;

namespace GameSystem.State
{
    public class StateSystem : IGameSystem
    {
        private GameManager _gameManager;
        private StateMachine<GameState> _gameStateMachine;

        public Task InitializeAsync()
        {
            _gameManager = GameManager.Instance;
            _gameStateMachine = new StateMachine<GameState>();
            _gameStateMachine.AddState(GameState.Splash, new SplashScreenState(_gameManager));
            _gameStateMachine.AddState(GameState.Loading, new LoadingState(_gameManager));
            _gameStateMachine.AddState(GameState.MainMenu, new MainMenuState(_gameManager));
            _gameStateMachine.AddState(GameState.Matchmaking, new MatchmakingState(_gameManager));
            _gameStateMachine.AddState(GameState.CharacterSelection, new CharacterSelectionState(_gameManager));
            // Add other states as needed

            _gameStateMachine.OnStateChanged += state => _gameManager.InvokeGameStateChanged(state);
            return Task.CompletedTask;
        }

        public void Update()
        {
            _gameStateMachine.Update();
        }

        public Task CleanupAsync()
        {
            return Task.CompletedTask;
        }

        public void RequestGameStateChange(GameState newState)
        {
            _gameStateMachine.TransitionTo(newState);
        }
    }
}