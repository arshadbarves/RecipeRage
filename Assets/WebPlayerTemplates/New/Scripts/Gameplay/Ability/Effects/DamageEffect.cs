using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Ability.Effects
{
    public class DamageEffect : BaseEffect
    {

        public enum DamageType
        {
            Physical,
            Fire,
            Ice,
            Poison
        }
        [SerializeField] private float damageAmount;
        [SerializeField] private DamageType damageType;

        private readonly NetworkVariable<float> _currentDamage = new NetworkVariable<float>();

        protected override void OnEffectStart()
        {
            if (!IsServer) return;

            _currentDamage.Value = damageAmount;
            ApplyDamage();

            if (!isPersistent)
            {
                NetworkObject.Despawn();
            }
        }

        private void ApplyDamage()
        {
            if (Target.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(new DamageInfo {
                    Amount = _currentDamage.Value, Type = damageType, Source = Source
                });
            }
        }
    }

    public struct DamageInfo
    {
        public float Amount;
        public DamageEffect.DamageType Type;
        public GameObject Source;
    }

    public interface IDamageable
    {
        void TakeDamage(DamageInfo damageInfo);
    }
}