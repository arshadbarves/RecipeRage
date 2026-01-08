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
                Debug.LogError("EOSCloudStorageProvider is not available, This should never happen");
                return null;
            }

            string json = await _provider.ReadAsync(SAVE_KEY);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("No data found in EOSCloudStorageProvider, This should never happen on first login");
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
