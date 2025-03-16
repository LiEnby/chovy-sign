using Li.Utilities;
using System;
using System.IO;
using System.Security.Cryptography;


namespace Vita.PsvImgTools
{
    public class PSVIMGStream : Stream
    {
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

        public byte[] Key
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
                return baseStream.Length - PSVIMGConstants.AES_BLOCK_SIZE;
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
                return baseStream.Position - PSVIMGConstants.AES_BLOCK_SIZE;
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }

        }

        public PSVIMGStream(Stream file, byte[] aesKey)
        {
            baseStream = file;
            key = aesKey;
            if (!verifyFooter())
            {
                throw new Exception("Invalid KEY!");
            }
            blockStream = new MemoryStream();
            baseStream.Seek(PSVIMGConstants.AES_BLOCK_SIZE, SeekOrigin.Begin);
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
            if (origin == SeekOrigin.Begin)
            {
                ret = baseStream.Seek(offset + PSVIMGConstants.AES_BLOCK_SIZE, SeekOrigin.Begin);
            }
            else if (origin == SeekOrigin.Current)
            {
                long pos = baseStream.Position;
                if (pos + offset >= PSVIMGConstants.AES_BLOCK_SIZE)
                {
                    ret = baseStream.Seek(offset, SeekOrigin.Current);
                }
                else
                {
                    ret = baseStream.Seek(offset + PSVIMGConstants.AES_BLOCK_SIZE, SeekOrigin.Current);
                }
            }
            else if (origin == SeekOrigin.End)
            {
                long pos = baseStream.Length;
                if (pos + offset >= PSVIMGConstants.AES_BLOCK_SIZE)
                {
                    ret = baseStream.Seek(offset, SeekOrigin.End);
                }
                else
                {
                    ret = baseStream.Seek(offset + PSVIMGConstants.AES_BLOCK_SIZE, SeekOrigin.End);
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


        public override void Close()
        {
            blockStream.Dispose();
            baseStream.Dispose();
            base.Close();
        }
        private void update()
        {
            long offset = Position % PSVIMGConstants.FULL_PSVIMG_BLOCK_SIZE;
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
                blockOffset = i * PSVIMGConstants.FULL_PSVIMG_BLOCK_SIZE + PSVIMGConstants.AES_BLOCK_SIZE;
                long remaining = getRemainingBase();
                if (remaining < PSVIMGConstants.FULL_PSVIMG_BLOCK_SIZE)
                {
                    fullBlock = blockOffset + remaining;
                }
                else
                {
                    fullBlock = blockOffset + PSVIMGConstants.FULL_PSVIMG_BLOCK_SIZE;
                }
                if (curPos >= blockOffset && curPos < fullBlock)
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
            baseStream.Seek(baseStream.Position - PSVIMGConstants.AES_BLOCK_SIZE, SeekOrigin.Begin);
            baseStream.Read(iv, 0x00, iv.Length);
            return iv;
        }

        private void seekToBlock(long blockIndex)
        {
            long blockOffset;
            blockOffset = blockIndex * PSVIMGConstants.FULL_PSVIMG_BLOCK_SIZE + PSVIMGConstants.AES_BLOCK_SIZE;

            if (blockOffset > baseStream.Length)
            {
                blockOffset = baseStream.Length;
            }

            baseStream.Seek(blockOffset, SeekOrigin.Begin);
        }

        private bool verifyFooter()
        {
            byte[] footer = new byte[0x10];
            byte[] iv = new byte[PSVIMGConstants.AES_BLOCK_SIZE];

            baseStream.Seek(baseStream.Length - (footer.Length + iv.Length), SeekOrigin.Begin);
            baseStream.Read(iv, 0x00, PSVIMGConstants.AES_BLOCK_SIZE);
            baseStream.Read(footer, 0x00, 0x10);

            byte[] footerDec = CryptoUtil.aes_cbc_decrypt(footer, iv, this.Key);
            ulong footerLen = 0;

            using (MemoryStream ms = new MemoryStream(footerDec))
            {
                PSVIMGStreamUtil fStreamUtil = new PSVIMGStreamUtil(ms);
                fStreamUtil.ReadInt32();
                fStreamUtil.ReadInt32();
                footerLen = fStreamUtil.ReadUInt64();
            }

            return Convert.ToUInt64(baseStream.Length) == footerLen;
        }

        private byte[] getBlock(long blockIndex)
        {
            byte[] iv = getIV(blockIndex);
            long remaining = getRemainingBase();
            byte[] encryptedBlock;
            if (PSVIMGConstants.FULL_PSVIMG_BLOCK_SIZE < remaining)
            {
                encryptedBlock = new byte[PSVIMGConstants.FULL_PSVIMG_BLOCK_SIZE];
            }
            else
            {
                encryptedBlock = new byte[remaining];
            }


            baseStream.Read(encryptedBlock, 0x00, encryptedBlock.Length);
            byte[] decryptedBlock = CryptoUtil.aes_cbc_decrypt(encryptedBlock, iv, this.Key);
            return decryptedBlock;
        }
    }
}