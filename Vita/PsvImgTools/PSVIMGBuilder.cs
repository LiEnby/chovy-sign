using Li.Progress;
using Li.Utilities;

using Org.BouncyCastle.Crypto.Digests;
using System.Security.Cryptography;
using System.Text;

namespace Vita.PsvImgTools
{
    public class PSVIMGBuilder : ProgressTracker
    {
        private const Int64 BUFFER_SZ = 0x10000;
        private byte[] iv = new byte[0x10];
        private byte[] key;
        private Random rand = new Random();
        private Stream mainStream;
        private Sha256Digest shaCtx;
        private byte[]? blockData;
        private MemoryStream? blockStream;
        private long contentSize = 0;

        //async
        private int blocksWritten = 0;
        private bool finished = false;
        
        public Int64 ContentSize
        {
            get
            {
                return contentSize;
            }
        }
        public Int32 BlocksWritten
        {
            get
            {
                return blocksWritten;
            }
        }

        public Boolean HasFinished
        {
            get
            {
                return finished;
            }
        }

        //Footer
        private long totalBytes = 0;



        internal virtual SceDateTime dateTimeToSceDateTime(DateTime dt)
        {
            SceDateTime sdt  = new SceDateTime();
            sdt.Day = Convert.ToUInt16(dt.Day);
            sdt.Month = Convert.ToUInt16(dt.Month);
            sdt.Year = Convert.ToUInt16(dt.Year);


            sdt.Hour = Convert.ToUInt16(dt.Hour);
            sdt.Minute = Convert.ToUInt16(dt.Minute);
            sdt.Second = Convert.ToUInt16(dt.Second);
            sdt.Microsecond = Convert.ToUInt32(dt.Millisecond * 1000);
            return sdt;
        }
        internal virtual SceIoStat sceIoStat()
        {
            SceIoStat stats = new SceIoStat();

            stats.Mode |= SceIoStat.Modes.Directory;
            // set size..
            stats.Size = 0;

            // fake the rest--
            stats.Mode |= SceIoStat.Modes.GroupRead;
            stats.Mode |= SceIoStat.Modes.GroupWrite;

            stats.Mode |= SceIoStat.Modes.OthersRead;
            stats.Mode |= SceIoStat.Modes.OthersWrite;

            stats.Mode |= SceIoStat.Modes.UserRead;
            stats.Mode |= SceIoStat.Modes.UserWrite;

            stats.CreationTime = dateTimeToSceDateTime(DateTime.Now);
            stats.AccessTime = dateTimeToSceDateTime(DateTime.Now);
            stats.ModificaionTime = dateTimeToSceDateTime(DateTime.Now);

            return stats;
        }
        internal virtual SceIoStat sceIoStat(Stream str)
        {
            SceIoStat stats = new SceIoStat();
            
            // streams being a directory doesnt really make sense ..
            stats.Mode |= SceIoStat.Modes.File;

            // set size..
            stats.Size = Convert.ToUInt64(str.Length);

            // fake the rest--
            stats.Mode |= SceIoStat.Modes.GroupRead;
            stats.Mode |= SceIoStat.Modes.GroupWrite;

            stats.Mode |= SceIoStat.Modes.OthersRead;
            stats.Mode |= SceIoStat.Modes.OthersWrite;

            stats.Mode |= SceIoStat.Modes.UserRead;
            stats.Mode |= SceIoStat.Modes.UserWrite;

            stats.CreationTime = dateTimeToSceDateTime(DateTime.Now);
            stats.AccessTime = dateTimeToSceDateTime(DateTime.Now);
            stats.ModificaionTime = dateTimeToSceDateTime(DateTime.Now);

            return stats;
        }
        internal virtual SceIoStat sceIoStat(string path)
        {
            SceIoStat stats = new SceIoStat();
            FileAttributes attrbutes = File.GetAttributes(path);
            
            if (attrbutes.HasFlag(FileAttributes.Directory))
            {
                stats.Mode |= SceIoStat.Modes.Directory;
                stats.Size = 0;
            }
            else
            {
                stats.Mode |= SceIoStat.Modes.File;
                stats.Size = Convert.ToUInt64(new FileInfo(path).Length);
            }
            
            if(attrbutes.HasFlag(FileAttributes.ReadOnly))
            {
                stats.Mode |= SceIoStat.Modes.GroupRead;
                
                stats.Mode |= SceIoStat.Modes.OthersRead;

                stats.Mode |= SceIoStat.Modes.UserRead;
            }
            else
            {
                stats.Mode |= SceIoStat.Modes.GroupRead;
                stats.Mode |= SceIoStat.Modes.GroupWrite;
                
                stats.Mode |= SceIoStat.Modes.OthersRead;
                stats.Mode |= SceIoStat.Modes.OthersWrite;

                stats.Mode |= SceIoStat.Modes.UserRead;
                stats.Mode |= SceIoStat.Modes.UserWrite;
            }

            stats.CreationTime = dateTimeToSceDateTime(File.GetCreationTimeUtc(path));
            stats.AccessTime = dateTimeToSceDateTime(File.GetLastAccessTimeUtc(path));
            stats.ModificaionTime = dateTimeToSceDateTime(File.GetLastWriteTimeUtc(path));
            
            return stats;
        }


