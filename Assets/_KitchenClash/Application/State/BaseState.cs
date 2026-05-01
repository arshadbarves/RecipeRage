using System.Threading;
using KitchenClash.Domain;

namespace KitchenClash.Application.State
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
        }

        public virtual void FixedUpdate()
        {
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
