using KitchenClash.Application;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Network.Cooking;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace KitchenClash.Infrastructure.Network.Stations
{
    /// <summary>
    /// A station for cooking ingredients (formerly CookingPot).
    /// Wired to HazardService for fire registration/extinguishing,
    /// ScoreService for fire penalties, and AbilityService for chef passives.
    /// </summary>
    public class CookingStation : ProcessingStation
    {
        [Header("Cooking Station Settings")]
        [SerializeField] private float _burningTime = 10f;
        [SerializeField] private GameObject _steamEffect;
        [SerializeField] private GameObject _fireEffect;
        [SerializeField] private AudioClip _boilingSound;
        [SerializeField] private AudioClip _burningSound;

        private bool _isBurning;
        private float _burningTimer;

        [Inject] private IEventBus _eventBus;
        [Inject] private IHazardService _hazardService;
        [Inject] private IScoreService _scoreService;
        [Inject] private IAbilityService _abilityService;

        /// <summary>Unique station ID derived from NetworkObjectId.</summary>
        private string StationId => NetworkObjectId.ToString();

        /// <summary>
        /// Effective burn time accounting for Yuki's SlowerBurnRate passive.
        /// Server-only: checked each frame during burn countdown.
        /// </summary>
        private float EffectiveBurningTime
        {
            get
            {
                float baseTime = _burningTime;

                // If a local player has SlowerBurnRate passive, extend burn timer
                PlayerController localPlayer = FindLocalPlayer();
                if (localPlayer != null)
                {
                    ChefDefinition chefDef = FindChefDefinition(localPlayer);
                    if (chefDef != null && chefDef.PassiveAbility == AbilityType.SlowerBurnRate)
                    {
                        AbilityDefinition passiveDef = _abilityService?.GetPassiveDefinition(chefDef.Id);
                        if (passiveDef != null)
                        {
                            // value = 0.30 → burn timer 30% slower → multiply by 1.3
                            baseTime *= (1f + passiveDef.Value);
                        }
                    }
                }

                return baseTime;
            }
        }

        /// <summary>
        /// Initialize the cooking station.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Set station name
            _stationName = "Cooking Station";

            // Hide effects
            if (_steamEffect != null)
            {
                _steamEffect.SetActive(false);
            }

            if (_fireEffect != null)
            {
                _fireEffect.SetActive(false);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            SubscribeToHazardEvents();
        }

        public override void OnNetworkDespawn()
        {
            UnsubscribeFromHazardEvents();
            base.OnNetworkDespawn();
        }

        /// <summary>
        /// Subscribe to HazardService events for visual/audio feedback hooks.
        /// Actual VFX/SFX implementation deferred to later phases.
        /// </summary>
        private void SubscribeToHazardEvents()
        {
            if (_hazardService == null)
            {
                return;
            }

            _hazardService.OnFireStarted += HandleFireStarted;
            _hazardService.OnFireExtinguished += HandleFireExtinguished;
            _hazardService.OnFirePenalty += HandleFirePenalty;
        }

        private void UnsubscribeFromHazardEvents()
        {
            if (_hazardService == null)
            {
                return;
            }

            _hazardService.OnFireStarted -= HandleFireStarted;
            _hazardService.OnFireExtinguished -= HandleFireExtinguished;
            _hazardService.OnFirePenalty -= HandleFirePenalty;
        }

        private void HandleFireStarted(string stationId)
        {
            if (stationId != StationId)
            {
                return;
            }
            // Visual/audio hook: fire started on this station
            // VFX/SFX wiring deferred to later phases
        }

        private void HandleFireExtinguished(string stationId)
        {
            if (stationId != StationId)
            {
                return;
            }

            // Visual/audio hook: fire extinguished on this station
            _isBurning = false;
            HideFireEffectClientRpc();
            StopSoundsClientRpc();
        }

        private void HandleFirePenalty(string stationId)
        {
            if (stationId != StationId)
            {
                return;
            }

            // Apply -5 fire penalty via ScoreService
            if (_scoreService != null)
            {
                // Determine which team owns the station area
                // Fire penalty applies to the team whose station caught fire
                TeamId teamId = DetermineStationTeam();
                var scoreEvent = new ScoreEvent(ScoreEventType.FirePenalty);
                _scoreService.AddScore(teamId, scoreEvent);
            }

            // Visual/audio hook: penalty applied
            _isBurning = false;
            HideFireEffectClientRpc();
            StopSoundsClientRpc();
        }

        /// <summary>
        /// Update the cooking station.
        /// </summary>
        protected override void Update()
        {
            base.Update();

            // Only update burning on the server
            if (!IsServer || !_isProcessing || _currentIngredient == null)
            {
                return;
            }

            // Keep HazardService time in sync
            _hazardService?.SetCurrentTime(Time.time);

            // Check if the ingredient is cooked
            if (_processingProgress >= 1f)
            {
                // Start burning timer
                _burningTimer += Time.deltaTime;

                // Check if the ingredient is burning (use EffectiveBurningTime for passive)
                if (_burningTimer >= EffectiveBurningTime && !_isBurning)
                {
                    _isBurning = true;

                    // Register fire with HazardService
                    _hazardService?.SetCurrentTime(Time.time);
                    _hazardService?.RegisterFire(StationId);

                    // Show fire effect
                    if (_fireEffect != null)
                    {
                        ShowFireEffectClientRpc();
                    }

                    // Play burning sound
                    if (_audioSource != null && _burningSound != null)
                    {
                        PlayBurningSoundClientRpc();
                    }

                    // Trigger camera shake
                    TriggerBurningShakeClientRpc();

                    // Burn the ingredient
                    if (_currentIngredient != null)
                    {
                        _currentIngredient.Burn();
                    }
                }
            }
        }

        /// <summary>
        /// Handle interaction — includes extinguish logic for burning stations.
        /// </summary>
        protected override void HandleInteraction(PlayerController player)
        {
            // If the station is burning, player interaction attempts to extinguish
            if (_isBurning)
            {
                _hazardService?.SetCurrentTime(Time.time);
                bool extinguished = _hazardService?.TryExtinguish(StationId, Time.time) ?? false;

                if (extinguished)
                {
                    // Fire was extinguished in time — no penalty
                    _isBurning = false;
                    HideFireEffectClientRpc();
                    StopSoundsClientRpc();
                }
                // If not extinguished (window expired), HandleFirePenalty handles cleanup via event

                return;
            }

            // Normal processing interaction
            base.HandleInteraction(player);
        }

        /// <summary>
        /// Allow interaction when station is burning (for extinguish) or normal conditions.
        /// </summary>
        public override bool CanInteract(object playerObj)
        {
            if (_isBurning)
            {
                return true;
            }

            return base.CanInteract(playerObj);
        }

        /// <summary>
        /// Get the interaction prompt text.
        /// </summary>
        public override string GetInteractionPrompt()
        {
            if (_isBurning)
            {
                return "Extinguish Fire!";
            }

            return base.GetInteractionPrompt();
        }

        /// <summary>
        /// Check if the ingredient can be cooked.
        /// </summary>
        protected override bool CanProcessIngredient(IngredientItem ingredientItem)
        {
            // Check if the ingredient requires cooking
            return ingredientItem != null &&
                   ingredientItem.Ingredient != null &&
                   ingredientItem.Ingredient.RequiresCooking &&
                   !ingredientItem.IsCooked;
        }

        /// <summary>
        /// Cook the ingredient.
        /// </summary>
        protected override bool ProcessIngredient(IngredientItem ingredientItem)
        {
            if (!CanProcessIngredient(ingredientItem))
            {
                return false;
            }

            // Cook the ingredient
            ingredientItem.Cook();

            return true;
        }

        /// <summary>
        /// Start processing an ingredient.
        /// </summary>
        protected override void StartProcessing(IngredientItem ingredientItem)
        {
            base.StartProcessing(ingredientItem);

            // Reset burning state
            _isBurning = false;
            _burningTimer = 0f;

            // Show steam effect
            if (_steamEffect != null)
            {
                ShowSteamEffectClientRpc();
            }

            // Play boiling sound
            if (_audioSource != null && _boilingSound != null)
            {
                PlayBoilingSoundClientRpc();
            }
        }

        /// <summary>
        /// Complete processing an ingredient.
        /// </summary>
        protected override void CompleteProcessing()
        {
            // Don't complete processing if the ingredient is still cooking
            // This allows the ingredient to burn if left too long
            if (_isProcessing && _currentIngredient != null && !_isBurning)
            {
                base.CompleteProcessing();
            }
        }

        /// <summary>
        /// Cancel processing an ingredient.
        /// </summary>
        protected override void CancelProcessing()
        {
            // Hide effects
            if (_steamEffect != null)
            {
                HideSteamEffectClientRpc();
            }

            if (_fireEffect != null)
            {
                HideFireEffectClientRpc();
            }

            // Stop sounds
            if (_audioSource != null)
            {
                StopSoundsClientRpc();
            }

            base.CancelProcessing();
        }

        /// <summary>
        /// Determine which team this station belongs to.
        /// Falls back to TeamA if no clear assignment.
        /// </summary>
        private TeamId DetermineStationTeam()
        {
            // Check parent hierarchy for a team marker or default to TeamA.
            // Stations are currently shared across teams in all GDD v3 kitchen layouts.
            // DEFERRED: If team-specific stations are introduced, add a [SerializeField] TeamId field
            //           and populate it in the Inspector per station prefab variant.
            return TeamId.TeamA;
        }

        /// <summary>
        /// Find the local player's PlayerController on the server.
        /// Used for checking chef passives (SlowerBurnRate).
        /// </summary>
        private PlayerController FindLocalPlayer()
        {
            if (!IsServer)
            {
                return null;
            }

            // On server, find the player who placed the ingredient
            // For simplicity, check all connected players
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                PlayerController pc = client.PlayerObject?.GetComponent<PlayerController>();
                if (pc != null)
                {
                    return pc;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the ChefDefinition for a player via ICharacterService lookup.
        /// </summary>
        private ChefDefinition FindChefDefinition(PlayerController player)
        {
            if (player == null)
            {
                return null;
            }

            // Try to resolve character service to get the selected chef
            try
            {
                var scope = VContainer.Unity.LifetimeScope.Find<VContainer.Unity.LifetimeScope>();
                if (scope != null)
                {
                    ICharacterService charService = scope.Container.Resolve<ICharacterService>();
                    return charService?.SelectedChef;
                }
            }
            catch
            {
                // Silently fail — passive won't apply
            }
            return null;
        }

        #region Client RPCs

        /// <summary>
        /// Show the steam effect on all clients.
        /// </summary>
        [ClientRpc]
        private void ShowSteamEffectClientRpc()
        {
            if (_steamEffect != null)
            {
                _steamEffect.SetActive(true);
            }
        }

        /// <summary>
        /// Hide the steam effect on all clients.
        /// </summary>
        [ClientRpc]
        private void HideSteamEffectClientRpc()
        {
            if (_steamEffect != null)
            {
                _steamEffect.SetActive(false);
            }
        }

        /// <summary>
        /// Show the fire effect on all clients.
        /// </summary>
        [ClientRpc]
        private void ShowFireEffectClientRpc()
        {
            if (_fireEffect != null)
            {
                _fireEffect.SetActive(true);
            }
        }

        /// <summary>
        /// Hide the fire effect on all clients.
        /// </summary>
        [ClientRpc]
        private void HideFireEffectClientRpc()
        {
            if (_fireEffect != null)
            {
                _fireEffect.SetActive(false);
            }
        }

        /// <summary>
        /// Play the boiling sound on all clients.
        /// </summary>
        [ClientRpc]
        private void PlayBoilingSoundClientRpc()
        {
            if (_audioSource != null && _boilingSound != null)
            {
                _audioSource.clip = _boilingSound;
                _audioSource.loop = true;
                _audioSource.Play();
            }
        }

        /// <summary>
        /// Play the burning sound on all clients.
        /// </summary>
        [ClientRpc]
        private void PlayBurningSoundClientRpc()
        {
            if (_audioSource != null && _burningSound != null)
            {
                _audioSource.clip = _burningSound;
                _audioSource.loop = true;
                _audioSource.Play();
            }
        }

        /// <summary>
        /// Stop all sounds on all clients.
        /// </summary>
        [ClientRpc]
        private void StopSoundsClientRpc()
        {
            if (_audioSource != null)
            {
                _audioSource.Stop();
                _audioSource.loop = false;
            }
        }

        [ClientRpc]
        private void TriggerBurningShakeClientRpc()
        {
            _eventBus?.Publish(new CameraShakeEvent(0.7f, 0.5f));
        }

        #endregion
    }
}
