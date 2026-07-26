using System;
using Cysharp.Threading.Tasks;
using KitchenClash.Application.Services;
using Playcenter.GameFlow;
using UnityEngine;
using Playcenter.UI;

namespace KitchenClash.Infrastructure.Flow
{
    /// <summary>
    /// 3-2-1-GO beat. Shows overlay, ticks unscaled time, then NotifyCountdownComplete.
    /// Input lock is implicit while overlay is full-screen; StartRound fires after GO.
    /// </summary>
    public sealed class CountdownFlowPort : ICountdownPort
    {
        private const string ScreenTypeName =
            "KitchenClash.Presentation.Screens.CountdownOverlayScreen, KitchenClash.Presentation";

        private readonly IUIService _uiService;
        private readonly IAppFlow _appFlow;
        private readonly float _beatSeconds;
        private readonly float _goHoldSeconds;

        private bool _active;
        private int _runId;

        public CountdownFlowPort(
            IUIService uiService,
            IAppFlow appFlow,
            float beatSeconds = 0.75f,
            float goHoldSeconds = 0.55f)
        {
            _uiService = uiService;
            _appFlow = appFlow;
            _beatSeconds = Mathf.Max(0.25f, beatSeconds);
            _goHoldSeconds = Mathf.Max(0.2f, goHoldSeconds);
        }

        public void EnterCountdown(FlowContext context)
        {
            _ = context;
            _active = true;
            int runId = ++_runId;

            Type screenType = Type.GetType(ScreenTypeName);
            if (screenType != null)
            {
                _uiService?.Show(screenType);
            }

            RunCountdownAsync(runId).Forget();
        }

        public void ExitCountdown()
        {
            _active = false;
            Type screenType = Type.GetType(ScreenTypeName);
            if (screenType != null)
            {
                _uiService?.Hide(screenType);
            }
        }

        private async UniTaskVoid RunCountdownAsync(int runId)
        {
            try
            {
                for (int n = 3; n >= 1; n--)
                {
                    if (!_active || runId != _runId)
                    {
                        return;
                    }

                    TrySetCount(n);
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(_beatSeconds),
                        DelayType.UnscaledDeltaTime);
                }

                if (!_active || runId != _runId)
                {
                    return;
                }

                TrySetGo();
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_goHoldSeconds),
                    DelayType.UnscaledDeltaTime);

                if (_active && runId == _runId)
                {
                    _appFlow?.NotifyCountdownComplete();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (_active && runId == _runId)
                {
                    _appFlow?.NotifyCountdownComplete();
                }
            }
        }

        private void TrySetCount(int value)
        {
            TryInvokeScreen("SetCount", new object[] { value }, new[] { typeof(int) });
        }

        private void TrySetGo()
        {
            TryInvokeScreen("SetGo", Array.Empty<object>(), Type.EmptyTypes);
        }

        private void TryInvokeScreen(string methodName, object[] args, Type[] argTypes)
        {
            if (_uiService == null)
            {
                return;
            }

            try
            {
                Type screenType = Type.GetType(ScreenTypeName);
                if (screenType == null)
                {
                    return;
                }

                var getScreen = typeof(IUIService).GetMethod(nameof(IUIService.GetScreen));
                if (getScreen == null)
                {
                    return;
                }

                object screen = getScreen.MakeGenericMethod(screenType).Invoke(_uiService, null);
                if (screen == null)
                {
                    return;
                }

                var method = screenType.GetMethod(methodName, argTypes);
                method?.Invoke(screen, args);
            }
            catch
            {
                // Cosmetic only.
            }
        }
    }
}
