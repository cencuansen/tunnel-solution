using System.Security.Cryptography;

namespace Cipher
{
    /// <summary>
    /// AES。
    /// Key 和 IV 须为128位（16字节）数据
    /// </summary>
    public class AESTool
    {
        public static readonly string DefaultKey = "AESEncryptionKey";

        public static readonly string DefaultIV = "AESEncryptionIV1";

        public static byte[] Encrypt(string original, byte[] key, byte[] iv)
        {
            byte[] encrypted;
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream msEncrypt = new();
                using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
                using (StreamWriter swEncrypt = new(csEncrypt))
                {
                    swEncrypt.Write(original);
                }
                encrypted = msEncrypt.ToArray();
            }

            return encrypted;
        }

        public static string Decrypt(byte[] encrypted, byte[] key, byte[] iv)
        {
            string decrypted;
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using MemoryStream msDecrypt = new(encrypted);
                using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new(csDecrypt);
                decrypted = srDecrypt.ReadToEnd();
            }

            return decrypted;
        }
    }
}