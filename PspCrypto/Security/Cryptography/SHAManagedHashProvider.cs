using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using static System.Numerics.BitOperations;

namespace PspCrypto.Security.Cryptography
{
    internal sealed class SHAManagedHashProvider : HashProvider
    {
        private int hashSizeInBytes;
        private SHAManagedImplementationBase impl;
        private MemoryStream buffer;

        public SHAManagedHashProvider(string hashAlgorithmId)
        {
            switch (hashAlgorithmId)
            {
                case HashAlgorithmNames.SHA224:
                    impl = new SHA224ManagedImplementation();
                    hashSizeInBytes = 28;
                    break;
                default:
                    throw new CryptographicException(string.Format("'{0}' is not a known hash algorithm.", hashAlgorithmId));
            }
        }

        public override void AppendHashData(ReadOnlySpan<byte> data)
        {
            buffer ??= new MemoryStream(1000);

            buffer.Write(data);
        }

        public override int FinalizeHashAndReset(Span<byte> destination)
        {
            GetCurrentHash(destination);
            buffer = null;

            return hashSizeInBytes;
        }

        public override int GetCurrentHash(Span<byte> destination)
        {
            Debug.Assert(destination.Length >= hashSizeInBytes);

            impl.Initialize();
            if (buffer != null)
            {
                impl.HashCore(buffer.GetBuffer(), 0, (int)buffer.Length);
            }
            impl.HashFinal().CopyTo(destination);

            return hashSizeInBytes;
        }

        public override int HashSizeInBytes => hashSizeInBytes;

        public override void Reset()
        {
            buffer = null;
            impl.Initialize();
        }

        public override void Dispose(bool disposing)
        {
        }

        private abstract class SHAManagedImplementationBase
        {
            public abstract void Initialize();
            public abstract void HashCore(byte[] partIn, int ibStart, int cbSize);
            public abstract byte[] HashFinal();
        }

        private sealed class SHA224ManagedImplementation : SHAManagedImplementationBase
        {
            private byte[] _buffer;
            private long _count; // Number of bytes in the hashed message
            private uint[] _stateSHA224;
            private uint[] _W;

            public SHA224ManagedImplementation()
            {
                _stateSHA224 = new uint[8];
                _buffer = new byte[64];
                _W = new uint[64];

                InitializeState();
            }

            public override void Initialize()
            {
                InitializeState();

                // Zeroize potentially sensitive information.
                Array.Clear(_buffer, 0, _buffer.Length);
                Array.Clear(_W, 0, _W.Length);
            }

            private void InitializeState()
            {
                _count = 0;

                _stateSHA224[0] = 0xc1059ed8;
                _stateSHA224[1] = 0x367cd507;
                _stateSHA224[2] = 0x3070dd17;
                _stateSHA224[3] = 0xf70e5939;
                _stateSHA224[4] = 0xffc00b31;
                _stateSHA224[5] = 0x68581511;
                _stateSHA224[6] = 0x64f98fa7;
                _stateSHA224[7] = 0xbefa4fa4;
            }

            /* SHA256 block update operation. Continues an SHA message-digest
            operation, processing another message block, and updating the
            context.
            */
            public override unsafe void HashCore(byte[] partIn, int ibStart, int cbSize)
            {
                int bufferLen;
                int partInLen = cbSize;
                int partInBase = ibStart;

                /* Compute length of buffer */
                bufferLen = (int)(_count & 0x3f);

                /* Update number of bytes */
                _count += partInLen;

                fixed (uint* stateSHA256 = _stateSHA224)
                {
                    fixed (byte* buffer = _buffer)
                    {
                        fixed (uint* expandedBuffer = _W)
                        {
                            if (bufferLen > 0 && bufferLen + partInLen >= 64)
                            {
                                Buffer.BlockCopy(partIn, partInBase, _buffer, bufferLen, 64 - bufferLen);
                                partInBase += 64 - bufferLen;
                                partInLen -= 64 - bufferLen;
                                SHATransform(expandedBuffer, stateSHA256, buffer);
                                bufferLen = 0;
                            }

                            /* Copy input to temporary buffer and hash */
                            while (partInLen >= 64)
                            {
                                Buffer.BlockCopy(partIn, partInBase, _buffer, 0, 64);
                                partInBase += 64;
                                partInLen -= 64;
                                SHATransform(expandedBuffer, stateSHA256, buffer);
                            }

                            if (partInLen > 0)
                            {
                                Buffer.BlockCopy(partIn, partInBase, _buffer, bufferLen, partInLen);
                            }
                        }
                    }
                }
            }

            /* SHA256 finalization. Ends an SHA256 message-digest operation, writing
            the message digest.
            */
            public override byte[] HashFinal()
            {
                byte[] pad;
                int padLen;
                long bitCount;
                byte[] hash = new byte[28]; // HashSizeValue = 224

                /* Compute padding: 80 00 00 ... 00 00 <bit count>
                */

                padLen = 64 - (int)(_count & 0x3f);
                if (padLen <= 8)
                    padLen += 64;

                pad = new byte[padLen];
                pad[0] = 0x80;

                //  Convert count to bit count
                bitCount = _count * 8;

                pad[padLen - 8] = (byte)(bitCount >> 56 & 0xff);
                pad[padLen - 7] = (byte)(bitCount >> 48 & 0xff);
                pad[padLen - 6] = (byte)(bitCount >> 40 & 0xff);
                pad[padLen - 5] = (byte)(bitCount >> 32 & 0xff);
                pad[padLen - 4] = (byte)(bitCount >> 24 & 0xff);
                pad[padLen - 3] = (byte)(bitCount >> 16 & 0xff);
                pad[padLen - 2] = (byte)(bitCount >> 8 & 0xff);
                pad[padLen - 1] = (byte)(bitCount >> 0 & 0xff);

                /* Digest padding */
                HashCore(pad, 0, pad.Length);

                /* Store digest */
                SHAUtils.DWORDToBigEndian(hash, _stateSHA224, 7);

                return hash;
            }

