//#define DEBUGING_PSVIMG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Li.Utilities
{
    public class CryptoUtil
    {
        public static byte[] aes_cbc_decrypt(byte[] cipherText, byte[] aesIv, byte[] aesKey, int size = -1)
        {
            if (size < 0) size = cipherText.Length;
            #if DEBUGING_PSVIMG
            return cipherText;
            #endif
            using (MemoryStream ms = new MemoryStream())
            {
                Aes alg = Aes.Create();
                alg.Mode = CipherMode.CBC;
                alg.Padding = PaddingMode.None;
                alg.KeySize = aesKey.Length * 8;
                alg.BlockSize = 128;
                alg.Key = aesKey;
                alg.IV = aesIv;
                using (CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherText, 0, size);

                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] plainText = ms.ToArray();
                    return plainText;
                }
            }
        }

        public static byte[] aes_ecb_decrypt(byte[] cipherText, byte[] aesKey, int size = -1)
        {
            if (size < 0) size = cipherText.Length;

            #if DEBUGING_PSVIMG
            return cipherText;
            #endif

            using (MemoryStream ms = new MemoryStream())
            {
                Aes alg = Aes.Create();
                alg.Mode = CipherMode.ECB;
                alg.Padding = PaddingMode.None;
                alg.KeySize = aesKey.Length * 8;
                alg.BlockSize = 128;
                alg.Key = aesKey;
                using (CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherText, 0, size);

                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] plainText = ms.ToArray();
                    return plainText;
                }
            }

        }

        public static byte[] aes_cbc_encrypt(byte[] plainText, byte[] aesIv, byte[] aesKey, int size = -1)
        {
            if (size < 0) size = plainText.Length;

            #if DEBUGING_PSVIMG
            return plainText;
            #endif

            using (MemoryStream ms = new MemoryStream())
            {
                Aes alg = Aes.Create();
                alg.Mode = CipherMode.CBC;
                alg.Padding = PaddingMode.None;
                alg.KeySize = aesKey.Length * 8;
                alg.BlockSize = 128;
                alg.Key = aesKey;
                alg.IV = aesIv;
                using (CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(plainText, 0, size);

                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] cipherText = ms.ToArray();
                    return cipherText;
                }
            }
        }

        public static byte[] aes_ecb_encrypt(byte[] plainText, byte[] aesKey, int size = -1)
        {
            if (size < 0) size = plainText.Length;

            #if DEBUGING_PSVIMG
            return plainText;
            #endif

            using (MemoryStream ms = new MemoryStream())
            {
                Aes alg = Aes.Create();
                alg.Mode = CipherMode.ECB;
                alg.Padding = PaddingMode.None;
                alg.KeySize = aesKey.Length * 8;
                alg.BlockSize = 128;
                alg.Key = aesKey;
                using (CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(plainText, 0, size);

                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] cipherText = ms.ToArray();
                    return cipherText;
                }
                
            }

        }
    }
}
