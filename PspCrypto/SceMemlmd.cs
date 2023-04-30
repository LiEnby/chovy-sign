using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PspCrypto
{
    public static class SceMemlmd
    {
        private static readonly Memory<byte> MemlmdKirkMemory = new byte[0x150]; // DAT_00002604
        private static readonly Memory<byte> MemlmdMem_BFC00000 = new byte[0x3000];
        private static readonly Memory<byte> MemlmdMem_BFC00280 = MemlmdMem_BFC00000.Slice(0x280, 0xC0);
        private static readonly Memory<byte> MemlmdMem_BFC00340 = MemlmdMem_BFC00000.Slice(0x340, 0x300);
        private static readonly Memory<byte> MemlmdMem_BFC00A00 = MemlmdMem_BFC00000.Slice(0xA00, 0x200);
        private static readonly Memory<byte> MemlmdMem_2780 = new byte[0x200];

        private static readonly byte[] key7A90 =
        {
            0x77, 0x3F, 0x4B, 0xE1, 0x4C, 0x0A, 0xB4, 0x52, 0x67, 0x2B, 0x67, 0x56, 0x82, 0x4C, 0xCF, 0x42,
            0xAA, 0x37, 0xFF, 0xC0, 0x89, 0x41, 0xE5, 0x63, 0x5E, 0x84, 0xE9, 0xFB, 0x53, 0xDA, 0x94, 0x9E,
            0x9B, 0xB7, 0xC2, 0xA4, 0x22, 0x9F, 0xDF, 0x1F
        };
        internal static readonly byte[] key_4C940AF0 = { 0xA8, 0xB1, 0x47, 0x77, 0xDC, 0x49, 0x6A, 0x6F, 0x38, 0x4C, 0x4D, 0x96, 0xBD, 0x49, 0xEC, 0x9B };
        internal static readonly byte[] key_4C940BF0 = { 0x3B, 0x9B, 0x1A, 0x56, 0x21, 0x80, 0x14, 0xED, 0x8E, 0x8B, 0x08, 0x42, 0xFA, 0x2C, 0xDC, 0x3A };
        internal static readonly byte[] key_4C9410F0 = { 0x31, 0x1F, 0x98, 0xD5, 0x7B, 0x58, 0x95, 0x45, 0x32, 0xAB, 0x3A, 0xE3, 0x89, 0x32, 0x4B, 0x34 };
        internal static readonly byte[] key_4C9412F0 = { 0x26, 0x38, 0x0A, 0xAC, 0xA5, 0xD8, 0x74, 0xD1, 0x32, 0xB7, 0x2A, 0xBF, 0x79, 0x9E, 0x6D, 0xDB };
        internal static readonly byte[] key_4C9413F0 = { 0x53, 0xE7, 0xAB, 0xB9, 0xC6, 0x4A, 0x4B, 0x77, 0x92, 0x17, 0xB5, 0x74, 0x0A, 0xDA, 0xA9, 0xEA };
        internal static readonly byte[] key_4C9414F0 = { 0x45, 0xEF, 0x5C, 0x5D, 0xED, 0x81, 0x99, 0x84, 0x12, 0x94, 0x8F, 0xAB, 0xE8, 0x05, 0x6D, 0x7D };
        internal static readonly byte[] key_4C9415F0 = { 0x70, 0x1B, 0x08, 0x25, 0x22, 0xA1, 0x4D, 0x3B, 0x69, 0x21, 0xF9, 0x71, 0x0A, 0xA8, 0x41, 0xA9 };
        internal static readonly byte[] key_4C949AF0 = { 0x48, 0x58, 0xAA, 0x38, 0x78, 0x9A, 0x6C, 0x0D, 0x42, 0xEA, 0xC8, 0x19, 0x23, 0x34, 0x4D, 0xF0 };
        internal static readonly byte[] key_4C949BF0 = { 0x20, 0x00, 0x5B, 0x67, 0x48, 0x77, 0x02, 0x60, 0xCF, 0x0C, 0xAB, 0x7E, 0xAE, 0x0C, 0x55, 0xA1 };
        internal static readonly byte[] key_4C949CF0 = { 0x3F, 0x67, 0x09, 0xA1, 0x47, 0x71, 0xD6, 0x9E, 0x27, 0x7C, 0x7B, 0x32, 0x67, 0x0E, 0x65, 0x8A };
        internal static readonly byte[] key_4C949DF0 = { 0x9B, 0x92, 0x99, 0x91, 0xA2, 0xE8, 0xAA, 0x4A, 0x87, 0x10, 0xA0, 0x9A, 0xBF, 0x88, 0xC0, 0xAC };
        internal static readonly byte[] key_4C949EF0 = { 0x90, 0x22, 0x66, 0xE9, 0x59, 0x11, 0x9B, 0x99, 0x67, 0x39, 0x49, 0x81, 0xAB, 0x98, 0x08, 0xA6 };
        internal static readonly byte[] key_4C949FF0 = { 0xA0, 0xA5, 0x55, 0x0A, 0xFA, 0xB2, 0x16, 0x62, 0x05, 0xDC, 0x4B, 0x8E, 0xDA, 0xD5, 0xA5, 0xCA };
        internal static readonly byte[] key_4C94A0F0 = { 0x78, 0x96, 0xAE, 0x9C, 0xE7, 0x89, 0x2D, 0xF5, 0x34, 0x9C, 0x29, 0x36, 0xD1, 0xF9, 0xE8, 0x3C };
        internal static readonly byte[] key_4C94A1F0 = { 0x71, 0x44, 0x53, 0xB6, 0xE6, 0x75, 0x3F, 0xF0, 0x8D, 0x5E, 0xB4, 0xB2, 0xEA, 0x06, 0x23, 0x6A };
        internal static readonly byte[] key_4C9491F0 = { 0x85, 0x93, 0x1F, 0xED, 0x2C, 0x4D, 0xA4, 0x53, 0x59, 0x9C, 0x3F, 0x16, 0xF3, 0x50, 0xDE, 0x46 };
        internal static readonly byte[] key_4C9494F0 = { 0x76, 0xF2, 0x6C, 0x0A, 0xCA, 0x3A, 0xBA, 0x4E, 0xAC, 0x76, 0xD2, 0x40, 0xF5, 0xC3, 0xBF, 0xF9 };
        internal static readonly byte[] key_4C9490F0 = { 0xFA, 0x79, 0x09, 0x36, 0xE6, 0x19, 0xE8, 0xA4, 0xA9, 0x41, 0x37, 0x18, 0x81, 0x02, 0xE9, 0xB3 };

        internal static readonly byte[] key_00000000 =
        {
            0x6A, 0x19, 0x71, 0xF3, 0x18, 0xDE, 0xD3, 0xA2, 0x6D, 0x3B, 0xDE, 0xC7, 0xBE, 0x98, 0xE2, 0x4C,
            0xE3, 0xDC, 0xDF, 0x42, 0x7B, 0x5B, 0x12, 0x28, 0x7D, 0xC0, 0x7A, 0x59, 0x86, 0xF0, 0xF5, 0xB5,
            0x58, 0xD8, 0x64, 0x18, 0x84, 0x24, 0x7F, 0xE9, 0x57, 0xAB, 0x4F, 0xC6, 0x92, 0x6D, 0x70, 0x29,
            0xD3, 0x61, 0x87, 0x87, 0xD0, 0xAE, 0x2C, 0xE7, 0x37, 0x77, 0xC7, 0x3C, 0x96, 0x7E, 0x21, 0x1F,
            0x65, 0x95, 0xC0, 0x61, 0x57, 0xAC, 0x64, 0xD8, 0x5A, 0x6D, 0x14, 0xD2, 0x9C, 0x54, 0xC6, 0x68,
            0x5D, 0xF5, 0xC3, 0xF0, 0x50, 0xDA, 0xEA, 0x19, 0x43, 0xA7, 0xAD, 0xC3, 0x2A, 0x14, 0xCA, 0xC8,
            0x4C, 0x83, 0x86, 0x18, 0xAE, 0x86, 0x49, 0xFB, 0x4F, 0x45, 0x75, 0xD2, 0xC3, 0xD6, 0xE1, 0x13,
            0x69, 0x37, 0xC6, 0x90, 0xCF, 0xF9, 0x79, 0xA1, 0x77, 0x3A, 0x3E, 0xBB, 0xBB, 0xD5, 0x3B, 0x84,
            0x1B, 0x9A, 0xB8, 0x79, 0xF0, 0xD3, 0x5F, 0x6F, 0x4C, 0xC0, 0x28, 0x87, 0xBC, 0xAE, 0xDA, 0x00
        };

        internal static readonly byte[] key_01000000 =
        {
            0x50, 0xCC, 0x03, 0xAC, 0x3F, 0x53, 0x1A, 0xFA, 0x0A, 0xA4, 0x34, 0x23, 0x86, 0x61, 0x7F, 0x97,
            0x84, 0x1C, 0x1A, 0x1D, 0x08, 0xD4, 0x50, 0xB6, 0xD9, 0x73, 0x27, 0x80, 0xD1, 0xDE, 0xEE, 0xCA,
            0x49, 0x8B, 0x84, 0x37, 0xDB, 0xF0, 0x70, 0xA2, 0xA6, 0x2B, 0x09, 0x4D, 0x3B, 0x29, 0xDE, 0x0B,
            0xE1, 0x6F, 0x04, 0x7A, 0xC4, 0x18, 0x7A, 0x69, 0x73, 0xBF, 0x02, 0xD8, 0xA1, 0xD0, 0x58, 0x7E,
            0x69, 0xCE, 0xAC, 0x5E, 0x1B, 0x0A, 0xF8, 0x19, 0xE6, 0x9A, 0xC0, 0xDE, 0xA0, 0xB2, 0xCE, 0x04,
            0x43, 0xC0, 0x9D, 0x50, 0x5D, 0x0A, 0xD7, 0xFD, 0xC6, 0x53, 0xAA, 0x13, 0xDD, 0x2C, 0x3B, 0x2B,
            0xBF, 0xAB, 0x7C, 0xF5, 0xA0, 0x4A, 0x79, 0xE3, 0xF1, 0x7B, 0x2E, 0xB2, 0xA3, 0xAC, 0x8E, 0x0A,
            0x38, 0x9B, 0x9E, 0xAA, 0xEC, 0x2B, 0xA3, 0x75, 0x13, 0x75, 0x77, 0x98, 0x6A, 0x66, 0x92, 0x65,
            0xBC, 0x97, 0x80, 0x0E, 0x32, 0x88, 0x9F, 0x64, 0xBA, 0x99, 0x8A, 0x72, 0x96, 0x9F, 0xE1, 0xE0
        };

        internal static readonly byte[] key_16D59E03 = { 0xC3, 0x24, 0x89, 0xD3, 0x80, 0x87, 0xB2, 0x4E, 0x4C, 0xD7, 0x49, 0xE4, 0x9D, 0x1D, 0x34, 0xD1 };

        internal static readonly byte[] key_4467415D =
        {
            0x66, 0x0F, 0xCB, 0x3B, 0x30, 0x75, 0xE3, 0x10, 0x0A, 0x95, 0x65, 0xC7, 0x3C, 0x93, 0x87, 0x22,
            0xF3, 0xA4, 0xB1, 0xE8, 0x9A, 0xFB, 0x53, 0x52, 0x8F, 0x64, 0xB2, 0xDA, 0xB7, 0x76, 0xB9, 0x56,
            0x96, 0xB6, 0x4C, 0x02, 0xE6, 0x9B, 0xAE, 0xED, 0x86, 0x48, 0xBA, 0xA6, 0x4F, 0x23, 0x15, 0x03,
            0x1F, 0xC4, 0xF7, 0x3A, 0x05, 0xC3, 0x3C, 0xE2, 0x2F, 0x36, 0xC4, 0x26, 0xF2, 0x42, 0x40, 0x1F,
            0x97, 0xEE, 0x9C, 0xC6, 0xD9, 0x68, 0xE0, 0xE7, 0xE3, 0x9F, 0xCE, 0x05, 0xE8, 0xD1, 0x8B, 0x1B,
            0x57, 0x34, 0x3D, 0x0D, 0xDF, 0xA8, 0x64, 0xBF, 0x8F, 0x4C, 0x37, 0x3F, 0x93, 0xD5, 0x45, 0x9E,
            0x2B, 0x25, 0x2C, 0x62, 0x74, 0xDE, 0xC1, 0x53, 0xAB, 0x6D, 0xDF, 0x2C, 0xCE, 0x5A, 0x6B, 0x1F,
            0x5E, 0x24, 0x4A, 0xFB, 0x7D, 0xFF, 0xE8, 0xF5, 0x19, 0x77, 0xEF, 0xCC, 0x74, 0xFE, 0x8B, 0x63,
            0x31, 0xAE, 0x99, 0x05, 0x7F, 0x51, 0xF2, 0x72, 0x6A, 0x20, 0x8D, 0x1C, 0xAC, 0x4C, 0xF6, 0x50
        };

        internal static readonly byte[] key_CFEF05F0 = { 0xCA, 0xFB, 0xBF, 0xC7, 0x50, 0xEA, 0xB4, 0x40, 0x8E, 0x44, 0x5C, 0x63, 0x53, 0xCE, 0x80, 0xB1 };
        internal static readonly byte[] key_CFEF06F0 = { 0x9F, 0x67, 0x1A, 0x7A, 0x22, 0xF3, 0x59, 0x0B, 0xAA, 0x6D, 0xA4, 0xC6, 0x8B, 0xD0, 0x03, 0x77 };
        internal static readonly byte[] key_CFEF08F0 = { 0x2E, 0x00, 0xF6, 0xF7, 0x52, 0xCF, 0x95, 0x5A, 0xA1, 0x26, 0xB4, 0x84, 0x9B, 0x58, 0x76, 0x2F };

        public static int memlmd_EF73E85B(Span<byte> modData, int size, out int newSize) => KernelModuleDecrypt(modData, size, out newSize, 0);

        public static int memlmd_CF03556B(Span<byte> modData, int size, out int newSize) => KernelModuleDecrypt(modData, size, out newSize, 1);

        static int KernelModuleDecrypt(Span<byte> modData, int size, out int newSize, int use_polling)
        {
            int ret;
            newSize = 0;
            Span<byte> local_80 = stackalloc byte[48];
            Span<byte> local_50 = stackalloc byte[32];
            if (modData.IsEmpty)
            {
                return -0xc9;
            }
            if (size < 0x160)
            {
                return -0xca;
            }
            //if ((modData[0] & 0x3f) != 0)
            //{
            //    return -0xcb;
            //}

            //if ((0x220202 >> (MemoryMarshal.Read<int>(modData) >> 0x1b)) == 0)
            //{
            //    return -0xcc;
            //}
            modData[..0x150].CopyTo(MemlmdKirkMemory.Span);
            var hdr = MemoryMarshal.Read<PSPHeader2>(MemlmdKirkMemory.Span);
            int? keySeed = null;
            bool? keyFlag = null;
            Span<byte> key = null;
            if (hdr.tag == 0x4C94A1F0)
            {
                keySeed = 0x43;
                keyFlag = true;
                key = key_4C94A1F0;
            }
            else if (hdr.tag == 0x4C949BF0)
            {
                keySeed = 0x43;
                keyFlag = true;
                key = key_4C949BF0;
            }
            else if (hdr.tag == 0xB1B9C434)
            {
            }
            else
            {
                if (hdr.tag == 0x4C9491F0)
                {
                    key = key_4C9491F0;
                }
                else if (hdr.tag == 0x4C9494F0)
                {
                    key = key_4C9494F0;
                }
                else if (hdr.tag == 0x4C9490F0)
                {
                    key = key_4C9490F0;
                }
                else
                {
                    if (hdr.tag == 0x00000000)
                    {
                        key = key_00000000;
                        keySeed = 0x42;
                    }
                    else if (hdr.tag == 0x01000000)
                    {
                        key = key_01000000;
                        keySeed = 0x43;
                    }
                    keyFlag = false;
                    goto keytagout;
                }
                keySeed = 0x43;
                keyFlag = true;
            }
        keytagout:
            //if (keyFlag == true && size < 0x160)
            //{
            //    return -0xca;
            //}
            if (keyFlag == true)
            {
                for (var i = 0; i < 0x30; i++)
                {
                    if (hdr.sCheck[i] != 0)
                    {
                        ret = -0x12e;
                        goto errout;
                    }
                }
            }
            if (keyFlag == false)
            {
                // TODO blacklistCheck
            }
            newSize = MemoryMarshal.Read<int>(hdr.sizeInfo);
            if (size - 0x150 < newSize)
            {
                ret = -0xce;
                goto errout;
            }
            if (keyFlag == true)
            {
                for (byte i = 0; i < 9; i++)
                {
                    key.CopyTo(MemlmdMem_BFC00A00.Span[(i * 0x10 + 0x14)..]);
                    MemlmdMem_BFC00A00.Span[i * 0x10 + 0x14] = i;
                }
                ref var refHdr = ref MemoryMarshal.AsRef<PSPHeader2>(modData);
                refHdr.sCheck.Slice(0x30, 0x28).CopyTo(MemlmdMem_2780.Span);
                refHdr.sCheck.Slice(0x30, 0x28).Fill(0);
                MemlmdMem_2780.Span.Slice(0, 0x28).CopyTo(local_80);
                var tmp = size - 4;
                MemoryMarshal.Write(modData, ref tmp);
                if (use_polling == 0)
                {
                    ret = KIRKEngine.sceUtilsBufferCopyWithRange(modData, size, modData, size, KIRKEngine.KIRK_CMD_SHA1_HASH);
                }
                else
                {
                    // TODO
                    ret = -1;
                }
                if (ret == 0)
                {
                    modData.Slice(0, 0x14).CopyTo(local_50);
                    MemlmdKirkMemory.Span.Slice(0, 0x20).CopyTo(modData);
                    key7A90.CopyTo(MemlmdMem_BFC00340);
                    local_50.Slice(0, 0x14).CopyTo(MemlmdMem_BFC00340.Span[0x28..]);
                    local_80.Slice(0, 0x28).CopyTo(MemlmdMem_BFC00340.Span[0x3C..]);
                    if (use_polling == 0)
                    {
                        ret = KIRKEngine.sceUtilsBufferCopyWithRange(null, 0, MemlmdMem_BFC00340.Span, 100, KIRKEngine.KIRK_CMD_ECDSA_VERIFY);
                    }
                    else
                    {
                        // TODO
                        ret = -1;
                    }
                    if (ret == 0)
                    {
                        // clear key_4C9494F0 psp 660
                        // clear key_4C9495F0 psp 660
                        // clear key_4C94A1F0 psv
                    }
                    else
                    {
                        ret = -0x132;
                        goto errout;
                    }
                }
                else
                {
                    if (ret < 0)
                    {
                        ret = -0x66;
                    }
                    else if (ret == 0xc)
                    {
                        ret = -0x6b;
                    }
                    else
                    {
                        ret = -0x69;
                    }
                    goto errout;
                }
            }
            else
            {
                key[..0x90].CopyTo(MemlmdMem_BFC00A00.Span[0x14..]);
            }
            ret = Kirk7(MemlmdMem_BFC00A00.Span, 0x90, keySeed.Value, use_polling);
            if (ret == 0)
            {
                MemlmdMem_BFC00A00[..0x90].CopyTo(MemlmdMem_BFC00280);
                if (keyFlag == true)
                {
                    hdr.CheckData[..0x5C].CopyTo(MemlmdMem_BFC00A00.Span);
                    hdr.keyData4.CopyTo(MemlmdMem_BFC00A00[0x5C..].Span);
                    hdr.sha1Hash.CopyTo(MemlmdMem_BFC00A00[0x6C..].Span);
                    hdr.keyData.CopyTo(MemlmdMem_BFC00A00[0x80..].Span);
                    hdr.cmacDataHash.CopyTo(MemlmdMem_BFC00A00[0xB0..].Span);
                    hdr.sizeInfo.CopyTo(MemlmdMem_BFC00A00[0xC0..].Span);
                    hdr.RawHdr.CopyTo(MemlmdMem_BFC00A00[0xD0..].Span);
                    MemlmdMem_BFC00A00.Slice(0x34, 0x28).Span.Fill(0);
                }
                else
                {
                    hdr.CheckData.CopyTo(MemlmdMem_BFC00A00.Span);
                    hdr.keyData50.CopyTo(MemlmdMem_BFC00A00[0x80..].Span);
                    hdr.RawHdr.CopyTo(MemlmdMem_BFC00A00[0xD0..].Span);
                }
                if (keyFlag == true)
                {
                    MemlmdMem_BFC00A00.Slice(0x5C, 0x60).CopyTo(MemlmdMem_BFC00340[0x14..]);
                    ret = Kirk7(MemlmdMem_BFC00340.Span, 0x60, keySeed.Value, use_polling);
                    if (ret == 0)
                    {
                        MemlmdMem_BFC00340.Slice(0, 0x60).CopyTo(MemlmdMem_BFC00A00[0x5C..]);
                    }
                    else
                    {
                        goto kirkerr;
                    }
                }
                if (keyFlag == true)
                {
                    MemlmdMem_BFC00A00.Slice(0x6C, 0x14).CopyTo(MemlmdMem_BFC00340);
                    MemlmdMem_BFC00A00.Slice(0x5C, 0x10).CopyTo(MemlmdMem_BFC00A00[0x70..]);
                    MemlmdMem_BFC00A00.Slice(0x18, 0x58).Span.Fill(0);
                    MemlmdMem_BFC00A00[..4].CopyTo(MemlmdMem_BFC00A00[4..]);
                    var tmp = 0x14c;
                    MemoryMarshal.Write(MemlmdMem_BFC00A00.Span, ref tmp);
                    MemlmdMem_BFC00280[..0x10].CopyTo(MemlmdMem_BFC00A00[8..]);
                    MemlmdMem_BFC00280[..0x10].Span.Fill(0);
                }
                else
                {
                    MemlmdMem_BFC00A00.Slice(4, 0x14).CopyTo(MemlmdMem_BFC00340);
                    var tmp = 0x14c;
                    MemoryMarshal.Write(MemlmdMem_BFC00A00.Span, ref tmp);
                    MemlmdMem_BFC00280[..0x14].CopyTo(MemlmdMem_BFC00A00[4..]);
                }
                if (use_polling == 0)
                {
                    ret = KIRKEngine.sceUtilsBufferCopyWithRange(MemlmdMem_BFC00A00.Span, 0x150, MemlmdMem_BFC00A00.Span, 0x150, KIRKEngine.KIRK_CMD_SHA1_HASH);
                }
                else
                {
                    // TODO
                    ret = -1;
                }
                if (ret == 0)
                {
                    if (!MemlmdMem_BFC00A00[..0x14].Span.SequenceEqual(MemlmdMem_BFC00340.Span[..0x14]))
                    {
                        ret = -0x12e;
                        goto errout;
                    }
                    if (keyFlag == true)
                    {
                        for (var i = 0; i < 0x40; i++)
                        {
                            MemlmdMem_BFC00A00[0x80..].Span[i] ^= MemlmdMem_BFC00280[0x10..].Span[i];
                        }
                        MemlmdMem_BFC00280.Slice(0x10, 0x40).Span.Fill(0);
                        ret = Kirk7(MemlmdMem_BFC00A00[0x6C..].Span, 0x40, keySeed.Value, use_polling);
                        if (ret != 0)
                        {
                            goto kirkerr;
                        }
                        for (var i = 0; i < 0x40; i++)
                        {
                            modData[0x40..][i] = (byte)(MemlmdMem_BFC00A00[0x6C..].Span[i] ^ MemlmdMem_BFC00280[0x50..].Span[i]);
                        }
                        MemlmdMem_BFC00280.Slice(0x50, 0x40).Span.Fill(0);
                        modData.Slice(0x80, 0x30).Fill(0);

                        ref var cmd1Hdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_CMD1_HEADER>(modData[0x40..]);
                        cmd1Hdr.mode = 1;
                        MemlmdMem_BFC00A00.Slice(0xc0, 0x10).Span.CopyTo(cmd1Hdr.off70);  // DAT_00009e80
                        modData.Slice(0xc0, 0x10).Fill(0);
                        MemlmdMem_BFC00A00.Slice(0xd0, 0x80).Span.CopyTo(modData[0xd0..]); // psp hdr size 0x80
                    }
                    else
                    {
                        for (var i = 0; i < 0x70; i++)
                        {
                            MemlmdMem_BFC00A00[0x40..].Span[i] ^= MemlmdMem_BFC00280[0x14..].Span[i];
                        }
                        ret = Kirk7(MemlmdMem_BFC00A00[0x2C..].Span, 0x70, keySeed.Value, use_polling);
                        if (ret == 0)
                        {
                            for (var i = 0; i < 0x70; i++)
                            {
                                modData[0x40..][i] = (byte)(MemlmdMem_BFC00A00[0x2C..].Span[i] ^ MemlmdMem_BFC00280[0x20..].Span[i]);
                            }
                            MemlmdMem_BFC00A00.Slice(0xB0, 0xA0).Span.CopyTo(modData[0xB0..]);
                            ref var cmd1ecdsaHdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_CMD1_ECDSA_HEADER>(modData[0x40..]);
                            if (cmd1ecdsaHdr.ecdsa_hash != 1)
                            {
                                ret = -0x12f;
                                goto errout;
                            }
                        }
                        else
                        {
                            // goto kirk err;
                            goto kirkerr;
                        }
                    }
                    if (use_polling == 0)
                    {
                        // File.WriteAllBytes("wrongdata", modData.ToArray());
                        ret = KIRKEngine.sceUtilsBufferCopyWithRange(modData, size, modData[0x40..], size - 0x40, KIRKEngine.KIRK_CMD_DECRYPT_PRIVATE);
                    }
                    else
                    {
                        // TODO
                    }
                    if (ret == 0)
                    {
                        goto rout;
                    }
                }
            }
        errout:

            MemlmdKirkMemory.Span.Fill(0);
            newSize = 0;
        rout:
            MemlmdMem_BFC00A00.Span.Fill(0);

            return ret;
        kirkerr:
            ret ^= 0xC;
            ret = -0x6a;
            goto errout;
        }

        static int Kirk7(Span<byte> data, int size, int seed, int use_polling)
        {
            KIRKEngine.KIRK_AES128CBC_HEADER hdr = new KIRKEngine.KIRK_AES128CBC_HEADER
            {
                mode = KIRKEngine.KIRK_MODE_DECRYPT_CBC,
                keyseed = seed,
                data_size = size
            };
            MemoryMarshal.Write(data, ref hdr);
            //using (var ms = new MemoryStream(data))
            //{
            //    ms.Seek(offset, SeekOrigin.Begin);
            //    using var bw = new BinaryWriter(ms);
            //    bw.Write(KIRKEngine.KIRK_MODE_DECRYPT_CBC);
            //    bw.Write(0);
            //    bw.Write(0);
            //    bw.Write(seed);
            //    bw.Write(size);
            //}
            if (use_polling == 0)
            {
                KIRKEngine.sceUtilsBufferCopyWithRange(data, size + 20, data, size + 20,
                    KIRKEngine.KIRK_CMD_DECRYPT_IV_0);
            }
            return 0;
        }
        static int Kirk8(Span<byte> buf, int size, int use_polling)
        {
            int retv;
            ref var hdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_AES128CBC_HEADER>(buf);
            hdr.mode = KIRKEngine.KIRK_MODE_DECRYPT_CBC;
            hdr.keyseed = 0x0100;
            hdr.data_size = size;

            retv = KIRKEngine.sceUtilsBufferCopyWithRange(buf, size + 0x14, buf, size, KIRKEngine.KIRK_CMD_DECRYPT_IV_FUSE);

            return retv;
        }

        private static readonly byte[] key_XOR_2304 =
        {
            0x71, 0xF6, 0xA8, 0x31, 0x1E, 0xE0, 0xFF, 0x1E, 0x50, 0xBA, 0x6C, 0xD2, 0x98, 0x2D, 0xD6, 0x2D
        };

        private static readonly byte[] key_XOR_2314 =
        {
            0xAA, 0x85, 0x4D, 0xB0, 0xFF, 0xCA, 0x47, 0xEB, 0x38, 0x7F, 0xD7, 0xE4, 0x3D, 0x62, 0xB0, 0x10
        };

        static int memlmd_6192F715(Span<byte> modData, int size) => KernelModuleSignCheck(modData, size, 0);

        static int memlmd_EA94592C(Span<byte> modData, int size) => KernelModuleSignCheck(modData, size, 1);

        static int KernelModuleSignCheck(Span<byte> modData, int size, int use_polling)
        {
            int ret = -0xc9;
            if (modData.IsEmpty)
            {
                goto errorout;
            }
            if (size < 0x15F)
            {
                ret = -0xca;
                goto errorout;
            }
            modData.Slice(0x80, 0xD0).CopyTo(MemlmdMem_BFC00A00.Span.Slice(0x14));

            for (var i = 0; i < 0xD0; i++)
            {
                MemlmdMem_BFC00A00[0x14..].Span[i] ^= key_XOR_2314[i & 0xF];
            }
            ret = Kirk8(MemlmdMem_BFC00A00.Span, 0xD0, use_polling);
            if (ret == 0)
            {
                for (var i = 0; i < 0xD0; i++)
                {
                    MemlmdMem_BFC00A00.Span[i] ^= key_XOR_2304[i & 0xF];
                }
                MemlmdMem_BFC00A00.Slice(0x40, 0x90).Span.CopyTo(modData[0x80..]);
                MemlmdMem_BFC00A00.Slice(0, 0x40).Span.CopyTo(modData[0x110..]);
            }
            else
            {
                if (ret < 0)
                {
                    ret = -0x67;
                }
                else if (ret != 0xc)
                {
                    ret = -0x6a;
                }
                else
                {
                    ret = -0x67;
                }
            }
        errorout:
            MemlmdMem_BFC00A00.Slice(0, 0x164).Span.Fill(0);
            return ret;
        }
    }
}
