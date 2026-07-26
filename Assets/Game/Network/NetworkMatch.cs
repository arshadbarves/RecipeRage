using Playcenter;
using Playcenter.Services;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Net
{
    /// <summary>
    /// Server-authoritative match. One MatchController per team on the server;
    /// clients mirror progress from NetworkVariables for HUD.
    /// </summary>
    public sealed class NetworkMatch : NetworkBehaviour
    {
        public readonly NetworkVariable<int> Seed = new NetworkVariable<int>();
        public readonly NetworkVariable<int> TeamACompleted = new NetworkVariable<int>();
        public readonly NetworkVariable<int> TeamBCompleted = new NetworkVariable<int>();
        public readonly NetworkVariable<float> RemainingSeconds = new NetworkVariable<float>();
        public readonly NetworkVariable<bool> IsOver = new NetworkVariable<bool>();

        private MatchController _teamA;
        private MatchController _teamB;
        private ITimeService _time;

        public override void OnNetworkSpawn()
        {
            _time = ServiceLocator.Get<ITimeService>();

            if (IsServer)
            {
                var catalog = ServiceLocator.Get<IRecipeCatalog>();
                var config = ServiceLocator.Get<IConfigService>();
                var eventBus = ServiceLocator.Get<IEventBus>();

                Seed.Value = Random.Range(0, int.MaxValue);
                _teamA = new MatchController(catalog, config, eventBus, _time);
                _teamB = new MatchController(catalog, config, eventBus, _time);
                // Both teams get the SAME list: same seed, same catalog.
                _teamA.StartMatch(Seed.Value);
                _teamB.StartMatch(Seed.Value);
            }
        }

        private void Update()
        {
            if (!IsServer || _teamA == null || IsOver.Value)
            {
                return;
            }

            _teamA.TickServer(_time.DeltaTime);
            _teamB.TickServer(_time.DeltaTime);

            TeamACompleted.Value = _teamA.CompletedCount;
            TeamBCompleted.Value = _teamB.CompletedCount;
            RemainingSeconds.Value = _teamA.RemainingSeconds;

            if (_teamA.IsMatchOver || _teamB.IsMatchOver || RemainingSeconds.Value <= 0f)
            {
                IsOver.Value = true;
                int winner = TeamACompleted.Value == TeamBCompleted.Value
                    ? -1
                    : TeamACompleted.Value > TeamBCompleted.Value ? 0 : 1;
                MatchEndedClientRpc(winner, TeamACompleted.Value, TeamBCompleted.Value);
            }
        }

        /// <summary>Server entry: a serving station validated a plate for a team.</summary>
        public void ServerServePlate(int teamId, Plate plate)
        {
            var match = teamId == 0 ? _teamA : _teamB;
            match.TryServePlate(plate);
        }

        [ClientRpc]
        private void MatchEndedClientRpc(int winnerTeam, int teamARecipes, int teamBRecipes)
        {
            ServiceLocator.Get<IEventBus>().Publish(
                new MatchEndedEvent(winnerTeam == 0, teamARecipes, teamBRecipes));
        }
    }
}
