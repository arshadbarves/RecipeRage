using System;
using System.Threading;
using Modules.Localization;
using Core.Reactive;
using Cysharp.Threading.Tasks;
using UI.Core;
using UnityEngine;
using VContainer;

namespace UI.ViewModels
{
    public class LoadingViewModel : BaseViewModel
    {
        private readonly ILocalizationManager _localization;
        private CancellationTokenSource _cts;
        private readonly string[] _tipKeys = { "loading_tip_1", "loading_tip_2", "loading_tip_3" };

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
            TipTitle.Value = _localization.GetText("loading_tip_title");
            
            // Set initial status
            StatusText.Value = _localization.GetText("loading_status_preheating");

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