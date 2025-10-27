namespace Core.Networking.Common
{
    /// <summary>
    /// Network message types for P2P communication
    /// </summary>
    public static partial class NetworkMessageType
    {
        public const byte ConnectionRequest = 0;
        public const byte PlayerAction = 1;
        public const byte ChatMessage = 2;
        public const byte Emote = 3;
        public const byte GameState = 4;
        public const byte TeamScore = 5;
        public const byte IngredientSpawned = 6;
        public const byte IngredientPickedUp = 7;
        public const byte IngredientDropped = 8;
        public const byte IngredientProcessed = 9;
        public const byte RecipeCompleted = 10;
        public const byte RecipeFailed = 11;
    }
}
