using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameSystem;
using GameSystem.Audio;
using GameSystem.Gameplay;
using GameSystem.Input;
using GameSystem.State;
using GameSystem.State.GameStates;
using GameSystem.UI;
using UnityEngine;
using Utilities;
using VContainer;
using VContainer.Unity;

namespace Core
{
    public class GameManager : NetworkSingleton<GameManager>, IStartable
    {
        [SerializeField] private ScriptableGameConfig gameConfig;

        private readonly Dictionary<Type, IGameSystem> _gameSystems = new Dictionary<Type, IGameSystem>();

        public event Action<GameState> OnGameStateChanged;

        [Inject] private IObjectResolver _container;

        public async void Start()
        {
            await InitializeSystems();
            GetSystem<StateSystem>().RequestGameStateChange(GameState.Splash);
        }

        private async Task InitializeSystems()
        {
            await AddSystemAsync<StateSystem>();
            await AddSystemAsync<InputSystem>();
            await AddSystemAsync<AudioSystem>();
            // await AddSystemAsync<CameraSystem>();
            // await AddSystemAsync<PlayerSystem>();
            // await AddSystemAsync<CookingSystem>();
            // await AddSystemAsync<BattleSystem>();
            // await AddSystemAsync<EnvironmentSystem>();
            await AddSystemAsync<UISystem>();
            // await AddSystemAsync<ProgressionSystem>();
            await AddSystemAsync<GameplaySystem>();
        }

        private async Task AddSystemAsync<T>() where T : IGameSystem
        {
            if (_container == null)
            {
                Debug.LogError("IObjectResolver is not initialized.");
                return;
            }

            T system = _container.Resolve<T>();
            if (system != null)
            {
                _gameSystems[typeof(T)] = system;
                await system.InitializeAsync();
            }
            else
            {
                Debug.LogError($"Failed to resolve system of type {typeof(T)}");
            }
        }

        public T GetSystem<T>() where T : IGameSystem
        {
            return (T)_gameSystems[typeof(T)];
        }

        private void Update()
        {
            foreach (var system in _gameSystems.Values)
            {
                system.Update();
            }
        }

        public override async void OnDestroy()
        {
            foreach (var system in _gameSystems.Values)
            {
                await system.CleanupAsync();
            }
        }

        public void InvokeGameStateChanged(GameState state)
        {
            OnGameStateChanged?.Invoke(state);
        }
    }
}