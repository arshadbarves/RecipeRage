using KitchenClash.Domain.Enums;
using Playcenter.Services;

namespace KitchenClash.Domain
{
    /// <summary>Published when a player is KO'd.</summary>
    public sealed class PlayerKoEvent
    {
        public ulong  VictimClientId   { get; }
        public ulong  AttackerClientId { get; }
        public int    VictimTeamId     { get; }

        public PlayerKoEvent(ulong victim, ulong attacker, int victimTeam)
        {
            VictimClientId   = victim;
            AttackerClientId = attacker;
            VictimTeamId     = victimTeam;
        }
    }

    /// <summary>Published when a KO'd player returns from respawn.</summary>
    public sealed class PlayerRespawnedEvent
    {
        public ulong ClientId { get; }
        public int   TeamId   { get; }

        public PlayerRespawnedEvent(ulong clientId, int teamId)
        {
            ClientId = clientId;
            TeamId   = teamId;
        }
    }

    /// <summary>Published on HP change (used by HUD to update health bar).</summary>
    public sealed class PlayerHpChangedEvent
    {
        public ulong ClientId  { get; }
        public int   CurrentHp { get; }
        public int   MaxHp     { get; }

        public PlayerHpChangedEvent(ulong clientId, int current, int max)
        {
            ClientId  = clientId;
            CurrentHp = current;
            MaxHp     = max;
        }
    }
}
