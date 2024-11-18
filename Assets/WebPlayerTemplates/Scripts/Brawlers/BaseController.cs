using Brawlers.Abilities;
using Brawlers.Stats;
using Gameplay.Data;
using Unity.Netcode;
using UnityEngine;

namespace Brawlers
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class BaseController : NetworkBehaviour
    {
        [SerializeField] private BrawlerData brawlerData;

        // Network Variables for Item/Plate Management
        private readonly NetworkVariable<IngredientType> _heldItem = new NetworkVariable<IngredientType>();
        private readonly NetworkVariable<bool> _isDirtyPlate = new NetworkVariable<bool>();
        private readonly NetworkVariable<bool> _isHoldingPlate = new NetworkVariable<bool>();
        protected Animator Animator;

        // Components
        protected CharacterController CharacterController;

        // Player Stats
        public float Health { get; private set; }
        public float Mana { get; }
        public Team CurrentTeam { get; private set; }

        public bool IsStunned { get; set; } = true;

        public BrawlerData BrawlerData => brawlerData;

        protected virtual void Awake()
        {
            CharacterController = GetComponent<CharacterController>();
            Animator = GetComponent<Animator>();

            if (brawlerData == null)
            {
                Debug.LogError($"{nameof(BrawlerData)} is not assigned on {gameObject.name}.");
                return;
            }

            Health = brawlerData.MaxHealth;
        }

        protected virtual void Update()
        {
            if (!IsStunned)
            {
                Move();
                UpdateAbilities();
            }
        }

        /// <summary>
        ///     Abstract method to handle player movement.
        /// </summary>
        protected abstract void Move();

        /// <summary>
        ///     Ability system abstract methods.
        /// </summary>
        protected abstract void PrimaryAttack();
        protected abstract void SecondaryAttack();
        protected abstract void PrimaryInteract();
        protected abstract void SecondaryInteract();

        public abstract Vector3 GetMoveDirection();

        /// <summary>
        ///     Iterates through all abilities and checks if they can be used, then activates them.
        /// </summary>
        protected virtual void UpdateAbilities()
        {
            foreach (Ability ability in brawlerData.Abilities)
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

        /// <summary>
        ///     Abstract method to check if an ability should be used.
        /// </summary>
        protected abstract bool ShouldUseAbility(Ability ability);

        /// <summary>
        ///     Finds a valid target for an ability, if required.
        /// </summary>
        protected BaseController FindTargetForAbility(Ability ability)
        {
            // Implement target-finding logic, e.g., based on range or team.
            return null;
        }

        /// <summary>
        ///     Reduces the health of the controller and checks for death.
        /// </summary>
        public virtual void TakeDamage(float amount)
        {
            Health -= amount;

            if (Health <= 0)
            {
                Die();
            }
        }

        /// <summary>
        ///     Handles player death logic.
        /// </summary>
        protected virtual void Die()
        {
            // Customize death handling, such as dropping items or playing death animations.
            Destroy(gameObject);
        }

        /// <summary>
        ///     Determines if the provided BaseController is an enemy.
        /// </summary>
        public bool IsEnemy(BaseController other)
        {
            // Modify logic as needed to define what constitutes an enemy.
            return other != null && other.CurrentTeam != CurrentTeam;
        }

        /// <summary>
        ///     Assigns the player to a specified team.
        /// </summary>
        public void AssignToTeam(Team team)
        {
            CurrentTeam?.RemoveMember(this);
            CurrentTeam = team;
            team.AddMember(this);
        }

        #region Item/Plate Management (Networked)

        [ServerRpc]
        public void PickUpItemServerRpc(IngredientType item)
        {
            _heldItem.Value = item;
        }

        [ServerRpc]
        public void DropItemServerRpc()
        {
            _heldItem.Value = IngredientType.None; // Set to 'None' when no item is held
        }

        [ServerRpc]
        public void PickUpPlateServerRpc(bool isDirty = false)
        {
            _isHoldingPlate.Value = true;
            _isDirtyPlate.Value = isDirty;
        }

        [ServerRpc]
        public void DropPlateServerRpc()
        {
            _isHoldingPlate.Value = false;
            _isDirtyPlate.Value = false;
        }

        /// <summary>
        ///     Checks if the player is holding any item.
        /// </summary>
        public bool IsHoldingItem()
        {
            return _isHoldingPlate.Value || _heldItem.Value != IngredientType.None;
        }

        /// <summary>
        ///     Returns the currently held ingredient.
        /// </summary>
        public IngredientType GetHeldIngredient()
        {
            return _heldItem.Value;
        }

        /// <summary>
        ///     Checks if the player is holding a plate.
        /// </summary>
        public bool IsHoldingPlate()
        {
            return _isHoldingPlate.Value;
        }

        /// <summary>
        ///     Checks if the player is holding a dirty plate.
        /// </summary>
        public bool IsHoldingDirtyPlate()
        {
            return _isHoldingPlate.Value && _isDirtyPlate.Value;
        }

        /// <summary>
        ///     Client-side method to pick up an item.
        /// </summary>
        public void PickUpItem(IngredientType item)
        {
            PickUpItemServerRpc(item);
        }

        /// <summary>
        ///     Client-side method to drop the held item.
        /// </summary>
        public void DropItem()
        {
            DropItemServerRpc();
        }

        /// <summary>
        ///     Client-side method to pick up a plate.
        /// </summary>
        public void PickUpPlate()
        {
            PickUpPlateServerRpc();
        }

        /// <summary>
        ///     Client-side method to pick up a dirty plate.
        /// </summary>
        public void PickUpDirtyPlate()
        {
            PickUpPlateServerRpc(true);
        }

        /// <summary>
        ///     Client-side method to drop the plate.
        /// </summary>
        public void DropPlate()
        {
            DropPlateServerRpc();
        }

        #endregion
    }
}