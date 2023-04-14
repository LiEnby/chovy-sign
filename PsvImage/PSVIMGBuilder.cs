using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PsvImage
{
    class PSVIMGBuilder
    {
        private byte[] iv;
        private byte[] key;
        private Stream mainStream;
        private SHA256 sha256;
        private Memory<byte> blockData;
        private int blockPosition;
        private long contentSize = 0;

        private int blocksWritten = 0;
        private bool finished = false;

        public long ContentSize => contentSize;

        public int BlocksWritten => blocksWritten;

        public bool HasFinished => finished;

        //Footer
        private long totalBytes = 0;

        public PSVIMGBuilder(Stream dst, byte[] Key)
        {

            totalBytes = 0;
            contentSize = 0;
            sha256 = SHA256.Create();
            mainStream = dst;
            key = Key;

            RandomNumberGenerator.Fill(iv);
            iv = AesHelper.AesEcbDecrypt(iv, key);

            mainStream.Write(iv.AsSpan());
            totalBytes += iv.Length;

            startNewBlock();
        }

        public void AddFile(string FilePath, string ParentPath, string PathRel)
        {
            var sz = Utils.ToSceIoStat(FilePath).Size;
        }

        private void startNewBlock()
        {
            blockData = new byte[PSVIMGConstants.FULL_PSVIMG_SIZE];
            blockPosition = 0;
        }

        private byte[] shaBlock(int length = PSVIMGConstants.PSVIMG_BLOCK_SIZE)
        {
            return sha256.ComputeHash(blockData.ToArray(), 0, length);
        }

        private void finishBlock(bool final = false)
        {
            int len = blockPosition;
            var shaBytes = shaBlock(len);
            shaBytes.CopyTo(blockData.Slice(blockPosition));
            len += PSVIMGConstants.SHA256_BLOCK_SIZE;

            //Get next IV
            var encryptedBlock = AesHelper.CbcEncrypt(blockData.ToArray(), iv, key, len);
            for (int i = 0; i < iv.Length; i++)
            {
                int encBlockOffset = (encryptedBlock.Length - iv.Length) + i;
                iv[i] = encryptedBlock[encBlockOffset];
            }

            mainStream.Write(encryptedBlock.AsSpan());
            totalBytes += encryptedBlock.Length;
        }

        private int remainingBlockSize()
        {
            return (int)(PSVIMGConstants.PSVIMG_BLOCK_SIZE - blockPosition);
        }

        private void writeBlock(Span<byte> date, bool update = false)
        {
            var dLen = date.Length;
            var writeTotal = 0;
            while (dLen > 0)
            {
                int remaining = remainingBlockSize();

                if (dLen > remaining)
                {
                    date.Slice(writeTotal, remaining).CopyTo(blockData.Span.Slice(blockPosition));
                    blockPosition += remaining;
                    writeTotal += remaining;
                    dLen -= remaining;

                    finishBlock();
                    startNewBlock();
                    if (update)
                    {
                        blocksWritten += 1;
                    }
                }
                else
                {
                    
                }
            }

        }
    }
}
