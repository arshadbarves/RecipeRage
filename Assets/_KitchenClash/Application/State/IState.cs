using System.Threading.Tasks;

namespace KitchenClash.Application
{
    public interface IState
    {
        Task EnterAsync();
        Task ExitAsync();
        void Tick();
    }
}
