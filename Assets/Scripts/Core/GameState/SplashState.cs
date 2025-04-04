using RecipeRage.Modules.Logging;
using UnityEngine;

namespace RecipeRage.Core.GameState
{
    /// <summary>
    /// Splash screen state shown when the game starts
    /// </summary>
    public class SplashState : IGameState
    {
        private readonly IGameStateManager _gameStateManager;
        private float _timer;
        private const float SPLASH_DURATION = 3.0f;
        
        /// <summary>
        /// The unique identifier for this state
        /// </summary>
        public GameStateType StateType => GameStateType.Splash;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gameStateManager">Reference to the game state manager</param>
        public SplashState(IGameStateManager gameStateManager)
        {
            _gameStateManager = gameStateManager;
        }
        
        /// <summary>
        /// Called when entering this state
        /// </summary>
        public void Enter()
        {
            LogHelper.Info("SplashState", "Entering splash state");
            _timer = 0f;
            
            // TODO: Show splash screen UI
        }
        
        /// <summary>
        /// Called when exiting this state
        /// </summary>
        public void Exit()
        {
            LogHelper.Info("SplashState", "Exiting splash state");
            
            // TODO: Hide splash screen UI
        }
        
        /// <summary>
        /// Called every frame while this state is active
        /// </summary>
        public void Update()
        {
            _timer += Time.deltaTime;
            
            // Transition to main menu after splash duration
            if (_timer >= SPLASH_DURATION)
            {
                _gameStateManager.ChangeState(GameStateType.MainMenu);
            }
        }
        
        /// <summary>
        /// Called at a fixed interval while this state is active
        /// </summary>
        public void FixedUpdate()
        {
            // Not used in this state
        }
        
        /// <summary>
        /// Called when the state is paused
        /// </summary>
        public void Pause()
        {
            LogHelper.Info("SplashState", "Pausing splash state");
        }
        
        /// <summary>
        /// Called when the state is resumed
        /// </summary>
        public void Resume()
        {
            LogHelper.Info("SplashState", "Resuming splash state");
        }
    }
}
