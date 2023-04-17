using Li.Progress;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using static Vita.PsvImgTools.SceIoStat;

namespace Vita.PsvImgTools
{
    public class PSVIMGBuilder : ProgressTracker
    {
        private const Int64 BUFFER_SZ = 0x33554432;
        private byte[] IV = new byte[0x10];
        private byte[] KEY;
        private Random rnd = new Random();
        private Stream mainStream;
        private Sha256Digest shaCtx;
        private byte[] blockData;
        private MemoryStream blockStream;
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

        private byte[] aes_cbc_encrypt(byte[] plainText, byte[] IV, byte[] KEY, int size=-1)
        {
            if (size < 0)
            {
                size = plainText.Length;
            }

            MemoryStream ms = new MemoryStream();
           /* DEBUG Disable Encryption
            ms.Write(plainText, 0x00, size);
            ms.Seek(0x00,SeekOrigin.Begin);
            return ms.ToArray();*/

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

        private byte[] aes_ecb_encrypt(byte[] plainText, byte[] KEY, int size = -1)
        {
            if (size < 0)
            {
                size = plainText.Length;
            }

            MemoryStream ms = new MemoryStream();
            /* DEBUG Disable Encryption
             ms.Write(plainText, 0x00, size);
             ms.Seek(0x00,SeekOrigin.Begin);
             return ms.ToArray();*/

            Aes alg = Aes.Create();
            alg.Mode = CipherMode.ECB;
            alg.Padding = PaddingMode.None;
            alg.KeySize = 256;
            alg.BlockSize = 128;
            alg.Key = KEY;
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(plainText, 0, size);
            cs.Close();
            byte[] cipherText = ms.ToArray();
            return cipherText;
        }

        // TODO: Switch to Li.Utilities.StreamUtil for this .
        private void writeUInt64(Stream dst,UInt64 value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x8);
        }
        private void writeInt64(Stream dst,Int64 value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x8);
        }
        private void writeUInt16(Stream dst, UInt16 value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x2);
        }
        private void writeInt16(Stream dst, Int16 value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x2);
        }

        private void writeInt32(Stream dst, Int32 value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x4);
        }
        private void writeUInt32(Stream dst, UInt32 value)
        {
            byte[] ValueBytes = BitConverter.GetBytes(value);
            dst.Write(ValueBytes, 0x00, 0x4);
        }

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

        internal virtual SceIoStat sceIoStat(Stream str)
        {
            SceIoStat stats = new SceIoStat();
            
            // streams being a directory doesnt really make sense ..
            stats.Mode |= Modes.File;

            // set size..
            stats.Size = Convert.ToUInt64(str.Length);

            // fake the rest--
            stats.Mode |= Modes.GroupRead;
            stats.Mode |= Modes.GroupWrite;

            stats.Mode |= Modes.OthersRead;
            stats.Mode |= Modes.OthersWrite;

            stats.Mode |= Modes.UserRead;
            stats.Mode |= Modes.UserWrite;

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
                stats.Mode |= Modes.Directory;
                stats.Size = 0;
            }
            else
            {
                stats.Mode |= Modes.File;
                stats.Size = Convert.ToUInt64(new FileInfo(path).Length);
            }
            
            if(attrbutes.HasFlag(FileAttributes.ReadOnly))
            {
                stats.Mode |= Modes.GroupRead;
                
                stats.Mode |= Modes.OthersRead;

                stats.Mode |= Modes.UserRead;
            }
            else
            {
                stats.Mode |= Modes.GroupRead;
                stats.Mode |= Modes.GroupWrite;
                
                stats.Mode |= Modes.OthersRead;
                stats.Mode |= Modes.OthersWrite;

                stats.Mode |= Modes.UserRead;
                stats.Mode |= Modes.UserWrite;
            }

            stats.CreationTime = dateTimeToSceDateTime(File.GetCreationTimeUtc(path));
            stats.AccessTime = dateTimeToSceDateTime(File.GetLastAccessTimeUtc(path));
            stats.ModificaionTime = dateTimeToSceDateTime(File.GetLastWriteTimeUtc(path));
            
            return stats;
        }

        internal virtual void writeSceDateTime(Stream dst,SceDateTime time)
        {
            writeUInt16(dst, time.Year);
            writeUInt16(dst, time.Month);
            writeUInt16(dst, time.Day);

            writeUInt16(dst, time.Hour);
            writeUInt16(dst, time.Minute);
            writeUInt16(dst, time.Second);
            writeUInt32(dst, time.Microsecond);
        }

        internal virtual void writeSceIoStat(Stream dst, SceIoStat stats)
        {
            writeUInt32(dst, Convert.ToUInt32(stats.Mode));
            writeUInt32(dst, Convert.ToUInt32(stats.Attributes));
            writeUInt64(dst, stats.Size);
            writeSceDateTime(dst, stats.CreationTime);
            writeSceDateTime(dst, stats.AccessTime);
            writeSceDateTime(dst, stats.ModificaionTime);
            foreach(UInt32 i in stats.Private)
            {
                writeUInt32(dst,i);
            }
        }

        internal virtual void memset(byte[] buf, byte content, long length)
        {
            for(int i = 0; i < length; i++)
            {
                buf[i] = content;
            }
        }

        internal virtual void writeStringWithPadding(Stream dst, string str, int padSize, byte padByte = 0x78)
        {
            int StrLen = str.Length;
            if(StrLen > padSize)
            {
                StrLen = padSize;
            }

            int PaddingLen = (padSize - StrLen)-1;
            writeString(dst, str, StrLen);
            dst.WriteByte(0x00);
            writePadding(dst, padByte, PaddingLen);
        }

        internal virtual void writeString(Stream dst, string str, int len=-1)
        {
            if(len < 0)
            {
                len = str.Length;
            }

            byte[] StrBytes = Encoding.UTF8.GetBytes(str);
            dst.Write(StrBytes, 0x00, len);
        }

        internal virtual void writePadding(Stream dst, byte paddingByte, long paddingLen)
        {
            byte[] paddingData = new byte[paddingLen];
            memset(paddingData, paddingByte, paddingLen);
            dst.Write(paddingData, 0x00, paddingData.Length);
        }
        internal virtual byte[] getHeader(SceIoStat Stat, string ParentPath, string PathRel)
        {
            using (MemoryStream Header = new MemoryStream())
            {
                writeInt64(Header, DateTime.UtcNow.Ticks); // SysTime
                writeInt64(Header, 0); // Flags
                writeSceIoStat(Header, Stat);
                writeStringWithPadding(Header, ParentPath, 256); // Parent Path
                writeUInt32(Header, 1); //unk_16C
                writeStringWithPadding(Header, PathRel, 256); //Relative Path
                writePadding(Header, 0x78, 904); //'x'
                writeString(Header, PSVIMGConstants.PSVIMG_HEADER_END); //EndOfHeader
                Header.Seek(0x00, SeekOrigin.Begin);
                return Header.ToArray();
            }
        }
        internal virtual byte[] getHeader(string FilePath, string ParentPath, string PathRel)
        {
            using (MemoryStream Header = new MemoryStream())
            {
                writeInt64(Header, DateTime.UtcNow.Ticks); // SysTime
                writeInt64(Header, 0); // Flags
                writeSceIoStat(Header, sceIoStat(FilePath));
                writeStringWithPadding(Header, ParentPath, 256); // Parent Path
                writeUInt32(Header, 1); //unk_16C
                writeStringWithPadding(Header, PathRel, 256); //Relative Path
                writePadding(Header, 0x78, 904); //'x'
                writeString(Header, PSVIMGConstants.PSVIMG_HEADER_END); //EndOfHeader
                Header.Seek(0x00, SeekOrigin.Begin);
                return Header.ToArray();
            }       
        }

        internal virtual void startNewBlock()
        {
            blockData = new byte[PSVIMGConstants.FULL_PSVIMG_SIZE];
            blockStream = new MemoryStream(blockData, 0x00, PSVIMGConstants.FULL_PSVIMG_SIZE);
        }


        internal virtual byte[] shaBlock(int length = PSVIMGConstants.PSVIMG_BLOCK_SIZE,bool final=false)
        {
            byte[] outbytes = new byte[PSVIMGConstants.SHA256_BLOCK_SIZE];
            shaCtx.BlockUpdate(blockData, 0x00, length);
            Sha256Digest shaTmp = (Sha256Digest)shaCtx.Copy();
            shaTmp.DoFinal(outbytes,0x00);
            return outbytes;
        }

        internal virtual void finishBlock(bool final = false)
        {
            int len = Convert.ToInt32(blockStream.Position);
            byte[] shaBytes = shaBlock(len, final);
            blockStream.Write(shaBytes, 0x00, PSVIMGConstants.SHA256_BLOCK_SIZE);
            len += PSVIMGConstants.SHA256_BLOCK_SIZE;
            
            //Get next IV
            byte[] encryptedBlock = aes_cbc_encrypt(blockData, IV, KEY, len);
            for (int i = 0; i < IV.Length; i++)
            {
                int encBlockOffset = (encryptedBlock.Length - IV.Length)+i;
                IV[i] = encryptedBlock[encBlockOffset];
            }

            mainStream.Write(encryptedBlock, 0x00, encryptedBlock.Length);
            totalBytes += encryptedBlock.Length;

            blockStream.Dispose();
        }

        internal virtual int remainingBlockSize()
        {
            return Convert.ToInt32((PSVIMGConstants.PSVIMG_BLOCK_SIZE - blockStream.Position));
        }

        internal virtual void writeBlock(byte[] data, bool update=false)
        {
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
                long paddingSize = PSVIMGPadding.GetPadding(size);
                if(paddingSize != 0)
                {
                    writePadding(ms, 0x2B, paddingSize-PSVIMGConstants.PSVIMG_PADDING_END.Length);
                    writeString(ms, PSVIMGConstants.PSVIMG_PADDING_END);
                }
                ms.Seek(0x00, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        internal virtual byte[] getTailer()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                writeUInt64(ms, 0x00);
                writePadding(ms, 0x7a, 1004);
                writeString(ms, PSVIMGConstants.PSVIMG_TAILOR_END);

                ms.Seek(0x00, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        internal virtual void writeStream(Stream dst)
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

                updateProgress(Convert.ToInt32(dst.Position), Convert.ToInt32(dst.Length), "PSVIMG Creation");
            }
        }
        
        private byte[] getFooter()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                totalBytes += 0x10; //number of bytes used by this footer.
                
                writeInt32(ms, 0x00); // int padding (idk wht this is)
                writeUInt32(ms, 0x00);
                writeInt64(ms, totalBytes);
                ms.Seek(0x00, SeekOrigin.Begin);
                return aes_cbc_encrypt(ms.ToArray(), IV, KEY);
            }
            
        }
        public void AddFile(Stream sData, string ParentPath, string PathRel)
        {
            SceIoStat stat = sceIoStat(sData);
            long sz = Convert.ToInt64(stat.Size);
            writeBlock(getHeader(stat, ParentPath, PathRel));
            writeStream(sData);
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
                writeStream(fs);
            writeBlock(getPadding(sz));
            writeBlock(getTailer());
            contentSize += sz;
            finished = true;
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

            blockStream.Dispose();
            mainStream.Dispose();
            return contentSize;
        }


        public PSVIMGBuilder(Stream dst, byte[] Key)
        {
            totalBytes = 0;
            contentSize = 0;
            shaCtx = new Sha256Digest();
            mainStream = dst;
            KEY = Key;

            rnd.NextBytes(IV);
            IV = aes_ecb_encrypt(IV, Key);

            mainStream.Write(IV, 0x00, IV.Length);
            totalBytes += IV.Length;

            startNewBlock();
        }
    }
}
