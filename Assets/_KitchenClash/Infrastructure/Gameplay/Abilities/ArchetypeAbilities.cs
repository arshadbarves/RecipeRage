using System.Collections.Generic;
using KitchenClash.Application.Config;
using KitchenClash.Domain;
using KitchenClash.Infrastructure.Network;
using KitchenClash.Infrastructure.Network.Stations;
using KitchenClash.Infrastructure.Network.Cooking;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Playcenter.Shell;
using Playcenter.Services;

namespace KitchenClash.Infrastructure.Gameplay.Abilities
{
    /// <summary>
    /// Shared target scans for archetype abilities.
    /// Uses NGO SpawnedObjects (no FindObjectOfType / FindObjectsByType).
    /// </summary>
    internal static class AbilityTargetScan
    {
        public static PlayerController FindNearestEnemyPlayer(
            PlayerController caster,
            float range,
            bool requireCarrying = false)
        {
            if (caster == null)
            {
                return null;
            }

            PlayerController nearest = null;
            float nearestDist = float.MaxValue;
            Vector3 origin = caster.transform.position;
            NetworkManager nm = caster.NetworkManager;

            if (nm?.SpawnManager?.SpawnedObjects == null)
            {
                return null;
            }

            foreach (KeyValuePair<ulong, NetworkObject> kvp in nm.SpawnManager.SpawnedObjects)
            {
                NetworkObject netObj = kvp.Value;
                if (netObj == null || !netObj.TryGetComponent(out PlayerController pc))
                {
                    continue;
                }

                if (pc == caster || pc.TeamId == caster.TeamId)
                {
                    continue;
                }

                if (requireCarrying && !pc.IsHoldingObject())
                {
                    continue;
                }

                float dist = Vector3.Distance(origin, pc.transform.position);
                if (dist <= range && dist < nearestDist)
                {
                    nearest = pc;
                    nearestDist = dist;
                }
            }

            return nearest;
        }

