using KitchenClash.Application.Config;
using KitchenClash.Domain;
using KitchenClash.Domain.Enums;
using KitchenClash.Infrastructure.Network.Cooking;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Playcenter.Shell;
using Playcenter.Services;

namespace KitchenClash.Infrastructure.Network.Stations
{
    /// <summary>
    /// v2 autonomous cooking station.
    ///
    /// Lifecycle (server-authoritative, replicated via NetworkVariables):
    ///   IDLE  →  player completes prime tap-burst  →  PRIMED
    ///   PRIMED → server tick starts cook timer     →  COOKING
    ///   COOKING → cook timer expires               →  READY
    ///   READY → grace window expires               →  BURNT
    ///   BURNT → lockout expires                    →  IDLE
    ///
    ///   COOKING → Controller sabotage ability hits →  BURNT (early)
    ///   READY   → priming team collects            →  IDLE  (dish handed to player)
    ///
    /// The player NEVER stands at the station after priming.
    /// Collection is a quick Interact() action — not a hold.
    /// </summary>
    [RequireComponent(typeof(StationNetworkController))]
    public class AutonomousCookingStation : StationBase
    {
        [Header("Autonomous Station")]
        [SerializeField] private int _recipeTier = 1;
        [SerializeField] private IngredientType _outputIngredientType = IngredientType.None;

        [Header("VFX / SFX References")]
        [SerializeField] private GameObject _cookingVfx;
        [SerializeField] private GameObject _readyVfx;
        [SerializeField] private GameObject _burntVfx;
        [SerializeField] private AudioClip   _primeSound;
        [SerializeField] private AudioClip   _doneSound;
        [SerializeField] private AudioClip   _burnSound;

        // ── Injected ──
        [Inject] private IConfigService _cfg;
        [Inject] private IEventBus      _eventBus;

        // ── Network state (server-authoritative, replicated to all clients) ──
        private readonly NetworkVariable<StationPhase> _phase =
            new(StationPhase.Idle,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        /// <summary>ClientId of the player who primed this station (server-only).</summary>
        private ulong _primingClientId = ulong.MaxValue;

        /// <summary>TeamId of the player who primed this station (server-only).</summary>
        private TeamId _primingTeam = TeamId.TeamA;

        /// <summary>Tracks remaining time for COOKING / READY / BURNT phases (server-only).</summary>
        private float _phaseTimer;

        // ── Cached RC values (read once on phase transition to avoid per-frame RC calls) ──
        private float CookDuration  => _cfg.Get(RemoteConfigKeys.StationCookDurationSec, RemoteConfigKeys.Defaults.StationCookDurationSec);
        private float BurnGrace     => _cfg.Get(RemoteConfigKeys.StationBurnGraceSec,    RemoteConfigKeys.Defaults.StationBurnGraceSec);
        private float BurntLockout  => _cfg.Get(RemoteConfigKeys.StationSabotageLockout, RemoteConfigKeys.Defaults.StationSabotageLockout);

        // ── Public accessors ──
        public StationPhase Phase => _phase.Value;
        public int RecipeTier     => _recipeTier;

        // ─────────────────────────────────────────────────────────────────
        // Unity lifecycle
        // ─────────────────────────────────────────────────────────────────

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _phase.OnValueChanged += OnPhaseChanged;
            RefreshVisuals(_phase.Value);
        }

        public override void OnNetworkDespawn()
        {
            _phase.OnValueChanged -= OnPhaseChanged;
            base.OnNetworkDespawn();
        }

        // Server-only tick
        private void Update()
        {
            if (!IsServer) return;
            TickPhase(Time.deltaTime);
        }

        // ─────────────────────────────────────────────────────────────────
        // Server: phase timer tick
        // ─────────────────────────────────────────────────────────────────

        private void TickPhase(float dt)
        {
            switch (_phase.Value)
            {
                case StationPhase.Primed:
                    // Immediately start cooking next frame after prime confirmation
                    TransitionTo(StationPhase.Cooking);
                    break;

                case StationPhase.Cooking:
                    _phaseTimer -= dt;
                    if (_phaseTimer <= 0f)
                        TransitionTo(StationPhase.Ready);
                    break;

                case StationPhase.Ready:
                    _phaseTimer -= dt;
                    if (_phaseTimer <= 0f)
                        TransitionTo(StationPhase.Burnt);
                    break;

                case StationPhase.Burnt:
                    _phaseTimer -= dt;
                    if (_phaseTimer <= 0f)
                        TransitionTo(StationPhase.Idle);
                    break;
            }
        }

