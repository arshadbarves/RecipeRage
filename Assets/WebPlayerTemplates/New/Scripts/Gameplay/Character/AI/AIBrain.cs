// using System.Collections.Generic;
// using UnityEngine;
//
// namespace Gameplay.Character.AI
// {
//     public class AIBrain
//     {
//         private AIBehaviorSettings settings;
//         private AICharacter character;
//         private AIMemory memory;
//
//         private Dictionary<AIActionType, float> actionWeights;
//         private List<AIActionStrategy> strategies;
//
//         public AIBrain(AIBehaviorSettings settings)
//         {
//             this.settings = settings;
//             InitializeWeights();
//             InitializeStrategies();
//         }
//
//         public void Initialize(AICharacter character, AIMemory memory)
//         {
//             this.character = character;
//             this.memory = memory;
//         }
//
//         public AIAction DecideNextAction(AIWorldState worldState)
//         {
//             // Get all possible actions
//             List<AIAction> possibleActions = GeneratePossibleActions(worldState);
//
//             // Score each action based on current state and strategies
//             float bestScore = float.MinValue;
//             AIAction bestAction = null;
//
//             foreach (var action in possibleActions)
//             {
//                 float score = EvaluateAction(action, worldState);
//                 if (score > bestScore)
//                 {
//                     bestScore = score;
//                     bestAction = action;
//                 }
//             }
//
//             return bestAction ?? new IdleAction();
//         }
//
//         private float EvaluateAction(AIAction action, AIWorldState worldState)
//         {
//             float score = 0f;
//
//             // Base weight for action type
//             score += actionWeights[action.Type];
//
//             // Evaluate using strategies
//             foreach (var strategy in strategies)
//             {
//                 score += strategy.EvaluateAction(action, worldState) * strategy.Weight;
//             }
//
//             // Apply personality modifiers
//             score *= GetPersonalityModifier(action.Type);
//
//             return score;
//         }
//
//         private float GetPersonalityModifier(AIActionType actionType)
//         {
//             switch (actionType)
//             {
//                 case AIActionType.Combat:
//                     return settings.Aggressiveness;
//                 case AIActionType.Cooking:
//                     return settings.Productivity;
//                 case AIActionType.Support:
//                     return settings.Teamwork;
//                 default:
//                     return 1f;
//             }
//         }
//
//         private List<AIAction> GeneratePossibleActions(AIWorldState worldState)
//         {
//             List<AIAction> actions = new List<AIAction>();
//
//             // Generate combat actions
//             if (worldState.HasHostileNearby)
//             {
//                 actions.Add(new CombatAction(worldState.NearestHostile));
//             }
//
//             // Generate cooking actions
//             if (worldState.HasIncompleteOrders)
//             {
//                 actions.Add(new CookingAction(worldState.HighestPriorityOrder));
//             }
//
//             // Generate support actions
//             if (worldState.HasAlliesNeedingHelp)
//             {
//                 actions.Add(new SupportAction(worldState.NearestAllyNeedingHelp));
//             }
//
//             return actions;
//         }
//
//         private void InitializeWeights()
//         {
//             actionWeights = new Dictionary<AIActionType, float>
//             {
//                 { AIActionType.Combat, 1.0f },
//                 { AIActionType.Cooking, 1.0f },
//                 { AIActionType.Support, 0.8f },
//                 { AIActionType.Gather, 0.6f },
//                 { AIActionType.Idle, 0.1f }
//             };
//         }
//
//         private void InitializeStrategies()
//         {
//             strategies = new List<AIActionStrategy>
//             {
//                 new SurvivalStrategy(0.8f),
//                 new ObjectiveStrategy(1.0f),
//                 new TeamworkStrategy(0.6f),
//                 new OpportunityStrategy(0.4f)
//             };
//         }
//
//         public void UpdateWeights(Dictionary<AIActionType, float> newWeights)
//         {
//             foreach (var kvp in newWeights)
//             {
//                 if (actionWeights.ContainsKey(kvp.Key))
//                 {
//                     actionWeights[kvp.Key] = kvp.Value;
//                 }
//             }
//         }
//     }
// }
