using System.Collections.Generic;

namespace Playcenter.MobileCore
{
    /// <summary>
    /// Generic ownership registry: bots claim targets (stations, orders) so two bots
    /// never commit to the same one. Ported from KitchenClash BotClaimRegistry and generalized.
    /// </summary>
    public sealed class ClaimRegistry<TKey>
    {
        private readonly Dictionary<TKey, string> _claims = new Dictionary<TKey, string>();
        private readonly Dictionary<string, TKey> _ownerClaims = new Dictionary<string, TKey>();

        public bool TryClaim(TKey key, string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return false;
            }

            if (_claims.TryGetValue(key, out string existing))
            {
                return existing == ownerId;
            }

            // one claim per owner: release previous before taking a new one
            if (_ownerClaims.TryGetValue(ownerId, out TKey previous))
            {
                _claims.Remove(previous);
            }

            _claims[key] = ownerId;
            _ownerClaims[ownerId] = key;
            return true;
        }

        public bool Release(TKey key, string ownerId)
        {
            if (!_claims.TryGetValue(key, out string existing) || existing != ownerId)
            {
                return false;
            }

            _claims.Remove(key);
            _ownerClaims.Remove(ownerId);
            return true;
        }

        public bool IsClaimedByOther(TKey key, string ownerId)
        {
            return _claims.TryGetValue(key, out string existing) && existing != ownerId;
        }
    }
}
