using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace PspCrypto
{
    public class Lz
    {
        public static byte[] compress(byte[] in_buf, bool np9660=false)
        {
            //Decoder decoder = new Decoder();
            //using var inStream = new MemoryStream(@in);
            //using var outStream = new MemoryStream(@out);
            //byte[] properties = new byte[5];
            //inStream.Read(properties, 0, 5);
            //decoder.SetDecoderProperties(properties);
            //decoder.Code(inStream, outStream, insize, size, null);
            //return 0;
            var lzrc = new Lzrc(np9660);

            // compress data, and get the compressed data length
            return lzrc.lzrc_compress(in_buf, in_buf.Length);
        }
        public static int decompress(byte[] @out, byte[] @in, int size, int insize, bool np9660=false)
        {
            //Decoder decoder = new Decoder();
            //using var inStream = new MemoryStream(@in);
            //using var outStream = new MemoryStream(@out);
            //byte[] properties = new byte[5];
            //inStream.Read(properties, 0, 5);
            //decoder.SetDecoderProperties(properties);
            //decoder.Code(inStream, outStream, insize, size, null);
            //return 0;
            var lzrc = new Lzrc(np9660);
            lzrc.lzrc_decompress(@out, size, @in, insize);
            return 0;
        }
    }
}
