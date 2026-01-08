using System;
using Newtonsoft.Json;
using UnityEngine;
using Modules.Logging;

namespace Gameplay.Characters
{
    /// <summary>
    /// Base class for character abilities in RecipeRage.
    /// </summary>
    public abstract class CharacterAbility
    {
        /// <summary>
        /// The character class that owns this ability.
        /// </summary>
        protected CharacterClass CharacterClass { get; private set; }
        
        /// <summary>
        /// The player controller that owns this ability.
        /// </summary>
        protected PlayerController PlayerController { get; private set; }
        
        /// <summary>
        /// The ability type.
        /// </summary>
        public AbilityType AbilityType { get; protected set; }
        
        /// <summary>
        /// The ability cooldown in seconds.
        /// </summary>
        public float Cooldown { get; protected set; }
        
        /// <summary>
        /// The ability duration in seconds (0 for instant).
        /// </summary>
        public float Duration { get; protected set; }
        
        /// <summary>
        /// The ability icon.
        /// </summary>
        public Sprite Icon { get; protected set; }
        
        /// <summary>
        /// The ability description.
        /// </summary>
        public string Description { get; protected set; }
        
        /// <summary>
        /// The current cooldown timer.
        /// </summary>
        public float CooldownTimer { get; protected set; }
        
        /// <summary>
        /// The current duration timer.
        /// </summary>
        public float DurationTimer { get; protected set; }
        
        /// <summary>
        /// Whether the ability is on cooldown.
        /// </summary>
        public bool IsOnCooldown => CooldownTimer > 0f;
        
        /// <summary>
        /// Whether the ability is active.
        /// </summary>
        public bool IsActive => DurationTimer > 0f;
        
        /// <summary>
        /// Event triggered when the ability is activated.
        /// </summary>
        public event Action<CharacterAbility> OnAbilityActivated;
        
        /// <summary>
        /// Event triggered when the ability is deactivated.
        /// </summary>
        public event Action<CharacterAbility> OnAbilityDeactivated;
        
        /// <summary>
        /// Event triggered when the ability cooldown starts.
        /// </summary>
        public event Action<CharacterAbility, float> OnAbilityCooldownStarted;
        
        /// <summary>
        /// Event triggered when the ability cooldown ends.
        /// </summary>
        public event Action<CharacterAbility> OnAbilityCooldownEnded;
        
        /// <summary>
        /// Initialize the ability.
        /// </summary>
        /// <param name="characterClass">The character class that owns this ability</param>
        /// <param name="playerController">The player controller that owns this ability</param>
        public virtual void Initialize(CharacterClass characterClass, PlayerController playerController)
        {
            CharacterClass = characterClass;
            PlayerController = playerController;
            
            AbilityType = characterClass.PrimaryAbilityType;
            Cooldown = characterClass.PrimaryAbilityCooldown;
            Duration = characterClass.PrimaryAbilityDuration;
            Icon = characterClass.PrimaryAbilityIcon;
            Description = characterClass.PrimaryAbilityDescription;
            
            CooldownTimer = 0f;
            DurationTimer = 0f;
        }
        
        /// <summary>
        /// Update the ability.
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public virtual void Update(float deltaTime)
        {
            // Update cooldown timer
            if (CooldownTimer > 0f)
            {
                CooldownTimer -= deltaTime;
                
                if (CooldownTimer <= 0f)
                {
                    CooldownTimer = 0f;
                    OnAbilityCooldownEnded?.Invoke(this);
                }
            }
            
            // Update duration timer
            if (DurationTimer > 0f)
            {
                DurationTimer -= deltaTime;
                
                if (DurationTimer <= 0f)
                {
                    DurationTimer = 0f;
                    Deactivate();
                }
            }
        }
        
        /// <summary>
        /// Activate the ability.
        /// </summary>
        /// <returns>True if the ability was activated, false otherwise</returns>
        public virtual bool Activate()
        {
            if (IsOnCooldown)
            {
                GameLogger.Log($"Cannot activate ability: on cooldown ({CooldownTimer:F1}s remaining)");
                return false;
            }
            
            // Start cooldown
            CooldownTimer = Cooldown;
            OnAbilityCooldownStarted?.Invoke(this, Cooldown);
            
            // Start duration if not instant
            if (Duration > 0f)
            {
                DurationTimer = Duration;
            }
            
            // Trigger event
            OnAbilityActivated?.Invoke(this);
            
            return true;
        }
        
