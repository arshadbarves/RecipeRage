namespace Core.Persistence
{
    public interface IEncryptionService
    {
        string Encrypt(string data);
        string Decrypt(string data);
    }
}
