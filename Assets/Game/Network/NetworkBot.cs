using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace RecipeRage.Net
{
    /// <summary>
    /// Server-owned bot replication. Bots are network objects but NOT NGO player
    /// objects — no client ever owns one. Transform syncs for client rendering;
    /// all decisions happen server-side in BotBrain.
    /// </summary>
    [RequireComponent(typeof(NetworkTransform))]
    public sealed class NetworkBot : NetworkBehaviour
    {
        public readonly NetworkVariable<int> TeamId = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public readonly NetworkVariable<int> ChefId = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }
}
