namespace KitchenClash.Domain
{
    /// <summary>
    /// Pure data block holding a chef's base stats. No Unity dependency.
    /// </summary>
    public sealed class ChefStatBlock
    {
        public float MoveSpeed { get; set; } = 1.0f;
        public float CookSpeedMult { get; set; } = 1.0f;
        public float BurnResistance { get; set; } = 0.0f;
        public int CarryCapacity { get; set; } = 1;
        public float InteractRange { get; set; } = 1.0f;
        public float ScoreMultiplier { get; set; } = 1.0f;

        public ChefStatBlock() { }

        public ChefStatBlock(float moveSpeed, float cookSpeedMult, float burnResistance, int carryCapacity, float interactRange, float scoreMultiplier = 1.0f)
        {
            MoveSpeed = moveSpeed;
            CookSpeedMult = cookSpeedMult;
            BurnResistance = burnResistance;
            CarryCapacity = carryCapacity;
            InteractRange = interactRange;
            ScoreMultiplier = scoreMultiplier;
        }
    }
}
