using System;

namespace Gameplay.Cooking
{
    /// <summary>
    /// Represents an order in the game.
    /// </summary>
    [Serializable]
    public class Order
    {
        /// <summary>
        /// The order ID.
        /// </summary>
        public string Id;
        
        /// <summary>
        /// The recipe for this order.
        /// </summary>
        public Recipe Recipe;
        
        /// <summary>
        /// The time limit for this order.
        /// </summary>
        public float TimeLimit;
        
        /// <summary>
        /// The remaining time for this order.
        /// </summary>
        public float RemainingTime;
        
        /// <summary>
        /// The timestamp when this order was created.
        /// </summary>
        public DateTime Timestamp;
    }
}
