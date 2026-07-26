namespace Playcenter
{
    public interface ILoggingService
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}
