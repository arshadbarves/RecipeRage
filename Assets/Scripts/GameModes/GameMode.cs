namespace GameModes
{
    public abstract class GameMode
    {
        public abstract void InitializeGame();
        public abstract void StartGame();
        public abstract void UpdateGame();
        public abstract void EndGame();
    }
}