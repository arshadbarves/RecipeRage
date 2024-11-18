using System.Threading.Tasks;

namespace Core.Data.Save
{
    public interface ISaveSystem
    {
        Task<bool> SaveData<T>(string key, T data);
        Task<T> LoadData<T>(string key);
        Task<bool> DeleteData(string key);
        Task<bool> HasData(string key);
        Task<bool> ClearAll();
    }
}