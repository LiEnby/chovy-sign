using System;
using System.Collections.Generic;
using System.Text;
using PspCrypto.Security.Cryptography;

namespace PspCrypto
{
    public class SHA224Managed_OLD : SHA224
    {

        private const int BLOCK_SIZE_BYTES = 64;

        private uint[] _H;
        private ulong count;
        private byte[] _ProcessingBuffer;   // Used to start data when passed less than a block worth.
        private int _ProcessingBufferCount; // Counts how much data we have stored that still needs processed.
        private uint[] buff;

        public SHA224Managed_OLD()
        {
            _H = new uint[8];
            _ProcessingBuffer = new byte[BLOCK_SIZE_BYTES];
            buff = new uint[64];
            Initialize();
        }

        private uint Ch(uint u, uint v, uint w)
        {
            return (u & v) ^ (~u & w);
        }

        private uint Maj(uint u, uint v, uint w)
        {
            return (u & v) ^ (u & w) ^ (v & w);
        }

        private uint Ro0(uint x)
        {
            return ((x >> 7) | (x << 25))
                ^ ((x >> 18) | (x << 14))
                ^ (x >> 3);
        }

        private uint Ro1(uint x)
        {
            return ((x >> 17) | (x << 15))
                ^ ((x >> 19) | (x << 13))
                ^ (x >> 10);
        }

        private uint Sig0(uint x)
        {
            return ((x >> 2) | (x << 30))
                ^ ((x >> 13) | (x << 19))
                ^ ((x >> 22) | (x << 10));
        }

        private uint Sig1(uint x)
        {
            return ((x >> 6) | (x << 26))
                ^ ((x >> 11) | (x << 21))
                ^ ((x >> 25) | (x << 7));
        }


        public void HashData2(ReadOnlySpan<byte> data, Span<byte> hash)
        {
            var tmp = ComputeHash(data.ToArray());
            tmp.CopyTo(hash);
        }

        protected override void HashCore(byte[] rgb, int start, int size)
        {
            int i;
            State = 1;

            if (_ProcessingBufferCount != 0)
            {
                if (size < (BLOCK_SIZE_BYTES - _ProcessingBufferCount))
                {
                    System.Buffer.BlockCopy(rgb, start, _ProcessingBuffer, _ProcessingBufferCount, size);
                    _ProcessingBufferCount += size;
                    return;
                }
                else
                {
                    i = (BLOCK_SIZE_BYTES - _ProcessingBufferCount);
                    System.Buffer.BlockCopy(rgb, start, _ProcessingBuffer, _ProcessingBufferCount, i);
                    ProcessBlock(_ProcessingBuffer, 0);
                    _ProcessingBufferCount = 0;
                    start += i;
                    size -= i;
                }
            }

            for (i = 0; i < size - size % BLOCK_SIZE_BYTES; i += BLOCK_SIZE_BYTES)
            {
                ProcessBlock(rgb, start + i);
            }

            if (size % BLOCK_SIZE_BYTES != 0)
            {
                System.Buffer.BlockCopy(rgb, size - size % BLOCK_SIZE_BYTES + start, _ProcessingBuffer, 0, size % BLOCK_SIZE_BYTES);
                _ProcessingBufferCount = size % BLOCK_SIZE_BYTES;
            }
        }

        protected override byte[] HashFinal()
        {
            byte[] hash = new byte[28];
            int i, j;

            ProcessFinalBlock(_ProcessingBuffer, 0, _ProcessingBufferCount);

            for (i = 0; i < 7; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    hash[i * 4 + j] = (byte)(_H[i] >> (24 - j * 8));
                }
            }

            State = 0;
            return hash;
        }

        public override void Initialize()
        {
            count = 0;
            _ProcessingBufferCount = 0;

            _H[0] = 0xC1059ED8;
            _H[1] = 0x367CD507;
            _H[2] = 0x3070DD17;
            _H[3] = 0xF70E5939;
            _H[4] = 0xFFC00B31;
            _H[5] = 0x68581511;
            _H[6] = 0x64F98FA7;
            _H[7] = 0xBEFA4FA4;
        }

