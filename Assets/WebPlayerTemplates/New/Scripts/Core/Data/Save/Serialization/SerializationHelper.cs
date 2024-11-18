using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Core.Data.Save.Serialization
{
    public static class SerializationHelper
    {
        private const string EncryptionKey = "YourSecretKey"; // Change this in production

        public static string Serialize<T>(T data)
        {
            string json = JsonUtility.ToJson(data);
            return Encrypt(json);
        }

        public static T Deserialize<T>(string encrypted)
        {
            string json = Decrypt(encrypted);
            return JsonUtility.FromJson<T>(json);
        }

        private static string Encrypt(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aes.IV = new byte[16];

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                    return Convert.ToBase64String(encrypted);
                }
            }
        }

        private static string Decrypt(string encrypted)
        {
            byte[] bytes = Convert.FromBase64String(encrypted);
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aes.IV = new byte[16];

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] decrypted = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                    return Encoding.UTF8.GetString(decrypted);
                }
            }
        }
    }
}