using System;
using System.Threading.Tasks;

namespace KitchenClash.Domain
{
    public interface IDailyStreakService
    {
        int CurrentDay { get; }
        int CycleDays { get; }
        Task LoadAsync();
        Task SaveAsync();
        bool CanClaim(DateTime utcNow);
        DailyStreakReward Claim(DateTime utcNow);
    }
}
