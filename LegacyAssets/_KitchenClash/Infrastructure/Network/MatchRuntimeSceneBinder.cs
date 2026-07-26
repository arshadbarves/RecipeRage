using UnityEngine;
using KitchenClash.Infrastructure.Gameplay;
using KitchenClash.Application.Services;

namespace KitchenClash.Infrastructure.Network
{
    public class MatchRuntimeSceneBinder : MonoBehaviour, IKitchenSupportRuntime, IBotKitchenRuntime
    {
        private readonly System.Collections.Generic.List<Component> _botStations = new System.Collections.Generic.List<Component>();

        public System.Collections.Generic.IReadOnlyList<Component> Stations => _botStations;

        public void EnsureKitchenSupportStations()
        {
        }
    }
}
