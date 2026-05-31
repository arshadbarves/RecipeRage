using System.Collections.Generic;

namespace KitchenClash.Infrastructure.Network
{
    public class PlayerNetworkManager : IPlayerNetworkManager
    {
        private readonly Dictionary<ulong, IPlayerController> _players = new Dictionary<ulong, IPlayerController>();

        public void RegisterPlayer(ulong clientId, IPlayerController player)
        {
            if (player == null || _players.ContainsKey(clientId))
            {
                return;
            }

            _players[clientId] = player;
        }

        public void UnregisterPlayer(ulong clientId)
        {
            if (!_players.ContainsKey(clientId))
            {
                return;
            }

            _players.Remove(clientId);
        }

        public IPlayerController GetPlayer(ulong clientId) => _players.TryGetValue(clientId, out IPlayerController p) ? p : null;
        public IReadOnlyList<IPlayerController> GetAllPlayers() => new List<IPlayerController>(_players.Values);
        public int GetPlayerCount() => _players.Count;
        public bool IsPlayerRegistered(ulong clientId) => _players.ContainsKey(clientId);
    }

    public interface IPlayerNetworkManager
    {
        void RegisterPlayer(ulong clientId, IPlayerController player);
        void UnregisterPlayer(ulong clientId);
        IPlayerController GetPlayer(ulong clientId);
        IReadOnlyList<IPlayerController> GetAllPlayers();
        int GetPlayerCount();
        bool IsPlayerRegistered(ulong clientId);
    }
}
