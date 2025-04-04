namespace RecipeRage.Core.GameState
{
    /// <summary>
    /// Interface for the game state manager
    /// </summary>
    public interface IGameStateManager
    {
        /// <summary>
        /// The current active game state
        /// </summary>
        IGameState CurrentState { get; }
        
        /// <summary>
        /// The previous game state
        /// </summary>
        IGameState PreviousState { get; }
        
        /// <summary>
        /// Initialize the game state manager
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Change to a new game state
        /// </summary>
        /// <param name="newStateType">The type of the new state</param>
        void ChangeState(GameStateType newStateType);
        
        /// <summary>
        /// Register a new game state
        /// </summary>
        /// <param name="state">The state to register</param>
        void RegisterState(IGameState state);
        
        /// <summary>
        /// Pause the current state and push it onto the stack
        /// </summary>
        /// <param name="newStateType">The type of the new state to enter</param>
        void PushState(GameStateType newStateType);
        
        /// <summary>
        /// Pop the current state and resume the previous state
        /// </summary>
        void PopState();
    }
}
