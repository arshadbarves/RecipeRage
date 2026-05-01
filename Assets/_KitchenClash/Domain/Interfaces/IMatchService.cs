using System.Collections.Generic;

namespace KitchenClash.Domain
{
    public interface IMatchService
    {
        IReadOnlyList<MatchQueueDefinition> GetQueues();
        bool TryGetQueue(string modeId, out MatchQueueDefinition queue);
        string GetCurrentMap(string queueId);
    }
}
