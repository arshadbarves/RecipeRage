using VContainer.Unity;
using Core.Logging;
using UnityEngine;

namespace Gameplay.GameModes
{
    /// <summary>
    /// Pure C# controller that manages the active game mode logic.
    /// Registered as ITickable in GameplayLifetimeScope.
    /// </summary>
    public class GameModeController : ITickable
    {
        private readonly IGameModeLogic _gameModeLogic;
        private readonly GamePhaseSync _phaseSync;
        private bool _matchStarted;

        public IGameModeLogic GameModeLogic => _gameModeLogic;
        public GamePhase CurrentPhase => _gameModeLogic?.CurrentPhase ?? GamePhase.Waiting;

        public GameModeController(IGameModeLogic logic, GamePhaseSync phaseSync)
        {
            _gameModeLogic = logic;
            _phaseSync = phaseSync;
            GameLogger.Log("GameModeController created");
        }

        public void StartMatch()
        {
            if (_matchStarted)
            {
                GameLogger.LogWarning("Match already started");
                return;
            }

            _matchStarted = true;
            _gameModeLogic?.OnMatchStart();
            SyncPhase();
            GameLogger.Log("Match started");
        }

        public void EndMatch()
        {
            if (!_matchStarted)
            {
                GameLogger.LogWarning("Match not started");
                return;
            }

            _gameModeLogic?.OnMatchEnd();
            _matchStarted = false;
            SyncPhase();
            GameLogger.Log("Match ended");
        }

        public void Tick()
        {
            if (!_matchStarted || _gameModeLogic == null)
                return;

            var previousPhase = _gameModeLogic.CurrentPhase;
            _gameModeLogic.Update(Time.deltaTime);

            // Sync phase if it changed
            if (_gameModeLogic.CurrentPhase != previousPhase)
            {
                SyncPhase();
            }
        }

        private void SyncPhase()
        {
            if (_phaseSync != null)
            {
                _phaseSync.SetPhase(_gameModeLogic.CurrentPhase);
            }
        }
    }
}
