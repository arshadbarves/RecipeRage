using System.Threading.Tasks;
using Core.SaveSystem;
using Modules.Core.Banking.Data;
using Modules.Core.Banking.Interfaces;
using UnityEngine;

namespace Modules.Core.Banking.Backends
{
    public class EOSBankBackend : IBankBackend
    {
        private const string SAVE_KEY = BankKeys.SaveKey;
        private readonly EOSCloudStorageProvider _provider;

        public EOSBankBackend()
        {
            _provider = new EOSCloudStorageProvider();
        }

        public async Task<BankData> LoadDataAsync()
        {
            if (!_provider.IsAvailable)
            {
                // Fallback to defaults if not logged in
                var defaultData = new BankData();
                defaultData.Balances[BankKeys.CurrencyCoins] = 1250;
                defaultData.Balances[BankKeys.CurrencyGems] = 85;
                return defaultData;
            }

            string json = await _provider.ReadAsync(SAVE_KEY);
            if (string.IsNullOrEmpty(json))
            {
                var data = new BankData();
                data.Balances[BankKeys.CurrencyCoins] = 1250;
                data.Balances[BankKeys.CurrencyGems] = 85;
                return data;
            }

            return JsonUtility.FromJson<BankData>(json);
        }

        public async Task SaveDataAsync(BankData data)
        {
            if (!_provider.IsAvailable) return;

            string json = JsonUtility.ToJson(data);
            await _provider.WriteAsync(SAVE_KEY, json);
        }
    }
}
