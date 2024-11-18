using Core.GameFramework.Event.Core;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Core.GameFramework.State
{
    public abstract class GameState : IState
    {
        protected EventManager EventManager;
        protected NetworkManager NetworkManager;
        protected IStateMachine StateMachine;
        protected bool IsInitialized { get; private set; }

        protected bool IsHost =>
            NetworkManager != null && NetworkManager.IsHost;

        protected bool IsClient =>
            NetworkManager != null && NetworkManager.IsClient;

        protected bool IsServer =>
            NetworkManager != null && NetworkManager.IsServer;

        public virtual void Enter()
        {
            if (!IsInitialized)
            {
                Debug.LogError($"State {GetType().Name} was not properly initialized. Ensure Construct is called.");
                return;
            }
            OnStateEnter();
        }

        public virtual void Exit()
        {
            if (!IsInitialized) return;
            
            OnStateExit();
            EventManager.Unsubscribe(this);
        }

        protected virtual void OnStateEnter() { }
        protected virtual void OnStateExit() { }
        
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void HandleInput() { }

        [Inject]
        public void Construct(
            IStateMachine stateMachine,
            EventManager eventManager,
            NetworkManager networkManager)
        {
            StateMachine = stateMachine;
            EventManager = eventManager;
            NetworkManager = networkManager;
            IsInitialized = true;
        }
    }
}