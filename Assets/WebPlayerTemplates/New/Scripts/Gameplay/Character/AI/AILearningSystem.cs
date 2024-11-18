// using System.Collections.Generic;
// using UnityEngine;
//
// namespace Gameplay.Character.AI
// {
//     public class AILearningSystem
//     {
//         private const int STATE_SPACE_SIZE = 10;
//         private const int ACTION_SPACE_SIZE = 5;
//         private const float LEARNING_RATE = 0.1f;
//         private const float DISCOUNT_FACTOR = 0.9f;
//
//         private float[,] qTable;
//         private Dictionary<string, int> stateMapping;
//         private int lastState;
//         private int lastAction;
//
//         public AILearningSystem()
//         {
//             qTable = new float[STATE_SPACE_SIZE, ACTION_SPACE_SIZE];
//             stateMapping = new Dictionary<string, int>();
//             InitializeQTable();
//         }
//
//         private void InitializeQTable()
//         {
//             for (int i = 0; i < STATE_SPACE_SIZE; i++)
//             {
//                 for (int j = 0; j < ACTION_SPACE_SIZE; j++)
//                 {
//                     qTable[i, j] = Random.Range(0f, 0.1f);
//                 }
//             }
//         }
//
//         public void UpdateLearning(List<AIEvent> recentEvents, float reward)
//         {
//             int currentState = EncodeState(recentEvents);
//
//             // Q-learning update
//             float maxFutureQ = 0f;
//             for (int a = 0; a < ACTION_SPACE_SIZE; a++)
//             {
//                 maxFutureQ = Mathf.Max(maxFutureQ, qTable[currentState, a]);
//             }
//
//             // Update Q-value
//             qTable[lastState, lastAction] = qTable[lastState, lastAction] +
//                 LEARNING_RATE * (reward + DISCOUNT_FACTOR * maxFutureQ - qTable[lastState, lastAction]);
//
//             lastState = currentState;
//         }
//
//         public int ChooseAction(List<AIEvent> currentEvents)
//         {
//             int state = EncodeState(currentEvents);
//
//             // Epsilon-greedy action selection
//             if (Random.value < GetExplorationRate())
//             {
//                 lastAction = Random.Range(0, ACTION_SPACE_SIZE);
//             }
//             else
//             {
//                 lastAction = GetBestAction(state);
//             }
//
//             return lastAction;
//         }
//
//         private int GetBestAction(int state)
//         {
//             int bestAction = 0;
//             float bestValue = float.MinValue;
//
//             for (int a = 0; a < ACTION_SPACE_SIZE; a++)
//             {
//                 if (qTable[state, a] > bestValue)
//                 {
//                     bestValue = qTable[state, a];
//                     bestAction = a;
//                 }
//             }
//
//             return bestAction;
//         }
//
//         private float GetExplorationRate()
//         {
//             // Decrease exploration rate over time
//             return Mathf.Max(0.1f, 1.0f - Time.timeSinceLevelLoad / 300f);
//         }
//
//         private int EncodeState(List<AIEvent> events)
//         {
//             // Create a state hash based on recent events
//             string stateKey = GenerateStateKey(events);
//
//             if (!stateMapping.ContainsKey(stateKey))
//             {
//                 stateMapping[stateKey] = stateMapping.Count % STATE_SPACE_SIZE;
//             }
//
//             return stateMapping[stateKey];
//         }
//
//         private string GenerateStateKey(List<AIEvent> events)
//         {
//             // Simplified state encoding - can be expanded based on needs
//             return string.Join("|", events.GetRange(Mathf.Max(0, events.Count - 3),
//                 Mathf.Min(3, events.Count)));
//         }
//     }
// }
