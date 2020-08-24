using System.IO;
using System;
using Ionic.Zlib;
namespace Popstation
{
    public class Compression
    {
        public static byte[] Decompress(byte[] bytes)
        {
            return ZlibStream.UncompressBuffer(bytes);
        }

        public static int Decompress(byte[] bytes, byte[] outbuf)
        {
            byte[] Decompressed = ZlibStream.UncompressBuffer(bytes);
            Array.ConstrainedCopy(Decompressed, 0, outbuf, 0, outbuf.Length);
            return Decompressed.Length;
        }

        public static byte[] Compress(byte[] inbuf, int level)
        {
            using (var ms = new MemoryStream())
            {
                using (var outStream = new ZlibStream(ms, CompressionMode.Compress, (CompressionLevel)level))
                {
                    outStream.Write(inbuf, 0, inbuf.Length);
                    outStream.Flush();
                    return ms.ToArray();
                }
            }
        }

        public static int Compress(byte[] inbuf, byte[] outbuf, int level)
        {
            using (var ms = new MemoryStream(outbuf))
            {
                using (var outStream = new ZlibStream(ms, CompressionMode.Compress, (CompressionLevel)level))
                {
                    outStream.Write(inbuf, 0, inbuf.Length);
                    outStream.Flush();
                    return (int)ms.Position;
                }
            }
        }
    }
}