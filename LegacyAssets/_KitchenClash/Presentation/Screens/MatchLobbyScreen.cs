using System.Collections.Generic;
using KitchenClash.Application;
using Playcenter.Services;
using Playcenter.Shell;
using Playcenter.UI;
using Playcenter.UI.Toolkit;
using UnityEngine.UIElements;
using VContainer;

namespace KitchenClash.Presentation.Screens
{
    /// <summary>
    /// Match lobby presentation: team slots sourced from match lobby members
    /// (falls back to party lobby only when match lobby is unavailable).
    /// </summary>
    [UIScreen(UIScreenCategory.Screen, "Screens/MatchLobbyViewTemplate")]
    public class MatchLobbyScreen : BaseUIScreen
    {
        private const int MaxSlotsPerTeam = 3;

        [Inject] private ISessionContext _sessionContext;

        private Label _timerLabel;
        private Button _readyButton;
        private Button _leaveButton;
        private Button _startButton;

        private readonly VisualElement[] _teamASlots = new VisualElement[MaxSlotsPerTeam];
        private readonly Label[] _teamANames = new Label[MaxSlotsPerTeam];
        private readonly Label[] _teamAStatuses = new Label[MaxSlotsPerTeam];
        private readonly VisualElement[] _teamBSlots = new VisualElement[MaxSlotsPerTeam];
        private readonly Label[] _teamBNames = new Label[MaxSlotsPerTeam];
        private readonly Label[] _teamBStatuses = new Label[MaxSlotsPerTeam];

        private ILobbyManager _lobbyManager;
        private IPlayerManager _playerManager;
        private bool _localReady;
        private bool _subscribed;

        protected override void OnInitialize()
        {
            _timerLabel = GetElement<Label>("lbl-lobby-timer");
            _readyButton = GetElement<Button>("btn-ready");
            _leaveButton = GetElement<Button>("btn-leave");
            _startButton = GetElement<Button>("btn-start");

            for (int i = 0; i < MaxSlotsPerTeam; i++)
            {
                int slot = i + 1;
                _teamASlots[i] = GetElement<VisualElement>($"team-a-slot-{slot}");
                _teamANames[i] = GetElement<Label>($"team-a-slot-{slot}-name");
                _teamAStatuses[i] = GetElement<Label>($"team-a-slot-{slot}-status");
                _teamBSlots[i] = GetElement<VisualElement>($"team-b-slot-{slot}");
                _teamBNames[i] = GetElement<Label>($"team-b-slot-{slot}-name");
                _teamBStatuses[i] = GetElement<Label>($"team-b-slot-{slot}-status");
            }

            if (_readyButton != null) _readyButton.clicked += OnReadyClicked;
            if (_leaveButton != null) _leaveButton.clicked += OnLeaveClicked;
            if (_startButton != null) _startButton.clicked += OnStartClicked;
        }

        protected override void OnShow()
        {
            base.OnShow();
            ResolveSessionServices();
            SubscribeLobby();
            RefreshFromLobby();
        }

        protected override void OnHide()
        {
            UnsubscribeLobby();
            base.OnHide();
        }

        protected override void OnDispose()
        {
            UnsubscribeLobby();
            if (_readyButton != null) _readyButton.clicked -= OnReadyClicked;
            if (_leaveButton != null) _leaveButton.clicked -= OnLeaveClicked;
            if (_startButton != null) _startButton.clicked -= OnStartClicked;
        }

        private void ResolveSessionServices()
        {
            _lobbyManager = _sessionContext?.LobbyManager;
            _playerManager = null;

            if (_sessionContext == null || !_sessionContext.IsSessionActive)
            {
                return;
            }

            try
            {
                _playerManager = _sessionContext.Resolve<IPlayerManager>();
            }
            catch
            {
                _playerManager = null;
            }
        }

        private void SubscribeLobby()
        {
            if (_subscribed || _lobbyManager == null)
            {
                return;
            }

            _lobbyManager.OnMatchLobbyUpdated += OnMatchLobbyUpdated;
            _lobbyManager.OnMatchLobbyJoined += OnMatchLobbyJoined;
            _lobbyManager.OnMatchLobbyLeft += OnMatchLobbyLeft;
            _subscribed = true;
        }

        private void UnsubscribeLobby()
        {
            if (!_subscribed || _lobbyManager == null)
            {
                return;
            }

            _lobbyManager.OnMatchLobbyUpdated -= OnMatchLobbyUpdated;
            _lobbyManager.OnMatchLobbyJoined -= OnMatchLobbyJoined;
            _lobbyManager.OnMatchLobbyLeft -= OnMatchLobbyLeft;
            _subscribed = false;
        }

        private void OnMatchLobbyUpdated() => RefreshFromLobby();

        private void OnMatchLobbyJoined(LobbyOpResult result, LobbyInfo lobby) => RefreshFromLobby();

        private void OnMatchLobbyLeft() => RefreshFromLobby();

