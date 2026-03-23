using System.Collections.Generic;

namespace Gameplay.Match
{
    /// <summary>
    /// Provides the launch queue catalog and queue-specific gameplay tuning.
    /// </summary>
    public interface IMatchService
    {
        IReadOnlyList<MatchQueueDefinition> GetQueues();
        bool TryGetQueue(string modeId, out MatchQueueDefinition queue);
    }
}
