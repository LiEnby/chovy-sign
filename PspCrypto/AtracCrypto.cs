using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PspCrypto
{
    public class AtracCrypto
    {

        const int NBYTES = 0x180;
        private static uint ROTR32(uint v, int n)
        {
            n &= 32 - 1;
            return (v >> n) | (v << (32 - n));
        }

        public static int UnscrambleAtracData(byte[] data, uint key)
        {
            int blocks = (data.Length / NBYTES) / 0x10;
            int chunks_rest = (data.Length / NBYTES) % 0x10;
            Span<uint> ptr = MemoryMarshal.Cast<byte, uint>(data);
            uint tmp2 = key;
            uint tmp;
            uint value;
            // for each block
            while (blocks > 0)
            {
                // for each chunk of block
                for (int i = 0; i < 0x10; i++)
                {
                    tmp = tmp2;

                    // for each value of chunk
                    for (int k = 0; k < (NBYTES / 4); k++)
                    {
                        value = ptr[k];
                        ptr[k] = tmp ^ value;
                        tmp = tmp2 + (value * 123456789);
                    }

                    tmp2 = ROTR32(tmp2, 1);
                    ptr = ptr[(NBYTES / 4)..]; // pointer on next chunk
                }

                blocks--;
            }

            // do rest chunks
            for (int i = 0; i < chunks_rest; i++)
            {
                tmp = tmp2;

                // for each value of chunk
                for (int k = 0; k < (NBYTES / 4); k++)
                {
                    value = ptr[k];
                    ptr[k] = tmp ^ value;
                    tmp = tmp2 + (value * 123456789);
                }

                tmp2 = ROTR32(tmp2, 1);
                ptr = ptr[(NBYTES / 4)..]; // next chunk
            }

            return 0;
        }

        public static void ScrambleAtracData(Stream input, Stream output, uint key)
        {
            int blocks = (Convert.ToInt32(input.Length) / NBYTES) / 0x10;
            int chunks_rest = (Convert.ToInt32(input.Length) / NBYTES) % 0x10;
            Span<byte> block = stackalloc byte[NBYTES];
            uint tmp2 = key;
            uint tmp;
            // for each block
            while (blocks > 0)
            {
                // for each chunk of block
                for (int i = 0; i < 0x10; i++)
                {
                    tmp = tmp2;

                    input.Read(block);
                    Span<uint> ptr = MemoryMarshal.Cast<byte, uint>(block);

                    // for each value of chunk
                    for (int k = 0; k < (NBYTES / 4); k++)
                    {
                        ptr[k] ^= tmp;
                        tmp = tmp2 + (ptr[k] * 123456789);
                    }
                    output.Write(block);

                    tmp2 = ROTR32(tmp2, 1);
                    ptr = ptr[(NBYTES / 4)..]; // pointer on next chunk
                }

                blocks--;
            }

            // do rest chunks
            for (int i = 0; i < chunks_rest; i++)
            {
                tmp = tmp2;

                input.Read(block);
                Span<uint> ptr = MemoryMarshal.Cast<byte, uint>(block);

                // for each value of chunk
                for (int k = 0; k < (NBYTES / 4); k++)
                {
                    ptr[k] ^= tmp;
                    tmp = tmp2 + (ptr[k] * 123456789);
                }
                output.Write(block);

                tmp2 = ROTR32(tmp2, 1);
                ptr = ptr[(NBYTES / 4)..]; // next chunk
            }
        }
    }
}
