namespace Core.SaveSystem
{
    /// <summary>
    /// Interface for file storage operations
    /// </summary>
    public interface IFileStorage
    {
        string Read(string filename);
        void Write(string filename, string content);
        bool Exists(string filename);
        void Delete(string filename);
    }
}
