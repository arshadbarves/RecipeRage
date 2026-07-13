using System;
using Cysharp.Threading.Tasks;
using KitchenClash.Application.Services;
using Playcenter.GameFlow;
using UnityEngine;

namespace KitchenClash.Infrastructure.Flow
{
    /// <summary>
    /// Match-found beat: show intro card for a short dwell, then NotifyMatchIntroReady.
    /// Presentation screen reads mode/map from IAppFlow.Context on show.
    /// Progress fill is cosmetic (map preload runs in parallel via MatchRuntime.EnterMatch).
    /// </summary>
    public sealed class MatchIntroFlowPort : IMatchIntroPort
    {
        private const string ScreenTypeName =
            "KitchenClash.Presentation.Screens.MatchIntroScreen, KitchenClash.Presentation";

        private readonly IUIService _uiService;
        private readonly IAppFlow _appFlow;
        private readonly float _dwellSeconds;

        private bool _active;
        private int _runId;

        public MatchIntroFlowPort(IUIService uiService, IAppFlow appFlow, float dwellSeconds = 1.6f)
        {
            _uiService = uiService;
            _appFlow = appFlow;
            _dwellSeconds = Mathf.Max(0.4f, dwellSeconds);
        }

        public void EnterMatchIntro(FlowContext context, MatchResolvedInfo info)
        {
            _ = context;
            _ = info;
            _active = true;
            int runId = ++_runId;

            Type screenType = Type.GetType(ScreenTypeName);
            if (screenType != null)
            {
                _uiService?.Show(screenType);
            }

            RunIntroAsync(runId).Forget();
        }

        public void ExitMatchIntro()
        {
            _active = false;
            Type screenType = Type.GetType(ScreenTypeName);
            if (screenType != null)
            {
                _uiService?.Hide(screenType);
            }
        }

        private async UniTaskVoid RunIntroAsync(int runId)
        {
            try
            {
                float elapsed = 0f;
                while (_active && runId == _runId && elapsed < _dwellSeconds)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / _dwellSeconds);
                    float progress = 0.08f + (0.92f * (1f - Mathf.Pow(1f - t, 2f)));
                    TrySetProgress(progress);
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }

                if (!_active || runId != _runId)
                {
                    return;
                }

                TrySetProgress(1f);
                await UniTask.Delay(TimeSpan.FromMilliseconds(120), DelayType.UnscaledDeltaTime);

                if (_active && runId == _runId)
                {
                    _appFlow?.NotifyMatchIntroReady();
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
                    _appFlow?.NotifyMatchIntroReady();
                }
            }
        }

        private void TrySetProgress(float value)
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

                var setProgress = screenType.GetMethod("SetProgress", new[] { typeof(float) });
                setProgress?.Invoke(screen, new object[] { value });
            }
            catch
            {
                // Cosmetic only — never fail the beat on UI progress.
            }
        }
    }
}
