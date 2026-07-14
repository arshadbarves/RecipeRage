using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Configuration;
using UnityEngine.SceneManagement;

namespace KitchenClash.Infrastructure.Flow.Handlers
{
    /// <summary>
    /// Home hub: main-menu music + MainMenu scene load.
    /// </summary>
    public sealed class HomePhase
    {
        private readonly IEventBus _eventBus;
        private CancellationTokenSource _cts;
        private bool _active;

        public HomePhase(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Enter()
        {
            Exit();
            _active = true;
            _cts = new CancellationTokenSource();
            _eventBus?.Publish(new MusicEvent(MusicTrack.MainMenu));
            EnterAsync(_cts.Token).Forget();
        }

        public void Exit()
        {
            _active = false;
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        private async UniTask EnterAsync(CancellationToken ct)
        {
            try
            {
                if (SceneManager.GetActiveScene().name != GameConstants.Scenes.MainMenu)
                {
                    await SceneManager.LoadSceneAsync(GameConstants.Scenes.MainMenu).ToUniTask(cancellationToken: ct);
                }

                if (!_active || ct.IsCancellationRequested)
                {
                    return;
                }

                await UniTask.Delay(1500, cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                GameLogger.Log("[HomePhase] Enter cancelled");
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }
    }
}
