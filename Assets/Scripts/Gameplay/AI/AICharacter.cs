using UnityEngine;
using Unity.Netcode;
using RecipeRage.Gameplay.Core;

namespace RecipeRage.Gameplay.AI
{
    public class AICharacter : BaseNetworkCharacter
    {
        [Header("AI Settings")]
        [SerializeField] private float decisionInterval = 0.5f;
        [SerializeField] private float targetUpdateInterval = 0.2f;
        
        private IAIBrain _brain;
        private float _nextDecisionTime;
        private float _nextTargetUpdateTime;
        private Vector3? _currentTarget;
        private IInteractable _currentInteractable;

        protected override void InitializeServerSide()
        {
            base.InitializeServerSide();
            
            if (IsServer)
            {
                _brain = GetComponent<IAIBrain>();
                enabled = true;
            }
        }

        private void Update()
        {
            if (!IsServer) return;

            UpdateTargeting();
            MakeDecisions();
            ExecuteActions();
        }

        private void UpdateTargeting()
        {
            if (Time.time < _nextTargetUpdateTime) return;
            _nextTargetUpdateTime = Time.time + targetUpdateInterval;

            // Update target position and interactable based on AI brain's decision
            var targetInfo = _brain.GetTargetInfo();
            _currentTarget = targetInfo.Position;
            _currentInteractable = targetInfo.Interactable;
        }

        private void MakeDecisions()
        {
            if (Time.time < _nextDecisionTime) return;
            _nextDecisionTime = Time.time + decisionInterval;

            var decision = _brain.MakeDecision(new AIContext
            {
                Character = this,
                CurrentState = CurrentState.Value,
                CurrentHealth = CurrentHealth.Value,
                CurrentScore = CurrentScore.Value,
                HasTarget = _currentTarget.HasValue,
                HasInteractable = _currentInteractable != null
            });

            ExecuteDecision(decision);
        }

        private void ExecuteDecision(AIDecision decision)
        {
            switch (decision.Type)
            {
                case AIDecisionType.Move:
                    if (_currentTarget.HasValue)
                    {
                        UpdateStateServerRpc(CharacterState.Moving);
                        // Movement handled by NetworkTransform
                    }
                    break;

                case AIDecisionType.Interact:
                    if (_currentInteractable != null)
                    {
                        var netObj = _currentInteractable as NetworkObject;
                        if (netObj != null)
                        {
                            InteractServerRpc(netObj);
                        }
                    }
                    break;

                case AIDecisionType.Attack:
                    if (Combat.TryAttack())
                    {
                        UpdateStateServerRpc(CharacterState.Attacking);
                    }
                    break;

                case AIDecisionType.UseSpecial:
                    if (Combat.TryUseSpecialAbility())
                    {
                        // Special ability state and effects handled by Combat component
                    }
                    break;

                case AIDecisionType.Flee:
                    // Implement fleeing behavior
                    break;
            }
        }

        private void ExecuteActions()
        {
            // Additional continuous actions or behaviors
        }

        protected override bool TryStartInteraction(IInteractable interactable)
        {
            if (!base.TryStartInteraction(interactable)) return false;

            interactable.StartInteraction(this);
            return true;
        }
    }

    public interface IAIBrain
    {
        AITargetInfo GetTargetInfo();
        AIDecision MakeDecision(AIContext context);
    }

    public struct AITargetInfo
    {
        public Vector3? Position;
        public IInteractable Interactable;
    }

    public struct AIContext
    {
        public BaseNetworkCharacter Character;
        public CharacterState CurrentState;
        public float CurrentHealth;
        public int CurrentScore;
        public bool HasTarget;
        public bool HasInteractable;
    }

    public struct AIDecision
    {
        public AIDecisionType Type;
        public float Priority;
        public object Data;
    }

    public enum AIDecisionType
    {
        None,
        Move,
        Interact,
        Attack,
        UseSpecial,
        Flee
    }
}
