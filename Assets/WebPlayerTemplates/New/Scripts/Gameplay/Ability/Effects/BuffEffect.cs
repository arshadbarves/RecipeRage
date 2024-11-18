// using Gameplay.Character.Stats;
// using Unity.Netcode;
// using UnityEngine;
//
// namespace Gameplay.Ability.Effects
// {
//     public class BuffEffect : BaseEffect
//     {
//         [SerializeField] private float statModifier;
//         [SerializeField] private StatType statType;
//
//         private readonly NetworkVariable<float> _currentBonus = new NetworkVariable<float>();
//
//         protected override void OnEffectStart()
//         {
//             if (!IsServer) return;
//
//             _currentBonus.Value = statModifier;
//             ApplyStatModifier();
//         }
//
//         protected override void OnEffectEnd()
//         {
//             if (!IsServer) return;
//
//             RemoveStatModifier();
//             if (NetworkObject != null)
//             {
//                 NetworkObject.Despawn();
//             }
//         }
//
//         private void ApplyStatModifier()
//         {
//             if (Target.TryGetComponent(out CharacterStats stats))
//             {
//                 stats.AddModifier(statType, _currentBonus.Value);
//             }
//         }
//
//         private void RemoveStatModifier()
//         {
//             if (Target.TryGetComponent(out CharacterStats stats))
//             {
//                 stats.RemoveModifier(statType, _currentBonus.Value);
//             }
//         }
//     }
// }
