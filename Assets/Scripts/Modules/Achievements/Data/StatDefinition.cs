using System;

namespace RecipeRage.Modules.Achievements.Interfaces
{
    /// <summary>
    /// Stat definition from provider
    /// Contains static stat data from provider
    /// Complexity Rating: 1
    /// </summary>
    [Serializable]
    public class StatDefinition
    {
        /// <summary>
        /// The aggregation method for the stat
        /// </summary>
        public enum AggregationMethod
        {
            /// <summary>
            /// Use the latest value
            /// </summary>
            Latest,

            /// <summary>
            /// Sum the values
            /// </summary>
            Sum,

            /// <summary>
            /// Use the maximum value
            /// </summary>
            Max,

            /// <summary>
            /// Use the minimum value
            /// </summary>
            Min,

            /// <summary>
            /// Calculate the average value
            /// </summary>
            Average
        }

        /// <summary>
        /// Create a new stat definition
        /// </summary>
        /// <param name="name"> Stat name </param>
        /// <param name="displayName"> Display name </param>
        /// <param name="provider"> Provider name </param>
        public StatDefinition(string name, string displayName, string provider)
        {
            Name = name;
            DisplayName = displayName;
            Provider = provider;

            Description = string.Empty;
            Aggregation = AggregationMethod.Latest;
            DefaultValue = 0;
            MinValue = double.MinValue;
            MaxValue = double.MaxValue;
            ProviderData = string.Empty;
        }

        /// <summary>
        /// Default constructor for serialization
        /// </summary>
        public StatDefinition()
        {
            Name = string.Empty;
            DisplayName = string.Empty;
            Provider = string.Empty;

            Description = string.Empty;
            Aggregation = AggregationMethod.Latest;
            DefaultValue = 0;
            MinValue = double.MinValue;
            MaxValue = double.MaxValue;
            ProviderData = string.Empty;
        }

        /// <summary>
        /// Stat name (unique identifier)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Display name (human-readable)
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Stat description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Aggregation method
        /// </summary>
        public AggregationMethod Aggregation { get; set; }

        /// <summary>
        /// Default value
        /// </summary>
        public double DefaultValue { get; set; }

        /// <summary>
        /// Minimum value
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// Maximum value
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// Provider-specific data
        /// </summary>
        public string ProviderData { get; set; }

        /// <summary>
        /// Provider name
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Create a PlayerStat from this definition
        /// </summary>
        /// <returns> PlayerStat object </returns>
        public PlayerStat CreatePlayerStat()
        {
            var stat = new PlayerStat
            {
                Name = Name,
                DisplayName = DisplayName,
                Value = DefaultValue,
                Provider = Provider,
                ProviderData = ProviderData
            };

            return stat;
        }

        /// <summary>
        /// Get aggregated value based on current value and new value
        /// </summary>
        /// <param name="currentValue"> Current stat value </param>
        /// <param name="newValue"> New stat value to aggregate </param>
        /// <param name="count"> Count for average calculation </param>
        /// <returns> Aggregated value </returns>
        public double GetAggregatedValue(double currentValue, double newValue, int count = 1)
        {
            double result;

            switch (Aggregation)
            {
                case AggregationMethod.Sum:
                    result = currentValue + newValue;
                    break;

                case AggregationMethod.Max:
                    result = Math.Max(currentValue, newValue);
                    break;

                case AggregationMethod.Min:
                    result = Math.Min(currentValue, newValue);
                    break;

                case AggregationMethod.Average:
                    if (count <= 1)
                    {
                        result = newValue;
                    }
                    else
                    {
                        // Calculate running average
                        result = currentValue + (newValue - currentValue) / count;
                    }
                    break;

                case AggregationMethod.Latest:
                default:
                    result = newValue;
                    break;
            }

            // Clamp to min/max values
            result = Math.Max(MinValue, Math.Min(MaxValue, result));

            return result;
        }
    }
}