            private static readonly uint[] _K = {
                0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
                0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
                0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
                0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
                0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
                0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
                0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
                0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
                0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
                0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
                0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
                0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
                0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
                0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
                0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
                0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
            };

            private static unsafe void SHATransform(uint* expandedBuffer, uint* state, byte* block)
            {
                uint a, b, c, d, e, f, h, g;
                uint aa, bb, cc, dd, ee, ff, hh, gg;
                uint T1;

                a = state[0];
                b = state[1];
                c = state[2];
                d = state[3];
                e = state[4];
                f = state[5];
                g = state[6];
                h = state[7];

                // fill in the first 16 bytes of W.
                SHAUtils.DWORDFromBigEndian(expandedBuffer, 16, block);
                SHA256Expand(expandedBuffer);

                /* Apply the SHA256 compression function */
                // We are trying to be smart here and avoid as many copies as we can
                // The perf gain with this method over the straightforward modify and shift
                // forward is >= 20%, so it's worth the pain
                for (int j = 0; j < 64;)
                {
                    T1 = h + Sigma_1(e) + Ch(e, f, g) + _K[j] + expandedBuffer[j];
                    ee = d + T1;
                    aa = T1 + Sigma_0(a) + Maj(a, b, c);
                    j++;

                    T1 = g + Sigma_1(ee) + Ch(ee, e, f) + _K[j] + expandedBuffer[j];
                    ff = c + T1;
                    bb = T1 + Sigma_0(aa) + Maj(aa, a, b);
                    j++;

                    T1 = f + Sigma_1(ff) + Ch(ff, ee, e) + _K[j] + expandedBuffer[j];
                    gg = b + T1;
                    cc = T1 + Sigma_0(bb) + Maj(bb, aa, a);
                    j++;

                    T1 = e + Sigma_1(gg) + Ch(gg, ff, ee) + _K[j] + expandedBuffer[j];
                    hh = a + T1;
                    dd = T1 + Sigma_0(cc) + Maj(cc, bb, aa);
                    j++;

                    T1 = ee + Sigma_1(hh) + Ch(hh, gg, ff) + _K[j] + expandedBuffer[j];
                    h = aa + T1;
                    d = T1 + Sigma_0(dd) + Maj(dd, cc, bb);
                    j++;

                    T1 = ff + Sigma_1(h) + Ch(h, hh, gg) + _K[j] + expandedBuffer[j];
                    g = bb + T1;
                    c = T1 + Sigma_0(d) + Maj(d, dd, cc);
                    j++;

                    T1 = gg + Sigma_1(g) + Ch(g, h, hh) + _K[j] + expandedBuffer[j];
                    f = cc + T1;
                    b = T1 + Sigma_0(c) + Maj(c, d, dd);
                    j++;

                    T1 = hh + Sigma_1(f) + Ch(f, g, h) + _K[j] + expandedBuffer[j];
                    e = dd + T1;
                    a = T1 + Sigma_0(b) + Maj(b, c, d);
                    j++;
                }

                state[0] += a;
                state[1] += b;
                state[2] += c;
                state[3] += d;
                state[4] += e;
                state[5] += f;
                state[6] += g;
                state[7] += h;
            }

            private static uint Ch(uint x, uint y, uint z)
            {
                return x & y ^ (x ^ 0xffffffff) & z;
            }

            private static uint Maj(uint x, uint y, uint z)
            {
                return x & y ^ x & z ^ y & z;
            }

            private static uint sigma_0(uint x)
            {
                return RotateRight(x, 7) ^ RotateRight(x, 18) ^ x >> 3;
            }

            private static uint sigma_1(uint x)
            {
                return RotateRight(x, 17) ^ RotateRight(x, 19) ^ x >> 10;
            }

            private static uint Sigma_0(uint x)
            {
                return RotateRight(x, 2) ^ RotateRight(x, 13) ^ RotateRight(x, 22);
            }

            private static uint Sigma_1(uint x)
            {
                return RotateRight(x, 6) ^ RotateRight(x, 11) ^ RotateRight(x, 25);
            }

            /* This function creates W_16,...,W_63 according to the formula
            W_j <- sigma_1(W_{j-2}) + W_{j-7} + sigma_0(W_{j-15}) + W_{j-16};
            */
            private static unsafe void SHA256Expand(uint* x)
            {
                for (int i = 16; i < 64; i++)
                {
                    x[i] = sigma_1(x[i - 2]) + x[i - 7] + sigma_0(x[i - 15]) + x[i - 16];
                }
            }
        }

        private static class SHAUtils
        {
            // digits == number of DWORDs
            public static unsafe void DWORDFromBigEndian(uint* x, int digits, byte* block)
            {
                int i;
                int j;

                for (i = 0, j = 0; i < digits; i++, j += 4)
                    x[i] = (uint)(block[j] << 24 | block[j + 1] << 16 | block[j + 2] << 8 | block[j + 3]);
            }

            // encodes x (DWORD) into block (unsigned char), most significant byte first.
            // digits == number of DWORDs
            public static void DWORDToBigEndian(byte[] block, uint[] x, int digits)
            {
                int i;
                int j;

                for (i = 0, j = 0; i < digits; i++, j += 4)
                {
                    block[j] = (byte)(x[i] >> 24 & 0xff);
                    block[j + 1] = (byte)(x[i] >> 16 & 0xff);
                    block[j + 2] = (byte)(x[i] >> 8 & 0xff);
                    block[j + 3] = (byte)(x[i] & 0xff);
                }
            }
        }
    }
}
