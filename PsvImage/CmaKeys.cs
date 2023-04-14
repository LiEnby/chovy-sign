using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PsvImage
{
    public class CmaKeys
    {
        private static readonly byte[] Passphrase = Encoding.ASCII.GetBytes("Sri Jayewardenepura Kotte");
        private static readonly byte[] Key = { 0xA9, 0xFA, 0x5A, 0x62, 0x79, 0x9F, 0xCC, 0x4C, 0x72, 0x6B, 0x4E, 0x2C, 0xE3, 0x50, 0x6D, 0x38 };

        public static byte[] GenerateKey(string aid)
        {
            var longlong = Convert.ToUInt64(aid, 16);
            var aidBytes = BitConverter.GetBytes(longlong);
            Array.Reverse(aidBytes);
            return GenerateKey(aidBytes);
        }

        public static byte[] GenerateKey(byte[] aid)
        {
            var derviedKey = aid.Concat(Passphrase).ToArray();

            using var sha = SHA256.Create();
            derviedKey = sha.ComputeHash(derviedKey);
            using var aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;
            aes.Key = Key;
            using var decryptor = aes.CreateDecryptor();
            derviedKey = decryptor.TransformFinalBlock(derviedKey, 0, derviedKey.Length);
            return derviedKey;
        }
    }
}
