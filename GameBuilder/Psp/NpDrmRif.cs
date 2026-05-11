using Ionic.Zlib;
using Li.Utilities;
using System.Text;

namespace GameBuilder.Psp
{
    public class NpDrmRif
    {
        public NpDrmRif(string zRif)
        {
            Rif = ZlibStream.UncompressBuffer(Convert.FromBase64String(zRif));
        }
        public NpDrmRif(byte[] rifData)
        {
            Rif = rifData;
        }

        public byte[] Rif;
        public string ZRif
        {
            get
            {
                return Convert.ToBase64String(ZlibStream.CompressBuffer(Rif));
            }
        }
        public UInt64 AccountIdBE
        {
            get
            {
                byte[] aidbytes = new byte[8];
                Array.Copy(Rif, 8, aidbytes, 0, aidbytes.Length);
                Array.Reverse(aidbytes);
                return BitConverter.ToUInt64(aidbytes);
            }
        }
        public UInt64 AccountId
        {
            get
            {
                return BitConverter.ToUInt64(Rif, 0x8);
            }
        }

        public string ContentId
        {
            get
            {
                return Encoding.UTF8.GetString(Rif, 0x10, 0x24);
            }
        }

        public static NpDrmRif CreateNoPspEmuDrmRif(string contentId)
        {
            using(MemoryStream rifStream = new MemoryStream())
            {
                StreamUtil rifUtil = new StreamUtil(rifStream);

                rifUtil.WriteUInt64(0x00); // start of rif
                rifUtil.WriteUInt64(0x0123456789ABCDEF); // accountId
                rifUtil.WriteCStrWithPadding(contentId, 0x00, 0x30); // contentId
                rifUtil.WritePadding(0x00, 0x30); // key1,key2
                rifUtil.WritePadding(0xFF, 0x28); // ecdsa sig

                rifStream.Seek(0, SeekOrigin.Begin);
                return new NpDrmRif(rifStream.ToArray());
            }
        }
    }
}
