using System.Threading.Tasks;
using KitchenClash.Application;
using KitchenClash.Application.Models;
using KitchenClash.Application.Services;
using KitchenClash.Domain;
using Playcenter.Services;
using Playcenter.Shell;

namespace KitchenClash.Infrastructure.Services
{
    /// <summary>Maps a purchased productId to RecipeRage gems via IAPCatalog → IEconomyService.</summary>
    public sealed class RecipeRageIapRewardGrantor : IIapRewardGrantor
    {
        private readonly ISessionContext _sessionContext;

        public RecipeRageIapRewardGrantor(ISessionContext sessionContext)
        {
            _sessionContext = sessionContext;
        }

        public Task GrantAsync(string productId)
        {
            IAPItem item = IAPCatalog.GetById(productId);
            if (item == null)
            {
                GameLogger.LogWarning($"[RecipeRageIapRewardGrantor] Unknown productId: {productId}");
                return Task.CompletedTask;
            }

            IEconomyService economy = _sessionContext?.EconomyService;
            if (item.Gems > 0 && economy != null)
            {
                economy.AddGems(item.Gems);
                GameLogger.Log($"[RecipeRageIapRewardGrantor] Granted {item.Gems} gems for {productId}");
            }
            return Task.CompletedTask;
        }
    }
}