        private void ProcessBlock(byte[] inputBuffer, int inputOffset)
        {
            uint a, b, c, d, e, f, g, h;
            uint t1, t2;
            int i;
            uint[] K1 = _K1;
            uint[] buff = this.buff;

            count += BLOCK_SIZE_BYTES;

            for (i = 0; i < 16; i++)
            {
                buff[i] = (uint)(((inputBuffer[inputOffset + 4 * i]) << 24)
                    | ((inputBuffer[inputOffset + 4 * i + 1]) << 16)
                    | ((inputBuffer[inputOffset + 4 * i + 2]) << 8)
                    | ((inputBuffer[inputOffset + 4 * i + 3])));
            }


            for (i = 16; i < 64; i++)
            {
                t1 = buff[i - 15];
                t1 = (((t1 >> 7) | (t1 << 25)) ^ ((t1 >> 18) | (t1 << 14)) ^ (t1 >> 3));

                t2 = buff[i - 2];
                t2 = (((t2 >> 17) | (t2 << 15)) ^ ((t2 >> 19) | (t2 << 13)) ^ (t2 >> 10));
                buff[i] = t2 + buff[i - 7] + t1 + buff[i - 16];
            }

            a = _H[0];
            b = _H[1];
            c = _H[2];
            d = _H[3];
            e = _H[4];
            f = _H[5];
            g = _H[6];
            h = _H[7];

            for (i = 0; i < 64; i++)
            {
                t1 = h + (((e >> 6) | (e << 26)) ^ ((e >> 11) | (e << 21)) ^ ((e >> 25) | (e << 7))) + ((e & f) ^ (~e & g)) + K1[i] + buff[i];

                t2 = (((a >> 2) | (a << 30)) ^ ((a >> 13) | (a << 19)) ^ ((a >> 22) | (a << 10)));
                t2 = t2 + ((a & b) ^ (a & c) ^ (b & c));
                h = g;
                g = f;
                f = e;
                e = d + t1;
                d = c;
                c = b;
                b = a;
                a = t1 + t2;
            }

            _H[0] += a;
            _H[1] += b;
            _H[2] += c;
            _H[3] += d;
            _H[4] += e;
            _H[5] += f;
            _H[6] += g;
            _H[7] += h;
        }

        private void ProcessFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            ulong total = count + (ulong)inputCount;
            int paddingSize = (56 - (int)(total % BLOCK_SIZE_BYTES));

            if (paddingSize < 1)
                paddingSize += BLOCK_SIZE_BYTES;

            byte[] fooBuffer = new byte[inputCount + paddingSize + 8];

            for (int i = 0; i < inputCount; i++)
            {
                fooBuffer[i] = inputBuffer[i + inputOffset];
            }

            fooBuffer[inputCount] = 0x80;
            for (int i = inputCount + 1; i < inputCount + paddingSize; i++)
            {
                fooBuffer[i] = 0x00;
            }

            // I deal in bytes. The algorithm deals in bits.
            ulong size = total << 3;
            AddLength(size, fooBuffer, inputCount + paddingSize);
            ProcessBlock(fooBuffer, 0);

            if (inputCount + paddingSize + 8 == 128)
            {
                ProcessBlock(fooBuffer, 64);
            }
        }

        internal void AddLength(ulong length, byte[] buffer, int position)
        {
            buffer[position++] = (byte)(length >> 56);
            buffer[position++] = (byte)(length >> 48);
            buffer[position++] = (byte)(length >> 40);
            buffer[position++] = (byte)(length >> 32);
            buffer[position++] = (byte)(length >> 24);
            buffer[position++] = (byte)(length >> 16);
            buffer[position++] = (byte)(length >> 8);
            buffer[position] = (byte)(length);
        }

        // SHA-224/256 Constants
        // Represent the first 32 bits of the fractional parts of the
        // cube roots of the first sixty-four prime numbers
        public readonly static uint[] _K1 = {
            0x428A2F98, 0x71374491, 0xB5C0FBCF, 0xE9B5DBA5,
            0x3956C25B, 0x59F111F1, 0x923F82A4, 0xAB1C5ED5,
            0xD807AA98, 0x12835B01, 0x243185BE, 0x550C7DC3,
            0x72BE5D74, 0x80DEB1FE, 0x9BDC06A7, 0xC19BF174,
            0xE49B69C1, 0xEFBE4786, 0x0FC19DC6, 0x240CA1CC,
            0x2DE92C6F, 0x4A7484AA, 0x5CB0A9DC, 0x76F988DA,
            0x983E5152, 0xA831C66D, 0xB00327C8, 0xBF597FC7,
            0xC6E00BF3, 0xD5A79147, 0x06CA6351, 0x14292967,
            0x27B70A85, 0x2E1B2138, 0x4D2C6DFC, 0x53380D13,
            0x650A7354, 0x766A0ABB, 0x81C2C92E, 0x92722C85,
            0xA2BFE8A1, 0xA81A664B, 0xC24B8B70, 0xC76C51A3,
            0xD192E819, 0xD6990624, 0xF40E3585, 0x106AA070,
            0x19A4C116, 0x1E376C08, 0x2748774C, 0x34B0BCB5,
            0x391C0CB3, 0x4ED8AA4A, 0x5B9CCA4F, 0x682E6FF3,
            0x748F82EE, 0x78A5636F, 0x84C87814, 0x8CC70208,
            0x90BEFFFA, 0xA4506CEB, 0xBEF9A3F7, 0xC67178F2
        };
    }
}