        internal virtual byte[] getHeader(SceIoStat stat, string parentPath, string pathRel)
        {
            using (MemoryStream header = new MemoryStream())
            {
                PSVIMGStreamUtil headerUtil = new PSVIMGStreamUtil(header);

                headerUtil.WriteInt64(DateTime.UtcNow.Ticks); // sysTime
                headerUtil.WriteInt64(0); // flags
                headerUtil.WriteSceIoStat(stat);
                headerUtil.WriteCStrWithPadding(parentPath, PSVIMGConstants.PSVIMG_HEAD_PAD_BYTE, 256); // Parent Path
                headerUtil.WriteUInt32(1); //unk_16C
                headerUtil.WriteCStrWithPadding(pathRel, PSVIMGConstants.PSVIMG_HEAD_PAD_BYTE, 256); //Relative Path
                headerUtil.WritePadding(PSVIMGConstants.PSVIMG_HEAD_PAD_BYTE, 904); //'x'
                headerUtil.WriteStr(PSVIMGConstants.PSVIMG_HEADER_END); //EndOfHeader
                header.Seek(0x00, SeekOrigin.Begin);
                return header.ToArray();
            }
        }
        internal virtual byte[] getHeader(string filePath, string parentPath, string pathRel)
        {
            return getHeader(sceIoStat(filePath), parentPath, pathRel);    
        }

        internal virtual void startNewBlock()
        {
            blockData = new byte[PSVIMGConstants.FULL_PSVIMG_BLOCK_SIZE];
            blockStream = new MemoryStream(blockData, 0x00, PSVIMGConstants.FULL_PSVIMG_BLOCK_SIZE);
        }


        internal virtual byte[] shaBlock(int length = PSVIMGConstants.PSVIMG_BLOCK_SIZE,bool final=false)
        {
            if (blockData is null) throw new NullReferenceException("blockData is null");

            byte[] outbytes = new byte[PSVIMGConstants.SHA256_BLOCK_SIZE];
            shaCtx.BlockUpdate(blockData, 0x00, length);
            Sha256Digest shaTmp = (Sha256Digest)shaCtx.Copy();
            shaTmp.DoFinal(outbytes,0x00);
            return outbytes;
        }

        internal virtual void finishBlock(bool final = false)
        {
            if (blockStream is null) throw new NullReferenceException("blockStream is null");
            if (blockData is null) throw new NullReferenceException("blockData is null");

            int len = Convert.ToInt32(blockStream.Position);
            byte[] shaBytes = shaBlock(len, final);
            blockStream.Write(shaBytes, 0x00, PSVIMGConstants.SHA256_BLOCK_SIZE);
            len += PSVIMGConstants.SHA256_BLOCK_SIZE;
            
            //Get next IV
            byte[] encryptedBlock = CryptoUtil.aes_cbc_encrypt(blockData, iv, key, len);
            for (int i = 0; i < iv.Length; i++)
            {
                int encBlockOffset = (encryptedBlock.Length - iv.Length)+i;
                iv[i] = encryptedBlock[encBlockOffset];
            }

            mainStream.Write(encryptedBlock, 0x00, encryptedBlock.Length);
            totalBytes += encryptedBlock.Length;

            blockStream.Dispose();
        }

        internal virtual int remainingBlockSize()
        {
            if (blockStream is null) throw new NullReferenceException("blockStream is null");
            return Convert.ToInt32((PSVIMGConstants.PSVIMG_BLOCK_SIZE - blockStream.Position));
        }

        internal virtual void writeBlock(byte[] data, bool update=false)
        {
            if (blockStream is null) throw new NullReferenceException("blockStream is null");

            long dLen = data.Length;
            long writeTotal = 0;
            while (dLen > 0)
            {
                int remaining = remainingBlockSize();

                if (dLen > remaining)
                {
                    byte[] dataRemains = new byte[remaining];
                    Array.Copy(data, writeTotal, dataRemains, 0, remaining);
                    blockStream.Write(dataRemains, 0x00, remaining);

                    writeTotal += remaining;
                    dLen -= remaining;
                    

                    finishBlock();
                    startNewBlock();
                    if (update)
                        blocksWritten += 1;
                }
                else
                {
                    byte[] dataRemains = new byte[dLen];
                    Array.Copy(data, writeTotal, dataRemains, 0, dLen);
                    blockStream.Write(dataRemains, 0x00, Convert.ToInt32(dLen));
                    
                    writeTotal += dLen;
                    dLen -= dLen;
                }
            }
        }
        internal virtual byte[] getPadding(long size)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                PSVIMGStreamUtil padUtil = new PSVIMGStreamUtil(ms);

