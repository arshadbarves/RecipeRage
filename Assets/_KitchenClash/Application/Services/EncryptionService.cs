using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using KitchenClash.Domain;

namespace KitchenClash.Application
{
    public sealed class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService(string passphrase = "KitchenClash_2026")
        {
            using var sha = SHA256.Create();
            _key = sha.ComputeHash(Encoding.UTF8.GetBytes(passphrase));
            _iv = new byte[16];
            Array.Copy(_key, _iv, 16);
        }

        public string Encrypt(string data)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            using var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(data);
            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(encrypted);
        }

        public string Decrypt(string data)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            using var decryptor = aes.CreateDecryptor();
            var bytes = Convert.FromBase64String(data);
            var decrypted = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
