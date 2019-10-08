using System;
using System.IO;
using static PSVIMGTOOLS.SceIoStat;

namespace PSVIMGTOOLS
{
    class PSVIMGFileStream : Stream
    {
        
        long length = 0;
        long startPos = 0;
        long endPos = 0;
        long position = 0;

        PSVIMGStream psvStream;
        public PSVIMGFileStream(PSVIMGStream psv, string Path)
        {
            psvStream = psv;
            findFile(Path);
        }

        public void WriteToFile(string FilePath)
        {
            using (FileStream fs = File.OpenWrite(FilePath))
            {
                fs.SetLength(0);
                int written = 0;
                long left = (length - written);
                byte[] work_buf;
                this.Seek(0x00, SeekOrigin.Begin);
                while (left > 0)
                {
                    left = (length - written);
                    if (left < 0x10000)
                    {
                        work_buf = new byte[left];
                    }
                    else
                    {
                        work_buf = new byte[0x10000];
                    }
                    this.Read(work_buf, 0x00, work_buf.Length);
                    fs.Write(work_buf, 0x00, work_buf.Length);
                    written += work_buf.Length;
                }


            }

        }
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
        public override long Length
        {
            get
            {
                return length;
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

        public override void Flush()
        {
            psvStream.Flush();
        }
        private int _read(byte[] buffer, int offset, int count)
        {

            using (MemoryStream ms = new MemoryStream())
            {
                int read = 0;
                while(true)
                {
                    int remainBlock = Convert.ToInt32(psvStream.BlockRemaining - PSVIMGConstants.SHA256_BLOCK_SIZE);
                    int remainRead = count - read;

                    if (remainRead < remainBlock)
                    {
                        byte[] tmp = new byte[remainRead];
                        psvStream.Read(tmp, 0x00, remainRead);
                        ms.Write(tmp,0x00,tmp.Length);
                        read += remainRead;
                        break;
                    }
                    else
                    {
                        byte[] tmp = new byte[remainBlock];
                        psvStream.Read(tmp, 0x00, remainBlock);
                        ms.Write(tmp, 0x00, tmp.Length);
                        psvStream.Seek(PSVIMGConstants.SHA256_BLOCK_SIZE, SeekOrigin.Current);
                        read += Convert.ToInt32(remainBlock);
                    }
                }
                ms.Seek(0x00, SeekOrigin.Begin);
                return ms.Read(buffer, offset, count);
            }
            
        }

        private long _seek(long amount,SeekOrigin orig)
        {
            long ToSeek = 0;
            long SeekAdd = 0;
            long remainBlock = psvStream.BlockRemaining - PSVIMGConstants.SHA256_BLOCK_SIZE;
            while (true)
            {
                long remainSeek = amount - ToSeek;

                if (remainSeek < remainBlock)
                {
                    ToSeek += remainSeek;
                    break;
                }
                else
                {
                    ToSeek += remainBlock;
                    SeekAdd += PSVIMGConstants.SHA256_BLOCK_SIZE;
                }
                remainBlock = PSVIMGConstants.PSVIMG_BLOCK_SIZE;
            }
            ToSeek += SeekAdd;
            return psvStream.Seek(ToSeek,orig);
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            if((startPos+count) > endPos)
            {
                count = Convert.ToInt32(endPos);
            }
            return _read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if(origin == SeekOrigin.Begin)
            {
                if(offset <= endPos)
                {
                    psvStream.Seek(startPos, SeekOrigin.Begin);
                    _seek(offset, SeekOrigin.Current);
                    position = offset;
                    return position;
                }
                else
                {
                    throw new IndexOutOfRangeException("Offset is out of range of file");
                }
                
            }
            else if(origin == SeekOrigin.Current)
            {
                if (offset <= endPos)
                {
                    _seek(offset, SeekOrigin.Current);
                    position += offset;
                    return position;
                }
                else
                {
                    throw new IndexOutOfRangeException("Offset is out of range of file");
                }
            }
            else
            {
                long realOffset = endPos + offset;
                if (realOffset <= endPos)
                {
                    _seek(realOffset, SeekOrigin.Begin);
                    return this.Position;
                }
                else
                {
                    throw new IndexOutOfRangeException("Offset is out of range of file");
                }
            }
            
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException("PSVFileStream is Read-Only");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException("PSVFileStream is Read-Only");
        }

        private ushort readUInt16()
        {
            byte[] intBuf = new byte[0x2];
            _read(intBuf, 0x00, 0x02);
            return BitConverter.ToUInt16(intBuf,0);
        }
        private uint readUInt32()
        {
            byte[] intBuf = new byte[0x4];
            _read(intBuf, 0x00, 0x04);
            return BitConverter.ToUInt32(intBuf,0);
        }
        private ulong readUInt64()
        {
            byte[] intBuf = new byte[0x8];
            _read(intBuf, 0x00, 0x08);
            return BitConverter.ToUInt64(intBuf,0);
        }
        private SceDateTime readDatetime()
        {
            SceDateTime dateTime = new SceDateTime();
            dateTime.Year = readUInt16();
            dateTime.Month = readUInt16();
            dateTime.Day = readUInt16();
            dateTime.Hour = readUInt16();
            dateTime.Minute = readUInt16();
            dateTime.Second = readUInt16();
            dateTime.Microsecond = readUInt32();
            return dateTime;
        }
        private SceIoStat readStats()
        {
            SceIoStat stat = new SceIoStat();
            stat.Mode = (Modes)readUInt32();
            stat.Attributes = (AttributesEnum)readUInt32();
            stat.Size = readUInt64();
            stat.CreationTime = readDatetime();
            stat.AccessTime = readDatetime();
            stat.ModificaionTime = readDatetime();
            for(int i = 0; i < stat.Private.Length; i++)
            {
                stat.Private[i] = readUInt32();
            }
            return stat;
        }
        private PsvImgHeader readHeader()
        {
            PsvImgHeader header = new PsvImgHeader();
            header.SysTime = readUInt64();
            header.Flags = readUInt64();
            header.Statistics = readStats();
            _read(header.bParentPath, 0x00, 256);
            header.unk_16C = readUInt32();
            _read(header.bPath, 0x00, 256);
            _read(header.Padding, 0x00, 904);
            _read(header.bEnd, 0x00, 12);
            return header;
        }

        private PsvImgTailer readTailer()
        {
            PsvImgTailer tailer = new PsvImgTailer();
            tailer.Flags = readUInt64();
            _read(tailer.Padding, 0x00, 1004);
            _read(tailer.bEnd, 0x00, 12);
            return tailer;
        }
        private void findFile(string path)
        {
            _seek(0x00, SeekOrigin.Begin);
            while (psvStream.Position < psvStream.Length)
            {
                PsvImgHeader header = readHeader();
                long size = (long)header.Statistics.Size;
                long padding = PSVIMGPadding.GetPadding(size);

                if (header.Path == path)
                {
                    length = size;
                    startPos = psvStream.Position;
                    endPos = startPos + length;
                    return;
                }
                else
                {
                    _seek(size+padding, SeekOrigin.Current);
                    PsvImgTailer tailer = readTailer();
                }

            }
            throw new FileNotFoundException("Cannot find file specified");

        }
    }
}
