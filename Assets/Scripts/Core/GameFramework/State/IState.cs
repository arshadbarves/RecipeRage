namespace Core.GameFramework.State
{
    /// <summary>
    /// Interface for game states.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Name of the state for debugging.
        /// </summary>
        string StateName { get; }
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        void Enter();

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        void Exit();

        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        void Update();

        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        void FixedUpdate();
    }
}
