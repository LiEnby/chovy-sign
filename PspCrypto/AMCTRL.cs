using System;
using System.Runtime.InteropServices;

namespace PspCrypto
{
    public class AMCTRL
    {
        static readonly Memory<byte> kirk_buf = new byte[0x0814];
        static readonly Memory<byte> kirk_buf2 = new byte[0x8014];

        // AMCTRL keys.
        static readonly byte[] amctrl_key1 = { 0xE3, 0x50, 0xED, 0x1D, 0x91, 0x0A, 0x1F, 0xD0, 0x29, 0xBB, 0x1C, 0x3E, 0xF3, 0x40, 0x77, 0xFB };
        static readonly byte[] amctrl_key2 = { 0x13, 0x5F, 0xA4, 0x7C, 0xAB, 0x39, 0x5B, 0xA4, 0x76, 0xB8, 0xCC, 0xA9, 0x8F, 0x3A, 0x04, 0x45 };
        static readonly byte[] amctrl_key3 = { 0x67, 0x8D, 0x7F, 0xA3, 0x2A, 0x9C, 0xA0, 0xD1, 0x50, 0x8A, 0xD8, 0x38, 0x5E, 0x4B, 0x01, 0x7E };

        public unsafe struct MAC_KEY
        {
            public int type;
            private fixed byte _key[16];

            public Span<byte> key
            {
                get
                {
                    fixed (byte* ptr = _key)
                    {
                        return new Span<byte>(ptr, 16);
                    }
                }
            }
            private fixed byte _pad[16];

            public Span<byte> pad
            {
                get
                {
                    fixed (byte* ptr = _pad)
                    {
                        return new Span<byte>(ptr, 16);
                    }
                }
            }
            public int pad_size;
        }

        public unsafe struct CIPHER_KEY
        {
            public int type;
            public int seed;
            fixed byte _key[16];

            public Span<byte> key
            {
                get
                {
                    fixed (byte* ptr = _key)
                    {
                        return new Span<byte>(ptr, 16);
                    }
                }
            }
        }

        /*
         * KIRK wrapper functions.
         */
        static int Kirk4(Span<byte> buf, int size, int type)
        {
            int retv;
            ref var hdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_AES128CBC_HEADER>(buf);
            hdr.mode = KIRKEngine.KIRK_MODE_ENCRYPT_CBC;
            hdr.keyseed = type;
            hdr.data_size = size;

            retv = KIRKEngine.sceUtilsBufferCopyWithRange(buf, size + 0x14, buf, size, KIRKEngine.KIRK_CMD_ENCRYPT_IV_0);

            if (retv != 0)
                return -2142174447; // 0x80510311;

            return 0;
        }

        static int Kirk5(Span<byte> buf, int size)
        {
            int retv;
            ref var hdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_AES128CBC_HEADER>(buf);
            hdr.mode = KIRKEngine.KIRK_MODE_ENCRYPT_CBC;
            hdr.keyseed = 0x0100;
            hdr.data_size = size;

            retv = KIRKEngine.sceUtilsBufferCopyWithRange(buf, size + 0x14, buf, size, KIRKEngine.KIRK_CMD_ENCRYPT_IV_FUSE);

            if (retv != 0)
                return -2142174446; // 0x80510312;

            return 0;
        }

        static int Kirk7(Span<byte> buf, int size, int type)
        {
            int retv;
            ref var hdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_AES128CBC_HEADER>(buf);
            hdr.mode = KIRKEngine.KIRK_MODE_DECRYPT_CBC;
            hdr.keyseed = type;
            hdr.data_size = size;

            retv = KIRKEngine.sceUtilsBufferCopyWithRange(buf, size + 0x14, buf, size, KIRKEngine.KIRK_CMD_DECRYPT_IV_0);

            if (retv != 0)
                return -2142174447; // 0x80510311;

            return 0;
        }

        static int Kirk8(Span<byte> buf, int size)
        {
            int retv;
            ref var hdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_AES128CBC_HEADER>(buf);
            hdr.mode = KIRKEngine.KIRK_MODE_DECRYPT_CBC;
            hdr.keyseed = 0x0100;
            hdr.data_size = size;

            retv = KIRKEngine.sceUtilsBufferCopyWithRange(buf, size + 0x14, buf, size, KIRKEngine.KIRK_CMD_DECRYPT_IV_FUSE);

            if (retv != 0)
                return -2142174446; // 0x80510312;

            return 0;
        }

