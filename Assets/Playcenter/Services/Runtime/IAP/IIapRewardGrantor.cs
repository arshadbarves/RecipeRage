using System.Threading.Tasks;

namespace Playcenter.Services
{
    /// <summary>Maps a purchased productId to the game's currency grant. Game-supplied.</summary>
    public interface IIapRewardGrantor
    {
        Task GrantAsync(string productId);
    }
}
