using KitchenClash.Application.Models;

namespace KitchenClash.Application
{
    public sealed class CharacterSelectedEvent
    {
        public CharacterClass Character { get; set; }
    }

    public sealed class SkinEquippedEvent
    {
        public int CharacterId { get; set; }
        public string SkinId { get; set; }
    }
}
