using System;
using KitchenClash.Application.Services;
using KitchenClash.Infrastructure.DI;
using KitchenClash.Application.State;
using Cysharp.Threading.Tasks;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Configuration;
using UnityEngine.SceneManagement;

namespace KitchenClash.Infrastructure.States
{
    public class MainMenuState : BaseState
    {
        private readonly IUIService _uiService;
        private readonly ISessionContext _sessionContext;
        private readonly IEventBus _eventBus;

        public MainMenuState(IUIService uiService, ISessionContext sessionContext, IEventBus eventBus)
        {
            _uiService = uiService;
            _sessionContext = sessionContext;
            _eventBus = eventBus;
        }

        public override void Enter()
        {
            base.Enter();
            _eventBus?.Publish(new MusicEvent(MusicTrack.MainMenu));
            EnterAsync().Forget();
        }

        private async UniTask EnterAsync()
        {
            try
            {
                if (SceneManager.GetActiveScene().name != GameConstants.Scenes.MainMenu)
                {
                    await SceneManager.LoadSceneAsync(GameConstants.Scenes.MainMenu).ToUniTask();
                }
                if (!IsStateActive)
                {
                    return;
                }

                await UniTask.Delay(1500, cancellationToken: StateCancellationToken);
                if (!IsStateActive)
                {
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                GameLogger.Log("[MainMenuState] Enter cancelled");
            }
            catch (Exception ex)
            {
                GameLogger.LogException(ex);
            }
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void Update() { }
        public override void FixedUpdate() { }
    }
}
