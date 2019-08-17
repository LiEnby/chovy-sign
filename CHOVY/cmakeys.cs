using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CHOVY
{
    class CmaKeys
    {
        static Byte[] Passphrase = Encoding.ASCII.GetBytes("Sri Jayewardenepura Kotte");
        static Byte[] Key = { 0xA9, 0xFA, 0x5A, 0x62, 0x79, 0x9F, 0xCC, 0x4C, 0x72, 0x6B, 0x4E, 0x2C, 0xE3, 0x50, 0x6D, 0x38 };

        public static string GenerateKeyStr(string Aid)
        {
            try
            {
                Int64 longlong = Convert.ToInt64(Aid, 16);

                byte[] AidBytes = BitConverter.GetBytes(longlong);
                Array.Reverse(AidBytes);

                byte[] DerivedKey = CmaKeys.GenerateKey(AidBytes);


                 return BitConverter.ToString(DerivedKey).Replace("-", "");
            }
            catch (Exception)
            {
                return "INVALID_AID";
            }
        }
        public static byte[] GenerateKey(byte[] Aid)
        {
            var ms = new MemoryStream();
            ms.Write(Aid, 0, Aid.Length);
            ms.Write(Passphrase, 0, Passphrase.Length);
            Byte[] DerviedKey = ms.ToArray();
            ms.Dispose();

            SHA256 sha = SHA256.Create();
            DerviedKey = sha.ComputeHash(DerviedKey);
            sha.Dispose();

            DerviedKey = Decrypt(DerviedKey, Key);

            return DerviedKey;
        }

        private static byte[] Decrypt(byte[] cipherData,
                                byte[] Key)
        {
            MemoryStream ms = new MemoryStream();
            Aes alg = Aes.Create();
            alg.Mode = CipherMode.ECB;
            alg.Padding = PaddingMode.None;
            alg.KeySize = 128;
            alg.Key = Key;
            CryptoStream cs = new CryptoStream(ms,
                alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }

    }
}