using System.Threading.Tasks;

namespace GameSystem
{
    public interface IGameSystem
    {
        Task InitializeAsync();
        void Update();
        Task CleanupAsync();
    }
}