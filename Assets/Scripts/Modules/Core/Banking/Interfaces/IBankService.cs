using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Currency;
using Core.Skins.Data;

namespace Modules.Core.Banking.Interfaces
{
    public interface IBankService : ICurrencyService
    {
        // Inventory / Skins Ownership & State
        List<string> GetUnlockedSkins();
        bool IsSkinUnlocked(string skinId);
        bool UnlockSkin(string skinId);
        string GetEquippedSkinId(int characterId);
        bool EquipSkin(int characterId, string skinId);

        event Action<string> OnSkinUnlocked;
        event Action<int, string> OnSkinEquipped;
        
        // Initialization
        Task InitializeAsync();
    }
}