using System.Collections.Generic;
using System.Text;
using KitchenClash.Application;
using Playcenter.GameFlow;
using Playcenter.Services;
using Playcenter.UI;
using Playcenter.UI.Toolkit;
using UnityEngine.UIElements;
using VContainer;

namespace KitchenClash.Presentation.Screens
{
    /// <summary>
    /// Brawl-style match-found beat: two-team VS layout + load bar.
    /// Flow port drives show/hide and progress; screen is presentation only.
    /// Motion-safe: layout is static; USS transitions honor .pc-reduce-motion.
    /// </summary>
    [UIScreen(UIScreenCategory.Overlay, "Screens/MatchIntroScreenTemplate")]
    public class MatchIntroScreen : BaseUIScreen
    {
        [Inject] private IAppFlow _appFlow;
        [Inject] private ISessionContext _sessionContext;

        private Label _statusLabel;
        private Label _modeLabel;
        private Label _mapLabel;
        private VisualElement _loadFill;
        private Label _hintLabel;
        private Label _teamANames;
        private Label _teamBNames;

        protected override void OnInitialize()
        {
            _statusLabel = GetElement<Label>("status-label");
            _modeLabel = GetElement<Label>("mode-label");
            _mapLabel = GetElement<Label>("map-label");
            _loadFill = GetElement<VisualElement>("load-fill");
            _hintLabel = GetElement<Label>("hint-label");
            _teamANames = GetElement<Label>("team-a-names");
            _teamBNames = GetElement<Label>("team-b-names");
            TransitionType = UITransitionType.Fade;
        }

        protected override void OnShow()
        {
            base.OnShow();
            ApplyResolvedInfo(_appFlow?.Context?.LastMatchResolved);
            BindTeamsFromMatchLobby();
            SetProgress(0.08f);
        }

        public void ApplyResolvedInfo(MatchResolvedInfo info)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = info != null && info.FilledWithBots
                    ? "MATCH READY"
                    : "MATCH FOUND";
            }

            if (_modeLabel != null)
            {
                string mode = !string.IsNullOrEmpty(info?.ModeId)
                    ? FormatId(info.ModeId)
                    : "RECIPE RAGE";

                if (info != null && info.TeamSize > 0)
                {
                    mode = $"{info.TeamSize}V{info.TeamSize}  {mode}";
                }

                _modeLabel.text = mode;
            }

            if (_mapLabel != null)
            {
                string map = !string.IsNullOrEmpty(info?.MapId)
                    ? FormatId(info.MapId)
                    : "LOADING ARENA";
                _mapLabel.text = map;
            }

            if (_hintLabel != null)
            {
                _hintLabel.text = "GET READY";
            }
        }

        public void SetProgress(float normalized01)
        {
            if (_loadFill == null)
            {
                return;
            }

            float clamped = UnityEngine.Mathf.Clamp01(normalized01);
            _loadFill.style.width = Length.Percent(clamped * 100f);
        }

        public void SetHint(string hint)
        {
            if (_hintLabel != null)
            {
                _hintLabel.text = hint ?? string.Empty;
            }
        }

        private void BindTeamsFromMatchLobby()
        {
            ILobbyManager lobbyManager = _sessionContext?.LobbyManager;
            LobbyInfo lobby = lobbyManager?.CurrentMatchLobby ?? lobbyManager?.CurrentPartyLobby;
            List<PlayerInfo> players = lobby?.Players;

            if (players == null || players.Count == 0)
            {
                if (_teamANames != null) _teamANames.text = "—";
                if (_teamBNames != null) _teamBNames.text = "—";
                return;
            }

            var teamA = new List<string>();
            var teamB = new List<string>();
            for (int i = 0; i < players.Count; i++)
            {
                PlayerInfo player = players[i];
                if (player == null)
                {
                    continue;
                }

                string name = string.IsNullOrEmpty(player.DisplayName) ? "Player" : player.DisplayName;
                if (player.IsBot)
                {
                    name += " (BOT)";
                }

                if (player.Team == TeamId.TeamB)
                {
                    teamB.Add(name);
                }
                else
                {
                    teamA.Add(name);
                }
            }

            if (_teamANames != null) _teamANames.text = JoinNames(teamA);
            if (_teamBNames != null) _teamBNames.text = JoinNames(teamB);
        }

        private static string JoinNames(List<string> names)
        {
            if (names == null || names.Count == 0)
            {
                return "—";
            }

            var sb = new StringBuilder();
            for (int i = 0; i < names.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append('\n');
                }

                sb.Append(names[i]);
            }

            return sb.ToString();
        }

        private static string FormatId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return string.Empty;
            }

            return id.Replace('_', ' ').Replace('-', ' ').ToUpperInvariant();
        }
    }
}