        static int Kirk14(Span<byte> buf)
        {
            int retv;

            retv = KIRKEngine.sceUtilsBufferCopyWithRange(buf, 0x14, null, 0, KIRKEngine.KIRK_CMD_PRNG);

            if (retv != 0)
                return -2142174443; // 0x80510315;

            return 0;
        }

        static int encrypt_buf(Span<byte> buf, int size, Span<byte> key, int key_type)
        {
            int i, retv;

            for (i = 0; i < 16; i++)
            {
                buf[0x14 + i] ^= key[i];
            }

            unsafe
            {
                fixed (byte* ptr = buf)
                {

                }
            }

            retv = Kirk4(buf, size, key_type);

            if (retv != 0)
                return retv;

            buf.Slice(size + 4, 16).CopyTo(key);

            return 0;
        }

        static int decrypt_buf(Span<byte> buf, int size, Span<byte> key, int key_type)
        {
            int i, retv;
            Span<byte> tmp = stackalloc byte[16];

            buf.Slice(size + 0x14 - 16, 16).CopyTo(tmp);

            retv = Kirk7(buf, size, key_type);

            if (retv != 0)
                return retv;

            for (i = 0; i < 16; i++)
            {
                buf[i] ^= key[i];
            }

            tmp.CopyTo(key);

            return 0;
        }

        static int cipher_buf(Span<byte> kbuf, Span<byte> dbuf, int size, ref CIPHER_KEY ckey)
        {
            int i, retv;
            Span<byte> tmp1 = stackalloc byte[16], tmp2 = stackalloc byte[16];

            ckey.key.CopyTo(kbuf[0x14..]);

            for (i = 0; i < 16; i++)
            {
                kbuf[0x14 + i] ^= amctrl_key3[i];
            }

            if (ckey.type == 2)
                retv = Kirk8(kbuf, 16);
            else
                retv = Kirk7(kbuf, 16, 0x39);

            if (retv != 0)
                return retv;

            for (i = 0; i < 16; i++)
            {
                kbuf[i] ^= amctrl_key2[i];
            }

            kbuf.Slice(0, 0x10).CopyTo(tmp2);

            if (ckey.seed == 1)
            {
                tmp1.Fill(0);
            }
            else
            {
                tmp2.CopyTo(tmp1);
                var tmp = ckey.seed - 1;
                MemoryMarshal.Write(tmp1[0x0c..], ref tmp);
            }

            for (i = 0; i < size; i += 16)
            {
                tmp2[..12].CopyTo(kbuf[(0x14 + i)..]);
                MemoryMarshal.Write(kbuf[(0x14 + i + 12)..], ref ckey.seed);
                ckey.seed += 1;
            }

            retv = decrypt_buf(kbuf, size, tmp1, 0x63);

            if (retv != 0)
                return retv;

            for (i = 0; i < size; i++)
            {
                dbuf[i] ^= kbuf[i];
            }

            return 0;
        }

        public static int sceDrmBBMacInit(Span<byte> mkey, int type)
        {
            ref MAC_KEY macKey = ref MemoryMarshal.AsRef<MAC_KEY>(mkey);
            macKey.type = type;
            macKey.key.Clear();
            macKey.pad.Clear();
            return 0;
        }

