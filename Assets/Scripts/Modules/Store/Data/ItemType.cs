using System;

namespace RecipeRage.Store
{
    /// <summary>
    /// Enumeration of item types in the store
    /// </summary>
    public enum ItemType
    {
        /// <summary>
        /// Unknown or undefined item type
        /// </summary>
        Unknown,
        
        /// <summary>
        /// Consumable item, can be used multiple times, decreases quantity
        /// </summary>
        Consumable,
        
        /// <summary>
        /// Durable item, owned permanently once purchased
        /// </summary>
        Durable,
        
        /// <summary>
        /// Currency item, represents virtual currency
        /// </summary>
        Currency,
        
        /// <summary>
        /// Bundle item, contains multiple other items
        /// </summary>
        Bundle,
        
        /// <summary>
        /// Subscription item, provides benefits for a limited time
        /// </summary>
        Subscription,
        
        /// <summary>
        /// Feature item, unlocks a feature in the game
        /// </summary>
        Feature,
        
        /// <summary>
        /// Cosmetic item, changes appearance without affecting gameplay
        /// </summary>
        Cosmetic,
        
        /// <summary>
        /// Booster item, provides temporary gameplay benefits
        /// </summary>
        Booster,
        
        /// <summary>
        /// Recipe item, provides crafting recipes
        /// </summary>
        Recipe
    }
} 