using KitchenClash.Application;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Playcenter.GameFlow;
using Playcenter.Services;
using Playcenter.Shell;
using Playcenter.UI;
using Playcenter.UI.Toolkit;
using UnityEngine.UIElements;
using VContainer;

namespace KitchenClash.Presentation.Screens
{
    /// <summary>
    /// Post-match results: scores/verdict from match HUD, reward delta from
    /// <see cref="MatchRewardEvent"/>, wallet chips from session economy/wallet.
    /// Display only — no economy writes.
    /// </summary>
    [UIScreen(UIScreenCategory.Screen, "Screens/GameOverScreenTemplate")]
    public class ResultsScreen : BaseUIScreen
    {
        [Inject] private IUIService _uiService;
        [Inject] private ISessionContext _sessionContext;
        [Inject] private IMatchHudPort _matchHud;
        [Inject] private IAppFlow _appFlow;
        [Inject] private IEventBus _eventBus;

        private Label _winnerLabel;
        private Label _scoreTeam0;
        private Label _scoreTeam1;
        private Button _lobbyButton;
        private Label _rewardDeltaCoins;
        private Label _walletCoins;
        private Label _walletGems;

        private int _lastCoinsAwarded;
        private bool _hasRewardEvent;

        protected override void OnInitialize()
        {
            _winnerLabel = GetElement<Label>("winner-label");
            _scoreTeam0 = GetElement<Label>("score-team-0");
            _scoreTeam1 = GetElement<Label>("score-team-1");
            _lobbyButton = GetElement<Button>("lobby-btn");
            _rewardDeltaCoins = GetElement<Label>("reward-delta-coins");
            _walletCoins = GetElement<Label>("wallet-coins");
            _walletGems = GetElement<Label>("wallet-gems");

            if (_lobbyButton != null)
            {
                _lobbyButton.clicked += OnLobbyButtonClicked;
            }

            _eventBus?.Subscribe<MatchRewardEvent>(OnMatchReward);
        }

        protected override void OnDispose()
        {
            _eventBus?.Unsubscribe<MatchRewardEvent>(OnMatchReward);
            if (_lobbyButton != null)
            {
                _lobbyButton.clicked -= OnLobbyButtonClicked;
            }
        }

        protected override void OnShow()
        {
            base.OnShow();
            UpdateScores();
            UpdateRewards();
            UpdateWalletChips();
        }

        private void OnMatchReward(MatchRewardEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            _lastCoinsAwarded = evt.CoinsAwarded;
            _hasRewardEvent = true;
            UpdateRewards();
            UpdateWalletChips();
        }

        private void UpdateScores()
        {
            _matchHud?.Refresh();

            if (_matchHud != null)
            {
                if (_scoreTeam0 != null)
                {
                    _scoreTeam0.text = _matchHud.GetTeamScore(0).ToString();
                }

                if (_scoreTeam1 != null)
                {
                    _scoreTeam1.text = _matchHud.GetTeamScore(1).ToString();
                }
            }

            if (_winnerLabel == null)
            {
                return;
            }

            MatchResultSnapshot result = _matchHud != null
                ? _matchHud.CurrentMatchResult
                : MatchResultSnapshot.None;

            if (!result.HasResult)
            {
                GameLogger.LogError("[ResultsScreen] Missing synchronized match result. Showing neutral fallback text.");
                _winnerLabel.text = "MATCH COMPLETE";
                return;
            }

            _winnerLabel.text = GetWinnerText(result);
        }

        private void UpdateRewards()
        {
            if (_rewardDeltaCoins == null)
            {
                return;
            }

            if (_hasRewardEvent)
            {
                _rewardDeltaCoins.text = _lastCoinsAwarded >= 0
                    ? $"+{_lastCoinsAwarded}"
                    : _lastCoinsAwarded.ToString();
                return;
            }

            // Fallback estimate from HUD if event already fired before subscribe window.
            if (_matchHud != null && _matchHud.HasMatchResult)
            {
                MatchResultSnapshot result = _matchHud.CurrentMatchResult;
                bool won = !result.IsDraw && result.WinningTeamId == _matchHud.LocalTeamId;
                int score = _matchHud.GetTeamScore(_matchHud.LocalTeamId);
                int estimate = won
                    ? EconomyService.MatchWinReward + UnityEngine.Mathf.FloorToInt(score * EconomyService.ScoreBonusCoinRate)
                    : EconomyService.MatchLossReward;
                _rewardDeltaCoins.text = $"+{estimate}";
                return;
            }

            _rewardDeltaCoins.text = "+0";
        }

        private void UpdateWalletChips()
        {
            int coins = 0;
            int gems = 0;
            bool found = false;

            if (_sessionContext != null && _sessionContext.IsSessionActive)
            {
                IEconomyService economy = _sessionContext.EconomyService;
                if (economy != null)
                {
                    coins = economy.Coins;
                    gems = economy.Gems;
                    found = true;
                }

                if (!found)
                {
                    try
                    {
                        IWallet wallet = _sessionContext.Resolve<IWallet>();
                        if (wallet != null)
                        {
                            coins = wallet.GetBalance(CurrencyId.Coins);
                            gems = wallet.GetBalance(CurrencyId.Gems);
                            found = true;
                        }
                    }
                    catch
                    {
                        // Wallet optional at results time.
                    }
                }
            }

            if (_walletCoins != null)
            {
                _walletCoins.text = found ? $"🪙 {coins}" : "🪙 —";
            }

            if (_walletGems != null)
            {
                _walletGems.text = found ? $"💎 {gems}" : "💎 —";
            }
        }

        public static string GetWinnerText(MatchResultSnapshot result)
        {
            if (!result.HasResult)
            {
                return "MATCH COMPLETE";
            }

            if (result.IsDraw)
            {
                return "DRAW!";
            }

            return result.WinningTeamId == 0 ? "TEAM 1 WINS!" : "TEAM 2 WINS!";
        }

        private void OnLobbyButtonClicked()
        {
            GameLogger.Log("Returning to Lobby...");
            _sessionContext?.GameStarter?.EndGame();
        }
    }
}
