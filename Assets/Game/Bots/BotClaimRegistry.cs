using System.Collections.Generic;

namespace RecipeRage.Bots
{
    /// <summary>
    /// Prevents two bots targeting the same station. Server-side only;
    /// human players are unaffected (they coordinate socially).
    /// </summary>
    public sealed class BotClaimRegistry
    {
        private readonly Dictionary<StationBase, int> _claims = new Dictionary<StationBase, int>(16);

        public bool TryClaim(StationBase station, int botId)
        {
            if (_claims.ContainsKey(station))
            {
                return false;
            }
            _claims[station] = botId;
            return true;
        }

        public void Release(StationBase station, int botId)
        {
            if (_claims.TryGetValue(station, out var owner) && owner == botId)
            {
                _claims.Remove(station);
            }
        }

        public bool IsClaimed(StationBase station) => _claims.ContainsKey(station);

        public void ReleaseAll(int botId)
        {
            var toRemove = new List<StationBase>();
            foreach (var kvp in _claims)
            {
                if (kvp.Value == botId)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var station in toRemove)
            {
                _claims.Remove(station);
            }
        }
    }
}