        private void TransitionTo(StationPhase next)
        {
            switch (next)
            {
                case StationPhase.Cooking:
                    _phaseTimer = CookDuration;
                    break;
                case StationPhase.Ready:
                    _phaseTimer = BurnGrace;
                    PlaySoundClientRpc(SoundId.Done);
                    break;
                case StationPhase.Burnt:
                    _phaseTimer = BurntLockout;
                    _primingClientId = ulong.MaxValue;
                    PlaySoundClientRpc(SoundId.Burn);
                    _eventBus?.Publish(new SFXEvent(SFXType.FireStart));
                    break;
                case StationPhase.Idle:
                    _primingClientId = ulong.MaxValue;
                    break;
            }

            _phase.Value = next;
        }

        // ─────────────────────────────────────────────────────────────────
        // StationBase: Interact
        // ─────────────────────────────────────────────────────────────────

        protected override void HandleInteraction(PlayerController player)
        {
            // HandleInteraction is server-only (StationBase guarantees this).
            switch (_phase.Value)
            {
                case StationPhase.Idle:
                    StartPrime(player);
                    break;

                case StationPhase.Ready:
                    TryCollect(player);
                    break;

                // Cooking / Primed / Burnt — no interaction allowed
                default:
                    GameLogger.Log($"[Station] Interact ignored — phase is {_phase.Value}");
                    break;
            }
        }

        public override bool CanInteract(object playerObj)
        {
            return _phase.Value == StationPhase.Idle || _phase.Value == StationPhase.Ready;
        }

        public override string GetInteractionPrompt()
        {
            return _phase.Value switch
            {
                StationPhase.Idle    => "Prime Station",
                StationPhase.Cooking => "Cooking...",
                StationPhase.Ready   => "Collect Dish",
                StationPhase.Burnt   => "Burnt — Unavailable",
                _                    => string.Empty,
            };
        }

        // ─────────────────────────────────────────────────────────────────
        // Prime logic
        // ─────────────────────────────────────────────────────────────────

        private void StartPrime(PlayerController player)
        {
            _primingClientId = player.OwnerClientId;
            _primingTeam     = (TeamId)player.TeamId;
            TransitionTo(StationPhase.Primed);
            PlaySoundClientRpc(SoundId.Prime);
            GameLogger.Log($"[Station] Primed by client {_primingClientId} (Team {_primingTeam})");
        }

        // ─────────────────────────────────────────────────────────────────
        // Collection logic
        // ─────────────────────────────────────────────────────────────────

        private void TryCollect(PlayerController player)
        {
            if ((TeamId)player.TeamId != _primingTeam)
            {
                GameLogger.Log($"[Station] Collect denied — wrong team (primed by {_primingTeam})");
                return;
            }

            if (player.IsCarryingMaxItems)
            {
                GameLogger.Log($"[Station] Collect denied — player carrying max items");
                return;
            }

            // Hand dish to player
            player.ReceiveCollectedDish(_recipeTier, _outputIngredientType);
            GameLogger.Log($"[Station] Dish (T{_recipeTier}) collected by client {player.OwnerClientId}");

            TransitionTo(StationPhase.Idle);
        }

        // ─────────────────────────────────────────────────────────────────
        // Sabotage (called by ControllerSabotageAbility server-side)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Force-burns this station immediately if it is currently COOKING.
        /// Called server-side only from ControllerSabotageAbility.
        /// </summary>
        public bool TrySabotage(ulong attackerClientId)
        {
            if (!IsServer) return false;
            if (_phase.Value != StationPhase.Cooking) return false;

            GameLogger.Log($"[Station] Sabotaged by client {attackerClientId}");
            TransitionTo(StationPhase.Burnt);
            return true;
        }

        // ─────────────────────────────────────────────────────────────────
        // Client-side visuals
        // ─────────────────────────────────────────────────────────────────

        private void OnPhaseChanged(StationPhase prev, StationPhase next) => RefreshVisuals(next);

        private void RefreshVisuals(StationPhase phase)
        {
            if (_cookingVfx) _cookingVfx.SetActive(phase == StationPhase.Cooking);
            if (_readyVfx)   _readyVfx.SetActive(phase   == StationPhase.Ready);
            if (_burntVfx)   _burntVfx.SetActive(phase   == StationPhase.Burnt);
        }

        // ─────────────────────────────────────────────────────────────────
        // Audio RPCs
        // ─────────────────────────────────────────────────────────────────

        private enum SoundId : byte { Prime, Done, Burn }

        [ClientRpc]
        private void PlaySoundClientRpc(SoundId sound)
        {
            AudioClip clip = sound switch
            {
                SoundId.Prime => _primeSound,
                SoundId.Done  => _doneSound,
                SoundId.Burn  => _burnSound,
                _             => null,
            };

            if (clip != null && _audioSource != null)
                _audioSource.PlayOneShot(clip);
        }
    }
}
