namespace Modules.Persistence
{
    /// <summary>
    /// Simple XOR encryption service
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private readonly string _key;

        public EncryptionService(string key = "RecipeRage")
        {
            _key = key;
        }

        public string Encrypt(string data)
        {
            return EncryptDecrypt(data);
        }

        public string Decrypt(string data)
        {
            return EncryptDecrypt(data);
        }

        private string EncryptDecrypt(string data)
        {
            if (string.IsNullOrEmpty(data)) return data;

            char[] result = new char[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (char)(data[i] ^ _key[i % _key.Length]);
            }
            return new string(result);
        }
    }
}
