namespace RecipeRage.Core.GameState
{
    /// <summary>
    /// Interface for all game states
    /// </summary>
    public interface IGameState
    {
        /// <summary>
        /// The unique identifier for this state
        /// </summary>
        GameStateType StateType { get; }
        
        /// <summary>
        /// Called when entering this state
        /// </summary>
        void Enter();
        
        /// <summary>
        /// Called when exiting this state
        /// </summary>
        void Exit();
        
        /// <summary>
        /// Called every frame while this state is active
        /// </summary>
        void Update();
        
        /// <summary>
        /// Called at a fixed interval while this state is active
        /// </summary>
        void FixedUpdate();
        
        /// <summary>
        /// Called when the state is paused
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Called when the state is resumed
        /// </summary>
        void Resume();
    }
    
    /// <summary>
    /// Enum defining all possible game states
    /// </summary>
    public enum GameStateType
    {
        None,
        Splash,
        MainMenu,
        Lobby,
        Loading,
        InGame,
        Paused,
        GameOver,
        Shop,
        Settings
    }
}
