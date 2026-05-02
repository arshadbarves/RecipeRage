using System;
using System.Threading.Tasks;
using KitchenClash.Domain;

namespace KitchenClash.Presentation.ViewModels
{
    public sealed class ResultsViewModel : ScreenViewModel
    {
        private readonly IAdsService _adsService;
        private readonly IEconomyService _economyService;

        public BindableProperty<int> FinalScoreA { get; } = new(0);
        public BindableProperty<int> FinalScoreB { get; } = new(0);
        public BindableProperty<bool> IsWinner { get; } = new(false);
        public BindableProperty<bool> IsDraw { get; } = new(false);
        public BindableProperty<int> TrophyChange { get; } = new(0);
        public BindableProperty<bool> CanShowRewardedAd { get; } = new(false);

        private bool _adClaimed;

        public ResultsViewModel() : this(null, null) { }

        public ResultsViewModel(IAdsService adsService, IEconomyService economyService)
        {
            _adsService = adsService;
            _economyService = economyService;
        }

        public override void OnEnter(object param)
        {
            if (param is MatchEndEvaluation eval)
            {
                IsWinner.Value = eval.WinningTeamId >= 0;
                IsDraw.Value = eval.IsDraw;
            }

            _adClaimed = false;
            CanShowRewardedAd.Value = _adsService != null && _adsService.IsRewardedReady;
        }

        public async void WatchAdForGemsCommand()
        {
            if (_adClaimed || _adsService == null || !_adsService.IsRewardedReady)
                return;

            CanShowRewardedAd.Value = false;

            try
            {
                var result = await _adsService.ShowRewardedAsync("post_match_gems");
                if (result != null && result.Granted)
                {
                    _economyService?.AddGems(3);
                    _adClaimed = true;
                    GameLogger.Log("[ResultsViewModel] Rewarded ad granted +3 gems");
                }
                else
                {
                    CanShowRewardedAd.Value = _adsService.IsRewardedReady;
                }
            }
            catch (Exception ex)
            {
                GameLogger.Log($"[ResultsViewModel] Rewarded ad error: {ex.Message}");
                CanShowRewardedAd.Value = _adsService.IsRewardedReady;
            }
        }
    }
}
