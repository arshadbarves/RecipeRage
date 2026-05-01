namespace KitchenClash.Application.Services
{
    public sealed class BotOrderDescriptor
    {
        public int OrderId { get; set; }
        public int RecipeId { get; set; }
        public bool IsExpired { get; set; }
        public bool IsCompleted { get; set; }
        public bool HasInvalidAssembly { get; set; }
        public float RemainingTime { get; set; }
        public float Priority { get; set; }
    }
}
