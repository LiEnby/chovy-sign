using Ionic.Zlib;
using Li.Utilities;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace Vita.PsvImgTools
{
    public class PSVMDBuilder
    {

        public static void CreatePsvmd(Stream outputStream, Stream encryptedPsvimg, long contentSize, string backupType, byte[] key)
        {
            byte[] iv = new byte[PSVIMGConstants.AES_BLOCK_SIZE];
            encryptedPsvimg.Seek(0x00, SeekOrigin.Begin);
            encryptedPsvimg.Read(iv, 0x00, iv.Length);
            iv = CryptoUtil.aes_ecb_decrypt(iv, key);

            byte[] psvMd;
            byte[] toEncrypt;

            using (MemoryStream ms = new MemoryStream())
            {
                PSVIMGStreamUtil psvMdUtil = new PSVIMGStreamUtil(ms);

                psvMdUtil.WriteUInt32(0xFEE1900D); // magic
                psvMdUtil.WriteUInt32(0x2); // type
                psvMdUtil.WriteUInt64(0x03000000); // fw ver
                ms.Write(new byte[0x10], 0x00, 0x10); // PSID
                psvMdUtil.WriteStrWithPadding(backupType, 0x40, 0x00); //backup type
                psvMdUtil.WriteInt64(encryptedPsvimg.Length); // total size
                psvMdUtil.WriteUInt64(0x2); //version
                psvMdUtil.WriteInt64(contentSize); // content size
                ms.Write(iv, 0x00, 0x10); // iv
                psvMdUtil.WriteUInt64(0x00); //ux0 info
                psvMdUtil.WriteUInt64(0x00); //ur0 info
                psvMdUtil.WriteUInt64(0x00); //unused 98
                psvMdUtil.WriteUInt64(0x00); //unused A0
                psvMdUtil.WriteUInt32(0x1); //add data

                ms.Seek(0x00, SeekOrigin.Begin);
                psvMd = ms.ToArray();
            }

            using (MemoryStream ms = new MemoryStream())
            {
                PSVIMGStreamUtil toEncUtil = new PSVIMGStreamUtil(ms);

                byte[] psvMdCompressed = ZlibStream.CompressBuffer(psvMd);
                psvMdCompressed[0x1] = 0x9C;
                ms.Write(psvMdCompressed, 0x00, psvMdCompressed.Length);

                SHA256 sha = SHA256.Create();
                byte[] shaData = sha.ComputeHash(psvMdCompressed);
                ms.Write(shaData, 0x00, shaData.Length);

                int paddingLen = Convert.ToInt32(PSVIMGConstants.AES_BLOCK_SIZE - (ms.Length & PSVIMGConstants.AES_BLOCK_SIZE - 1));
                toEncUtil.WritePadding(0x00, paddingLen);

                toEncUtil.WriteInt32(paddingLen); // padding length
                toEncUtil.WriteUInt32(0x00);
                toEncUtil.WriteInt64(ms.Length + 0x8 + iv.Length);

                ms.Seek(0x00, SeekOrigin.Begin);
                toEncrypt = ms.ToArray();
            }

            byte[] encryptedData = CryptoUtil.aes_cbc_encrypt(toEncrypt, iv, key);
            outputStream.Write(iv, 0x00, iv.Length);
            outputStream.Write(encryptedData, 0x00, encryptedData.Length);

            return;
        }


        public static byte[] DecryptPsvmd(Stream psvMdFile, byte[] key)
        {
            byte[] iv = new byte[PSVIMGConstants.AES_BLOCK_SIZE];
            psvMdFile.Read(iv, 0x00, iv.Length);
            byte[] remaining = new byte[psvMdFile.Length - iv.Length];
            psvMdFile.Read(remaining, 0x00, remaining.Length);
            byte[] zlibCompressed = CryptoUtil.aes_cbc_decrypt(remaining, iv, key);
            return zlibCompressed;
            // return ZlibStream.UncompressBuffer(zlibCompressed);
        }
    }
}
