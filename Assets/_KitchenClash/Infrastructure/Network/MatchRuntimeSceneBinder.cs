using UnityEngine;

namespace KitchenClash.Infrastructure.Network
{
    public class MatchRuntimeSceneBinder : MonoBehaviour, Application.Services.IKitchenSupportRuntime, Application.Services.IBotKitchenRuntime
    {
        private readonly System.Collections.Generic.List<Component> _botStations = new System.Collections.Generic.List<Component>();

        public System.Collections.Generic.IReadOnlyList<Component> Stations => _botStations;

        public void EnsureKitchenSupportStations()
        {
        }
    }
}
