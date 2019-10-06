using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSVIMGTOOLS
{
    internal class strings
    {
        internal static string readTerminator(byte[] StringBytes)
        {
            string str = "";
            foreach (byte sByte in StringBytes)
            {
                if (sByte != 0x00)
                {
                    str += (char)sByte;
                }
                else
                {
                    return str;
                }
            }
            return str;
        }
    }
    class SceDateTime
    {
        public UInt16 Year;
        public UInt16 Month;
        public UInt16 Day;
        public UInt16 Hour;
        public UInt16 Minute;
        public UInt16 Second;
        public UInt32 Microsecond;

        public SceDateTime()
        {

        }
    }

    class SceIoStat
    {
        public UInt32 Mode;
        public uint Attributes;
        /** Size of the file in bytes. */
        public UInt64 Size;
        /** Creation time. */
        public SceDateTime CreationTime;
        /** Access time. */
        public SceDateTime AccessTime;
        /** Modification time. */
        public SceDateTime ModificaionTime;
        /** Device-specific data. */
        public UInt32[] Private = new UInt32[6];
        public SceIoStat()
        {

        }
    };

    class PsvImgTailer
    {
        public UInt64 Flags;
        public Byte[] Padding = new Byte[1004];
        public Byte[] bEnd = new Byte[12];

        public String End
        {
            get
            {
                return strings.readTerminator(bEnd);
            }
        }
    }
    class PsvImgHeader
    {
        public UInt64 SysTime;
        public UInt64 Flags;
        public SceIoStat Statistics;
        public Byte[] bParentPath = new Byte[256];
        public UInt32 unk_16C; // set to 1
        public Byte[] bPath = new Byte[256];
        public Byte[] Padding = new Byte[904];
        public Byte[] bEnd = new Byte[12];

        public String Path
        {
            get
            {
                return strings.readTerminator(bPath);
            }
        }

        public String End
        {
            get
            {
                return strings.readTerminator(bEnd);
            }
        }

        public String ParentPath
        {
            get
            {
                return strings.readTerminator(bParentPath);
            }
        }
        public PsvImgHeader()
        {

        }
    }


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
                    int remainBlock = Convert.ToInt32(psvStream.BlockRemaining - psvStream.SHA256_BLOCK_SIZE);
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
                        psvStream.Seek(psvStream.SHA256_BLOCK_SIZE, SeekOrigin.Current);
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
            long remainBlock = psvStream.BlockRemaining - psvStream.SHA256_BLOCK_SIZE;
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
                    SeekAdd += psvStream.SHA256_BLOCK_SIZE;
                }
                remainBlock = psvStream.PSVIMG_BLOCK_SIZE;
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
            stat.Mode = readUInt32();
            stat.Attributes = readUInt32();
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

                //Read Padding (if any)
                int padding;
                if ((size & (psvStream.PSVIMG_ENTRY_ALIGN - 1))>=1)
                {
                    padding = Convert.ToInt32(psvStream.PSVIMG_ENTRY_ALIGN - (size & (psvStream.PSVIMG_ENTRY_ALIGN - 1)));
                }
                else
                {
                    padding = 0;
                }

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
