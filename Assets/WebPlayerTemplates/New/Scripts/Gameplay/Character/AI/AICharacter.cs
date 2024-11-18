// using Gameplay.Character.Controller;
// using Unity.Netcode;
// using UnityEngine;
// using UnityEngine.AI;
//
// namespace Gameplay.Character.AI
// {
//     public class AICharacter : BaseCharacter
//     {
//         [Header("AI Settings"), SerializeField]
//          private float decisionInterval = 0.25f;
//         [SerializeField] private float viewRadius = 10f;
//         [SerializeField] private float viewAngle = 120f;
//
//         // AI Components
//         private NavMeshAgent _agent;
//         private readonly NetworkVariable<AIState> _aiState = new NetworkVariable<AIState>();
//         [SerializeField] private AIBehaviorSettings _behaviorSettings;
//         private AIBrain _brain;
//         private AIAction _currentAction;
//         private AILearningSystem _learningSystem;
//         private AIMemory _memory;
//
//         // State tracking
//         private float _nextDecisionTime;
//
//         protected override void Awake()
//         {
//             base.Awake();
//
//             // Initialize AI components
//             _agent = gameObject.AddComponent<NavMeshAgent>();
//             _agent.updateRotation = false;
//             _agent.updateUpAxis = false;
//
//             _memory = new AIMemory();
//             _brain = new AIBrain(_behaviorSettings);
//             _learningSystem = new AILearningSystem();
//         }
//
//         protected override void Update()
//         {
//             base.Update();
//
//             if (!IsServer) return;
//
//             UpdateAI();
//         }
//
//         private void OnDrawGizmosSelected()
//         {
//             // Draw view radius
//             Gizmos.color = Color.yellow;
//             Gizmos.DrawWireSphere(transform.position, viewRadius);
//
//             // Draw view angle
//             Vector3 viewAngleA = DirFromAngle(-viewAngle * 0.5f);
//             Vector3 viewAngleB = DirFromAngle(viewAngle * 0.5f);
//
//             Gizmos.color = Color.red;
//             Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
//             Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
//         }
//
//         public override void OnNetworkSpawn()
//         {
//             base.OnNetworkSpawn();
//
//             if (IsServer)
//             {
//                 // Initialize AI behavior
//                 _brain.Initialize(this, _memory);
//                 StartAI();
//             }
//         }
//
//         private void StartAI()
//         {
//             _aiState.Value = AIState.Active;
//             _nextDecisionTime = Time.time;
//         }
//
//         private void UpdateAI()
//         {
//             if (_aiState.Value != AIState.Active) return;
//
//             // Update perception
//             UpdatePerception();
//
//             // Make decisions at regular intervals
//             if (Time.time >= _nextDecisionTime)
//             {
//                 MakeDecision();
//                 _nextDecisionTime = Time.time + decisionInterval;
//             }
//
//             // Execute current action
//             if (_currentAction != null)
//             {
//                 ExecuteCurrentAction();
//             }
//         }
//
//         private void UpdatePerception()
//         {
//             // Update vision
//             Collider2D[] visibleObjects = Physics2D.OverlapCircleAll(transform.position, viewRadius);
//             foreach (Collider2D obj in visibleObjects)
//             {
//                 if (IsInFieldOfView(obj.transform.position))
//                 {
//                     ProcessPerceivedObject(obj);
//                 }
//             }
//
//             // Update memory decay
//             _memory.UpdateMemory(Time.deltaTime);
//
//             // Update learning system
//             _learningSystem.UpdateState(_memory.GetCurrentState());
//         }
//
//         private void ProcessPerceivedObject(Collider2D obj)
//         {
//             // Process different types of objects
//             if (obj.TryGetComponent<BaseCharacter>(out BaseCharacter character))
//             {
//                 ProcessCharacter(character);
//             }
//             else if (obj.TryGetComponent<IInteractable>(out var interactable))
//             {
//                 ProcessInteractable(interactable);
//             }
//             else if (obj.TryGetComponent<IPickup>(out var pickup))
//             {
//                 ProcessPickup(pickup);
//             }
//         }
//
//         private void ProcessCharacter(BaseCharacter character)
//         {
//             if (character == this) return;
//
//             AIMemoryEntity entity = new AIMemoryEntity {
//                 Type = AIEntityType.Character,
//                 Position = character.transform.position,
//                 TeamId = character.TeamId,
//                 IsHostile = character.TeamId != TeamId,
//                 LastSeenTime = Time.time,
//                 NetworkId = character.NetworkObjectId
//             };
//
//             _memory.UpdateEntity(entity);
//         }
//
//         private void MakeDecision()
//         {
//             // Get current state evaluation
//             var worldState = _memory.GetWorldState();
//
//             // Get brain's decision
//             AIAction newAction = _brain.DecideNextAction(worldState);
//
//             // Use learning system to potentially modify the decision
//             newAction = _learningSystem.ModifyAction(newAction, worldState);
//
//             // Change current action if needed
//             if (ShouldChangeAction(newAction))
//             {
//                 ChangeAction(newAction);
//             }
//         }
//
//         private bool ShouldChangeAction(AIAction newAction)
//         {
//             if (_currentAction == null) return true;
//             if (newAction.Priority > _currentAction.Priority) return true;
//             if (_currentAction.IsComplete) return true;
//             if (!_currentAction.IsValid) return true;
//
//             return false;
//         }
//
//         private void ChangeAction(AIAction newAction)
//         {
//             // Clean up current action
//             if (_currentAction != null)
//             {
//                 _currentAction.OnActionComplete -= HandleActionComplete;
//                 _currentAction.Cleanup();
//             }
//
//             // Set up new action
//             _currentAction = newAction;
//             _currentAction.OnActionComplete += HandleActionComplete;
//             _currentAction.Initialize(this);
//
//             // Update AI state
//             UpdateAIStateServerRpc(newAction.GetAIState());
//
//             // Notify learning system
//             _learningSystem.OnActionChanged(newAction);
//         }
//
//         private void ExecuteCurrentAction()
//         {
//             if (_currentAction != null && _currentAction.IsValid)
//             {
//                 _currentAction.Execute();
//
//                 // Update navigation
//                 if (_currentAction.RequiresMovement)
//                 {
//                     UpdateNavigation(_currentAction.GetTargetPosition());
//                 }
//             }
//         }
//
//         private void UpdateNavigation(Vector2 targetPosition)
//         {
//             if (_agent.isActiveAndEnabled)
//             {
//                 _agent.SetDestination(targetPosition);
//
//                 // Update animation based on movement
//                 if (networkAnimator != null)
//                 {
//                     networkAnimator.SetFloat("Speed", _agent.velocity.magnitude);
//                 }
//             }
//         }
//
//         private void HandleActionComplete(AIAction action, bool success)
//         {
//             // Update learning system with action result
//             _learningSystem.UpdateActionResult(action, success);
//
//             // Clear current action if it's the completed one
//             if (action == _currentAction)
//             {
//                 _currentAction = null;
//             }
//         }
//
//         public void TakeOverControl()
//         {
//             if (!IsServer) return;
//
//             _aiState.Value = AIState.Inactive;
//             if (_currentAction != null)
//             {
//                 _currentAction.Cleanup();
//                 _currentAction = null;
//             }
//
//             // Save learning state for future use
//             _learningSystem.SaveState();
//         }
//
//         public void ResumeControl()
//         {
//             if (!IsServer) return;
//
//             _aiState.Value = AIState.Active;
//             _nextDecisionTime = Time.time;
//
//             // Load previous learning state
//             _learningSystem.LoadState();
//         }
//
//                 #region RPCs
//
//         [ServerRpc]
//         private void UpdateAIStateServerRpc(AIState state)
//         {
//             _aiState.Value = state;
//         }
//
//                 #endregion
//
//         private Vector3 DirFromAngle(float angleInDegrees)
//         {
//             angleInDegrees += transform.eulerAngles.z;
//             return new Vector3(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad),
//                 Mathf.Sin(angleInDegrees * Mathf.Deg2Rad));
//         }
//     }
//
//     public enum AIState
//     {
//         Inactive,
//         Active,
//         Combat,
//         Cooking,
//         Following,
//         Searching
//     }
// }
