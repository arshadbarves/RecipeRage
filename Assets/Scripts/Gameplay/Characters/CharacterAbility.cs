using System;
using Core.Logging;
using UnityEngine;

namespace Gameplay.Characters
{
    /// <summary>
    /// Base class for character abilities in RecipeRage.
    /// </summary>
    public abstract class CharacterAbility
    {
        protected CharacterClass CharacterClass { get; private set; }
        protected PlayerController PlayerController { get; private set; }
        protected CharacterAbilityData AbilityData { get; private set; }

        public AbilityType AbilityType => AbilityData?.AbilityType ?? AbilityType.None;
        public float Cooldown => AbilityData?.Cooldown ?? 30f;
        public float Duration => AbilityData?.Duration ?? 0f;
        public Sprite Icon => AbilityData?.Icon;
        public string Description => AbilityData?.Description ?? string.Empty;

        public float CooldownTimer { get; protected set; }
        public float DurationTimer { get; protected set; }

        public bool IsOnCooldown => CooldownTimer > 0f;
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
        
        public virtual void Initialize(CharacterClass characterClass, PlayerController playerController)
        {
            CharacterClass = characterClass;
            PlayerController = playerController;
            AbilityData = characterClass.PrimaryAbility;

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
    
    public class NoneAbility : CharacterAbility
    {
        public override bool Activate()
        {
            return false;
        }
    }
    
    public class SpeedBoostAbility : CharacterAbility
    {
        private float _originalSpeed;

        public override bool Activate()
        {
            if (!base.Activate()) return false;

            _originalSpeed = PlayerController.MovementSpeed;
            PlayerController.MovementSpeed *= AbilityData.SpeedMultiplier;

            return true;
        }

        public override void Deactivate()
        {
            if (!IsActive) return;

            PlayerController.MovementSpeed = _originalSpeed;
            base.Deactivate();
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
        public override bool Activate()
        {
            if (!base.Activate()) return false;
            // TODO: Implement push other players logic using AbilityData.PushForce and AbilityData.Range
            return true;
        }
    }
    
    public class StealIngredientAbility : CharacterAbility
    {
        public override bool Activate()
        {
            if (!base.Activate()) return false;
            // TODO: Implement steal ingredient logic using AbilityData.Range
            return true;
        }
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
        public override bool Activate()
        {
            if (!base.Activate()) return false;
            // TODO: Implement ingredient magnet logic using AbilityData.Range and AbilityData.PushForce
            return true;
        }

        public override void Deactivate()
        {
            if (!IsActive) return;
            // TODO: Implement disable magnet logic
            base.Deactivate();
        }
    }
}
