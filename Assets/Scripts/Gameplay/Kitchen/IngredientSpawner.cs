using UnityEngine;
using Unity.Netcode;
using System;
using RecipeRage.Gameplay.Core;
using RecipeRage.Gameplay.Core.States;
using RecipeRage.Gameplay.Player;
using Unity.Collections;

namespace RecipeRage.Gameplay.Kitchen
{
    public class IngredientSpawner : NetworkBehaviour, IInteractable
    {
        [Header("Spawn Settings")]
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private int maxSpawnedItems = 3;
        [SerializeField] private float interactionRadius = 1.5f;
        [SerializeField] private IngredientType[] availableIngredients;

        [Header("Quality Settings")]
        [SerializeField] private float baseQuality = 1f;
        [SerializeField] private float qualityVariance = 0.2f;
        [SerializeField] private float qualityDecayRate = 0.1f;

        // Network state
        private readonly NetworkVariable<StateType> _currentState = new();
        private readonly NetworkList<SpawnedIngredient> _spawnedIngredients;
        private readonly NetworkVariable<float> _nextSpawnTime = new();

        // Events
        public event Action<SpawnedIngredient> OnIngredientSpawned;
        public event Action<SpawnedIngredient> OnIngredientCollected;
        public event Action<StateType> OnStateChanged;

        public IngredientSpawner()
        {
            _spawnedIngredients = new NetworkList<SpawnedIngredient>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _currentState.Value = StateType.Available;
                _nextSpawnTime.Value = Time.time + spawnInterval;
            }

            _currentState.OnValueChanged += (_, newState) => OnStateChanged?.Invoke(newState);
            _spawnedIngredients.OnListChanged += HandleSpawnedIngredientsChanged;
        }

        private void Update()
        {
            if (!IsServer) return;

            // Update ingredient quality
            for (int i = _spawnedIngredients.Count - 1; i >= 0; i--)
            {
                var ingredient = _spawnedIngredients[i];
                ingredient.Quality = Mathf.Max(0.1f, ingredient.Quality - qualityDecayRate * Time.deltaTime);
                _spawnedIngredients[i] = ingredient;
            }

            // Check for spawn
            if (Time.time >= _nextSpawnTime.Value && _spawnedIngredients.Count < maxSpawnedItems)
            {
                SpawnIngredient();
                _nextSpawnTime.Value = Time.time + spawnInterval;
            }
        }

        private void SpawnIngredient()
        {
            if (!IsServer) return;

            var ingredient = new SpawnedIngredient
            {
                Id = NetworkObjectId + "_" + Time.time,
                Type = availableIngredients[UnityEngine.Random.Range(0, availableIngredients.Length)],
                Quality = baseQuality + UnityEngine.Random.Range(-qualityVariance, qualityVariance),
                SpawnTime = Time.time
            };

            _spawnedIngredients.Add(ingredient);
            OnIngredientSpawned?.Invoke(ingredient);
        }

        public bool CanInteract(BaseNetworkCharacter character)
        {
            if (_currentState.Value != StateType.Available) return false;
            if (_spawnedIngredients.Count == 0) return false;

            float distance = Vector3.Distance(transform.position, character.transform.position);
            return distance <= interactionRadius;
        }

        public void StartInteraction(BaseNetworkCharacter character)
        {
            if (!IsServer) return;
            if (!CanInteract(character)) return;

            // Get the best quality ingredient
            int bestIndex = 0;
            float bestQuality = float.MinValue;
            for (int i = 0; i < _spawnedIngredients.Count; i++)
            {
                if (_spawnedIngredients[i].Quality > bestQuality)
                {
                    bestQuality = _spawnedIngredients[i].Quality;
                    bestIndex = i;
                }
            }

            // Try to add to player's inventory
            var inventory = character.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                var ingredient = _spawnedIngredients[bestIndex];
                var item = new InventoryItem
                {
                    ItemId = (int)ingredient.Type,
                    Type = ItemType.Ingredient,
                    Quality = ingredient.Quality
                };

                if (inventory.TryAddItem(item))
                {
                    OnIngredientCollected?.Invoke(ingredient);
                    _spawnedIngredients.RemoveAt(bestIndex);
                }
            }
        }

        public void CompleteInteraction(BaseNetworkCharacter character)
        {
            // Instant interaction, no completion needed
        }

        public void CancelInteraction(BaseNetworkCharacter character)
        {
            // Instant interaction, no cancellation needed
        }

        private void HandleSpawnedIngredientsChanged(NetworkListEvent<SpawnedIngredient> changeEvent)
        {
            // Update visual representation of available ingredients
        }
    }

    public struct SpawnedIngredient : INetworkSerializable, IEquatable<SpawnedIngredient>
    {
        public FixedString32Bytes Id;
        public IngredientType Type;
        public float Quality;
        public float SpawnTime;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref Quality);
            serializer.SerializeValue(ref SpawnTime);
        }

        public bool Equals(SpawnedIngredient other)
        {
            return Id == other.Id &&
                   Type == other.Type &&
                   Quality.Equals(other.Quality) &&
                   SpawnTime.Equals(other.SpawnTime);
        }
    }

    public enum IngredientType
    {
        Tomato,
        Onion,
        Garlic,
        Carrot,
        Potato,
        Meat,
        Fish,
        Rice,
        Pasta,
        Cheese,
        Egg,
        Spices
    }
}
