using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Li.Utilities
{
    public class StreamUtil
    {
        private Stream s;
        public StreamUtil(Stream s)
        {
            this.s = s;
        }
        public string ReadStrLen(int len)
        {
            return Encoding.UTF8.GetString(ReadBytes(len));
        }
        public string ReadCStr()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    byte c = (byte)s.ReadByte();
                    if (c == 0)
                        break;
                    ms.WriteByte(c);
                }
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
        public UInt32 ReadUInt32At(int location)
        {
            long oldPos = s.Position;
            s.Seek(location, SeekOrigin.Begin);
            UInt32 outp = ReadUInt32();
            s.Seek(oldPos, SeekOrigin.Begin);
            return outp;
        }

        public Int32 ReadInt32At(int location)
        {
            long oldPos = s.Position;
            s.Seek(location, SeekOrigin.Begin);
            Int32 outp = ReadInt32();
            s.Seek(oldPos, SeekOrigin.Begin);
            return outp;
        }

        public byte[] ReadBytesAt(int location, int length)
        {
            long oldPos = s.Position;
            s.Seek(location, SeekOrigin.Begin);
            byte[] work_buf = ReadBytes(length);
            s.Seek(oldPos, SeekOrigin.Begin);
            return work_buf;
        }

        public string ReadStringAt(int location)
        {
            long oldPos = s.Position;
            s.Seek(location, SeekOrigin.Begin);
            string outp = ReadCStr();
            s.Seek(oldPos, SeekOrigin.Begin);
            return outp;
        }
        public byte ReadByte()
        {
            return (byte)s.ReadByte();
        }
        public byte[] ReadBytes(int len)
        {
            byte[] data = new byte[len];
            s.Read(data, 0x00, len);
            return data;
        }
        public UInt16 ReadUInt16()
        {
            byte[] vbytes = ReadBytes(0x2);
            return BitConverter.ToUInt16(vbytes);
        }
        public Int16 ReadInt16()
        {
            byte[] vbytes = ReadBytes(0x2);
            return BitConverter.ToInt16(vbytes);
        }
        public UInt32 ReadUInt32()
        {
            byte[] vbytes = ReadBytes(0x4);
            return BitConverter.ToUInt32(vbytes);
        }
        public Int32 ReadInt32()
        {
            byte[] vbytes = ReadBytes(0x4);
            return BitConverter.ToInt32(vbytes);
        }
        public void WriteBytesWithPadding(byte[] data, byte b, int len)
        {
            if (len < data.Length)
            {
                s.Write(data, 0, len);
                return;
            }
            else
            {
                WriteBytes(data);
                WritePadding(b, (len - data.Length));
            }
        }
        public void WriteStrWithPadding(string str, byte b, int len)
        {
            WriteBytesWithPadding(Encoding.UTF8.GetBytes(str), b, len);            
        }
        public void WriteInt64(Int64 v)
        {
            WriteBytes(BitConverter.GetBytes(v));
        }
        public void WriteUInt16(UInt16 v)
        {
            WriteBytes(BitConverter.GetBytes(v));
        }
        public void WriteInt16(Int16 v)
        {
            WriteBytes(BitConverter.GetBytes(v));
        }

        public void WriteUInt32(UInt32 v)
        {
            WriteBytes(BitConverter.GetBytes(v));
        }
        public void WriteInt32BE(Int32 v)
        {
            WriteBytes(BitConverter.GetBytes(v).Reverse().ToArray());
        }
        public void WriteInt32(Int32 v)
        {
            WriteBytes(BitConverter.GetBytes(v));
        }
        public void WriteCStr(string str)
        {
            WriteStr(str);
            WriteByte(0x00);
        }
        public void WriteStr(string str)
        {
            WriteBytes(Encoding.UTF8.GetBytes(str));
        }

        public void WritePadding(byte b, int len)
        {
            if (len < 0) return;
            for(int i = 0; i < len; i++)
            {
                WriteByte(b);
            }
        }

        public void AlignTo(byte padByte, int align)
        {
            int padAmt = MathUtil.CalculatePaddingAmount(Convert.ToInt32(s.Position), align); 

            this.WritePadding(padByte, padAmt);
        }
        public void PadUntil(byte b, int len)
        {
            int remain = Convert.ToInt32(len - s.Length);
            WritePadding(b, remain);
        }
        public void WriteBytes(byte[] bytes)
        {
            s.Write(bytes, 0, bytes.Length);
        }
        public void WriteByte(byte b)
        {
            s.WriteByte(b);
        }

    }
}
