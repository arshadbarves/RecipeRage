using Gameplay.Ability.Components;
using Gameplay.Ability.Effects;
using Gameplay.Character.Stats;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Gameplay.Character.Controller
{
    [RequireComponent(typeof(CharacterStats)),
     RequireComponent(typeof(AbilityComponent)),
     RequireComponent(typeof(NetworkTransform)),
     RequireComponent(typeof(CharacterController)),
     RequireComponent(typeof(NetworkAnimator))]
    public class BaseCharacter : NetworkBehaviour, IDamageable
    {
        protected const float RotationSpeed = 20.0f;
        protected const float GravityValue = -9.81f;

        // Animation hashes
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsHolding = Animator.StringToHash("IsHolding");
        protected static readonly int IsCooking = Animator.StringToHash("IsCooking");

        [Header("Character Settings"), SerializeField]
        private Transform holdPoint;
        [SerializeField] protected Transform interactionSource;

        private readonly NetworkVariable<bool> _isBot = new NetworkVariable<bool>();

        // Network state
        private readonly NetworkVariable<CharacterState> _netState = new NetworkVariable<CharacterState>();
        private readonly NetworkVariable<ushort> _teamId = new NetworkVariable<ushort>();

        // Components
        protected AbilityComponent AbilityComponent;
        public CharacterController CharacterController;
        protected CharacterStats CharacterStats;
        protected bool GroundedPlayer;
        protected bool IsHoldingObject;
        protected bool IsInteracting;

        protected Vector2 MoveInput;

        // protected EventManager EventManager;
        protected NetworkAnimator NetworkAnimator;

        protected Vector3 PlayerVelocity;

        public ushort TeamId => _teamId.Value;
        public bool IsBot => _isBot.Value;
        public CharacterStats Stats => CharacterStats;

        protected virtual void Update()
        {
            if (!IsOwner) return;

            UpdateState();
        }

        public virtual void TakeDamage(DamageInfo damageInfo)
        {
            if (!IsServer) return;

            CharacterStats.ApplyDamage(damageInfo);

            OnDamageTakenClientRpc(damageInfo.Amount, damageInfo.Source.GetComponent<NetworkObject>().NetworkObjectId);

            // Publish damage event
            // EventManager.Publish(new GameEvents.CharacterDamagedEvent(this, damageInfo));
        }

        // [Inject]
        // public void Construct(EventManager eventManager)
        // {
        //     EventManager = eventManager;
        // }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                InitializeComponents();
                RegisterServerCallbacks();
            }

            if (IsClient)
            {
                RegisterClientCallbacks();
                // InitializeUI(); // TODO: Implement UI, health bars, etc.
            }
        }

        private void InitializeComponents()
        {
            AbilityComponent = GetComponent<AbilityComponent>();
            CharacterStats = GetComponent<CharacterStats>();
            NetworkAnimator = GetComponent<NetworkAnimator>();
            CharacterController = GetComponent<CharacterController>();

            if (holdPoint == null)
            {
                Debug.LogWarning("Hold point not set for character: " + name);
            }

            if (interactionSource == null)
            {
                interactionSource = transform;
            }
        }

        private void RegisterServerCallbacks()
        {
            CharacterStats.OnHealthDepleted += HandleDeath;
            CharacterStats.OnStatChanged += HandleStatChanged;
        }

        private void RegisterClientCallbacks()
        {
            _netState.OnValueChanged += OnStateChanged;
            _teamId.OnValueChanged += OnTeamChanged;
        }

        public virtual void AssignToTeam(ushort newTeamId)
        {
            if (!IsServer) return;
            _teamId.Value = newTeamId;
        }

        public virtual void SetAsBot(bool value)
        {
            if (!IsServer) return;
            _isBot.Value = value;
        }

        protected virtual void UpdateState()
        {
            CharacterState newState = DetermineState();
            if (newState != _netState.Value)
            {
                UpdateStateServerRpc(newState);
            }
        }

        // TODO: Do we use this?
        protected virtual CharacterState DetermineState()
        {
            // TODO: Do we need this?
            // if (CharacterStats.IsDead) return CharacterState.Dead;
            // if (CharacterStats.IsStunned) return CharacterState.Stunned;
            // if (NetworkTransform.IsMoving()) return CharacterState.Moving;
            return CharacterState.Idle;
        }

        protected virtual void HandleDeath()
        {
            if (!IsServer) return;

            // Notify clients
            OnDeathClientRpc();

            // Publish death event
            // EventManager.Publish(new GameEvents.CharacterDeathEvent(this));

            // Handle respawn or game over logic
            HandleCharacterDeath();
        }

        protected virtual void HandleStatChanged(StatType statType, float newValue)
        {
            if (!IsServer) return;
        }

        protected virtual void HandleCharacterDeath()
        {
            // Override in derived classes for specific death behavior
        }

        protected virtual void SpawnHitEffects(float damage)
        {
            // Override in derived classes for specific hit effects
        }

        protected virtual void SpawnDeathEffects()
        {
            // Override in derived classes for specific death effects
        }

        protected virtual void OnStateChanged(CharacterState previousState, CharacterState newState)
        {
            // Handle state change visuals and effects
            UpdateCharacterVisuals(newState);
        }

        protected virtual void OnTeamChanged(ushort previousTeam, ushort newTeam)
        {
            // Update team-based visuals
            UpdateTeamVisuals(newTeam);
        }

        protected virtual void UpdateCharacterVisuals(CharacterState newState)
        {
            // Override in derived classes for specific visual updates
        }

        protected virtual void UpdateTeamVisuals(ushort newTeam)
        {
            // Override in derived classes for team-specific visual updates
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                UnregisterServerCallbacks();
            }

            if (IsClient)
            {
                UnregisterClientCallbacks();
            }

            base.OnNetworkDespawn();
        }

        private void UnregisterServerCallbacks()
        {
            if (CharacterStats != null)
            {
                CharacterStats.OnHealthDepleted -= HandleDeath;
                CharacterStats.OnStatChanged -= HandleStatChanged;
            }
        }

        private void UnregisterClientCallbacks()
        {
            _netState.OnValueChanged -= OnStateChanged;
            _teamId.OnValueChanged -= OnTeamChanged;
        }

        #region Item Handling

        public bool CanHoldItem()
        {
            return !IsHoldingObject;
        }

        public Transform GetHoldPoint()
        {
            return holdPoint;
        }

        #endregion

        #region RPCs

        [ServerRpc]
        protected void UpdateStateServerRpc(CharacterState newState)
        {
            _netState.Value = newState;
        }

        [ClientRpc]
        protected void OnDamageTakenClientRpc(float damage, ulong sourceId)
        {
            // Play hit animation
            if (NetworkAnimator != null)
            {
                NetworkAnimator.SetTrigger("Hit");
            }

            // Spawn hit effects
            SpawnHitEffects(damage);
        }

        [ClientRpc]
        protected void OnDeathClientRpc()
        {
            // Play death animation
            if (NetworkAnimator != null)
            {
                NetworkAnimator.SetTrigger("Death");
            }

            // Spawn death effects
            SpawnDeathEffects();
        }

        #endregion
    }

    public enum CharacterState
    {
        None,
        Idle,
        Moving,
        Attacking,
        Cooking,
        Stunned,
        Dead
    }
}