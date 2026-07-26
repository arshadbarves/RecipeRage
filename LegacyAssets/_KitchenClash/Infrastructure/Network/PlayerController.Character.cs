using KitchenClash.Application;
using KitchenClash.Application.Models;
using KitchenClash.Application.Services;
using KitchenClash.Infrastructure.Gameplay;
using Unity.Netcode;
using UnityEngine;
using KitchenClash.Domain;
using VContainer.Unity;
using Playcenter.Shell;


namespace KitchenClash.Infrastructure.Network
{
    public partial class PlayerController
    {
        #region Character Class Management

        private void SetupCharacterClass()
        {
            if (_characterService == null)
            {
                GameLogger.LogError("Character service not available");
                return;
            }

            // Apply GDD chef stats from the selected ChefDefinition
            ChefDefinition selectedChef = _characterService.SelectedChef;
            if (selectedChef != null && _movementController != null)
            {
                ChefStatBlock stats = selectedChef.Stats;
                _movementController.MovementSpeed = _baseMovementSpeed * stats.MoveSpeed;
                InteractionSpeed.BaseValue = stats.InteractRange;
                CarryingCapacity.BaseValue = stats.CarryCapacity;
            }

            // Legacy SO-based character class (skins, ability prefab data)
            CharacterClass = _characterService.SelectedCharacter;
            if (CharacterClass != null)
            {
                _characterClassId = CharacterClass.Id;
            }
            else
            {
                // Fallback: lookup SO by name match via Resources
                CharacterClass[] allClasses = Resources.LoadAll<CharacterClass>("ScriptableObjects/CharacterClasses");
                foreach (CharacterClass cc in allClasses)
                {
                    if (cc != null && cc.DisplayName == selectedChef?.DisplayName)
                    {
                        CharacterClass = cc;
                        _characterClassId = cc.Id;
                        break;
                    }
                }
            }

            if (CharacterClass != null)
            {
                PrimaryAbility = CharacterAbility.CreateAbility(
                    CharacterClass.PrimaryAbility != null ? CharacterClass.PrimaryAbility.AbilityType : AbilityType.None,
                    CharacterClass, this);
            }

            // Register chef abilities with AbilityService (Phase 8)
            if (selectedChef != null)
            {
                // AbilityService is resolved by MatchLifetimeScope; access via service locator pattern
                IAbilityService abilityService = FindAbilityService();
                if (abilityService != null)
                {
                    abilityService.RegisterChefAbilities(selectedChef.Id);
                    GameLogger.Log($"[AbilityService] Registered abilities for {selectedChef.DisplayName}");
                }
            }

            EnsureValidSkinForCharacter();
        }

        public void SetCharacterClass(int characterClassId)
        {
            _characterClassId = characterClassId;
            SetupCharacterClass();

            if (IsLocalPlayer)
            {
                SetCharacterClassServerRpc(characterClassId);
            }
        }

        [ServerRpc]
        private void SetCharacterClassServerRpc(int characterClassId)
        {
            _characterClassId = characterClassId;
            SetCharacterClassClientRpc(characterClassId);
        }

        [ClientRpc]
        private void SetCharacterClassClientRpc(int characterClassId)
        {
            if (IsLocalPlayer)
            {
                return;
            }

            _characterClassId = characterClassId;
            SetupCharacterClass();
        }

        #endregion

    }
}
