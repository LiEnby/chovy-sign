﻿using Ionic.Zlib;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace Vita.PsvImgTools
{
    public class PSVMDBuilder
    {
        private static void memset(byte[] buf, byte content, long length)
        {
            for (int i = 0; i < length; i++)
            {
                buf[i] = content;
            }
        }

        private static void writeUInt64(Stream dst, ulong value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x8);
        }
        private static void writeInt64(Stream dst, long value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x8);
        }
        private static void writeUInt16(Stream dst, ushort value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x2);
        }
        private static void writeInt16(Stream dst, short value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x2);
        }

        private static void writeInt32(Stream dst, int value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x4);
        }
        private static void writeUInt32(Stream dst, uint value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x4);
        }


        private static void writeString(Stream dst, string str, int len = -1)
        {
            if (len < 0)
            {
                len = str.Length;
            }

            byte[] StrBytes = Encoding.UTF8.GetBytes(str);
            dst.Write(StrBytes, 0x00, len);
        }

        private static void writePadding(Stream dst, byte paddingByte, long paddingLen)
        {
            byte[] paddingData = new byte[paddingLen];
            memset(paddingData, paddingByte, paddingLen);
            dst.Write(paddingData, 0x00, paddingData.Length);
        }
        private static void writeStringWithPadding(Stream dst, string str, int padSize, byte padByte = 0x78)
        {
            int StrLen = str.Length;
            if (StrLen > padSize)
            {
                StrLen = padSize;
            }

            int PaddingLen = padSize - StrLen - 1;
            writeString(dst, str, StrLen);
            dst.WriteByte(0x00);
            writePadding(dst, padByte, PaddingLen);
        }

        private static byte[] aes_ecb_decrypt(byte[] cipherText, byte[] KEY, int size = -1)
        {
            if (size < 0)
            {
                size = cipherText.Length;
            }

            MemoryStream ms = new MemoryStream();

            Aes alg = Aes.Create();
            alg.Mode = CipherMode.ECB;
            alg.Padding = PaddingMode.None;
            alg.KeySize = 256;
            alg.BlockSize = 128;
            alg.Key = KEY;
            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherText, 0, size);
            cs.Close();
            byte[] plainText = ms.ToArray();
            return plainText;
        }

        private static byte[] aes_cbc_decrypt(byte[] cipherData, byte[] IV, byte[] Key)
        {
            MemoryStream ms = new MemoryStream();
            Aes alg = Aes.Create();
            alg.Mode = CipherMode.CBC;
            alg.Padding = PaddingMode.None;
            alg.KeySize = 256;
            alg.BlockSize = 128;
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }
        private static byte[] aes_cbc_encrypt(byte[] plainText, byte[] IV, byte[] KEY, int size = -1)
        {
            if (size < 0)
            {
                size = plainText.Length;
            }

            MemoryStream ms = new MemoryStream();

            Aes alg = Aes.Create();
            alg.Mode = CipherMode.CBC;
            alg.Padding = PaddingMode.None;
            alg.KeySize = 256;
            alg.BlockSize = 128;
            alg.Key = KEY;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(plainText, 0, size);
            cs.Close();
            byte[] cipherText = ms.ToArray();
            return cipherText;
        }


        public static void CreatePsvmd(Stream OutputStream, Stream EncryptedPsvimg, long ContentSize, string BackupType, byte[] Key)
        {
            byte[] IV = new byte[PSVIMGConstants.AES_BLOCK_SIZE];
            EncryptedPsvimg.Seek(0x00, SeekOrigin.Begin);
            EncryptedPsvimg.Read(IV, 0x00, IV.Length);
            IV = aes_ecb_decrypt(IV, Key);
            MemoryStream ms = new MemoryStream();

            writeUInt32(ms, 0xFEE1900D); // magic
            writeUInt32(ms, 0x2); // type
            writeUInt64(ms, 0x03000000); // fw ver
            ms.Write(new byte[0x10], 0x00, 0x10); // PSID
            writeStringWithPadding(ms, BackupType, 0x40, 0x00); //backup type
            writeInt64(ms, EncryptedPsvimg.Length); // total size
            writeUInt64(ms, 0x2); //version
            writeInt64(ms, ContentSize); // content size
            ms.Write(IV, 0x00, 0x10); // IV
            writeUInt64(ms, 0x00); //ux0 info
            writeUInt64(ms, 0x00); //ur0 info
            writeUInt64(ms, 0x00); //unused 98
            writeUInt64(ms, 0x00); //unused A0
            writeUInt32(ms, 0x1); //add data

            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] psvMd = ms.ToArray();
            ms.Close();

            ms = new MemoryStream();
            byte[] psvMdCompressed = ZlibStream.CompressBuffer(psvMd);
            psvMdCompressed[0x1] = 0x9C;
            ms.Write(psvMdCompressed, 0x00, psvMdCompressed.Length);

            SHA256 sha = SHA256.Create();
            byte[] shadata = sha.ComputeHash(psvMdCompressed);
            ms.Write(shadata, 0x00, shadata.Length);

            int PaddingLen = Convert.ToInt32(PSVIMGConstants.AES_BLOCK_SIZE - (ms.Length & PSVIMGConstants.AES_BLOCK_SIZE - 1));
            writePadding(ms, 0x00, PaddingLen);

            writeInt32(ms, PaddingLen); //Padding Length
            writeUInt32(ms, 0x00);
            writeInt64(ms, ms.Length + 0x8 + IV.Length);
            ms.Seek(0x00, SeekOrigin.Begin);
            byte[] toEncrypt = ms.ToArray();
            ms.Close();

            byte[] EncryptedData = aes_cbc_encrypt(toEncrypt, IV, Key);
            OutputStream.Write(IV, 0x00, IV.Length);
            OutputStream.Write(EncryptedData, 0x00, EncryptedData.Length);
            return;
        }


        public static byte[] DecryptPsvmd(Stream PsvMdFile, byte[] Key)
        {
            byte[] IV = new byte[PSVIMGConstants.AES_BLOCK_SIZE];
            PsvMdFile.Read(IV, 0x00, IV.Length);
            byte[] remaining = new byte[PsvMdFile.Length - IV.Length];
            PsvMdFile.Read(remaining, 0x00, remaining.Length);
            byte[] zlibCompressed = aes_cbc_decrypt(remaining, IV, Key);
            return zlibCompressed;
            // return ZlibStream.UncompressBuffer(zlibCompressed);
        }
    }
}
