using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Gameplay.Data;
using GameSystem.Gameplay;
using GameSystem.Progression;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.GameMode
{
    public class CookingClashBaseGameMode : BaseGameMode
    {
        [SerializeField] private int maxSimultaneousOrders = 3;
        private readonly List<Order> _activeOrders = new List<Order>();
        private float _remainingTime;

        private NetworkVariable<int> Score { get; } = new NetworkVariable<int>();

        private void Awake()
        {
            _remainingTime = LevelData.GameDuration;
            Score.OnValueChanged += OnScoreChanged;
        }

        private void Update()
        {
            if (CurrentState != GameModeState.InGame) return;

            if (!IsServer) return;

            _remainingTime -= Time.deltaTime;
            if (_remainingTime <= 0)
            {
                EndGameServerRpc();
            }

            foreach (Order order in _activeOrders.ToList())
            {
                order.TimeRemaining -= Time.deltaTime;
                if (order.TimeRemaining <= 0)
                {
                    _activeOrders.Remove(order);
                    ExpireOrderClientRpc(order.OrderId);
                }
            }
        }

        private void OnScoreChanged(int previousScore, int newScore)
        {
            // GameManager.Instance.GetSystem<UISystem>().UpdateScore(newScore);
            // GameManager.Instance.GetSystem<AudioSystem>().PlaySfx(AudioType.Score);
        }

        public override void StartGame()
        {
            throw new NotImplementedException();
        }

        public override void GameOver()
        {
            CurrentState = GameModeState.GameOver;
            // GameManager.Instance.GetSystem<UISystem>().ShowPanel<GameOverPanel>();
            // GameManager.Instance.GetSystem<AudioSystem>().PlaySfx(AudioType.GameOver);
            GameManager.Instance.GetSystem<GameplaySystem>().SetGameMode(null); // TODO: Set this when player leaves, to avoid null reference during play again
            GameManager.Instance.GetSystem<ProgressionSystem>().UpdatePlayerProgression(StatType.Trophies, Score.Value);
        }

        private IEnumerator GenerateOrders()
        {
            while (_remainingTime > 0)
            {
                if (_activeOrders.Count < maxSimultaneousOrders)
                {
                    RecipeData randomRecipe =
                        LevelData.AvailableRecipes[Random.Range(0, LevelData.AvailableRecipes.Count)];
                    string orderId = Guid.NewGuid().ToString();
                    SpawnOrderClientRpc(orderId, randomRecipe.RecipeId);
                }

                yield return new WaitForSeconds(Random.Range(LevelData.MinOrderDelay, LevelData.MaxOrderDelay));
            }
        }

        [ClientRpc]
        private void SpawnOrderClientRpc(string orderId, string recipeId)
        {
            // GameManager.Instance.GetSystem<UISystem>().ShowPanel<OrderPanel>(order);
            // GameManager.Instance.GetSystem<AudioSystem>().PlaySfx(AudioType.OrderReceived);
            RecipeData recipeData =
                GameManager.Instance.GetSystem<GameplaySystem>().CurrentGameMode.GetRecipeData(recipeId);
            Order newOrder = new Order(orderId, recipeData);
            _activeOrders.Add(newOrder);
        }

        [ServerRpc]
        private void DeliverOrderServerRpc(string orderId, List<string> ingredientIds)
        {
            Order order = _activeOrders.Find(o => o.OrderId == orderId);
            RecipeData recipeData = GameManager.Instance.GetSystem<GameplaySystem>().CurrentGameMode.GetRecipeData(order.RecipeId);
            bool isCorrect = ingredientIds.All(ingredientId => recipeData.Ingredients.Any(ingredient => ingredient.Ingredient.IngredientId == ingredientId));
            if (isCorrect)
            {
                // TODO: game mode stats that how many delivered, failed, star player based on the number of delivered orders, and etc, to show that in network sync
                Score.Value += recipeData.Reward;
                DeliverOrderClientRpc(orderId);
            }
            else
            {
                FailOrderClientRpc(orderId);
            }
        }

        [ClientRpc]
        private void DeliverOrderClientRpc(string orderId)
        {
            // GameManager.Instance.GetSystem<AudioSystem>().PlaySfx(AudioType.OrderDelivered);
            // GameManager.Instance.GetSystem<UISystem>().HidePanel<OrderPanel>();
            // GameManager.Instance.GetSystem<UISystem>().ShowPanel<SuccessPanel>();

            // TODO: Update the player stats for achievements, progression, and etc note to do this at game over state
        }

        [ClientRpc]
        private void FailOrderClientRpc(string orderId)
        {
            // GameManager.Instance.GetSystem<AudioSystem>().PlaySfx(AudioType.OrderFailed);
            // GameManager.Instance.GetSystem<UISystem>().HidePanel<OrderPanel>();
            // GameManager.Instance.GetSystem<UISystem>().ShowPanel<FailPanel>();
        }

        [ClientRpc]
        private void ExpireOrderClientRpc(string orderId)
        {
            // GameManager.Instance.GetSystem<AudioSystem>().PlaySfx(AudioType.OrderExpired);
            // GameManager.Instance.GetSystem<UISystem>().HidePanel<OrderPanel>();
            // GameManager.Instance.GetSystem<UISystem>().ShowPanel<FailPanel>();
        }
    }
}