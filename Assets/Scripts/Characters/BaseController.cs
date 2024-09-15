using Characters.Abilities;
using Characters.Stats;
using Unity.Netcode;
using UnityEngine;

namespace Characters
{
    public abstract class BaseController : NetworkBehaviour
    {
        [SerializeField] private CharacterStats characterStats;
        
        protected CharacterController CharacterController;
        protected Animator Animator;

        public float Health { get; private set; }
        public Team CurrentTeam { get; private set; }
        
        // TODO: Implement the following properties
        public float Mana { get; private set; }
        public bool stun = true;
        
        public CharacterStats CharacterStats => characterStats;
        
        protected virtual void Awake()
        {
            CharacterController = GetComponent<CharacterController>();
            Animator = GetComponent<Animator>();
            Health = characterStats.MaxHealth;
        }
        
        protected virtual void Update()
        {
            Move();
            UpdateAbilities();
        }

        protected abstract void Move();
        protected abstract void Attack();
        protected abstract void Interact();
    
        public abstract Vector3 GetMoveDirection();

        protected virtual void UpdateAbilities()
        {
            foreach (Ability ability in characterStats.Abilities)
            {
                BaseController target = null;

                if (ability.RequiresTarget)
                {
                    target = FindTargetForAbility(ability);
                }

                if (ability.CanUse(this, target) && ShouldUseAbility(ability))
                {
                    ability.Activate(this, target);
                }
            }
        }

        protected abstract bool ShouldUseAbility(Ability ability);

        protected BaseController FindTargetForAbility(Ability ability)
        {
            // Implement logic to find the appropriate target for the ability
            return null;
        }

        public virtual void TakeDamage(float amount)
        {
            Health -= amount;
            if (Health <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            // Implement death logic
            Destroy(gameObject);
        }
        
        public bool IsEnemy(BaseController other)
        {
            // Implement logic to determine if the other controller is an enemy
            return other != null && other != this;
        }
        
        public void AssignToTeam(Team team)
        {
            CurrentTeam?.RemoveMember(this);
            CurrentTeam = team;
            team.AddMember(this);
        }
    }
}
