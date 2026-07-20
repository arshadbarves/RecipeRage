using Cysharp.Threading.Tasks;
using Playcenter.GameFlow;
using UnityEngine;

namespace KitchenClash.Infrastructure.Flow
{
    /// <summary>
    /// Production splash port: brief dwell then advance to Boot.
    /// Dwell defaults to 0.5s since BootstrapState no longer owns splash display.
    /// </summary>
    public sealed class SplashFlowPort : ISplashPort
    {
        private readonly IAppFlow _appFlow;
        private readonly float _dwellSeconds;
        private int _runId;

        public SplashFlowPort(IAppFlow appFlow, float dwellSeconds = 0.5f)
        {
            _appFlow = appFlow;
            _dwellSeconds = Mathf.Max(0f, dwellSeconds);
        }

        public void EnterSplash(FlowContext context)
        {
            int id = ++_runId;
            RunAsync(id).Forget();
        }

        public void ExitSplash()
        {
            _runId++;
        }

        private async UniTaskVoid RunAsync(int id)
        {
            if (_dwellSeconds > 0f)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(_dwellSeconds),
                    DelayType.UnscaledDeltaTime);
            }

            if (id == _runId)
            {
                // SDK now owns the Splash/Loading screens; this port is a no-op legacy adapter.
                // If somehow invoked, fail-close to Home rather than calling the removed NotifySplashComplete.
                _appFlow.ReturnHome();
            }
        }
    }
}
