using System;
using UnityEngine;

namespace Brawlers.Abilities
{
    [Serializable]
    public abstract class Ability : MonoBehaviour
    {
        // The name of the ability (e.g., "Fireball", "Healing Wave").
        public string Name { get; set; }

        // Time in seconds that the ability must wait before being used again.
        public float Cooldown { get; set; }

        // Time when the ability was last used.
        private float _lastUsedTime;

        // Duration in seconds that the ability lasts once activated.
        public float Duration { get; set; }

        // The maximum distance the ability can affect or reach (range).
        public float Range { get; set; }

        // --- DAMAGE RELATED PROPERTIES ---

        public float Damage { get; set; }
        public float DamageOverTime { get; set; }
        public float DamageOverTimeDuration { get; set; }

        public float DamageReduction { get; set; }
        public float DamageReductionDuration { get; set; }

        // --- HEALING RELATED PROPERTIES ---

        public float Heal { get; set; }
        public float HealOverTime { get; set; }
        public float HealOverTimeDuration { get; set; }

        public float HealReduction { get; set; }
        public float HealReductionDuration { get; set; }

        // --- MOVEMENT-RELATED PROPERTIES ---

        public float SpeedBoost { get; set; }
        public float SpeedBoostDuration { get; set; }

        public float Slow { get; set; }
        public float SlowDuration { get; set; }

        // --- CROWD CONTROL RELATED PROPERTIES ---

        public bool Stun { get; set; }
        public float StunDuration { get; set; }

        // Whether this ability requires a target to be activated.
        public bool RequiresTarget { get; set; } = true;
        public bool IsAreaEffect { get; set; } = false;

        // Abstract method that must be implemented to define the ability's activation logic.
        public abstract void Activate(BaseController caster, BaseController target = null);

        // Method that checks if the ability can be used in the current game state.
        public bool CanUse(BaseController caster, BaseController target = null)
        {
            bool isOnCooldown = Time.time - _lastUsedTime < Cooldown;
            bool hasEnoughResources = caster.Mana >= 0;
            bool hasValidTarget = !RequiresTarget || (target != null &&
                                                      Vector3.Distance(caster.transform.position,
                                                          target.transform.position) <= Range &&
                                                      target.IsEnemy(caster) && !target.IsStunned);

            bool canUse = !isOnCooldown && hasEnoughResources && hasValidTarget;
            return canUse;
        }
    }
}