using System.Threading;
using Core.Logging;

namespace Gameplay.App.State
{
    public abstract class BaseState : IState
    {
        private CancellationTokenSource _stateCts;

        public string StateName => GetType().Name;
        protected CancellationToken StateCancellationToken => _stateCts?.Token ?? CancellationToken.None;
        protected bool IsStateActive => _stateCts != null && !_stateCts.IsCancellationRequested;

        public virtual void Enter()
        {
            _stateCts?.Cancel();
            _stateCts?.Dispose();
            _stateCts = new CancellationTokenSource();
            GameLogger.Log($"[{StateName}] Entered");
        }

        public virtual void Exit()
        {
            _stateCts?.Cancel();
            _stateCts?.Dispose();
            _stateCts = null;
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
