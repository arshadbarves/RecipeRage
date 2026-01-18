using Core.Logging;

namespace Gameplay.App.State
{
    public abstract class BaseState : IState
    {
        public string StateName => GetType().Name;

        public virtual void Enter()
        {
            GameLogger.Log($"[{StateName}] Entered");
        }

        public virtual void Exit()
        {
            GameLogger.Log($"[{StateName}] Exited");
        }

        public virtual void Update()
        {
            // Base implementation does nothing
        }

        public virtual void FixedUpdate()
        {
            // Base implementation does nothing
        }

        protected void LogMessage(string message)
        {
            GameLogger.Log($"[{StateName}] {message}");
        }

        protected void LogWarning(string message)
        {
            GameLogger.LogWarning($"[{StateName}] {message}");
        }

        protected void LogError(string message)
        {
            GameLogger.LogError($"[{StateName}] {message}");
        }
    }
}
