using Playcenter;
using Unity.Netcode;

namespace RecipeRage.Net
{
    /// <summary>
    /// Carries the player's locked chef choice into the match. Set on the client
    /// before connect; server reads it when building the roster.
    /// </summary>
    public sealed class ChefSelectionSync : NetworkBehaviour
    {
        public readonly NetworkVariable<int> SelectedChefId = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                SelectedChefId.Value = (int)ServiceLocator.Get<IChefProgressionService>().GetSelectedChef();
            }
        }
    }
}
