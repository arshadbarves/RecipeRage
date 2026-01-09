namespace Core.Core.Persistence
{
    /// <summary>
    /// Interface for encryption/decryption
    /// </summary>
    public interface IEncryptionService
    {
        string Encrypt(string data);
        string Decrypt(string data);
    }
}
