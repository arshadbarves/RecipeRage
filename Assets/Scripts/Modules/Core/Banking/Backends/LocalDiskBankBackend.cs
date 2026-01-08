using System.Threading.Tasks;
using Core.SaveSystem;
using Modules.Core.Banking.Data;
using Modules.Core.Banking.Interfaces;

namespace Modules.Core.Banking.Backends
{
    public class LocalDiskBankBackend : IBankBackend
    {
        private const string SAVE_KEY = "player_bank_data";
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
                data = new BankData(1250, 85); // Default values
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
