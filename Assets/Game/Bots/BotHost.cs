using System.Collections.Generic;
using Playcenter;
using Playcenter.Services;
using UnityEngine;

namespace RecipeRage.Bots
{
    /// <summary>
    /// Server-side bot host. Ticks all brains within the frame budget and owns
    /// bot spawn/despawn for the match. Registered by GameplayCompositionRoot;
    /// driven from its Update only on the server.
    /// </summary>
    public sealed class BotHost : MonoBehaviour
    {
        [SerializeField] private BotController _botPrefab;

        private readonly List<BotBrain> _brains = new List<BotBrain>(6);
        private BotBudget _budget;
        private float _tickInterval = 0.1f; // 10Hz
        private float _tickTimer;

        private void Start()
        {
            var config = ServiceLocator.Get<IConfigService>();
            _budget = new BotBudget(config.Get("bot_budget_ms", 2));
        }

        public void RegisterBrain(BotBrain brain)
        {
            _brains.Add(brain);
        }

        public void SetDifficulty(float dwellScale)
        {
            foreach (var brain in _brains)
            {
                brain.SetDifficulty(dwellScale);
            }
        }

        private void Update()
        {
            _tickTimer -= Time.deltaTime;
            if (_tickTimer > 0f)
            {
                return;
            }
            _tickTimer = _tickInterval;

            _budget.BeginTick();
            foreach (var brain in _brains)
            {
                if (!_budget.TryConsume(200)) // reserve 200µs per brain
                {
                    break;
                }
                brain.Tick();
            }
        }
    }
}
