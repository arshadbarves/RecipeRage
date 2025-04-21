namespace RecipeRage.Core.Networking
{
    /// <summary>
    /// Defines the types of network messages used in the game.
    /// </summary>
    public static class NetworkMessageType
    {
        // System messages (0-9)
        public const byte Ping = 0;
        public const byte Pong = 1;
        public const byte PlayerInfo = 2;
        public const byte Disconnect = 3;
        
        // Lobby messages (10-19)
        public const byte LobbyState = 10;
        public const byte TeamChange = 11;
        public const byte CharacterChange = 12;
        public const byte GameModeChange = 13;
        public const byte MapChange = 14;
        public const byte StartCountdown = 15;
        public const byte CancelCountdown = 16;
        
        // Game messages (20-29)
        public const byte GameState = 20;
        public const byte TeamScore = 21;
        public const byte GameEnded = 22;
        public const byte PlayerReady = 23;
        public const byte PlayerAction = 24;
        
        // Gameplay messages (30-49)
        public const byte PlayerMovement = 30;
        public const byte PlayerInteraction = 31;
        public const byte PlayerSpecialAbility = 32;
        public const byte ItemPickup = 33;
        public const byte ItemDrop = 34;
        public const byte ItemUse = 35;
        public const byte RecipeStarted = 36;
        public const byte RecipeCompleted = 37;
        public const byte RecipeFailed = 38;
        public const byte OrderCreated = 39;
        public const byte OrderCompleted = 40;
        public const byte OrderFailed = 41;
        public const byte StationStateChanged = 42;
        
        // Chat messages (50-59)
        public const byte ChatMessage = 50;
        public const byte EmoteMessage = 51;
        
        // Custom messages (60+)
        // Add your custom message types here
    }
}