        /// <summary>
        /// Deactivate the ability.
        /// </summary>
        public virtual void Deactivate()
        {
            if (!IsActive)
            {
                return;
            }
            
            // Reset duration timer
            DurationTimer = 0f;
            
            // Trigger event
            OnAbilityDeactivated?.Invoke(this);
        }
        
        /// <summary>
        /// Reset the ability.
        /// </summary>
        public virtual void Reset()
        {
            CooldownTimer = 0f;
            DurationTimer = 0f;
        }
        
        /// <summary>
        /// Create an ability instance based on the ability type.
        /// </summary>
        /// <param name="abilityType">The ability type</param>
        /// <param name="characterClass">The character class</param>
        /// <param name="playerController">The player controller</param>
        /// <returns>The created ability instance</returns>
        public static CharacterAbility CreateAbility(AbilityType abilityType, CharacterClass characterClass, PlayerController playerController)
        {
            CharacterAbility ability = null;
            
            switch (abilityType)
            {
                case AbilityType.None:
                    ability = new NoneAbility();
                    break;
                case AbilityType.SpeedBoost:
                    ability = new SpeedBoostAbility();
                    break;
                case AbilityType.FreezeTime:
                    ability = new FreezeTimeAbility();
                    break;
                case AbilityType.DoubleIngredients:
                    ability = new DoubleIngredientsAbility();
                    break;
                case AbilityType.InstantCook:
                    ability = new InstantCookAbility();
                    break;
                case AbilityType.InstantChop:
                    ability = new InstantChopAbility();
                    break;
                case AbilityType.TeleportToStation:
                    ability = new TeleportToStationAbility();
                    break;
                case AbilityType.PushOtherPlayers:
                    ability = new PushOtherPlayersAbility();
                    break;
                case AbilityType.StealIngredient:
                    ability = new StealIngredientAbility();
                    break;
                case AbilityType.PreventBurning:
                    ability = new PreventBurningAbility();
                    break;
                case AbilityType.AutoPlate:
                    ability = new AutoPlateAbility();
                    break;
                case AbilityType.IngredientMagnet:
                    ability = new IngredientMagnetAbility();
                    break;
                default:
                    GameLogger.LogError($"Unknown ability type: {abilityType}");
                    ability = new NoneAbility();
                    break;
            }
            
            ability.Initialize(characterClass, playerController);
            return ability;
        }
    }
    
    /// <summary>
    /// No ability.
    /// </summary>
    public class NoneAbility : CharacterAbility
    {
        public override void Initialize(CharacterClass characterClass, PlayerController playerController)
        {
            base.Initialize(characterClass, playerController);
            AbilityType = AbilityType.None;
        }
        
        public override bool Activate()
        {
            // No effect
            return false;
        }
    }
    
    /// <summary>
    /// Speed boost ability.
    /// </summary>
    public class SpeedBoostAbility : CharacterAbility
    {
        private float _speedMultiplier = 1.5f;
        private float _originalSpeed;
        
        public override void Initialize(CharacterClass characterClass, PlayerController playerController)
        {
            base.Initialize(characterClass, playerController);
            
            // Parse parameters
            if (!string.IsNullOrEmpty(characterClass.PrimaryAbilityParameters))
            {
                try
                {
                    SpeedBoostParameters parameters = JsonConvert.DeserializeObject<SpeedBoostParameters>(characterClass.PrimaryAbilityParameters);
                    if (parameters != null)
                    {
                        _speedMultiplier = parameters.SpeedMultiplier;
                    }
                }
                catch (Exception e)
                {
                    GameLogger.LogError($"Failed to parse parameters: {e.Message}");
                }
            }
        }
        
        public override bool Activate()
        {
            if (!base.Activate())
            {
                return false;
            }
            
            // Store original speed
            _originalSpeed = PlayerController.MovementSpeed;
            
            // Apply speed boost
            PlayerController.MovementSpeed *= _speedMultiplier;
            
            return true;
        }
        
        public override void Deactivate()
        {
            if (!IsActive)
            {
                return;
            }
            
            // Restore original speed
            PlayerController.MovementSpeed = _originalSpeed;
            
            base.Deactivate();
        }
        
        private class SpeedBoostParameters
        {
            public float SpeedMultiplier { get; set; } = 1.5f;
        }
    }
    
    public class FreezeTimeAbility : CharacterAbility
    {
        public override bool Activate()
        {
            if (!base.Activate()) return false;
            // TODO: Implement freeze time logic
            return true;
        }
        
        public override void Deactivate()
        {
            if (!IsActive) return;
            // TODO: Implement unfreeze time logic
            base.Deactivate();
        }
    }
    
