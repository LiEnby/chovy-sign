using System;
using System.Linq;
using System.Security.Cryptography;

namespace PspCrypto
{
    public static class AesHelper
    {
        private static readonly byte[] padding = { 0xff, 0xee, 0xdd, 0xcc, 0xbb, 0xaa, 0x99, 0x88, 0x77, 0x66, 0x55, 0x44, 0x33, 0x22, 0x11 };

        static readonly byte[] IV0 = new byte[16];
        static readonly byte[] Z = new byte[16];

        public static Aes CreateAes()
        {
            var aes = Aes.Create();
            aes.KeySize = 128;
            if (aes == null)
            {
                throw new Exception("Create Aes Failed");
            }
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            aes.IV = IV0;
            return aes;
        }

        public static Aes CreateKirkAes()
        {
            var aes = CreateAes();
            aes.Key = KeyVault.kirk1_key;
            return aes;
        }

#if false
        public static byte[] Cmac(Aes aes, byte[] orgdata, int offset = 0, int len = -1)
        {
            if (len == -1)
            {
                len = orgdata.Length;
            }

            byte[] data;
            if (offset == 0 && len == orgdata.Length)
            {
                data = orgdata;
            }
            else
            {
                data = new byte[len];
                Buffer.BlockCopy(orgdata, offset, data, 0, len);
            }
            // SubKey generation
            // step 1, AES-128 with key K is applied to an all-zero input block.
            byte[] L = AesEncrypt(aes, Z);

            // step 2, K1 is derived through the following operation:
            byte[] FirstSubkey = Rol(L); //If the most significant bit of L is equal to 0, K1 is the left-shift of L by 1 bit.
            if ((L[0] & 0x80) == 0x80)
                FirstSubkey[15] ^= 0x87; // Otherwise, K1 is the exclusive-OR of c

            // step 3, K2 is derived through the following operation:
            byte[] SecondSubkey = Rol(FirstSubkey); // If the most significant bit of K1 is equal to 0, K2 is the left-shift of K1 by 1 bit.
            if ((FirstSubkey[0] & 0x80) == 0x80)
                SecondSubkey[15] ^= 0x87; // Otherwise, K2 is the exclusive-OR of const_Rb and the left-shift of K1 by 1 bit.

            // MAC computing
            if (((data.Length != 0) && (data.Length % 16 == 0)))
            {
                // If the size of the input message block is equal to a positive multiple of the block size (namely, 128 bits),
                // the last block shall be exclusive-OR'ed with K1 before processing
                for (int j = 0; j < FirstSubkey.Length; j++)
                    data[data.Length - 16 + j] ^= FirstSubkey[j];
            }
            else
            {
                // Otherwise, the last block shall be padded with 10^i
                byte[] padding = new byte[16 - data.Length % 16];
                padding[0] = 0x80;

                data = data.Concat(padding).ToArray();

                // and exclusive-OR'ed with K2
                for (int j = 0; j < SecondSubkey.Length; j++)
                    data[data.Length - 16 + j] ^= SecondSubkey[j];
            }

            // The result of the previous process will be the input of the last encryption.
            byte[] encResult = AesEncrypt(aes, data);

            byte[] HashValue = new byte[16];
            //Array.Copy(encResult, encResult.Length - HashValue.Length, HashValue, 0, HashValue.Length);
            Buffer.BlockCopy(encResult, encResult.Length - HashValue.Length, HashValue, 0, HashValue.Length);

            return HashValue;
        }
        
#endif
        public static void Cmac(Aes aes, Span<byte> dst, ReadOnlySpan<byte> src)
        {
            byte[] data = src.ToArray();
            // SubKey generation
            // step 1, AES-128 with key K is applied to an all-zero input block.
            byte[] L = AesEncrypt(aes, Z);

            // step 2, K1 is derived through the following operation:
            byte[] FirstSubkey = Rol(L); //If the most significant bit of L is equal to 0, K1 is the left-shift of L by 1 bit.
            if ((L[0] & 0x80) == 0x80)
                FirstSubkey[15] ^= 0x87; // Otherwise, K1 is the exclusive-OR of c

            // step 3, K2 is derived through the following operation:
            byte[] SecondSubkey = Rol(FirstSubkey); // If the most significant bit of K1 is equal to 0, K2 is the left-shift of K1 by 1 bit.
            if ((FirstSubkey[0] & 0x80) == 0x80)
                SecondSubkey[15] ^= 0x87; // Otherwise, K2 is the exclusive-OR of const_Rb and the left-shift of K1 by 1 bit.

            // MAC computing
            if (((data.Length != 0) && (data.Length % 16 == 0)))
            {
                // If the size of the input message block is equal to a positive multiple of the block size (namely, 128 bits),
                // the last block shall be exclusive-OR'ed with K1 before processing
                for (int j = 0; j < FirstSubkey.Length; j++)
                    data[data.Length - 16 + j] ^= FirstSubkey[j];
            }
            else
            {
                // Otherwise, the last block shall be padded with 10^i
                byte[] padding = new byte[16 - data.Length % 16];
                padding[0] = 0x80;

                data = data.Concat(padding).ToArray();

                // and exclusive-OR'ed with K2
                for (int j = 0; j < SecondSubkey.Length; j++)
                    data[data.Length - 16 + j] ^= SecondSubkey[j];
            }

            // The result of the previous process will be the input of the last encryption.
            byte[] encResult = AesEncrypt(aes, data);

            encResult.AsSpan(encResult.Length - 16,16).CopyTo(dst);

            //byte[] HashValue = new byte[16];
            //Array.Copy(encResult, encResult.Length - HashValue.Length, HashValue, 0, HashValue.Length);
            //Buffer.BlockCopy(encResult, encResult.Length - HashValue.Length, HashValue, 0, HashValue.Length);

            //return HashValue;
        }

