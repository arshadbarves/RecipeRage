using System.Threading.Tasks;
using Modules.Core.Banking.Data;

namespace Modules.Core.Banking.Interfaces
{
    public interface IBankBackend
    {
        Task<BankData> LoadDataAsync();
        Task SaveDataAsync(BankData data);
    }
}