        public static int sceDrmBBMacUpdate(Span<byte> mkey, ReadOnlySpan<byte> buf, int size)
        {
            ref MAC_KEY macKey = ref MemoryMarshal.AsRef<MAC_KEY>(mkey);

            int retv = 0, ksize, p, type;
            int kbuf;

            if (macKey.pad_size > 16)
            {
                retv = -2142174462; // 0x80510302
                return retv;
            }

            if (macKey.pad_size + size <= 16)
            {
                buf[..size].CopyTo(macKey.pad[macKey.pad_size..]);
                macKey.pad_size += size;
                retv = 0;
            }
            else
            {
                kbuf = 0x14;
                macKey.pad[..macKey.pad_size].CopyTo(kirk_buf[0x14..].Span);

                p = macKey.pad_size;

                macKey.pad_size += size;
                macKey.pad_size &= 0x0f;
                if (macKey.pad_size == 0)
                    macKey.pad_size = 16;

                size -= macKey.pad_size;
                buf.Slice(size, macKey.pad_size).CopyTo(macKey.pad);

                type = (macKey.type == 2) ? 0x3A : 0x38;

                int offset = 0;

                while (size > 0)
                {
                    ksize = (size + p >= 0x0800) ? 0x0800 : size + p;
                    buf.Slice(offset, ksize - p).CopyTo(kirk_buf[(kbuf + p)..].Span);
                    retv = encrypt_buf(kirk_buf.Span, ksize, macKey.key, type);

                    if (retv != 0)
                        return retv;

                    size -= (ksize - p);
                    offset += ksize - p;
                    p = 0;
                }
            }

            return retv;
        }

        public static int sceDrmBBMacUpdate2(Span<byte> mkey, Span<byte> buf, int size)
        {
            ref MAC_KEY macKey = ref MemoryMarshal.AsRef<MAC_KEY>(mkey);

            int retv = 0, ksize, p, type;
            int kbuf;

            if (macKey.pad_size > 16)
            {
                retv = -2142174462; // 0x80510302
                return retv;
            }

            if (macKey.pad_size + size <= 16)
            {
                buf.Slice(0, size).CopyTo(macKey.pad.Slice(macKey.pad_size));
                macKey.pad_size += size;
                retv = 0;
            }
            else
            {
                kbuf = 0x14;
                macKey.pad.Slice(0, macKey.pad_size).CopyTo(kirk_buf.Slice(0x14).Span);

                p = macKey.pad_size;

                macKey.pad_size += (size & 0x0f);
                //mkey.pad_size &= 0x0f;
                if (macKey.pad_size == 0)
                    macKey.pad_size = 16;

                size -= macKey.pad_size;
                buf.Slice(size, macKey.pad_size).CopyTo(macKey.pad);

                type = (macKey.type == 2) ? 0x3A : 0x38;

                int idx = 0;

                ksize = size + p;
                int offset = 0;
                if (size + p >= 0x8001)
                {
                    ;
                    buf.Slice(offset, 0x8000 - p).CopyTo(kirk_buf2.Slice(kbuf + p).Span);
                    retv = encrypt_buf(kirk_buf2.Span, 0x8000, macKey.key, type);
                    idx = 0x8000 - p;
                    var fix = -p;
                    while (retv == 0)
                    {
                        if (size <= 0x10000 - fix)
                        {
                            p = 0;
                            ksize = size;
                            break;
                        }
                        buf.Slice(offset + idx, 0x8000).CopyTo(kirk_buf2.Slice(kbuf).Span);
                        retv = encrypt_buf(kirk_buf2.Span, 0x8000, macKey.key, type);
                        fix = idx;
                        idx += 0x8000;
                    }

                    if (retv != 0)
                    {
                        return retv;
                    }
                }
                buf.Slice(offset + idx, size - idx).CopyTo(kirk_buf2.Slice(kbuf + p).Span);
                retv = encrypt_buf(kirk_buf2.Span, ksize - idx, macKey.key, type);
            }

            return retv;
        }

