using System.Collections;
using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>
    /// Cloud file storage (EOS Player Data Storage in production).
    /// </summary>
    public interface IStorageService
    {
        bool IsReady { get; }
        IEnumerator Initialize();
        Task<bool> WriteFile(string key, byte[] data);
        Task<byte[]> ReadFile(string key);
        Task<bool> DeleteFile(string key);
    }
}
