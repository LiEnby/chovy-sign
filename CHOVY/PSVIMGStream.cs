using System;
using System.IO;
using System.Security.Cryptography;


namespace PSVIMGTOOLS
{
    class PSVIMGStream : Stream
    {

        private Stream baseStream;
        private MemoryStream blockStream;
        private long _position = 0;
        private byte[] key;
        public Stream BaseStream
        {
            get
            {
                return baseStream;
            }
        }

        private long position
        {
            get
            {
                return _position;
            }
            set
            {
                if (value > this.Length)
                {
                    _position = this.Length;
                }
                else if(value < 0)
                {
                    _position = 0;
                }
                else
                {
                    _position = value;
                }
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
                return position;
            }
            set
            {
                this.Seek(value, SeekOrigin.Begin);
            }

        }

        private bool verifyFooter()
        {
            byte[] Footer = new byte[0x10];
            byte[] IV = new byte[PSVIMGConstants.AES_BLOCK_SIZE];
            
            baseStream.Seek(baseStream.Length - (Footer.Length + IV.Length), SeekOrigin.Begin);
            baseStream.Read(IV, 0x00, PSVIMGConstants.AES_BLOCK_SIZE);
            baseStream.Read(Footer, 0x00, 0x10);

            byte[] FooterDec = aes_ecb_decrypt(Footer, IV);
            UInt64 FooterLen;
            using (MemoryStream ms = new MemoryStream(FooterDec))
            {
                ms.Seek(0x4, SeekOrigin.Current);
                ms.Seek(0x4, SeekOrigin.Current);
                byte[] LenInt = new byte[0x8];
                ms.Read(LenInt, 0x00, 0x8);
                FooterLen = BitConverter.ToUInt64(LenInt, 0x00);
            }
            if(Convert.ToUInt64(baseStream.Length) == FooterLen)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public PSVIMGStream(Stream file, byte[] KEY)
        {
            baseStream = file;
            key = KEY;
            if(!verifyFooter())
            {
                throw new Exception("Invalid KEY!");
            }
            blockStream = new MemoryStream();
            this.Seek(0x00, SeekOrigin.Begin);
            update();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long totalRead = 0;
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    
                    if (this.Position >= this.Length)
                    {
                        break;
                    }

                    long rem = getRemainingBlock();
                    long totalLeft = (count - totalRead);
                    if (rem < totalLeft)
                    {
                        byte[] remB = new byte[rem];
                        long read = blockStream.Read(remB, 0x00, remB.Length);
                        totalRead += read;
                        ms.Write(remB, 0x00, remB.Length);

                        long indx = getBlockIndex() + 1;
                        seekToBlock(indx);
                        update();
                    }
                    else
                    {
                        byte[] remB = new byte[totalLeft];
                        long read = blockStream.Read(remB, 0x00, remB.Length);
                        totalRead += read;
                        ms.Write(remB, 0x00, remB.Length);
                        break;
                    }
                   

                }
                ms.Seek(0x00, SeekOrigin.Begin);
                totalRead = ms.Read(buffer, offset, count);
                position += totalRead;
            }
            
            return Convert.ToInt32(totalRead);
        }

        public override void Flush()
        {
            baseStream.Flush();
            blockStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long ret = 0;
            if (origin == SeekOrigin.Begin)
            {
                long blockNo = getBlockIndex(offset);
                seekToBlock(blockNo);
                update();

                long onwards = offset % blockStream.Length;
                ret = blockStream.Seek(onwards, SeekOrigin.Begin);
                position = (baseStream.Position - PSVIMGConstants.AES_BLOCK_SIZE) + BlockPosition;
            }
            else if (origin == SeekOrigin.Current)
            {
                long currentPos = this.Position;
                this.Seek(currentPos + offset, SeekOrigin.Begin);
            }
            else if (origin == SeekOrigin.End)
            {
                long len = this.Length;
                this.Seek(Length + offset, SeekOrigin.Begin);
            }
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
            long blockIndex = getBlockIndex();
            byte[] decryptedBlock = getBlock(blockIndex);

            blockStream.Dispose();
            blockStream = new MemoryStream(decryptedBlock, 0x00, decryptedBlock.Length);
            blockStream.Write(decryptedBlock, 0x00, decryptedBlock.Length);
            blockStream.Seek(0x00, SeekOrigin.Begin);

            seekToBlock(blockIndex);
        }

        private long getBlockIndex(long pos = -1)
        {
            long blockOffset = 0;
            long blockEnd = blockOffset + PSVIMGConstants.FULL_PSVIMG_SIZE;
            long blockIndex = 0;
            if (pos < 0)
            {
                pos = this.Position;
            }
            while (blockOffset < this.Length)
            {
                blockOffset = (blockIndex * PSVIMGConstants.FULL_PSVIMG_SIZE);
                blockEnd = blockOffset + PSVIMGConstants.FULL_PSVIMG_SIZE;


                if (pos >= blockOffset && pos < blockEnd)
                {
                    return blockIndex;
                }
                blockIndex++;
            }
            throw new Exception("Not found somehow..");
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
            long blockOffset = (blockIndex * PSVIMGConstants.FULL_PSVIMG_SIZE) + PSVIMGConstants.AES_BLOCK_SIZE;

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
            if (PSVIMGConstants.FULL_PSVIMG_SIZE < remaining)
            {
                encryptedBlock = new byte[PSVIMGConstants.FULL_PSVIMG_SIZE];
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
