using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PsvImage
{
    static class AesHelper
    {
        public static byte[] CbcEncrypt(byte[] plainText, byte[] iv, byte[] key, int size = -1)
        {
            if (size < 0)
            {
                size = plainText.Length;
            }

            using var aes = Aes.Create();
            aes.Padding = PaddingMode.None;
            aes.Key = key;
            aes.IV = iv;
            using var encrypt = aes.CreateEncryptor();
            var cipherText = encrypt.TransformFinalBlock(plainText, 0, size);
            return cipherText;
        }

        public static byte[] AesEcbDecrypt(byte[] cipherText, byte[] key, int size = -1)
        {
            if (size < 0)
            {
                size = cipherText.Length;
            }

            using var aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;
            aes.Key = key;
            using var decrypt = aes.CreateDecryptor();
            var plainText = decrypt.TransformFinalBlock(cipherText, 0, size);
            return plainText;
        }
    }
}
