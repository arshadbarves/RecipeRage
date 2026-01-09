using System.Threading.Tasks;
using Core.Core.Banking.Data;

namespace Core.Core.Banking.Interfaces
{
    public interface IBankBackend
    {
        Task<BankData> LoadDataAsync();
        Task SaveDataAsync(BankData data);
    }
}