        public static int sceDrmBBMacFinal(Span<byte> mkey, Span<byte> buf, ReadOnlySpan<byte> vkey)
        {
            ref MAC_KEY macKey = ref MemoryMarshal.AsRef<MAC_KEY>(mkey);
            int i, retv, code;
            Span<byte> tmp = stackalloc byte[16], tmp1 = stackalloc byte[16];
            int kbuf;
            uint t0, v0, v1;

            if (macKey.pad_size > 16)
                return -2142174462; //0x80510302;

            code = (macKey.type == 2) ? 0x3A : 0x38;
            kbuf = 0x14;

            kirk_buf.Slice(kbuf, 16).Span.Fill(0);
            retv = Kirk4(kirk_buf.Span, 16, code);
            if (retv != 0)
            {
                return retv;
            }

            kirk_buf.Slice(kbuf, 16).Span.CopyTo(tmp);

            t0 = ((tmp[0] & 0x80) > 0) ? 0x87u : 0;
            for (i = 0; i < 15; i++)
            {
                v1 = tmp[i + 0];
                v0 = tmp[i + 1];
                v1 <<= 1;
                v0 >>= 7;
                v0 |= v1;
                tmp[i + 0] = (byte)v0;
            }
            v0 = tmp[15];
            v0 <<= 1;
            v0 ^= t0;
            tmp[15] = (byte)v0;

            if (macKey.pad_size < 16)
            {
                t0 = ((tmp[0] & 0x80) > 0) ? 0x87u : 0;
                for (i = 0; i < 15; i++)
                {
                    v1 = tmp[i + 0];
                    v0 = tmp[i + 1];
                    v1 <<= 1;
                    v0 >>= 7;
                    v0 |= v1;
                    tmp[i + 0] = (byte)v0;
                }
                v0 = tmp[15];
                v0 <<= 1;
                v0 ^= t0;
                tmp[15] = (byte)v0;

                macKey.pad[macKey.pad_size] = 0x80;
                if (macKey.pad_size + 1 < 16)
                {
                    macKey.pad.Slice(macKey.pad_size + 1, 16 - macKey.pad_size - 1).Fill(0);
                }
            }

            for (i = 0; i < 16; i++)
            {
                macKey.pad[i] ^= tmp[i];
            }

            macKey.pad.CopyTo(kirk_buf.Slice(kbuf).Span);
            macKey.key.CopyTo(tmp1);

            retv = encrypt_buf(kirk_buf.Span, 0x10, tmp1, code);

            if (retv != 0)
                return retv;

            for (i = 0; i < 0x10; i++)
            {
                tmp1[i] ^= amctrl_key1[i];
            }

            if (macKey.type == 2)
            {
                tmp1.CopyTo(kirk_buf.Slice(kbuf).Span);

                retv = Kirk5(kirk_buf.Span, 0x10);

                if (retv != 0)
                    return retv;

                retv = Kirk4(kirk_buf.Span, 0x10, code);

                if (retv != 0)
                    return retv;

                kirk_buf.Slice(kbuf, 16).Span.CopyTo(tmp1);
            }

            if (vkey != null)
            {
                for (i = 0; i < 0x10; i++)
                {
                    tmp1[i] ^= vkey[i];
                }
                tmp1.CopyTo(kirk_buf.Slice(kbuf).Span);

                retv = Kirk4(kirk_buf.Span, 0x10, code);

                if (retv != 0)
                    return retv;

                kirk_buf.Slice(kbuf, 16).Span.CopyTo(tmp1);
            }

            tmp1.CopyTo(buf);

            macKey.key.Fill(0);
            macKey.pad.Fill(0);

            macKey.pad_size = 0;
            macKey.type = 0;
            retv = 0;

            return retv;
        }



        public static int bbmac_getkey(Span<byte> mkey, ReadOnlySpan<byte> bbmac, Span<byte> vkey)
        {
            int i, retv, type, code;
            Span<byte> tmp = stackalloc byte[16], tmp1 = stackalloc byte[16];
            int kbuf;
            ref MAC_KEY macKey = ref MemoryMarshal.AsRef<MAC_KEY>(mkey);

            type = macKey.type;
            retv = sceDrmBBMacFinal(mkey, tmp, null);

            if (retv != 0)
                return retv;

            kbuf = 0x14;

            if (type == 3)
            {
                bbmac[..0x10].CopyTo(kirk_buf[kbuf..].Span);
                Kirk7(kirk_buf.Span, 0x10, 0x63);
            }
            else
            {
                bbmac[..0x10].CopyTo(kirk_buf.Span);
            }

            kirk_buf[..16].Span.CopyTo(tmp1);
            tmp1.CopyTo(kirk_buf[kbuf..].Span);

            code = (type == 2) ? 0x3A : 0x38;
            Kirk7(kirk_buf.Span, 0x10, code);

            for (i = 0; i < 0x10; i++)
            {
                vkey[i] = (byte)(tmp[i] ^ kirk_buf.Span[i]);
            }

            return 0;
        }

