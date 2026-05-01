namespace KitchenClash.Domain
{
    public sealed class ChefDefinition
    {
        public ChefId Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string UnlockRequirement { get; }

        public ChefDefinition(ChefId id, string name, string description, string unlockRequirement)
        {
            Id = id;
            Name = name;
            Description = description;
            UnlockRequirement = unlockRequirement;
        }
    }
}
