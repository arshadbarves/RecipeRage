using System;
using System.Collections.Generic;
using Modules.Skins.Data;

namespace Modules.Skins
{
    /// <summary>
    /// Interface for skins service
    /// Manages character skins and their unlock status
    /// </summary>
    public interface ISkinsService
    {
        /// <summary>
        /// Get all available skins
        /// </summary>
        List<SkinItem> GetAllSkins();
        
        /// <summary>
        /// Get skins for a specific character
        /// </summary>
        List<SkinItem> GetSkinsForCharacter(int characterId);
        
        /// <summary>
        /// Get the default skin for a character
        /// </summary>
        SkinItem GetDefaultSkinForCharacter(int characterId);
        
        /// <summary>
        /// Get a specific skin by ID
        /// </summary>
        SkinItem GetSkin(string skinId);
        
        /// <summary>
        /// Check if a skin is unlocked for the player
        /// </summary>
        bool IsSkinUnlocked(string skinId);
        
        /// <summary>
        /// Unlock a skin for the player
        /// </summary>
        bool UnlockSkin(string skinId);
        
        /// <summary>
        /// Get the currently equipped skin for a character
        /// </summary>
        SkinItem GetEquippedSkin(int characterId);
        
        /// <summary>
        /// Equip a skin for a character
        /// </summary>
        bool EquipSkin(int characterId, string skinId);
        
        /// <summary>
        /// Event triggered when a skin is unlocked
        /// </summary>
        event Action<string> OnSkinUnlocked;
        
        /// <summary>
        /// Event triggered when a skin is equipped
        /// </summary>
        event Action<int, string> OnSkinEquipped;
    }
}
