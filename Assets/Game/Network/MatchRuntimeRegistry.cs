using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace RecipeRage.Net
{
    /// <summary>
    /// Scene object lookup for the running match. Stations register on spawn.
    /// The ONLY way gameplay systems find scene objects — no FindObjectOfType.
    /// </summary>
    public sealed class MatchRuntimeRegistry : MonoBehaviour
    {
        private readonly Dictionary<ulong, NetworkBehaviour> _objects = new Dictionary<ulong, NetworkBehaviour>(32);
        private readonly List<CookingStation> _cookingStations = new List<CookingStation>(8);
        private readonly List<StationBase> _allStations = new List<StationBase>(16);

        public IReadOnlyList<CookingStation> CookingStations => _cookingStations;
        public IReadOnlyList<StationBase> AllStations => _allStations;

        public void Register(NetworkBehaviour behaviour)
        {
            _objects[behaviour.NetworkObjectId] = behaviour;
            if (behaviour is NetworkCookingStation cooking && cooking.Station != null)
            {
                _cookingStations.Add(cooking.Station);
            }
        }

        public void Unregister(NetworkBehaviour behaviour)
        {
            _objects.Remove(behaviour.NetworkObjectId);
            if (behaviour is NetworkCookingStation cooking && cooking.Station != null)
            {
                _cookingStations.Remove(cooking.Station);
            }
        }

        public void RegisterStation(StationBase station) => _allStations.Add(station);
        public void UnregisterStation(StationBase station) => _allStations.Remove(station);

        public bool TryGet(ulong networkObjectId, out NetworkBehaviour behaviour)
        {
            return _objects.TryGetValue(networkObjectId, out behaviour);
        }
    }
}
