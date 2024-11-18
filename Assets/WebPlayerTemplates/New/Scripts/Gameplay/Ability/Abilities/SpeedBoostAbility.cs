// using System.Threading.Tasks;
// using Gameplay.Ability.Components;
// using Gameplay.Ability.Effects;
// using Unity.Netcode;
// using UnityEngine;
//
// namespace Gameplay.Ability.Abilities
// {
//     public class SpeedBoostAbility : BaseAbility
//     {
//         [Header("Speed Boost Settings")]
//         [SerializeField] private float boostDuration = 5f;
//         [SerializeField] private float speedMultiplier = 1.5f;
//
//         private const int ResourceCost = 35;
//
//         protected override async Task<bool> ExecuteAbility(Vector3 targetPosition, GameObject target)
//         {
//             if (!IsServer) return false;
//
//             // Apply speed boost effect
//             GameObject effectInstance = Instantiate(effectPrefab, Owner.transform.position, Quaternion.identity);
//             NetworkObject networkObject = effectInstance.GetComponent<NetworkObject>();
//             networkObject.Spawn();
//
//             BuffEffect effect = effectInstance.GetComponent<BuffEffect>();
//             effect.Initialize(Owner.gameObject, Owner.gameObject);
//             await effect.ApplyEffect();
//
//             await Task.Delay((int)(boostDuration * 1000));
//
//             networkObject.Despawn();
//             return true;
//         }
//
//         protected override int GetResourceCost() => ResourceCost;
//     }
// }
