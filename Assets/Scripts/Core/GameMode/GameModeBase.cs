using System;
using System.Collections.Generic;
using RecipeRage.Core.Player;
using UnityEngine;

namespace RecipeRage.Core.GameMode
{
    /// <summary>
    ///     Base class for all game modes in RecipeRage
    /// </summary>
    public abstract class GameModeBase : MonoBehaviour
    {
        #region Events

        public event Action<GameState> OnGameStateChanged;
        public event Action<float> OnTimeChanged;
        public event Action OnGameStart;
        public event Action OnGameEnd;

        #endregion

        #region Properties

        public GameState CurrentState { get; private set; }
        public float RemainingTime { get; private set; }
        public bool IsGameActive { get; private set; }
        public List<PlayerController> Players { get; } = new List<PlayerController>();

        #endregion

        #region Serialized Fields

        [Header("Game Settings"), SerializeField]
         protected float gameDuration = 300f; // 5 minutes default
        [SerializeField] protected int minPlayers = 2;
        [SerializeField] protected int maxPlayers = 4;
        [SerializeField] protected bool useTeams;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            CurrentState = GameState.WaitingForPlayers;
            RemainingTime = gameDuration;
            IsGameActive = false;
        }

        protected virtual void Update()
        {
            if (IsGameActive)
            {
                UpdateGameTime();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Initializes the game mode with specific settings
        /// </summary>
        /// <returns>True if initialization was successful</returns>
        public virtual bool Initialize()
        {
            if (Players.Count < minPlayers)
            {
                Debug.LogWarning($"Cannot start game: Not enough players. Minimum required: {minPlayers}");
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Starts the game if all conditions are met
        /// </summary>
        /// <returns>True if game started successfully</returns>
        public virtual bool StartGame()
        {
            if (!Initialize())
                return false;

            IsGameActive = true;
            ChangeGameState(GameState.InProgress);
            OnGameStart?.Invoke();
            return true;
        }

        /// <summary>
        ///     Ends the game and calculates final results
        /// </summary>
        public virtual void EndGame()
        {
            IsGameActive = false;
            ChangeGameState(GameState.Completed);
            OnGameEnd?.Invoke();
        }

        /// <summary>
        ///     Adds a player to the game
        /// </summary>
        /// <param name="player">Player to add</param>
        /// <returns>True if player was added successfully</returns>
        public virtual bool AddPlayer(PlayerController player)
        {
            if (Players.Count >= maxPlayers)
            {
                Debug.LogWarning("Cannot add player: Maximum player count reached");
                return false;
            }

            Players.Add(player);
            return true;
        }

        /// <summary>
        ///     Removes a player from the game
        /// </summary>
        /// <param name="player">Player to remove</param>
        public virtual void RemovePlayer(PlayerController player)
        {
            Players.Remove(player);

            if (IsGameActive && Players.Count < minPlayers)
            {
                EndGame();
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Changes the current game state and notifies listeners
        /// </summary>
        /// <param name="newState">New game state</param>
        protected virtual void ChangeGameState(GameState newState)
        {
            CurrentState = newState;
            OnGameStateChanged?.Invoke(newState);
        }

        /// <summary>
        ///     Updates the game timer and checks for time-based events
        /// </summary>
        protected virtual void UpdateGameTime()
        {
            if (RemainingTime > 0)
            {
                RemainingTime -= Time.deltaTime;
                OnTimeChanged?.Invoke(RemainingTime);

                if (RemainingTime <= 0)
                {
                    RemainingTime = 0;
                    EndGame();
                }
            }
        }

        #endregion
    }

    /// <summary>
    ///     Represents the current state of the game
    /// </summary>
    public enum GameState
    {
        WaitingForPlayers,
        InProgress,
        Paused,
        Completed
    }
}