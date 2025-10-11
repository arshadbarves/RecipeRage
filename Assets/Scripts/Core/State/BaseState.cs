namespace Core.State
{
    /// <summary>
    /// Base class for all game states with common functionality.
    /// </summary>
    public abstract class BaseState : IState
    {
        /// <summary>
        /// Name of the state for debugging.
        /// </summary>
        public string StateName => GetType().Name;

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public virtual void Enter()
        {
            StateUtility.LogStateEnter(StateName);
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public virtual void Exit()
        {
            StateUtility.LogStateExit(StateName);
        }

        /// <summary>
        /// Called every frame to update the state.
        /// </summary>
        public virtual void Update()
        {
            // Base implementation does nothing
        }

        /// <summary>
        /// Called at fixed intervals for physics updates.
        /// </summary>
        public virtual void FixedUpdate()
        {
            // Base implementation does nothing
        }

        /// <summary>
        /// Log a message with the state name prefix.
        /// </summary>
        /// <param name="message">Message to log</param>
        protected void LogMessage(string message)
        {
            StateUtility.LogStateAction(StateName, message);
        }

        /// <summary>
        /// Log a warning with the state name prefix.
        /// </summary>
        /// <param name="message">Warning message to log</param>
        protected void LogWarning(string message)
        {
            StateUtility.LogStateWarning(StateName, message);
        }

        /// <summary>
        /// Log an error with the state name prefix.
        /// </summary>
        /// <param name="message">Error message to log</param>
        protected void LogError(string message)
        {
            StateUtility.LogStateError(StateName, message);
        }
    }
}