        public static AutonomousCookingStation FindNearestCookingStation(
            PlayerController caster,
            float range,
            Domain.Enums.StationPhase requiredPhase)
        {
            if (caster == null)
            {
                return null;
            }

            AutonomousCookingStation nearest = null;
            float nearestDist = float.MaxValue;
            Vector3 origin = caster.transform.position;
            NetworkManager nm = caster.NetworkManager;

            if (nm?.SpawnManager?.SpawnedObjects == null)
            {
                return null;
            }

            foreach (KeyValuePair<ulong, NetworkObject> kvp in nm.SpawnManager.SpawnedObjects)
            {
                NetworkObject netObj = kvp.Value;
                if (netObj == null || !netObj.TryGetComponent(out AutonomousCookingStation station))
                {
                    continue;
                }

                if (station.Phase != requiredPhase)
                {
                    continue;
                }

                float dist = Vector3.Distance(origin, station.transform.position);
                if (dist <= range && dist < nearestDist)
                {
                    nearest = station;
                    nearestDist = dist;
                }
            }

            return nearest;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Rusher — Short-range shove
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Rusher archetype active ability.
    /// Shoves the nearest enemy within rusher_shove_range_tiles, applying
    /// a knockback impulse and dealing a small amount of damage.
    /// RC-keyed cooldown comes from the AbilityDefinition passed at construction.
    /// </summary>
    public sealed class RusherShoveAbility : ActiveAbilityBase
    {
        [Inject] private IConfigService _cfg;

        // Server-side reference: set by PlayerAbilityController after spawn
        public PlayerController Caster { private get; set; }

        public RusherShoveAbility(AbilityDefinition definition) : base(definition) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            if (Caster == null) return;

            float range = _cfg.Get(RemoteConfigKeys.RusherShoveRange,
                                   RemoteConfigKeys.Defaults.RusherShoveRange);
            float knockbackTiles = _cfg.Get(RemoteConfigKeys.RusherShoveKnockback,
                                            RemoteConfigKeys.Defaults.RusherShoveKnockback);

            PlayerController target = AbilityTargetScan.FindNearestEnemyPlayer(Caster, range);
            if (target == null) return;

            Vector3 dir = (target.transform.position - Caster.transform.position).normalized;
            var combat = target.GetComponent<PlayerCombatController>();
            combat?.ApplyKnockback(dir, knockbackTiles * 3f);
            combat?.TakeDamage(20, Caster.OwnerClientId);

            GameLogger.Log($"[Rusher] Shoved client {target.OwnerClientId}");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Cook — Prime-input speed buff
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Cook archetype active ability.
    /// Reduces the tap count needed to prime the next station by
    /// cook_prime_buff_tap_reduction (default 2 taps) for the next prime action.
    /// </summary>
    public sealed class CookPrimeBuffAbility : ActiveAbilityBase
    {
        [Inject] private IConfigService _cfg;
        [Inject] private IEventBus      _eventBus;

        public PlayerController Caster { private get; set; }

        public CookPrimeBuffAbility(AbilityDefinition definition) : base(definition) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            int tapReduction = _cfg.Get(RemoteConfigKeys.CookPrimeBuffTapReduction,
                                         RemoteConfigKeys.Defaults.CookPrimeBuffTapReduction);

            // Publish event — PlayerInteractionController listens and applies tap reduction for next prime
            _eventBus?.Publish(new CookPrimeBuffActivatedEvent(Caster, tapReduction));

            GameLogger.Log($"[Cook] Prime buff activated: -{tapReduction} taps");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Controller — Sabotage (force-burn enemy COOKING station)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Controller archetype active ability.
    /// Force-burns the nearest enemy COOKING station within
    /// station_sabotage_range_tiles (default 2 tiles).
    /// </summary>
    public sealed class ControllerSabotageAbility : ActiveAbilityBase
    {
        [Inject] private IConfigService _cfg;

        public PlayerController Caster { private get; set; }

        public ControllerSabotageAbility(AbilityDefinition definition) : base(definition) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            if (Caster == null) return;

            float range = _cfg.Get(RemoteConfigKeys.StationSabotageRange,
                                   RemoteConfigKeys.Defaults.StationSabotageRange);

            AutonomousCookingStation target = AbilityTargetScan.FindNearestCookingStation(
                Caster, range, Domain.Enums.StationPhase.Cooking);
            if (target == null)
            {
                GameLogger.Log("[Controller] No cooking station in range to sabotage");
                return;
            }

            bool sabotaged = target.TrySabotage(Caster.OwnerClientId);
            GameLogger.Log(sabotaged
                ? $"[Controller] Sabotaged station at {target.transform.position}"
                : "[Controller] Station not in COOKING phase — sabotage failed");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Disruptor — Steal from hands
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Disruptor archetype active ability.
    /// Steals the carried item from an enemy within disruptor_steal_range_tiles (default 1 tile).
    /// The stolen item is added to the disruptor's carry slot if empty.
    /// </summary>
    public sealed class DisruptorStealAbility : ActiveAbilityBase
    {
        [Inject] private IConfigService _cfg;

        public PlayerController Caster { private get; set; }

        public DisruptorStealAbility(AbilityDefinition definition) : base(definition) { }

        protected override void ApplyEffect(AbilityContext ctx)
        {
            if (Caster == null || Caster.IsCarryingMaxItems) return;

            float range = _cfg.Get(RemoteConfigKeys.DisruptorStealRange,
                                   RemoteConfigKeys.Defaults.DisruptorStealRange);

            PlayerController target = AbilityTargetScan.FindNearestEnemyPlayer(
                Caster, range, requireCarrying: true);
            if (target == null)
            {
                GameLogger.Log("[Disruptor] No carrying enemy in range");
                return;
            }

            var stolenItems = target.GetAndClearCarriedItems();
            if (stolenItems == null || stolenItems.Count == 0)
            {
                GameLogger.Log("[Disruptor] Target not carrying anything");
                return;
            }

            var item = stolenItems[0];
            Caster.ReceiveCollectedDish(item.RecipeTier, item.IngredientType);
            GameLogger.Log($"[Disruptor] Stole T{item.RecipeTier} from client {target.OwnerClientId}");
        }
    }

    // ── Supporting domain event ─────────────────────────────────────────

    /// <summary>Published when Cook activates prime-buff ability.</summary>
    public sealed class CookPrimeBuffActivatedEvent
    {
        public PlayerController Caster       { get; }
        public int              TapReduction { get; }

        public CookPrimeBuffActivatedEvent(PlayerController caster, int reduction)
        {
            Caster       = caster;
            TapReduction = reduction;
        }
    }
}