                long paddingSize = PSVIMGPadding.GetPadding(size);
                if(paddingSize != 0)
                {
                    padUtil.WritePadding(PSVIMGConstants.PSVIMG_PAD_BYTE, Convert.ToInt32(paddingSize-PSVIMGConstants.PSVIMG_PADDING_END.Length));
                    padUtil.WriteStr(PSVIMGConstants.PSVIMG_PADDING_END);
                }
                ms.Seek(0x00, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        internal virtual byte[] getTailer()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                PSVIMGStreamUtil tailUtil = new PSVIMGStreamUtil(ms);
                tailUtil.WriteUInt64(0x00);
                tailUtil.WritePadding(PSVIMGConstants.PSVIMG_TAIL_PAD_BYTE, 1004);
                tailUtil.WriteStr(PSVIMGConstants.PSVIMG_TAILOR_END);

                ms.Seek(0x00, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        internal virtual void writeStream(Stream dst, string file)
        {
            while(dst.Position < dst.Length)
            {
                byte[] work_buf;
                Int64 bytes_remain = (dst.Length - dst.Position);
                if (bytes_remain > BUFFER_SZ)
                    work_buf = new byte[BUFFER_SZ];
                else
                    work_buf = new byte[bytes_remain];
                dst.Read(work_buf, 0x00, work_buf.Length);
                writeBlock(work_buf, true);

                updateProgress(Convert.ToInt32(dst.Position), Convert.ToInt32(dst.Length), "PSVIMG Package File: " + Path.GetFileName(file));
            }
        }
        
        private byte[] getFooter()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                PSVIMGStreamUtil footUtil = new PSVIMGStreamUtil(ms);
                totalBytes += 0x10; //number of bytes used by this footer.
                
                footUtil.WriteInt32(0x00); // int padding (idk wht this is)
                footUtil.WriteUInt32(0x00);
                footUtil.WriteInt64(totalBytes);
                ms.Seek(0x00, SeekOrigin.Begin);
                return CryptoUtil.aes_cbc_encrypt(ms.ToArray(), iv, key);
            }
            
        }
        public void AddFile(byte[] sData, string ParentPath, string PathRel)
        {
            using (MemoryStream ms = new MemoryStream(sData))
            {
                AddFile(ms, ParentPath, PathRel);
            }
        }
        public void AddFile(Stream sData, string ParentPath, string PathRel)
        {
            SceIoStat stat = sceIoStat(sData);
            long sz = Convert.ToInt64(stat.Size);
            writeBlock(getHeader(stat, ParentPath, PathRel));
            writeStream(sData, PathRel);
            writeBlock(getPadding(sz));
            writeBlock(getTailer());
            contentSize += sz;
            finished = true;
        }
        public void AddFile(string FilePath, string ParentPath, string PathRel)
        {

            long sz = Convert.ToInt64(sceIoStat(FilePath).Size);
            writeBlock(getHeader(FilePath, ParentPath, PathRel));
            using (FileStream fs = File.OpenRead(FilePath))
                writeStream(fs, PathRel);
            writeBlock(getPadding(sz));
            writeBlock(getTailer());
            contentSize += sz;
            finished = true;
        }
        public void AddDir(string ParentPath, string PathRel)
        {
            writeBlock(getHeader(sceIoStat(), ParentPath, PathRel));
            writeBlock(getPadding(0));
            writeBlock(getTailer());
        }
        public void AddDir(string DirPath, string ParentPath, string PathRel)
        {
            writeBlock(getHeader(DirPath, ParentPath, PathRel));
            writeBlock(getPadding(0));
            writeBlock(getTailer());
        }
        public long Finish()
        {
            finishBlock(true);
            byte[] footer = getFooter();
            mainStream.Write(footer, 0x00, footer.Length);

            if (blockStream is not null) blockStream.Dispose();
            return contentSize;
        }


        public PSVIMGBuilder(Stream dst, byte[] Key)
        {
            this.totalBytes = 0;
            this.contentSize = 0;
            this.shaCtx = new Sha256Digest();
            this.mainStream = dst;
            this.key = Key;

            rand.NextBytes(iv);
            this.iv = CryptoUtil.aes_ecb_encrypt(iv, Key);

            this.mainStream.Write(iv, 0x00, iv.Length);
            totalBytes += iv.Length;

            startNewBlock();
        }
    }
}
