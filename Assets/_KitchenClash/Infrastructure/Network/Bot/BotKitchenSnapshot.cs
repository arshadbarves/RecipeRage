using KitchenClash.Application.Services;
using KitchenClash.Infrastructure.Network.Stations;
using UnityEngine;

namespace KitchenClash.Infrastructure.Network.Bot
{
    public sealed class BotKitchenSnapshot
    {
        private readonly IMatchContext _matchContext;

        public BotKitchenSnapshot(IMatchContext matchContext)
        {
            _matchContext = matchContext;
        }

        public BotPlanningSnapshot Capture(PlayerController player, string botId, BotClaimRegistry claimRegistry)
        {
            return new BotPlanningSnapshot();
        }

        public Component ResolveStation(string stationId)
        {
            return null;
        }

        public CounterStation FindNearestAvailableCounter(Vector3 position, string botId, BotClaimRegistry claimRegistry)
        {
            return null;
        }
    }
}