        /// <summary>
        /// Prefer match lobby members; party lobby is fallback only.
        /// </summary>
        private LobbyInfo GetDisplayLobby()
        {
            if (_lobbyManager == null)
            {
                return null;
            }

            if (_lobbyManager.CurrentMatchLobby != null)
            {
                return _lobbyManager.CurrentMatchLobby;
            }

            if (_lobbyManager.IsInMatchLobby == false && _lobbyManager.CurrentPartyLobby != null)
            {
                return _lobbyManager.CurrentPartyLobby;
            }

            return _lobbyManager.CurrentPartyLobby;
        }

        private void RefreshFromLobby()
        {
            LobbyInfo lobby = GetDisplayLobby();
            List<PlayerInfo> players = lobby?.Players ?? new List<PlayerInfo>();

            var teamA = new List<PlayerInfo>();
            var teamB = new List<PlayerInfo>();
            foreach (PlayerInfo player in players)
            {
                if (player == null)
                {
                    continue;
                }

                if (player.Team == TeamId.TeamB)
                {
                    teamB.Add(player);
                }
                else
                {
                    teamA.Add(player);
                }
            }

            int teamSize = lobby != null && lobby.TeamSize > 0
                ? lobby.TeamSize
                : MaxSlotsPerTeam;
            if (teamSize > MaxSlotsPerTeam)
            {
                teamSize = MaxSlotsPerTeam;
            }

            BindTeamSlots(_teamASlots, _teamANames, _teamAStatuses, teamA, teamSize);
            BindTeamSlots(_teamBSlots, _teamBNames, _teamBStatuses, teamB, teamSize);

            PlayerInfo local = FindLocalPlayer(players);
            _localReady = local != null && local.IsReady;
            UpdateReadyButton();

            bool isOwner = _lobbyManager != null && _lobbyManager.IsMatchLobbyOwner;
            if (_startButton != null)
            {
                _startButton.style.display = isOwner ? DisplayStyle.Flex : DisplayStyle.None;
                bool allReady = _lobbyManager != null && _lobbyManager.AreAllPlayersReady();
                _startButton.SetEnabled(isOwner && allReady);
            }

            if (_timerLabel != null)
            {
                string source = lobby == _lobbyManager?.CurrentMatchLobby ? "MATCH" : "PARTY";
                int count = lobby?.CurrentPlayers ?? players.Count;
                int max = lobby?.MaxPlayers > 0 ? lobby.MaxPlayers : teamSize * 2;
                _timerLabel.text = $"{source}  {count}/{max}";
            }
        }

        private static PlayerInfo FindLocalPlayer(List<PlayerInfo> players)
        {
            if (players == null)
            {
                return null;
            }

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != null && players[i].IsLocal)
                {
                    return players[i];
                }
            }

            return null;
        }

        private static void BindTeamSlots(
            VisualElement[] slots,
            Label[] names,
            Label[] statuses,
            List<PlayerInfo> members,
            int visibleCount)
        {
            for (int i = 0; i < MaxSlotsPerTeam; i++)
            {
                VisualElement slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                bool visible = i < visibleCount;
                slot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                if (!visible)
                {
                    continue;
                }

                PlayerInfo player = i < members.Count ? members[i] : null;
                if (player == null)
                {
                    slot.EnableInClassList("pc-player-slot--empty", true);
                    slot.EnableInClassList("pc-player-slot--ready", false);
                    if (names[i] != null) names[i].text = "Waiting...";
                    if (statuses[i] != null) statuses[i].text = "OPEN";
                    continue;
                }

                slot.EnableInClassList("pc-player-slot--empty", false);
                slot.EnableInClassList("pc-player-slot--ready", player.IsReady);

                string name = string.IsNullOrEmpty(player.DisplayName) ? "Player" : player.DisplayName;
                if (player.IsLocal)
                {
                    name += " (YOU)";
                }
                else if (player.IsBot)
                {
                    name += " (BOT)";
                }

                if (names[i] != null) names[i].text = name;

                if (statuses[i] != null)
                {
                    if (player.IsReady)
                    {
                        statuses[i].text = "READY";
                    }
                    else if (player.IsHost)
                    {
                        statuses[i].text = "HOST";
                    }
                    else
                    {
                        statuses[i].text = "NOT READY";
                    }
                }
            }
        }

        private void UpdateReadyButton()
        {
            if (_readyButton == null)
            {
                return;
            }

            _readyButton.SetEnabled(true);
            _readyButton.text = _localReady ? "UNREADY" : "READY";
            _readyButton.EnableInClassList("pc-btn--primary", !_localReady);
            _readyButton.EnableInClassList("pc-btn--secondary", _localReady);
        }

        private void OnReadyClicked()
        {
            _localReady = !_localReady;
            try
            {
                _playerManager?.SetPlayerReady(_localReady);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[MatchLobbyScreen] SetPlayerReady failed: {ex.Message}");
            }

            UpdateReadyButton();
            RefreshFromLobby();
        }

        private void OnLeaveClicked()
        {
            try
            {
                _lobbyManager?.LeaveMatchLobby();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[MatchLobbyScreen] LeaveMatchLobby failed: {ex.Message}");
            }
        }

        private void OnStartClicked()
        {
            if (_lobbyManager == null || !_lobbyManager.IsMatchLobbyOwner)
            {
                return;
            }

            try
            {
                _sessionContext?.GameStarter?.StartGame();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[MatchLobbyScreen] StartGame failed: {ex.Message}");
            }
        }
    }
}
