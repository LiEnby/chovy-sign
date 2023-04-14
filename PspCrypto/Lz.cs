using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using SevenZip.Compression.LZMA;
using Decoder = SevenZip.Compression.LZMA.Decoder;

namespace PspCrypto
{
    public class Lz
    {
        public static byte[] compress(byte[] in_buf)
        {
            //Decoder decoder = new Decoder();
            //using var inStream = new MemoryStream(@in);
            //using var outStream = new MemoryStream(@out);
            //byte[] properties = new byte[5];
            //inStream.Read(properties, 0, 5);
            //decoder.SetDecoderProperties(properties);
            //decoder.Code(inStream, outStream, insize, size, null);
            //return 0;
            var lzrc = new Lzrc();

            // create a buffer hopefully big enough to hold compression result
            byte[] compression_result = new byte[in_buf.Length + 0xffff]; // in worst case (i.e random data),
                                                                           // compression makes the data larger
                                                                           // so have to make sure theres extra space ..

            // compress data, and get the compressed data length
            int compressed_length = lzrc.lzrc_compress(compression_result, compression_result.Length, in_buf, in_buf.Length);

            // resize array to actual compressed length ...
            Array.Resize(ref compression_result, compressed_length);

            return compression_result;
        }
        public static int decompress(byte[] @out, byte[] @in, int size, int insize)
        {
            //Decoder decoder = new Decoder();
            //using var inStream = new MemoryStream(@in);
            //using var outStream = new MemoryStream(@out);
            //byte[] properties = new byte[5];
            //inStream.Read(properties, 0, 5);
            //decoder.SetDecoderProperties(properties);
            //decoder.Code(inStream, outStream, insize, size, null);
            //return 0;
            var lzrc = new Lzrc();
            lzrc.lzrc_decompress(@out, size, @in, insize);
            return 0;
        }
    }
}
