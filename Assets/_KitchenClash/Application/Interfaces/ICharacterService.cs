using System;
using KitchenClash.Application.Models;
using KitchenClash.Domain;
using System.Collections.Generic;

namespace KitchenClash.Application
{
    public interface ICharacterService
    {
        event Action<CharacterClass> OnCharacterSelected;

        // ── New GDD v3 chef API ──

        /// <summary>Currently selected chef definition for this session.</summary>
        ChefDefinition SelectedChef { get; }

        /// <summary>All 6 GDD chefs.</summary>
        IReadOnlyList<ChefDefinition> GetAllChefs();

        /// <summary>Chefs unlocked for the current player.</summary>
        IReadOnlyList<ChefDefinition> GetUnlockedChefs();

        /// <summary>Whether a specific chef is unlocked.</summary>
        bool IsUnlocked(ChefId chefId);

        /// <summary>Select a chef for the current session. Returns false if locked.</summary>
        bool SelectChef(ChefId chefId);

        /// <summary>Attempt to purchase a shop-locked chef. Returns false if insufficient funds.</summary>
        bool TryPurchaseChef(ChefId chefId);

        // ── Legacy SO-based API (used by existing Presentation layer) ──

        CharacterClass SelectedCharacter { get; }
        CharacterClass[] GetAvailableCharacters();
        CharacterClass[] GetUnlockedCharacters();
        CharacterClass GetCharacter(int id);
        bool IsUnlocked(int characterId);
        bool Unlock(int characterId);
        bool SelectCharacter(int characterId);
    }
}
