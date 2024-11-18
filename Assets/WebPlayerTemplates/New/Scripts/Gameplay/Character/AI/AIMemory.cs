// using System;
// using System.Collections.Generic;
// using UnityEngine;
//
// namespace Gameplay.Character.AI
// {
//     public class AIMemory
//     {
//         private const float MEMORY_DECAY_RATE = 0.1f;
//         private const float MEMORY_THRESHOLD = 0.2f;
//
//         private Dictionary<EntityType, Dictionary<ulong, MemoryEntity>> entityMemory;
//         private List<AIEvent> recentEvents;
//         private float learningScore;
//
//         public AIMemory()
//         {
//             entityMemory = new Dictionary<EntityType, Dictionary<ulong, MemoryEntity>>();
//             recentEvents = new List<AIEvent>();
//
//             foreach (EntityType type in Enum.GetValues(typeof(EntityType)))
//             {
//                 entityMemory[type] = new Dictionary<ulong, MemoryEntity>();
//             }
//         }
//
//         public void UpdateEntityPosition(EntityType type, ulong id, Vector2 position)
//         {
//             if (!entityMemory[type].TryGetValue(id, out var entity))
//             {
//                 entity = new MemoryEntity();
//                 entityMemory[type][id] = entity;
//             }
//
//             entity.position = position;
//             entity.lastSeenTime = Time.time;
//             entity.certainty = 1f;
//         }
//
//         public void UpdateMemory(float deltaTime)
//         {
//             foreach (var typeDict in entityMemory.Values)
//             {
//                 List<ulong> removeIds = new List<ulong>();
//
//                 foreach (var kvp in typeDict)
//                 {
//                     kvp.Value.certainty -= MEMORY_DECAY_RATE * deltaTime;
//                     if (kvp.Value.certainty < MEMORY_THRESHOLD)
//                     {
//                         removeIds.Add(kvp.Key);
//                     }
//                 }
//
//                 foreach (var id in removeIds)
//                 {
//                     typeDict.Remove(id);
//                 }
//             }
//         }
//
//         public void RecordEvent(AIEvent aiEvent)
//         {
//             recentEvents.Add(aiEvent);
//             if (recentEvents.Count > 100) // Keep last 100 events
//             {
//                 recentEvents.RemoveAt(0);
//             }
//         }
//
//         public List<AIEvent> GetRecentEvents()
//         {
//             return recentEvents;
//         }
//
//         public void UpdateLearningScore(float delta)
//         {
//             learningScore += delta;
//         }
//
//         public float GetLearningScore()
//         {
//             return learningScore;
//         }
//
//         public MemoryEntity GetNearestEntity(EntityType type, Vector2 position, float maxDistance = float.MaxValue)
//         {
//             MemoryEntity nearest = null;
//             float nearestDistance = maxDistance;
//
//             foreach (var entity in entityMemory[type].Values)
//             {
//                 float distance = Vector2.Distance(position, entity.position);
//                 if (distance < nearestDistance)
//                 {
//                     nearest = entity;
//                     nearestDistance = distance;
//                 }
//             }
//
//             return nearest;
//         }
//
//         public class MemoryEntity
//         {
//             public Vector2 position;
//             public float lastSeenTime;
//             public float certainty;
//         }
//
//         public enum EntityType
//         {
//             Character,
//             Interactable,
//             Item,
//             Hazard
//         }
//     }
// }
