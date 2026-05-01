using System.Collections.Generic;

namespace KitchenClash.Application.Services
{
    public sealed class BotClaimRegistry
    {
        public static BotClaimRegistry Shared { get; } = new BotClaimRegistry();

        private readonly Dictionary<int, string> _orderClaims = new Dictionary<int, string>();
        private readonly Dictionary<string, int> _botOrders = new Dictionary<string, int>();
        private readonly Dictionary<int, string> _orderCounters = new Dictionary<int, string>();

        public bool TryClaimOrder(int orderId, string botId)
        {
            if (string.IsNullOrWhiteSpace(botId)) return false;
            if (_orderClaims.TryGetValue(orderId, out string existingBot)) return existingBot == botId;
            ReleaseOrderForBot(botId);
            _orderClaims[orderId] = botId;
            _botOrders[botId] = orderId;
            return true;
        }

        public void AssignCounter(int orderId, string counterId)
        {
            if (!string.IsNullOrWhiteSpace(counterId)) _orderCounters[orderId] = counterId;
        }

        public bool IsCounterClaimedByAnotherBot(string counterId, string botId)
        {
            if (string.IsNullOrWhiteSpace(counterId)) return false;
            foreach (var pair in _orderCounters)
            {
                if (pair.Value != counterId) continue;
                if (_orderClaims.TryGetValue(pair.Key, out string claimant) && claimant != botId) return true;
            }
            return false;
        }

        public int? GetClaimedOrderId(string botId)
        {
            return !string.IsNullOrWhiteSpace(botId) && _botOrders.TryGetValue(botId, out int orderId) ? orderId : null;
        }

        public string GetClaimedCounterId(int orderId)
        {
            return _orderCounters.TryGetValue(orderId, out string counterId) ? counterId : null;
        }

        public bool IsOrderClaimedByAnotherBot(int orderId, string botId)
        {
            return _orderClaims.TryGetValue(orderId, out string claimant) && claimant != botId;
        }

        public void ReleaseOrder(int orderId)
        {
            if (_orderClaims.TryGetValue(orderId, out string botId)) _botOrders.Remove(botId);
            _orderClaims.Remove(orderId);
            _orderCounters.Remove(orderId);
        }

        public void ReleaseOrderForBot(string botId)
        {
            if (string.IsNullOrWhiteSpace(botId)) return;
            if (_botOrders.TryGetValue(botId, out int orderId))
            {
                _botOrders.Remove(botId);
                _orderClaims.Remove(orderId);
                _orderCounters.Remove(orderId);
            }
        }

        public void Clear()
        {
            _orderClaims.Clear();
            _botOrders.Clear();
            _orderCounters.Clear();
        }
    }
}
