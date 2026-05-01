using System.Collections.Generic;

namespace KitchenClash.Domain
{
    public interface IMatchService
    {
        IReadOnlyList<MatchQueueDefinition> GetQueues();
        bool TryGetQueue(string modeId, out MatchQueueDefinition queue);
        string GetCurrentMap(string queueId);

        /// <summary>
        /// Returns true if a player at the given level meets the queue's minimum level requirement.
        /// </summary>
        bool CanJoinQueue(string queueId, int playerLevel);
    }
}