        private static byte[] Rol(byte[] b)
        {
            byte[] r = new byte[b.Length];
            byte carry = 0;

            for (int i = b.Length - 1; i >= 0; i--)
            {
                ushort u = (ushort)(b[i] << 1);
                r[i] = (byte)((u & 0xff) + carry);
                carry = (byte)((u & 0xff00) >> 8);
            }

            return r;
        }

        private static byte[] AesEncrypt(Aes aes, byte[] data)
        {
            using var encryptor = aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }
#if false

        public static byte[] AesEncrypt(Aes aes, byte[] data, int offset, int length)
        {
            using var encryptor = aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(data, offset, length);
        }
#endif

        public static void AesEncrypt(Aes aes, Span<byte> oubput, ReadOnlySpan<byte> input, int size = 0)
        {
            byte[] buffer;
            if (size == 0)
            {
                size = input.Length;
            }
            if (size % 16 != 0)
            {
                buffer = new byte[(size + 15) / 16 * 16];
                var bufferSpan = new Span<byte>(buffer);
                input.CopyTo(bufferSpan);
                padding.AsSpan(0, buffer.Length - size).CopyTo(bufferSpan[size..]);
            }
            else
            {
                buffer = input[..size].ToArray();
            }
            using var encryptor = aes.CreateEncryptor();
            var encData = encryptor.TransformFinalBlock(buffer, 0, buffer.Length);
            encData.AsSpan().CopyTo(oubput);
        }
#if false
        public static byte[] AesDecrypt(Aes aes, byte[] data, int offset, int length)
        {
            aes.Padding = PaddingMode.None;
            using var encryptor = aes.CreateDecryptor();
            int fixLength = length;
            if (length % 16 != 0)
            {
                fixLength = (length / 16 + 1) * 16;
            }

            byte[] ret = encryptor.TransformFinalBlock(data, offset, fixLength);
            return ret.Take(length).ToArray();
        }

        public static void AesDecrypt(Aes aes, byte[] src, byte[] dst, int size)
        {
            var tmp = AesDecrypt(aes, src, 0, size);
            Buffer.BlockCopy(tmp, 0, dst, 0, size);
        }
#endif

        public static void AesDecrypt(Aes aes, Span<byte> dst, ReadOnlySpan<byte> src, int length)
        {

            int fixLength = length;
            if (length % 16 != 0)
            {
                fixLength = (length + 15) / 16 * 16;
            }
            var buffer = src[..fixLength].ToArray();
            using var encryptor = aes.CreateDecryptor();
            var decData = encryptor.TransformFinalBlock(buffer, 0, buffer.Length);
            decData.AsSpan(0, length).CopyTo(dst);
        }

        public static int AesDecrypt(ReadOnlySpan<byte> src, Span<byte> dst, ReadOnlySpan<byte> key)
        {
            Aes aes = Aes.Create();
            aes.Key = key[..16].ToArray();
            return aes.DecryptEcb(src[..16], dst, PaddingMode.None);
        }

        public static int AesEncrypt(ReadOnlySpan<byte> src, Span<byte> dst, ReadOnlySpan<byte> key)
        {
            Aes aes = Aes.Create();
            aes.Key = key[..16].ToArray();
            return aes.EncryptEcb(src[..16], dst, PaddingMode.None);
        }
    }
}
