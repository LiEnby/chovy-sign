using PspCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopsBuilder
{
    public class StreamUtil
    {
        private Stream s;
        public StreamUtil(Stream s)
        {
            this.s = s;
        }

        public void WriteStrWithPadding(string str, byte b, int len)
        {
            byte[] sdata = Encoding.UTF8.GetBytes(str);
            if (len < sdata.Length)
            {
                s.Write(sdata, 0, len);
                return;
            }
            else
            {
                WriteBytes(sdata);
                WritePadding(b, (len - sdata.Length));
            }
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
        public void WriteStr(string str)
        {
            WriteBytes(Encoding.UTF8.GetBytes(str));
        }

        public void WritePadding(byte b, int len)
        {
            for(int i = 0; i < len; i++)
            {
                WriteByte(b);
            }
        }

        public void AlignTo(byte padByte, int align)
        {
            int padAmt = Convert.ToInt32(align - (s.Position % align));
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
