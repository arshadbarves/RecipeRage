using System;

namespace KitchenClash.Domain
{
    public sealed class CompleteResult
    {
        public bool Success { get; set; }
        public int Score { get; set; }
        public Guid OrderId { get; set; }
        public float TimeBonus { get; set; }
        public int ComboMultiplier { get; set; }
    }
}
