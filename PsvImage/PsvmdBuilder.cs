using System;
using System.Collections.Generic;
using System.IO;

namespace PsvImage
{
    class PsvmdBuilder
    {

        public static void CreatePsvmd(Stream OutputStream, Stream EncryptedPsvimg, long ContentSize, string BackupType,
            byte[] Key)
        {
            Span<byte> iv = new byte[PSVIMGConstants.AES_BLOCK_SIZE];
            EncryptedPsvimg.Seek(0, SeekOrigin.Begin);
            EncryptedPsvimg.Read(iv);
            iv = AesHelper.AesEcbDecrypt(iv.ToArray(), Key);

        }


    }
}
