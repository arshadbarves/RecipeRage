using System.Threading.Tasks;
using Core.Banking.Data;

namespace Core.Banking.Interfaces
{
    public interface IBankBackend
    {
        Task<BankData> LoadDataAsync();
        Task SaveDataAsync(BankData data);
    }
}