    public class DoubleIngredientsAbility : CharacterAbility
    {
        public override bool Activate()
        {
            if (!base.Activate()) return false;
            // TODO: Implement double ingredients logic
            return true;
        }
    }
    
    public class InstantCookAbility : CharacterAbility
    {
        public override bool Activate()
        {
            if (!base.Activate()) return false;
            // TODO: Implement instant cook logic
            return true;
        }
    }
    
    public class InstantChopAbility : CharacterAbility
    {
        public override bool Activate()
        {
            if (!base.Activate()) return false;
            // TODO: Implement instant chop logic
            return true;
        }
    }
    
    public class TeleportToStationAbility : CharacterAbility
    {
        public override bool Activate()
        {
            if (!base.Activate()) return false;
            // TODO: Implement teleport to station logic
            return true;
        }
    }
    
    public class PushOtherPlayersAbility : CharacterAbility
    {
        private float _pushForce = 5f;
        private float _pushRadius = 3f;
        
        public override void Initialize(CharacterClass characterClass, PlayerController playerController)
        {
            base.Initialize(characterClass, playerController);
            if (!string.IsNullOrEmpty(characterClass.PrimaryAbilityParameters))
            {
                try
                {
                    PushParameters parameters = JsonConvert.DeserializeObject<PushParameters>(characterClass.PrimaryAbilityParameters);
                    if (parameters != null)
                    {
                        _pushForce = parameters.PushForce;
                        _pushRadius = parameters.PushRadius;
                    }
                }
                catch (Exception e) { GameLogger.LogError($"Failed to parse push parameters: {e.Message}"); }
            }
        }
        
        public override bool Activate()
        {
            if (!base.Activate()) return false;
            // TODO: Implement push other players logic
            return true;
        }
        
        private class PushParameters { public float PushForce { get; set; } = 5f; public float PushRadius { get; set; } = 3f; }
    }
    
    public class StealIngredientAbility : CharacterAbility
    {
        private float _stealRadius = 2f;
        
        public override void Initialize(CharacterClass characterClass, PlayerController playerController)
        {
            base.Initialize(characterClass, playerController);
            if (!string.IsNullOrEmpty(characterClass.PrimaryAbilityParameters))
            {
                try
                {
                    StealParameters parameters = JsonConvert.DeserializeObject<StealParameters>(characterClass.PrimaryAbilityParameters);
                    if (parameters != null) _stealRadius = parameters.StealRadius;
                }
                catch (Exception e) { GameLogger.LogError($"Failed to parse steal parameters: {e.Message}"); }
            }
        }
        
        public override bool Activate()
        {
            if (!base.Activate()) return false;
            // TODO: Implement steal ingredient logic
            return true;
        }
        
        private class StealParameters { public float StealRadius { get; set; } = 2f; }
    }
    
    public class PreventBurningAbility : CharacterAbility
    {
        public override bool Activate()
        {
            if (!base.Activate()) return false;
            // TODO: Implement prevent burning logic
            return true;
        }
        
        public override void Deactivate()
        {
            if (!IsActive) return;
            // TODO: Implement restore burning logic
            base.Deactivate();
        }
    }

    public class AutoPlateAbility : CharacterAbility
    {
        public override bool Activate()
        {
            if (!base.Activate()) return false;
            // TODO: Implement auto plate logic
            return true;
        }
    }
    
    public class IngredientMagnetAbility : CharacterAbility
    {
        private float _magnetRadius = 5f;
        private float _magnetForce = 10f;
        
        public override void Initialize(CharacterClass characterClass, PlayerController playerController)
        {
            base.Initialize(characterClass, playerController);
            if (!string.IsNullOrEmpty(characterClass.PrimaryAbilityParameters))
            {
                try
                {
                    MagnetParameters parameters = JsonConvert.DeserializeObject<MagnetParameters>(characterClass.PrimaryAbilityParameters);
                    if (parameters != null)
                    {
                        _magnetRadius = parameters.MagnetRadius;
                        _magnetForce = parameters.MagnetForce;
                    }
                }
                catch (Exception e) { GameLogger.LogError($"Failed to parse magnet parameters: {e.Message}"); }
            }
        }
        
        public override bool Activate()
        {
            if (!base.Activate()) return false;
            // TODO: Implement ingredient magnet logic
            return true;
        }
        
        public override void Deactivate()
        {
            if (!IsActive) return;
            // TODO: Implement disable magnet logic
            base.Deactivate();
        }
        
        private class MagnetParameters { public float MagnetRadius { get; set; } = 5f; public float MagnetForce { get; set; } = 10f; }
    }
}
