using System;
using System.Threading.Tasks;

namespace Modules.Core.Banking.Interfaces
{
    public interface IBankService
    {
        // Generic Currency
        long GetBalance(string currencyId);
        void ModifyBalance(string currencyId, long amount); // Can be positive (add) or negative (spend)

        // Generic Inventory
        bool HasItem(string itemId);
        void AddItem(string itemId);

        // Generic Data
        string GetData(string key);
        void SetData(string key, string value);

        // Transaction
        bool Purchase(string itemId, long cost, string currencyId);

        // Events
        event Action<string, long> OnBalanceChanged; // currencyId, newBalance
        event Action<string> OnItemUnlocked;         // itemId
        
        // Initialization
        Task InitializeAsync();
    }
}
