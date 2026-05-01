namespace KitchenClash.Domain
{
    public sealed class DailyStreakReward
    {
        public int Day { get; }
        public string RewardType { get; }
        public int Amount { get; }
        public string Description { get; }

        public DailyStreakReward(int day, string rewardType, int amount, string description)
        {
            Day = day;
            RewardType = rewardType;
            Amount = amount;
            Description = description;
        }
    }
}
