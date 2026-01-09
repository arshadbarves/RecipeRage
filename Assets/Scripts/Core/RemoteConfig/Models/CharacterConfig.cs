using System;
using System.Collections.Generic;
using Core.RemoteConfig.Interfaces;
using Newtonsoft.Json;

namespace Core.RemoteConfig.Models
{
    /// <summary>
    /// Configuration model for character definitions
    /// </summary>
    [Serializable]
    public class CharacterConfig : IConfigModel
    {
        [JsonProperty("characters")]
        public List<CharacterDefinition> Characters { get; set; }

        public CharacterConfig()
        {
            Characters = new List<CharacterDefinition>();
        }

        public bool Validate()
        {
            if (Characters == null || Characters.Count == 0)
            {
                return false;
            }

            foreach (var character in Characters)
            {
                if (string.IsNullOrEmpty(character.CharacterId))
                {
                    return false;
                }

                if (character.MovementSpeed <= 0 || character.CookingSpeed <= 0)
                {
                    return false;
                }
            }

            return true;
        }
    }

    [Serializable]
    public class CharacterDefinition
    {
        [JsonProperty("characterId")]
        public string CharacterId { get; set; }

        [JsonProperty("characterName")]
        public string CharacterName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("rarity")]
        public string Rarity { get; set; }

        // Stats
        [JsonProperty("movementSpeed")]
        public float MovementSpeed { get; set; }

        [JsonProperty("cookingSpeed")]
        public float CookingSpeed { get; set; }

        [JsonProperty("carryCapacity")]
        public int CarryCapacity { get; set; }

        [JsonProperty("specialAbilityId")]
        public string SpecialAbilityId { get; set; }

        [JsonProperty("abilityParameters")]
        public string AbilityParameters { get; set; }

        // Unlock Requirements
        [JsonProperty("unlockLevel")]
        public int UnlockLevel { get; set; }

        [JsonProperty("unlockCost")]
        public int UnlockCost { get; set; }

        [JsonProperty("unlockCurrency")]
        public string UnlockCurrency { get; set; }

        [JsonProperty("isStarterCharacter")]
        public bool IsStarterCharacter { get; set; }

        // Asset References
        [JsonProperty("prefabAddress")]
        public string PrefabAddress { get; set; }

        [JsonProperty("iconAddress")]
        public string IconAddress { get; set; }

        [JsonProperty("portraitAddress")]
        public string PortraitAddress { get; set; }

        public CharacterDefinition()
        {
            MovementSpeed = 5.0f;
            CookingSpeed = 1.0f;
            CarryCapacity = 1;
            UnlockCurrency = "Coins";
        }
    }
}
