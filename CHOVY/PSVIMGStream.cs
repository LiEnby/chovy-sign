using System;
using System.IO;
using System.Security.Cryptography;


namespace PSVIMGTOOLS
{
    class PSVIMGStream : Stream
    {

        public int AES_BLOCK_SIZE = 0x10;
        public int SHA256_BLOCK_SIZE = 0x20;
        public int PSVIMG_BLOCK_SIZE = 0x8000;
        public int FULL_PSVIMG_SIZE = 0x8020;
        public int PSVIMG_ENTRY_ALIGN = 0x400;
        private Stream baseStream;
        private MemoryStream blockStream;
        private byte[] key;
        public Stream BaseStream
        {
            get
            {
                return baseStream;
            }
        }

        public Byte[] Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
            }
        }

        public long BlockNo
        {
            get
            {
                return getBlockIndex();
            }
            set
            {
                seekToBlock(value);
                update();
            }
        }

        public long BlockRemaining
        {
            get
            {
                return getRemainingBlock();
            }
        }

        public long BlockPosition
        {
            get
            {
                return blockStream.Position;
            }
            set
            {
                blockStream.Seek(value, SeekOrigin.Begin);
            }
        }
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }
        public override long Length
        {
            get
            {
                return baseStream.Length - AES_BLOCK_SIZE;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override long Position
        {
            get
            {
                return baseStream.Position - AES_BLOCK_SIZE;
            }
            set
            {
                this.Seek(value, SeekOrigin.Begin);
            }

        }

        public PSVIMGStream(Stream file, byte[] KEY)
        {
            baseStream = file;
            key = KEY;
            blockStream = new MemoryStream();
            baseStream.Seek(AES_BLOCK_SIZE, SeekOrigin.Begin);
            update();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int remaining = (int)getRemainingBlock();
            int read = 0;

            if (count < remaining)
            {
                read += blockStream.Read(buffer, offset, count);
                baseStream.Seek(count, SeekOrigin.Current);
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    while (true)
                    {
                        update();
                        remaining = (int)getRemainingBlock();
                        int curPos = count - read;

                        if (curPos > remaining)
                        {
                            read += remaining;
                            blockStream.CopyTo(ms, remaining);
                            baseStream.Seek(remaining, SeekOrigin.Current);
                        }
                        else
                        {
                            read += curPos;
                            blockStream.CopyTo(ms, curPos);
                            baseStream.Seek(curPos, SeekOrigin.Current);
                            break;
                        }

                    }
                    ms.Seek(0x00, SeekOrigin.Begin);
                    ms.Read(buffer, offset, count);
                }
            }
            return read;

        }

        public override void Flush()
        {
            update();
            baseStream.Flush();
            blockStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long ret = 0;
            if(origin == SeekOrigin.Begin)
            {
                ret = baseStream.Seek(offset + AES_BLOCK_SIZE, SeekOrigin.Begin);
            }
            else if (origin == SeekOrigin.Current)
            {
                long pos = baseStream.Position;
                if ((pos + offset) >= AES_BLOCK_SIZE)
                {
                    ret = baseStream.Seek(offset, SeekOrigin.Current);
                }
                else
                {
                    ret = baseStream.Seek(offset+ AES_BLOCK_SIZE, SeekOrigin.Current);
                }
            }
            else if (origin == SeekOrigin.End)
            {
                long pos = baseStream.Length;
                if ((pos + offset) >= AES_BLOCK_SIZE)
                {
                    ret = baseStream.Seek(offset, SeekOrigin.End);
                }
                else
                {
                    ret = baseStream.Seek(offset + AES_BLOCK_SIZE, SeekOrigin.End);
                }
            }
            update();
            return ret;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException("PSVIMGStream is Read-Only");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException("PSVIMGStream is Read-Only");
        }


        public new void Close()
        {
            blockStream.Close();
            blockStream.Dispose();
            baseStream.Close();
            baseStream.Dispose();
            this.Dispose();
        }
        private void update()
        {
            long offset = (this.Position % FULL_PSVIMG_SIZE);
            long blockIndex = getBlockIndex();
            byte[] decryptedBlock = getBlock(blockIndex);
            blockStream.Seek(0x00, SeekOrigin.Begin);
            blockStream.SetLength(decryptedBlock.Length);
            blockStream.Write(decryptedBlock, 0x00, decryptedBlock.Length);
            seekToBlock(blockIndex);
            baseStream.Seek(offset, SeekOrigin.Current);
            blockStream.Seek(offset, SeekOrigin.Begin);
        }

        private long getBlockIndex()
        {
            long i = 0;
            long curPos = baseStream.Position;
            long fullBlock;
            long blockOffset;

            while (true)
            {
                blockOffset = (i * FULL_PSVIMG_SIZE) + AES_BLOCK_SIZE;
                long remaining = getRemainingBase();
                if (remaining < FULL_PSVIMG_SIZE)
                {
                    fullBlock = blockOffset + remaining;
                }
                else
                {
                    fullBlock = blockOffset + FULL_PSVIMG_SIZE;
                }
                if ((curPos >= blockOffset) && (curPos < fullBlock))
                {
                    break;
                }
                if (blockOffset > baseStream.Length)
                {
                    break;
                }
                i++;
            }
            return i;


        }
        private long getRemainingBase()
        {
            return baseStream.Length - baseStream.Position;
        }
        private long getRemainingBlock()
        {
            return blockStream.Length - blockStream.Position;
        }
        private byte[] getIV(long blockindex)
        {
            byte[] iv = new byte[0x10];
            seekToBlock(blockindex);
            baseStream.Seek(baseStream.Position - AES_BLOCK_SIZE, SeekOrigin.Begin);
            baseStream.Read(iv, 0x00, iv.Length);
            return iv;
        }
        private byte[] aes_ecb_decrypt(byte[] cipherData, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            Aes alg = Aes.Create();
            alg.Mode = CipherMode.CBC;
            alg.Padding = PaddingMode.None;
            alg.KeySize = 256;
            alg.BlockSize = 128;
            alg.Key = key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }

        private void seekToBlock(long blockIndex)
        {
            long blockOffset;
            blockOffset = (blockIndex * FULL_PSVIMG_SIZE) + AES_BLOCK_SIZE;

            if (blockOffset > baseStream.Length)
            {
                blockOffset = baseStream.Length;
            }


            baseStream.Seek(blockOffset, SeekOrigin.Begin);
        }
        private byte[] getBlock(long blockIndex)
        {
            byte[] iv = getIV(blockIndex);
            long remaining = getRemainingBase();
            byte[] encryptedBlock;
            if (FULL_PSVIMG_SIZE < remaining)
            {
                encryptedBlock = new byte[FULL_PSVIMG_SIZE];
            }
            else
            {
                encryptedBlock = new byte[remaining];
            }
   
           
            baseStream.Read(encryptedBlock, 0x00, encryptedBlock.Length);
            byte[] decryptedBlock = aes_ecb_decrypt(encryptedBlock, iv);
            return decryptedBlock;
        }
    }
}
