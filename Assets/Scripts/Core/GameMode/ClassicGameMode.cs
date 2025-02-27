using System;
using System.Collections.Generic;
using System.Linq;
using RecipeRage.Core.Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RecipeRage.Core.GameMode
{
    /// <summary>
    ///     Classic game mode implementation where players compete to complete orders
    /// </summary>
    public class ClassicGameMode : GameModeBase
    {
        #region Events

        public event Action<Order> OnNewOrder;
        public event Action<Order> OnOrderCompleted;
        public event Action<Order> OnOrderFailed;
        public event Action<PlayerScore> OnScoreUpdated;

        #endregion

        #region Serialized Fields

        [Header("Classic Mode Settings"), SerializeField]
         private int _maxActiveOrders = 3;
        [SerializeField] private float _orderSpawnInterval = 15f;
        [SerializeField] private float _orderTimeLimit = 120f;
        [SerializeField] private int _pointsPerOrder = 100;
        [SerializeField] private int _perfectBonus = 50;
        [SerializeField] private int _failPenalty = 50;
        [SerializeField] private float _timePenaltyPerSecond = 10;

        [Header("Recipe Settings"), SerializeField]
         private List<RecipeData> _availableRecipes;

        #endregion

        #region Private Fields

        private float _nextOrderTime;
        private readonly List<Order> _activeOrders = new List<Order>();
        private readonly Dictionary<PlayerController, PlayerScore> _playerScores = new Dictionary<PlayerController, PlayerScore>();

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            _nextOrderTime = Time.time + _orderSpawnInterval;
        }

        protected override void Update()
        {
            base.Update();

            if (IsGameActive)
            {
                UpdateOrders();
                CheckForNewOrder();
            }
        }

        #endregion

        #region Public Methods

        public override bool Initialize()
        {
            if (!base.Initialize())
                return false;

            // Initialize player scores
            _playerScores.Clear();
            foreach (PlayerController player in Players)
            {
                _playerScores[player] = new PlayerScore();
            }

            return true;
        }

        public override bool StartGame()
        {
            if (!base.StartGame())
                return false;

            // Spawn initial orders
            SpawnOrder();

            return true;
        }

        public override void EndGame()
        {
            // Calculate final scores
            KeyValuePair<PlayerController, PlayerScore> winner = _playerScores.OrderByDescending(s => s.Value.TotalScore).First();
            Debug.Log($"Game Over! Winner: Player {Players.IndexOf(winner.Key)} with {winner.Value.TotalScore} points!");

            base.EndGame();
        }

        /// <summary>
        ///     Called when a player completes an order
        /// </summary>
        /// <param name="player">Player who completed the order</param>
        /// <param name="order">Completed order</param>
        /// <param name="isPerfect">Whether the order was completed perfectly</param>
        public void CompleteOrder(PlayerController player, Order order, bool isPerfect)
        {
            if (!_activeOrders.Contains(order))
                return;

            _activeOrders.Remove(order);

            // Calculate score
            float timeBonus = Mathf.Max(0, order.TimeRemaining);
            int score = _pointsPerOrder;

            if (isPerfect)
            {
                score += _perfectBonus;
            }

            // Update player score
            if (_playerScores.TryGetValue(player, out PlayerScore playerScore))
            {
                playerScore.OrdersCompleted++;
                playerScore.TotalScore += score;
                if (isPerfect) playerScore.PerfectOrders++;

                OnScoreUpdated?.Invoke(playerScore);
            }

            OnOrderCompleted?.Invoke(order);
        }

        /// <summary>
        ///     Called when an order fails (times out)
        /// </summary>
        /// <param name="order">Failed order</param>
        public void FailOrder(Order order)
        {
            if (!_activeOrders.Contains(order))
                return;

            _activeOrders.Remove(order);
            OnOrderFailed?.Invoke(order);
        }

        #endregion

        #region Private Methods

        private void UpdateOrders()
        {
            for (int i = _activeOrders.Count - 1; i >= 0; i--)
            {
                Order order = _activeOrders[i];
                order.TimeRemaining -= Time.deltaTime;

                if (order.TimeRemaining <= 0)
                {
                    FailOrder(order);
                }
            }
        }

        private void CheckForNewOrder()
        {
            if (Time.time >= _nextOrderTime && _activeOrders.Count < _maxActiveOrders)
            {
                SpawnOrder();
                _nextOrderTime = Time.time + _orderSpawnInterval;
            }
        }

        private void SpawnOrder()
        {
            // Ensure we have recipes to choose from
            if (_availableRecipes == null || _availableRecipes.Count == 0)
                return;

            // Create new order
            RecipeData recipe = _availableRecipes[Random.Range(0, _availableRecipes.Count)];
            Order order = new Order {
                Recipe = recipe, TimeRemaining = _orderTimeLimit, TimeLimit = _orderTimeLimit
            };

            _activeOrders.Add(order);
            OnNewOrder?.Invoke(order);
        }

        #endregion
    }

    /// <summary>
    ///     Represents a player's score in the game
    /// </summary>
    public class PlayerScore
    {
        public int TotalScore { get; set; }
        public int OrdersCompleted { get; set; }
        public int PerfectOrders { get; set; }
        public int FailedOrders { get; set; }
    }

    /// <summary>
    ///     Represents an active order in the game
    /// </summary>
    public class Order
    {
        public RecipeData Recipe { get; set; }
        public float TimeRemaining { get; set; }
        public float TimeLimit { get; set; }
        public float ProgressPercentage => 1f - TimeRemaining / TimeLimit;
    }

    /// <summary>
    ///     Scriptable Object containing recipe data
    /// </summary>
    [CreateAssetMenu(fileName = "New Recipe", menuName = "RecipeRage/Recipe Data")]
    public class RecipeData : ScriptableObject
    {
        public string recipeName;
        public Sprite recipeIcon;
        public List<IngredientStep> steps;
        public int pointValue;
        public float difficultyMultiplier = 1f;
    }

    /// <summary>
    ///     Represents a step in a recipe
    /// </summary>
    [Serializable]
    public class IngredientStep
    {
        public string ingredientName;
        public CookingMethod cookingMethod;
        public float cookingTime;
        public bool requiresPerfectTiming;
    }

    /// <summary>
    ///     Available cooking methods
    /// </summary>
    public enum CookingMethod
    {
        Chop,
        Fry,
        Boil,
        Bake,
        Mix
    }
}