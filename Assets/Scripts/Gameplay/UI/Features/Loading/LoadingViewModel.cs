using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Core.Localization;
using Core.Shared;
using Core.UI.Core;
using Gameplay.UI.Localization;
using UnityEngine;
using VContainer;

namespace Gameplay.UI.Features.Loading
{
    public class LoadingViewModel : BaseViewModel
    {
        private readonly ILocalizationManager _localization;
        private CancellationTokenSource _cts;
        private readonly string[] _tipKeys = { LocKeys.LoadingTip1, LocKeys.LoadingTip2, LocKeys.LoadingTip3 };

        public BindableProperty<string> StatusText { get; } = new BindableProperty<string>("");
        public BindableProperty<string> ProgressText { get; } = new BindableProperty<string>("0%");
        public BindableProperty<float> ProgressValue { get; } = new BindableProperty<float>(0f);
        public BindableProperty<string> TipTitle { get; } = new BindableProperty<string>("");
        public BindableProperty<string> TipText { get; } = new BindableProperty<string>("");
        public BindableProperty<string> VersionText { get; } = new BindableProperty<string>($"v{Application.version}");

        [Inject]
        public LoadingViewModel(ILocalizationManager localization)
        {
            _localization = localization;
        }

        public override void Initialize()
        {
            base.Initialize();
            TipTitle.Value = _localization.GetText(LocKeys.LoadingTipTitle);
            
            // Set initial status
            StatusText.Value = _localization.GetText(LocKeys.LoadingStatusPreheating);

            StartTipCycling();
        }

        public void UpdateProgress(float progress, string status = null)
        {
            ProgressValue.Value = progress;
            ProgressText.Value = $"{Mathf.RoundToInt(progress * 100)}%";
            
            if (!string.IsNullOrEmpty(status))
            {
                StatusText.Value = status.ToUpper();
            }
        }

        private void StartTipCycling()
        {
            _cts = new CancellationTokenSource();
            CycleTipsAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid CycleTipsAsync(CancellationToken token)
        {
            int index = 0;
            while (!token.IsCancellationRequested)
            {
                string key = _tipKeys[index % _tipKeys.Length];
                TipText.Value = _localization.GetText(key);
                index++;

                await UniTask.Delay(TimeSpan.FromSeconds(4), cancellationToken: token);
            }
        }

        public override void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            base.Dispose();
        }
    }
}