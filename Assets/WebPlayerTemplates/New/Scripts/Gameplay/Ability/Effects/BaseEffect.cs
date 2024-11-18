using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Ability.Effects
{
    public abstract class BaseEffect : NetworkBehaviour
    {
        [SerializeField] protected float duration;
        [SerializeField] protected bool isPersistent;

        protected GameObject Source;
        protected GameObject Target;

        public virtual void Initialize(GameObject source, GameObject target)
        {
            Source = source;
            Target = target;
        }

        public virtual async Task ApplyEffect()
        {
            if (IsServer)
            {
                OnEffectStart();

                if (duration > 0)
                {
                    await Task.Delay((int)(duration * 1000));
                    OnEffectEnd();
                }
            }
        }

        protected virtual void OnEffectStart() { }
        protected virtual void OnEffectEnd() { }
    }
}
