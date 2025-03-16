using Li.Utilities;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Vita.ContentManager
{
    public class KeyGenerator
    {
        private static byte[] AID_SALT_BYTES = Encoding.ASCII.GetBytes("Sri Jayewardenepura Kotte");
        private static byte[] AID_SALT_KEY = { 0xA9, 0xFA, 0x5A, 0x62, 0x79, 0x9F, 0xCC, 0x4C, 0x72, 0x6B, 0x4E, 0x2C, 0xE3, 0x50, 0x6D, 0x38 };

        public static string GenerateKeyStr(string accountId)
        {
            try
            {
                Int64 aidUint = Convert.ToInt64(accountId, 16);

                byte[] aidBytes = BitConverter.GetBytes(aidUint);
                Array.Reverse(aidBytes);

                byte[] derivedKey = GenerateKey(aidBytes);

                return BitConverter.ToString(derivedKey).Replace("-", "");
            }
            catch (Exception)
            {
                return "INVALID_AID";
            }
        }
        public static byte[] GenerateKey(byte[] accountId)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(accountId, 0, accountId.Length);
                ms.Write(AID_SALT_BYTES, 0, AID_SALT_BYTES.Length);
                ms.Seek(0, SeekOrigin.Begin);
                byte[] derivedKey = ms.ToArray();
                using (SHA256 sha = SHA256.Create())
                {
                    derivedKey = sha.ComputeHash(derivedKey);
                    derivedKey = CryptoUtil.aes_ecb_decrypt(derivedKey, AID_SALT_KEY);
                    return derivedKey;
                }
            }
        }


    }
}