        public static int sceDrmBBMacFinal2(Span<byte> mkey, ReadOnlySpan<byte> hash, ReadOnlySpan<byte> vkey)
        {
            int retv, type;
            byte[] tmp = new byte[16];
            int kbuf;
            ref MAC_KEY macKey = ref MemoryMarshal.AsRef<MAC_KEY>(mkey);

            type = macKey.type;
            retv = sceDrmBBMacFinal(mkey, tmp, vkey);
            if (retv != 0)
                return retv;

            kbuf = 0x14;

            if (type == 3)
            {
                hash[..0x10].CopyTo(kirk_buf[kbuf..].Span);
                Kirk7(kirk_buf.Span, 0x10, 0x63);
            }
            else
            {
                hash[..0x10].CopyTo(kirk_buf.Span);
            }

            retv = 0;
            if (!kirk_buf.Span[..0x10].SequenceEqual(tmp))
            {
                retv = -2142174464; //0x80510300;
            }
            //for (i = 0; i < 0x10; i++)
            //{
            //    if (kirk_buf.Span[i] != tmp[i])
            //    {
            //        retv = -2142174464; //0x80510300;
            //        break;
            //    }
            //}

            return retv;
        }


        /*
            BBCipher functions.
        */
        public static int sceDrmBBCipherInit(out CIPHER_KEY ckey, int type, int mode, ReadOnlySpan<byte> header_key, ReadOnlySpan<byte> version_key, int seed)
        {
            int i, retv;
            int kbuf;

            kbuf = 0x14;
            ckey = new CIPHER_KEY { type = type };
            if (mode == 2)
            {
                ckey.seed = seed + 1;
                for (i = 0; i < 16; i++)
                {
                    ckey.key[i] = header_key[i];
                }
                if (version_key != null)
                {
                    for (i = 0; i < 16; i++)
                    {
                        ckey.key[i] ^= version_key[i];
                    }
                }
                retv = 0;
            }
            else if (mode == 1)
            {
                ckey.seed = 1;
                retv = Kirk14(kirk_buf.Span);

                if (retv != 0)
                    return retv;

                kirk_buf.Slice(0, 0x10).CopyTo(kirk_buf.Slice(kbuf));
                kirk_buf.Slice(kbuf + 0xC, 4).Span.Fill(0);

                if (ckey.type == 2)
                {
                    for (i = 0; i < 16; i++)
                    {
                        kirk_buf.Span[i + kbuf] ^= amctrl_key2[i];
                    }
                    retv = Kirk5(kirk_buf.Span, 0x10);
                    for (i = 0; i < 16; i++)
                    {
                        kirk_buf.Span[i + kbuf] ^= amctrl_key3[i];
                    }
                }
                else
                {
                    for (i = 0; i < 16; i++)
                    {
                        kirk_buf.Span[i + kbuf] ^= amctrl_key2[i];
                    }
                    retv = Kirk4(kirk_buf.Span, 0x10, 0x39);
                    for (i = 0; i < 16; i++)
                    {
                        kirk_buf.Span[i + kbuf] ^= amctrl_key3[i];
                    }
                }

                if (retv != 0)
                    return retv;

                kirk_buf.Slice(kbuf, 0x10).Span.CopyTo(ckey.key);
                // kirk_buf.Slice(kbuf, 0x10).Span.CopyTo(header_key);

                if (version_key != null)
                {
                    for (i = 0; i < 16; i++)
                    {
                        ckey.key[i] ^= version_key[i];
                    }
                }
            }
            else
            {
                retv = 0;
            }

            return retv;
        }

        public static int sceDrmBBCipherUpdate(ref CIPHER_KEY ckey, Span<byte> data, int size)
        {
            int p, retv, dsize;

            retv = 0;
            p = 0;

            while (size > 0)
            {
                dsize = (size >= 0x0800) ? 0x0800 : size;
                retv = cipher_buf(kirk_buf.Span, data.Slice(p), dsize, ref ckey);

                if (retv != 0)
                    break;

                size -= dsize;
                p += dsize;
            }

            return retv;
        }

        public static int sceDrmBBCipherFinal(ref CIPHER_KEY ckey)
        {
            ckey.key.Fill(0);
            ckey.type = 0;
            ckey.seed = 0;
            return 0;
        }
    }
}
