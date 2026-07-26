using System.Collections.Generic;
using KitchenClash.Application.Models;

namespace KitchenClash.Application
{
    public interface ISkinsService
    {
        List<SkinItem> GetAllSkins();
        List<SkinItem> GetSkinsForCharacter(int characterId);
        SkinItem GetDefaultSkinForCharacter(int characterId);
        SkinItem GetSkin(string skinId);
        bool IsSkinUnlocked(string skinId);
        bool UnlockSkin(string skinId);
        SkinItem GetEquippedSkin(int characterId);
        bool EquipSkin(int characterId, string skinId);
    }
}
