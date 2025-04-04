using System;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeRage.Core.GameFramework.State.States
{
    /// <summary>
    /// State for loading game assets and initializing systems
    /// </summary>
    public class LoadingState : GameState
    {
        /// <summary>
        /// Event triggered when loading is complete
        /// </summary>
        public event Action OnLoadingComplete;
        
        /// <summary>
        /// List of loading tasks to complete
        /// </summary>
        private List<Func<bool>> _loadingTasks = new List<Func<bool>>();
        
        /// <summary>
        /// Current loading progress (0-1)
        /// </summary>
        public float Progress { get; private set; }
        
        /// <summary>
        /// Whether loading is complete
        /// </summary>
        public bool IsLoadingComplete { get; private set; }
        
        /// <summary>
        /// Initialize the list of allowed state transitions
        /// </summary>
        protected override void InitializeAllowedTransitions()
        {
            AllowTransitionTo<MainMenuState>();
            AllowTransitionTo<GameplayState>();
        }
        
        /// <summary>
        /// Called when the state is entered
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            
            // Reset loading state
            Progress = 0f;
            IsLoadingComplete = false;
            
            // Start loading process
            StartLoading();
        }
        
        /// <summary>
        /// Called when the state is updated
        /// </summary>
        public override void Update()
        {
            base.Update();
            
            if (IsLoadingComplete)
            {
                return;
            }
            
            // Check if all tasks are complete
            bool allTasksComplete = true;
            int completedTasks = 0;
            
            for (int i = 0; i < _loadingTasks.Count; i++)
            {
                if (_loadingTasks[i]())
                {
                    completedTasks++;
                }
                else
                {
                    allTasksComplete = false;
                }
            }
            
            // Update progress
            Progress = _loadingTasks.Count > 0 ? (float)completedTasks / _loadingTasks.Count : 1f;
            
            // Check if loading is complete
            if (allTasksComplete && !IsLoadingComplete)
            {
                IsLoadingComplete = true;
                OnLoadingComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// Start the loading process
        /// </summary>
        private void StartLoading()
        {
            Debug.Log("Starting loading process...");
            
            // Add loading tasks here
            // Example: _loadingTasks.Add(() => LoadAssets());
            
            // If no tasks, complete immediately
            if (_loadingTasks.Count == 0)
            {
                IsLoadingComplete = true;
                Progress = 1f;
                OnLoadingComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// Add a loading task to the list
        /// </summary>
        /// <param name="task">Function that returns true when the task is complete</param>
        public void AddLoadingTask(Func<bool> task)
        {
            if (task != null)
            {
                _loadingTasks.Add(task);
                
                // Recalculate progress
                if (IsLoadingComplete)
                {
                    IsLoadingComplete = false;
                    Progress = 0f;
                }
                else
                {
                    int completedTasks = 0;
                    foreach (var existingTask in _loadingTasks)
                    {
                        if (existingTask())
                        {
                            completedTasks++;
                        }
                    }
                    Progress = (float)completedTasks / _loadingTasks.Count;
                }
            }
        }
    }
}
