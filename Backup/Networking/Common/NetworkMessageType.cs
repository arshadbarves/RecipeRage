namespace RecipeRage.Core.Networking.Common
{
    /// <summary>
    /// Constants for network message types.
    /// </summary>
    public static class NetworkMessageType
    {
        // System messages (0-9)
        public const byte Heartbeat = 0;
        public const byte Disconnect = 1;
        public const byte PlayerInfo = 2;
        
        // Lobby messages (10-19)
        public const byte LobbyState = 10;
        public const byte PlayerReady = 11;
        public const byte TeamChange = 12;
        public const byte CharacterChange = 13;
        public const byte GameModeChange = 14;
        public const byte MapChange = 15;
        
        // Game messages (20-29)
        public const byte GameState = 20;
        public const byte TeamScore = 21;
        public const byte GameEnded = 22;
        public const byte PlayerAction = 23;
        public const byte ObjectState = 24;
        
        // Gameplay messages (30-39)
        public const byte IngredientSpawned = 30;
        public const byte IngredientPickedUp = 31;
        public const byte IngredientDropped = 32;
        public const byte IngredientProcessed = 33;
        public const byte RecipeCompleted = 34;
        public const byte RecipeFailed = 35;
        
        // Chat messages (40-49)
        public const byte ChatMessage = 40;
        public const byte Emote = 41;
        
        // Custom messages (50+)
        public const byte Custom = 50;
    }
}
