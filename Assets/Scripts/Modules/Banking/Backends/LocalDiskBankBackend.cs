using System.Threading.Tasks;
using Modules.Persistence;
using Modules.Core.Banking.Data;
using Modules.Core.Banking.Interfaces;

namespace Modules.Banking.Backends
{
    public class LocalDiskBankBackend : IBankBackend
    {
        private const string SAVE_KEY = BankKeys.SaveKey;
        private readonly ISaveService _saveService;

        public LocalDiskBankBackend(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public Task<BankData> LoadDataAsync()
        {
            BankData data = _saveService.LoadData<BankData>(SAVE_KEY);
            if (data == null)
            {
                data = new BankData();
                data.Balances[BankKeys.CurrencyCoins] = 1250;
                data.Balances[BankKeys.CurrencyGems] = 85;
            }
            return Task.FromResult(data);
        }

        public Task SaveDataAsync(BankData data)
        {
            _saveService.SaveData(SAVE_KEY, data);
            return Task.CompletedTask;
        }
    }
}