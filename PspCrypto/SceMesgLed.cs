using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Elf32_Half = System.UInt16;
using Elf32_Word = System.UInt32;
using Elf32_Addr = System.UInt32;
using Elf32_Off = System.UInt32;
using System.Linq;

namespace PspCrypto
{
    public static class SceMesgLed
    {
        #region sceMesgLed_driver
        private static readonly byte[] key_2E5E90F0 = { 0x67, 0xE4, 0x8F, 0x4C, 0x08, 0xA0, 0x7D, 0xB1, 0x5F, 0x51, 0xA7, 0x72, 0x98, 0xA8, 0x2D, 0x7E };
        private static readonly byte[] key_2E5E90F0_xor = { 0x69, 0xBA, 0x55, 0x34, 0xF0, 0xC0, 0xD6, 0x71, 0xE3, 0x1F, 0xDB, 0x97, 0xE0, 0x7C, 0xD2, 0x2A };
        private static readonly byte[] key_2E5E80F0 = { 0x0F, 0x74, 0xAF, 0x43, 0x75, 0xCD, 0xDA, 0x39, 0x81, 0x56, 0xD9, 0x61, 0x3E, 0x16, 0xC8, 0x92 };
        private static readonly byte[] key_2E5E80F0_xor = { 0x69, 0xBA, 0x55, 0x34, 0xF0, 0xC0, 0xD6, 0x71, 0xE3, 0x1F, 0xDB, 0x97, 0xE0, 0x7C, 0xD2, 0x2A };
        private static readonly byte[] key_2E5E11F0 = { 0x75, 0xEB, 0xE8, 0x43, 0xF4, 0x87, 0x8F, 0xD0, 0x14, 0x7F, 0x7E, 0x39, 0xAD, 0xAF, 0x04, 0x9D };
        private static readonly byte[] key_2E5E11F0_xor = { 0x69, 0xBA, 0x55, 0x34, 0xF0, 0xC0, 0xD6, 0x71, 0xE3, 0x1F, 0xDB, 0x97, 0xE0, 0x7C, 0xD2, 0x2A };
        private static readonly byte[] key_2E5E12F0 = { 0x8A, 0x7B, 0xC9, 0xD6, 0x52, 0x58, 0x88, 0xEA, 0x51, 0x83, 0x60, 0xCA, 0x16, 0x79, 0xE2, 0x07 };
        private static readonly byte[] key_2E5E13F0 = { 0xFF, 0xA4, 0x68, 0xC3, 0x31, 0xCA, 0xB7, 0x4C, 0xF1, 0x23, 0xFF, 0x01, 0x65, 0x3D, 0x26, 0x36 };
        private static readonly byte[] key_2E5E10F0 = { 0x9D, 0x5C, 0x5B, 0xAF, 0x8C, 0xD8, 0x69, 0x7E, 0x51, 0x9F, 0x70, 0x96, 0xE6, 0xD5, 0xC4, 0xE8 };
        private static readonly byte[] key_2E5E10F0_xor = { 0x69, 0xBA, 0x55, 0x34, 0xF0, 0xC0, 0xD6, 0x71, 0xE3, 0x1F, 0xDB, 0x97, 0xE0, 0x7C, 0xD2, 0x2A };

        public static int sceMesgLed_driver_31D6D8AA(Span<byte> modData, int size, out int newSize, ReadOnlySpan<byte> versionKey)
        {
            var ret = Decrypt(0x2E5E90F0, key_2E5E90F0, 0x48, modData, size, out newSize, 0, null, 10, key_2E5E90F0_xor, versionKey);
            if (ret == -0x12d)
            {
                ret = Decrypt(0x2E5E80F0, key_2E5E80F0, 0x48, modData, size, out newSize, 0, null, 7, key_2E5E80F0_xor, versionKey);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0x2E5E13F0, key_2E5E13F0, 0x48, modData, size, out newSize, 0, null, 5, key_2E5E11F0_xor, versionKey);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0x2E5E12F0, key_2E5E12F0, 0x48, modData, size, out newSize, 0, null, 5, key_2E5E11F0_xor, versionKey);
                        if (ret == -0x12d)
                        {
                            ret = Decrypt(0x2E5E11F0, key_2E5E11F0, 0x48, modData, size, out newSize, 0, null, 5, key_2E5E11F0_xor, versionKey);
                            if (ret == -0x12d)
                            {
                                ret = Decrypt(0x2E5E10F0, key_2E5E10F0, 0x48, modData, size, out newSize, 0, null, 5, key_2E5E10F0_xor, versionKey);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        private static readonly byte[] key_38029AF0 = { 0xF9, 0x4A, 0x6B, 0x96, 0x79, 0x3F, 0xEE, 0x0A, 0x04, 0xC8, 0x8D, 0x7E, 0x5F, 0x38, 0x3A, 0xCF };
        private static readonly byte[] key_380293F0 = { 0xCB, 0x93, 0x12, 0x38, 0x31, 0xC0, 0x2D, 0x2E, 0x7A, 0x18, 0x5C, 0xAC, 0x92, 0x93, 0xAB, 0x32 };
        private static readonly byte[] key_380292F0 = { 0xD1, 0xB0, 0xAE, 0xC3, 0x24, 0x36, 0x13, 0x49, 0xD6, 0x49, 0xD7, 0x88, 0xEA, 0xA4, 0x99, 0x86 };
        private static readonly byte[] key_380291F0 = { 0x86, 0xA0, 0x7D, 0x4D, 0xB3, 0x6B, 0xA2, 0xFD, 0xF4, 0x15, 0x85, 0x70, 0x2D, 0x6A, 0x0D, 0x3A };
        private static readonly byte[] key_380290F0 = { 0xF9, 0x4A, 0x6B, 0x96, 0x79, 0x3F, 0xEE, 0x0A, 0x04, 0xC8, 0x8D, 0x7E, 0x5F, 0x38, 0x3A, 0xCF };
        private static readonly byte[] key_02000000 =
        {
            0x72, 0x81, 0x2F, 0xB3, 0x39, 0x5A, 0x3D, 0xBD, 0x38, 0x8A, 0x10, 0x74, 0x96, 0x55, 0xB1, 0xDF,
            0x88, 0x9F, 0xEE, 0xA1, 0xB5, 0x71, 0x74, 0x89, 0x56, 0xE1, 0xA3, 0xBB, 0x7E, 0x9F, 0xC3, 0xC2,
            0x9E, 0xF8, 0x9B, 0xB9, 0x87, 0xBD, 0x22, 0x88, 0x57, 0xDE, 0x1B, 0x88, 0xC9, 0x9A, 0x3B, 0x1A,
            0xBA, 0xBD, 0xC7, 0xA6, 0x58, 0xCB, 0x8F, 0xA1, 0x0E, 0xDF, 0x64, 0x3B, 0x4A, 0x96, 0x96, 0xCB,
            0x36, 0xD0, 0x4F, 0x2D, 0x32, 0xDD, 0x19, 0xAB, 0xE1, 0xD6, 0x54, 0xFE, 0x97, 0x13, 0x57, 0x5C,
            0x7A, 0x68, 0x05, 0x71, 0x34, 0x7D, 0x31, 0x1E, 0x33, 0x66, 0xDD, 0x6D, 0x7B, 0x76, 0x17, 0x1B,
            0x25, 0x9B, 0xAF, 0x21, 0x79, 0x17, 0x72, 0x10, 0xFD, 0xB5, 0x55, 0x35, 0xA9, 0xBE, 0x55, 0xAE,
            0x72, 0x45, 0xCE, 0x55, 0xA2, 0x70, 0x80, 0xE5, 0xAD, 0xD0, 0xBE, 0xB9, 0xE4, 0x7E, 0x02, 0xA9,
            0x92, 0x46, 0xC3, 0x35, 0x05, 0xF1, 0x7A, 0x93, 0xC1, 0x3A, 0x1A, 0x48, 0x99, 0x3B, 0x3C, 0x1B
        };

        // sceKernelWaitSema
        public static int sceMesgLed_driver_5C3A61FE(Span<byte> modData, int size, out int newSize)
        {
            var ret = Decrypt(0x38029AF0, key_38029AF0, 0x5A, modData, size, out newSize, 0, null, 9, null, null);
            if (ret == -0x12d)
            {
                ret = Decrypt(0x380293F0, key_380293F0, 0x5A, modData, size, out newSize, 0, null, 9, null, null);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0x380292F0, key_380292F0, 0x5A, modData, size, out newSize, 0, null, 9, null, null);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0x380291F0, key_380291F0, 0x5A, modData, size, out newSize, 0, null, 9, null, null);
                        if (ret == -0x12d)
                        {
                            ret = Decrypt(0x380290F0, key_380290F0, 0x5A, modData, size, out newSize, 0, null, 9, null, null);
                            if (ret == -0x12d)
                            {
                                ret = Decrypt(0x02000000, key_02000000, 0x45, modData, size, out newSize, 0, null, 8, null, null);
                            }
                        }
                    }
                }
            }

            return ret;
        }

        // sceKernelPollSema
        static int sceMesgLed_driver_3783B0AD(Span<byte> modData, int size, out int newSize) => sceMesgLed_driver_5C3A61FE(modData, size, out newSize);

        private static readonly byte[] key_EFD228F0 = { 0x22, 0xDD, 0x51, 0x6E, 0xF2, 0xD2, 0x1C, 0xA1, 0xE9, 0xD5, 0xBD, 0x88, 0x49, 0x52, 0xDE, 0x5B };
        private static readonly byte[] key_EFD21EF0 = { 0x30, 0x33, 0x7F, 0xFE, 0x67, 0xE1, 0x95, 0x8D, 0xF2, 0xC2, 0xD1, 0x70, 0x8A, 0xD5, 0x4A, 0xE5 };
        private static readonly byte[] key_EFD210F0 = { 0xE2, 0x7E, 0xDE, 0xBC, 0x27, 0x3D, 0x41, 0xD4, 0x03, 0x4A, 0x1A, 0x2B, 0xAA, 0xEE, 0x4A, 0x21 };
        private static readonly byte[] key_0A000000 =
        {
            0x80, 0xE4, 0x89, 0x5A, 0x27, 0xDB, 0x39, 0x7D, 0x6B, 0x7B, 0x7B, 0xF2, 0x3D, 0xDF, 0x92, 0xDD,
            0x50, 0xD4, 0xB5, 0x72, 0xAC, 0x6A, 0xF8, 0x7C, 0x67, 0x29, 0x42, 0x32, 0x61, 0x37, 0x79, 0x13,
            0x74, 0xE4, 0x5F, 0xB9, 0x57, 0x56, 0x1A, 0x14, 0xF3, 0x10, 0x5D, 0x8B, 0x7C, 0x74, 0x5C, 0x2F,
            0xBB, 0x96, 0x6B, 0xE4, 0x00, 0x67, 0xEC, 0xF6, 0x28, 0x81, 0xFC, 0x61, 0x01, 0xF0, 0x42, 0x83,
            0x71, 0x7F, 0xCE, 0xB6, 0xD4, 0x59, 0x84, 0xCE, 0xE9, 0xAB, 0x38, 0x5B, 0x7C, 0xD1, 0xB7, 0xFB,
            0x30, 0xDC, 0x87, 0xDF, 0x3B, 0x0C, 0x78, 0xC8, 0x35, 0xF7, 0xA8, 0xC7, 0x2F, 0x33, 0xC7, 0xA0,
            0x31, 0xDD, 0x23, 0x40, 0xE2, 0x77, 0xDF, 0xC2, 0x06, 0xD7, 0x4E, 0x78, 0x4C, 0xA4, 0xD4, 0x22,
            0x73, 0x49, 0x2D, 0x98, 0xEE, 0x17, 0xA1, 0x9A, 0x90, 0xF7, 0x96, 0x48, 0xDB, 0x6D, 0x81, 0x71,
            0xF1, 0xDB, 0x07, 0x85, 0x91, 0x68, 0xE5, 0x26, 0x95, 0xEA, 0xDF, 0xEE, 0xA1, 0xB8, 0x9D, 0x36
        };

        // sceKernelWaitSema
        public static int sceMesgLed_driver_B2CDAC3F(Span<byte> modData, int size, out int newSize)
        {
            var ret = Decrypt(0xEFD228F0, key_EFD228F0, 0x4D, modData, size, out newSize, 0, null, 2, null, null);
            if (ret == -0x12d)
            {
                ret = Decrypt(0xEFD21EF0, key_EFD21EF0, 0x4D, modData, size, out newSize, 0, null, 2, null, null);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0xEFD210F0, key_EFD210F0, 0x4D, modData, size, out newSize, 0, null, 2, null, null);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0x0A000000, key_0A000000, 0x4D, modData, size, out newSize, 0, null, 0, null, null);
                    }
                }
            }
            return ret;
        }

        // sceKernelPollSema
        static int sceMesgLed_driver_B5596BE4(Span<byte> modData, int size, out int newSize) => sceMesgLed_driver_B2CDAC3F(modData, size, out newSize);

        private static readonly byte[] key_0B000000 =
        {
            0x0B, 0x01, 0x1C, 0xE7, 0x31, 0x15, 0x6B, 0x83, 0x3E, 0x26, 0x0D, 0xCC, 0x69, 0x36, 0x12, 0xCB,
            0xA7, 0xFD, 0x26, 0x66, 0x93, 0x2A, 0x6E, 0x1A, 0x91, 0x2E, 0xC6, 0xFC, 0xD8, 0x2F, 0x00, 0x13,
            0x5A, 0xE2, 0xDF, 0xB6, 0xA2, 0xE4, 0x27, 0xC8, 0x18, 0xC3, 0x50, 0x50, 0xB7, 0xE9, 0x4A, 0xED,
            0xCC, 0x3C, 0x30, 0xFD, 0x10, 0x6A, 0x2B, 0x0A, 0x22, 0xCB, 0xC6, 0xE0, 0x20, 0x65, 0x12, 0xEB,
            0x7D, 0x4E, 0x2A, 0x37, 0x0B, 0x0A, 0xEF, 0x88, 0xDA, 0x06, 0x54, 0xD4, 0x30, 0xAF, 0xCD, 0xCA,
            0x9A, 0xF9, 0xDA, 0x1A, 0xB0, 0x1B, 0xBB, 0x62, 0x0C, 0xDB, 0xF8, 0x44, 0x73, 0x56, 0x14, 0x8E,
            0x93, 0xB1, 0x2C, 0xFD, 0x67, 0xE2, 0x5D, 0xCB, 0x48, 0x5B, 0xD9, 0xB3, 0x54, 0x14, 0xD7, 0x9F,
            0x79, 0x9C, 0x24, 0xE9, 0xC2, 0x7A, 0x4E, 0x8C, 0x4D, 0x24, 0x19, 0x94, 0xFF, 0xC9, 0xC2, 0x2D,
            0x23, 0x63, 0x51, 0xB8, 0xFA, 0xD6, 0x7F, 0xE6, 0x5E, 0xBC, 0x32, 0xB2, 0x02, 0x13, 0xC4, 0x76
        };
        //private static readonly byte[] key_0B000000_blackList =
        //{
        //    0xFD, 0xB7, 0xC4, 0xDD, 0x64, 0x48, 0x2C, 0x6C, 0x53, 0xFE, 0x58, 0x42, 0xA1, 0x13, 0x3A, 0xD3,
        //    0x4A, 0x07, 0x0E, 0xD2, 0xEA, 0xBA, 0x2F, 0xAD, 0x21, 0x31, 0x48, 0xB8, 0x9D, 0x3B, 0x1E, 0x35,
        //    0xBE, 0x8A, 0x5F, 0x54, 0x8E, 0x25, 0xB9, 0xB8, 0xB8, 0xAA, 0x8D, 0xE4, 0x2A, 0xF7, 0x66, 0xE3,
        //    0xC1, 0x8A, 0xA6, 0x89, 0x15, 0xFD, 0x88, 0xE3, 0xF1, 0x93, 0x60, 0x63, 0x84, 0x9D, 0x09, 0xD4,
        //    0x75, 0xDD, 0x9C, 0x3F, 0xBC, 0x6F, 0x61, 0xC0, 0xB1, 0x82, 0x11, 0x8A, 0xE3, 0x56, 0x68, 0x9E,
        //    0x2F, 0x2B, 0xD9, 0x07, 0xA1, 0x14, 0xB4, 0x6F, 0xF6, 0x4E, 0x85, 0x51, 0x5F, 0x64, 0x04, 0xF0,
        //    0xF2, 0xDD, 0xE4, 0xBC, 0x09, 0x1E, 0xF2, 0xEC, 0xCB, 0x56, 0xD2, 0x66, 0x3E, 0x2F, 0xE4, 0x31,
        //    0x3A, 0xD7, 0xF3, 0x2A, 0x20, 0xB1, 0x01, 0x7E, 0xB9, 0xA2, 0x1C, 0x5D, 0x3E, 0x1C, 0x77, 0x71,
        //    0x05, 0x71, 0x9C, 0x14, 0xB0, 0xC7, 0x35, 0xBA, 0x25, 0x04, 0xA1, 0xF8, 0xA8, 0x23, 0x2B, 0x5F
        //};

        // sceKernelWaitSema
        public static int sceMesgLed_driver_C79E3488(Span<byte> modData, int size, out int newSize)
        {
            return Decrypt(0x0B000000, key_0B000000, 0x4E, modData, size, out newSize, 0, null, 8, null, null);
        }

        // sceKernelPollSema
        static int sceMesgLed_driver_6BF453D3(Span<byte> modData, int size, out int newSize) => sceMesgLed_driver_C79E3488(modData, size, out newSize);

        private static readonly byte[] key_457B9AF0 = { 0x08, 0x57, 0xC2, 0x49, 0x15, 0xD6, 0x2C, 0xDB, 0x62, 0xBE, 0x86, 0x6C, 0x75, 0x19, 0xDC, 0x4D };
        private static readonly byte[] key_457B93F0 = { 0x88, 0xAF, 0x18, 0xE9, 0xC3, 0xAA, 0x6B, 0x56, 0xF7, 0xC5, 0xA8, 0xBF, 0x1A, 0x84, 0xE9, 0xF3 };
        private static readonly byte[] key_457B92F0 = { 0x92, 0x8C, 0xA4, 0x12, 0xD6, 0x5C, 0x55, 0x31, 0x5B, 0x94, 0x23, 0x9B, 0x62, 0xB3, 0xDB, 0x47 };
        private static readonly byte[] key_457B91F0 = { 0xC5, 0x9C, 0x77, 0x9C, 0x41, 0x01, 0xE4, 0x85, 0x79, 0xC8, 0x71, 0x63, 0xA5, 0x7D, 0x4F, 0xFB };
        private static readonly byte[] key_457B90F0 = { 0xBA, 0x76, 0x61, 0x47, 0x8B, 0x55, 0xA8, 0x72, 0x89, 0x15, 0x79, 0x6D, 0xD7, 0x2F, 0x78, 0x0E };
        private static readonly byte[] key_457B8AF0 = { 0x47, 0xEC, 0x60, 0x15, 0x12, 0x2C, 0xE3, 0xE0, 0x4A, 0x22, 0x6F, 0x31, 0x9F, 0xFA, 0x97, 0x3E };
        private static readonly byte[] key_457B80F0 = { 0xD4, 0x35, 0x18, 0x02, 0x29, 0x68, 0xFB, 0xA0, 0x6A, 0xA9, 0xA5, 0xED, 0x78, 0xFD, 0x2E, 0x9D };
        private static readonly byte[] key_457B10F0 = { 0x71, 0x10, 0xF0, 0xA4, 0x16, 0x14, 0xD5, 0x93, 0x12, 0xFF, 0x74, 0x96, 0xDF, 0x1F, 0xDA, 0x89 };
        private static readonly byte[] key_457B1EF0 = { 0xA3, 0x5D, 0x51, 0xE6, 0x56, 0xC8, 0x01, 0xCA, 0xE3, 0x77, 0xBF, 0xCD, 0xFF, 0x24, 0xDA, 0x4D };
        private static readonly byte[] key_457B28F0 = { 0xB1, 0xB3, 0x7F, 0x76, 0xC3, 0xFB, 0x88, 0xE6, 0xF8, 0x60, 0xD3, 0x35, 0x3C, 0xA3, 0x4E, 0xF3 };
        private static readonly byte[] key_457B0CF0 = { 0xAC, 0x34, 0xBA, 0xB1, 0x97, 0x8D, 0xAE, 0x6F, 0xBA, 0xE8, 0xB1, 0xD6, 0xDF, 0xDF, 0xF1, 0xA2 };
        private static readonly byte[] key_457B0BF0 = { 0x7B, 0x94, 0x72, 0x27, 0x4C, 0xCC, 0x54, 0x3B, 0xAE, 0xDF, 0x46, 0x37, 0xAC, 0x01, 0x4D, 0x87 };
        private static readonly byte[] key_457B0AF0 = { 0xE8, 0xBE, 0x2F, 0x06, 0xB1, 0x05, 0x2A, 0xB9, 0x18, 0x18, 0x03, 0xE3, 0xEB, 0x64, 0x7D, 0x26 };
        private static readonly byte[] key_457B08F0 = { 0xA4, 0x60, 0x8F, 0xAB, 0xAB, 0xDE, 0xA5, 0x65, 0x5D, 0x43, 0x3A, 0xD1, 0x5E, 0xC3, 0xFF, 0xEA };
        private static readonly byte[] key_457B06F0 = { 0x15, 0x07, 0x63, 0x26, 0xDB, 0xE2, 0x69, 0x34, 0x56, 0x08, 0x2A, 0x93, 0x4E, 0x4B, 0x8A, 0xB2 };
        private static readonly byte[] key_457B05F0 = { 0x40, 0x9B, 0xC6, 0x9B, 0xA9, 0xFB, 0x84, 0x7F, 0x72, 0x21, 0xD2, 0x36, 0x96, 0x55, 0x09, 0x74 };
        private static readonly byte[] key_76202403 = { 0xF3, 0xAC, 0x6E, 0x7C, 0x04, 0x0A, 0x23, 0xE7, 0x0D, 0x33, 0xD8, 0x24, 0x73, 0x39, 0x2B, 0x4A };
        private static readonly byte[] key_3ACE4DCE =
        {
            0x2F, 0x66, 0xAE, 0x00, 0x01, 0x02, 0x66, 0xEE, 0xA7, 0xC6, 0x58, 0x0C, 0x01, 0x12, 0xB2, 0x88,
            0xE9, 0x9C, 0x04, 0x92, 0x4D, 0xD1, 0xB8, 0x3D, 0x7E, 0x4C, 0x73, 0xDD, 0xF9, 0x20, 0x1F, 0x05,
            0x67, 0x01, 0x54, 0x4F, 0xB1, 0x41, 0x34, 0xEF, 0xC2, 0x4D, 0x9A, 0x5A, 0x6B, 0x21, 0xAA, 0xF6,
            0x6E, 0x03, 0xD5, 0xDE, 0x3E, 0x24, 0xE4, 0x27, 0x1A, 0x19, 0x5E, 0x9C, 0x78, 0x1F, 0x5C, 0x88,
            0x27, 0x8F, 0x47, 0xCD, 0x95, 0x12, 0x88, 0x70, 0x32, 0xFB, 0x5F, 0x1F, 0x21, 0xB1, 0x39, 0x14,
            0x93, 0x46, 0x3D, 0x07, 0xB5, 0x83, 0xD2, 0x7D, 0xFF, 0x25, 0x75, 0xD2, 0x33, 0x4B, 0xFD, 0xDB,
            0x7C, 0x88, 0xFF, 0x89, 0xB3, 0x74, 0x01, 0x8D, 0xAA, 0xFC, 0xE4, 0x2E, 0x52, 0x6E, 0xB6, 0x1F,
            0x98, 0x74, 0x62, 0xD9, 0xB4, 0xFA, 0x81, 0x4F, 0xE9, 0xCC, 0xDB, 0xA7, 0x1C, 0x4E, 0x84, 0x6F,
            0xB0, 0x8B, 0xA4, 0x0E, 0x8B, 0x41, 0x72, 0x30, 0xDA, 0xBB, 0x4C, 0x03, 0x87, 0x83, 0x42, 0xD9
        };
        private static readonly byte[] key_03000000 =
        {
            0x22, 0x4E, 0x3B, 0x01, 0xDE, 0xC8, 0x3F, 0x2C, 0x25, 0x00, 0x07, 0x9E, 0x16, 0x48, 0xBC, 0xCB,
            0xAE, 0x1D, 0x13, 0x4B, 0x34, 0xBB, 0x73, 0x83, 0xFA, 0x6A, 0xF9, 0xA1, 0xF3, 0xAE, 0x8E, 0xB7,
            0x6A, 0x25, 0x73, 0x6B, 0x0A, 0xB7, 0x7A, 0x1D, 0xCA, 0x3A, 0x75, 0x34, 0x46, 0x69, 0xD5, 0x52,
            0x6D, 0x62, 0xEC, 0x80, 0x5E, 0xB1, 0xD0, 0x64, 0xE4, 0x1C, 0x1E, 0x29, 0x65, 0x9F, 0x7D, 0xC5,
            0x4D, 0x71, 0x4F, 0xA9, 0xB8, 0xBE, 0xF9, 0xB4, 0x9E, 0xA8, 0x99, 0x63, 0x2C, 0x26, 0x8A, 0x68,
            0x77, 0xF5, 0x6B, 0x8E, 0xAC, 0xAC, 0x15, 0x8A, 0x1E, 0xD6, 0x40, 0xE0, 0x11, 0xE0, 0xB7, 0x8D,
            0x30, 0x04, 0x37, 0x73, 0x3A, 0x26, 0x5E, 0xCE, 0x66, 0x31, 0x73, 0x3C, 0xAB, 0x5A, 0xFF, 0xD0,
            0x87, 0xF8, 0x38, 0x4E, 0x88, 0x0F, 0x45, 0x65, 0x26, 0x5D, 0xDF, 0xD7, 0x49, 0x76, 0xCC, 0xB8,
            0xC5, 0x21, 0x9F, 0xCB, 0x85, 0x6F, 0x19, 0xAF, 0x39, 0x4F, 0x72, 0x9F, 0x79, 0x07, 0xFD, 0x07
        };

        // sceKernelWaitSema
        public static int sceMesgLed_driver_2CB700EC(Span<byte> modData, int size, out int newSize)
        {
            var ret = Decrypt(0x457B9AF0, key_457B9AF0, 0x5B, modData, size, out newSize, 0, null, 9, null, null);
            if (ret == -0x12d)
            {
                ret = Decrypt(0x457B93F0, key_457B93F0, 0x5B, modData, size, out newSize, 0, null, 9, null, null);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0x457B92F0, key_457B92F0, 0x5B, modData, size, out newSize, 0, null, 9, null, null);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0x457B91F0, key_457B91F0, 0x5B, modData, size, out newSize, 0, null, 9, null, null);
                        if (ret == -0x12d)
                        {
                            ret = Decrypt(0x457B90F0, key_457B90F0, 0x5B, modData, size, out newSize, 0, null, 9, null, null);
                            if (ret == -0x12d)
                            {
                                ret = Decrypt(0x457B8AF0, key_457B8AF0, 0x5B, modData, size, out newSize, 0, null, 6, null, null);
                                if (ret == -0x12d)
                                {
                                    ret = Decrypt(0x457B80F0, key_457B80F0, 0x5B, modData, size, out newSize, 0, null, 6, null, null);
                                    if (ret == -0x12d)
                                    {
                                        ret = Decrypt(0x457B28F0, key_457B28F0, 0x5B, modData, size, out newSize, 0, null, 2, null, null);
                                        if (ret == -0x12d)
                                        {
                                            ret = Decrypt(0x457B1EF0, key_457B1EF0, 0x5B, modData, size, out newSize, 0, null, 2, null, null);
                                            if (ret == -0x12d)
                                            {
                                                ret = Decrypt(0x457B10F0, key_457B10F0, 0x5B, modData, size, out newSize, 0, null, 2, null, null);
                                                if (ret == -0x12d)
                                                {
                                                    ret = Decrypt(0x457B0CF0, key_457B0CF0, 0x5B, modData, size, out newSize, 0, null, 2, null, null);
                                                    if (ret == -0x12d)
                                                    {
                                                        ret = Decrypt(0x457B0BF0, key_457B0BF0, 0x5B, modData, size, out newSize, 0, null, 2, null, null);
                                                        if (ret == -0x12d)
                                                        {
                                                            ret = Decrypt(0x457B0AF0, key_457B0AF0, 0x5B, modData, size, out newSize, 0, null, 2, null, null);
                                                            if (ret == -0x12d)
                                                            {
                                                                ret = Decrypt(0x457B08F0, key_457B08F0, 0x5B, modData, size, out newSize, 0, null, 2, null, null);
                                                                if (ret == -0x12d)
                                                                {
                                                                    ret = Decrypt(0x457B06F0, key_457B06F0, 0x5B, modData, size, out newSize, 0, null, 2, null, null);
                                                                    if (ret == -0x12d)
                                                                    {
                                                                        ret = Decrypt(0x457B05F0, key_457B05F0, 0x5B, modData, size, out newSize, 0, null, 2, null, null);
                                                                        if (ret == -0x12d)
                                                                        {
                                                                            ret = Decrypt(0x76202403, key_76202403, 0x5B, modData, size, out newSize, 0, null, 2, null, null);
                                                                            if (ret == -0x12d)
                                                                            {
                                                                                ret = Decrypt(0x3ACE4DCE, key_3ACE4DCE, 0x5B, modData, size, out newSize, 0, null, 1, null, null);
                                                                                if (ret == -0x12d)
                                                                                {
                                                                                    ret = Decrypt(0x03000000, key_03000000, 0x46, modData, size, out newSize, 0, null, 0, null, null);
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }

        // sceKernelPollSema
        static int sceMesgLed_driver_308D37FF(Span<byte> modData, int size, out int newSize) => sceMesgLed_driver_2CB700EC(modData, size, out newSize);

        private static readonly byte[] key_B2B91F0 = { 0x46, 0x1D, 0xC9, 0xC2, 0x1D, 0x44, 0xA6, 0x68, 0xF2, 0x06, 0x37, 0xBF, 0x62, 0xCD, 0x11, 0x9E };

        // sceKernelWaitSema
        public static int sceResmgr_9DC14891(Span<byte> modData, int size, out int newSize)
        {
            return Decrypt(0xB2B91F0, key_B2B91F0, 0x5C, modData, size, out newSize, 0, null, 9, null, null);
        }

        private static readonly byte[] key_D91605F0 = { 0xB8, 0x8C, 0x45, 0x8B, 0xB6, 0xE7, 0x6E, 0xB8, 0x51, 0x59, 0xA6, 0x53, 0x7C, 0x5E, 0x86, 0x31 };
        private static readonly byte[] key_D91606F0 = { 0xED, 0x10, 0xE0, 0x36, 0xC4, 0xFE, 0x83, 0xF3, 0x75, 0x70, 0x5E, 0xF6, 0xA4, 0x40, 0x05, 0xF7 };
        private static readonly byte[] key_D9160AF0 = { 0x10, 0xA9, 0xAC, 0x16, 0xAE, 0x19, 0xC0, 0x7E, 0x3B, 0x60, 0x77, 0x86, 0x01, 0x6F, 0xF2, 0x63 };
        private static readonly byte[] key_D9160BF0 = { 0x83, 0x83, 0xF1, 0x37, 0x53, 0xD0, 0xBE, 0xFC, 0x8D, 0xA7, 0x32, 0x52, 0x46, 0x0A, 0xC2, 0xC2 };
        private static readonly byte[] key_D91610F0 = { 0x89, 0x07, 0x73, 0xB4, 0x09, 0x08, 0x3F, 0x54, 0x31, 0x87, 0x00, 0xF3, 0x35, 0x14, 0x55, 0xCC };
        private static readonly byte[] key_D91611F0 = { 0x61, 0xB0, 0xC0, 0x58, 0x71, 0x57, 0xD9, 0xFA, 0x74, 0x67, 0x0E, 0x5C, 0x7E, 0x6E, 0x95, 0xB9 };
        private static readonly byte[] key_D91612F0 = { 0x9E, 0x20, 0xE1, 0xCD, 0xD7, 0x88, 0xDE, 0xC0, 0x31, 0x9B, 0x10, 0xAF, 0xC5, 0xB8, 0x73, 0x23 };
        private static readonly byte[] key_D91613F0 = { 0xEB, 0xFF, 0x40, 0xD8, 0xB4, 0x1A, 0xE1, 0x66, 0x91, 0x3B, 0x8F, 0x64, 0xB6, 0xFC, 0xB7, 0x12 };
        private static readonly byte[] key_D91614F0 = { 0xFD, 0xF7, 0xB7, 0x3C, 0x9F, 0xD1, 0x33, 0x95, 0x11, 0xB8, 0xB5, 0xBB, 0x54, 0x23, 0x73, 0x85 };
        private static readonly byte[] key_D91617F0 = { 0x02, 0xFA, 0x48, 0x73, 0x75, 0xAF, 0xAE, 0x0A, 0x67, 0x89, 0x2B, 0x95, 0x4B, 0x09, 0x87, 0xA3 };
        private static readonly byte[] key_D91618F0 = { 0x96, 0x96, 0x7C, 0xC3, 0xF7, 0x12, 0xDA, 0x62, 0x1B, 0xF6, 0x9A, 0x9A, 0x44, 0x44, 0xBC, 0x48 };
        private static readonly byte[] key_D9161AF0 = { 0x27, 0xE5, 0xA7, 0x49, 0x52, 0xE1, 0x94, 0x67, 0x35, 0x66, 0x91, 0x0C, 0xE8, 0x9A, 0x25, 0x24 };
        private static readonly byte[] key_D9161EF0 = { 0x5B, 0x4A, 0xD2, 0xF6, 0x49, 0xD4, 0xEB, 0x0D, 0xC0, 0x0F, 0xCB, 0xA8, 0x15, 0x2F, 0x55, 0x08 };
        private static readonly byte[] key_D91628F0 = { 0x49, 0xA4, 0xFC, 0x66, 0xDC, 0xE7, 0x62, 0x21, 0xDB, 0x18, 0xA7, 0x50, 0xD6, 0xA8, 0xC1, 0xB6 };
        private static readonly byte[] key_D91680F0 = { 0x2C, 0x22, 0x9B, 0x12, 0x36, 0x74, 0x11, 0x67, 0x49, 0xD1, 0xD1, 0x88, 0x92, 0xF6, 0xA1, 0xD8 };
        private static readonly byte[] key_D91681F0 = { 0x52, 0xB6, 0x36, 0x6C, 0x8C, 0x46, 0x7F, 0x7A, 0xCC, 0x11, 0x62, 0x99, 0xC1, 0x99, 0xBE, 0x98 };
        private static readonly byte[] key_D91690F0 = { 0x42, 0x61, 0xE2, 0x57, 0x94, 0x49, 0x42, 0xB5, 0xAA, 0x6D, 0x0D, 0x08, 0x3D, 0x24, 0xF7, 0x4B };
        private static readonly byte[] key_8004FD03 = { 0xF4, 0xAE, 0xF4, 0xE1, 0x86, 0xDD, 0xD2, 0x9C, 0x7C, 0xC5, 0x42, 0xA6, 0x95, 0xA0, 0x83, 0x88 };
        private static readonly byte[] key_C0CB167C =
        {
            0x8F, 0xAA, 0x27, 0x39, 0x65, 0x94, 0xA2, 0x17, 0x41, 0xBF, 0x28, 0xF2, 0x58, 0xAA, 0x77, 0x0F,
            0xBA, 0xD2, 0x89, 0x91, 0xC3, 0xD4, 0x79, 0xB5, 0xD1, 0xA6, 0xB9, 0xFB, 0xD4, 0x40, 0x19, 0xA0,
            0x11, 0x1A, 0x11, 0x1E, 0x26, 0x8D, 0x20, 0x82, 0xEB, 0x31, 0x39, 0xA7, 0xE4, 0x69, 0xDC, 0xFF,
            0x4F, 0x04, 0xE2, 0xCF, 0x22, 0x41, 0x9A, 0xE4, 0x48, 0xDC, 0xE4, 0x81, 0x84, 0xAA, 0x20, 0x5D,
            0x55, 0x7B, 0xB3, 0xE3, 0xCE, 0xB4, 0xEB, 0x18, 0x73, 0x52, 0xF4, 0x4E, 0xD4, 0x52, 0x88, 0x24,
            0x37, 0x32, 0x1D, 0xFF, 0xCC, 0xCD, 0x41, 0x91, 0xC3, 0xF6, 0x72, 0x94, 0xFB, 0x25, 0x01, 0xEB,
            0x6A, 0x94, 0x98, 0x14, 0x3C, 0xC1, 0x46, 0xF2, 0xD2, 0xDC, 0xF6, 0xDF, 0x77, 0x6C, 0x8A, 0xBC,
            0xE2, 0xDD, 0xCC, 0xDA, 0xD4, 0x6F, 0xB4, 0x33, 0x0F, 0xE2, 0xDC, 0xA2, 0x69, 0x97, 0x33, 0x08,
            0x41, 0xFD, 0x86, 0x4E, 0xF6, 0x01, 0x81, 0x0B, 0x45, 0x38, 0x20, 0xB6, 0xC0, 0x36, 0x9B, 0xF3
        };
        private static readonly byte[] key_08000000 =
        {
            0x5B, 0x16, 0x91, 0x75, 0x44, 0xC5, 0xC5, 0x02, 0x6C, 0x76, 0x94, 0xC0, 0x55, 0xF1, 0x12, 0x95,
            0x71, 0x93, 0x91, 0x7E, 0x81, 0xD2, 0x77, 0x31, 0xF1, 0x28, 0x30, 0x70, 0x9B, 0x73, 0x77, 0x4A,
            0x11, 0xD7, 0x56, 0xF1, 0x0E, 0x4C, 0xC1, 0xB0, 0x92, 0x04, 0xE4, 0xE2, 0xAC, 0xA8, 0x46, 0x07,
            0x85, 0xEA, 0x69, 0x9A, 0xD7, 0xD2, 0xEC, 0xF1, 0xA0, 0x43, 0x42, 0x1C, 0xC6, 0xAF, 0xC6, 0x21,
            0xE8, 0x24, 0x06, 0x29, 0x01, 0xBC, 0xB6, 0x59, 0x4D, 0x78, 0x0D, 0x00, 0xD3, 0xFD, 0x95, 0xE7,
            0x3F, 0xD1, 0x45, 0x16, 0xDB, 0x0A, 0x01, 0x18, 0x0B, 0x70, 0xE4, 0x8D, 0x8B, 0xF9, 0xC4, 0x5A,
            0x85, 0x10, 0xDB, 0x78, 0x05, 0xDF, 0xFB, 0x4A, 0xA9, 0xDD, 0x31, 0xC6, 0x6A, 0xC1, 0x98, 0xB4,
            0x65, 0x90, 0x58, 0xF5, 0x65, 0x3C, 0xCD, 0xB0, 0x6A, 0x2F, 0xF6, 0xFD, 0xA4, 0xC7, 0x86, 0x62,
            0xE6, 0xF8, 0x5E, 0xE8, 0x66, 0x16, 0x20, 0x92, 0xF0, 0x8E, 0xEA, 0x17, 0x9F, 0x1A, 0xF3, 0xDD
        };

        // sceKernelWaitSema
        public static int sceMesgLed_driver_337D0DD3(Span<byte> modData, int size, out int newSize)
        {
            var ret = Decrypt(0xD91690F0, key_D91690F0, 0x5d, modData, size, out newSize, 0, null, 9, null, null);
            if (ret == -0x12d)
            {
                ret = Decrypt(0xD91681F0, key_D91681F0, 0x5d, modData, size, out newSize, 0, null, 6, null, null);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0xD91680F0, key_D91680F0, 0x5d, modData, size, out newSize, 0, null, 6, null, null);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0xD9161AF0, key_D9161AF0, 0x5d, modData, size, out newSize, 0, null, 2, null, null);
                        if (ret == -0x12d)
                        {
                            ret = Decrypt(0xD91618F0, key_D91618F0, 0x5d, modData, size, out newSize, 0, null, 2, null, null);
                            if (ret == -0x12d)
                            {
                                ret = Decrypt(0xD91617F0, key_D91617F0, 0x5d, modData, size, out newSize, 0, null, 2, null, null);
                                if (ret == -0x12d)
                                {
                                    ret = Decrypt(0xD91614F0, key_D91614F0, 0x5d, modData, size, out newSize, 0, null, 2, null, null);
                                    if (ret == -0x12d)
                                    {
                                        ret = Decrypt(0xD91613F0, key_D91613F0, 0x5d, modData, size, out newSize, 0, null, 2, null, null);
                                        if (ret == -0x12d)
                                        {
                                            ret = Decrypt(0xD91612F0, key_D91612F0, 0x5d, modData, size, out newSize, 0, null, 2, null, null);
                                            if (ret == -0x12d)
                                            {
                                                ret = Decrypt(0xD91628F0, key_D91628F0, 0x5d, modData, size, out newSize, 0, null, 2, null, null);
                                                if (ret == -0x12d)
                                                {
                                                    ret = Decrypt(0xD9161EF0, key_D9161EF0, 0x5d, modData, size, out newSize, 0, null, 2, null, null);
                                                    if (ret == -0x12d)
                                                    {
                                                        ret = Decrypt(0xD91610F0, key_D91610F0, 0x5d, modData, size, out newSize, 0, null, 2, null, null);
                                                        if (ret == -0x12d)
                                                        {
                                                            ret = Decrypt(0xD91611F0, key_D91611F0, 0x5d, modData, size, out newSize, 0, null, 2, null, null);
                                                            if (ret == -0x12d)
                                                            {
                                                                ret = Decrypt(0xD9160BF0, key_D9160BF0, 0x5d, modData, size, out newSize, 0, null, 2, null, null);
                                                                if (ret == -0x12d)
                                                                {
                                                                    ret = Decrypt(0xD9160AF0, key_D9160AF0, 0x5d, modData, size, out newSize, 0, null, 2, null, null);
                                                                    if (ret == -0x12d)
                                                                    {
                                                                        ret = Decrypt(0xD91606F0, key_D91606F0, 0x5d, modData, size, out newSize, 0, null, 2, null, null);
                                                                        if (ret == -0x12d)
                                                                        {
                                                                            ret = Decrypt(0xD91605F0, key_D91605F0, 0x5d, modData, size, out newSize, 0, null, 2, null, null);
                                                                            if (ret == -0x12d)
                                                                            {
                                                                                ret = Decrypt(0x8004FD03, key_8004FD03, 0x5d, modData, size, out newSize, 0, null, 2, null, null);
                                                                                if (ret == -0x12d)
                                                                                {
                                                                                    ret = Decrypt(0xC0CB167C, key_C0CB167C, 0x5d, modData, size, out newSize, 0, null, 1, null, null);
                                                                                    if (ret == -0x12d)
                                                                                    {
                                                                                        ret = Decrypt(0x08000000, key_08000000, 0x4B, modData, size, out newSize, 0, null, 0, null, null);
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }

        // sceKernelPollSema
        static int sceMesgLed_driver_792A6126(Span<byte> modData, int size, out int newSize) => sceMesgLed_driver_337D0DD3(modData, size, out newSize);

        private static readonly byte[] key_7B0528F0 = { 0xCE, 0x40, 0xE0, 0x47, 0xA9, 0x97, 0xCA, 0x68, 0xAD, 0x40, 0x1C, 0x68, 0xC3, 0xFF, 0xC2, 0x7A };
        private static readonly byte[] key_7B051EF0 = { 0xDC, 0xAE, 0xCE, 0xD7, 0x3C, 0xA4, 0x43, 0x44, 0xB6, 0x57, 0x70, 0x90, 0x00, 0x78, 0x56, 0xC4 };
        private static readonly byte[] key_7B0510F0 = { 0x1C, 0x0D, 0x41, 0x05, 0xE9, 0x4B, 0x1E, 0x31, 0x5C, 0xC8, 0xD7, 0x33, 0xE3, 0xC4, 0xC2, 0xBE };
        private static readonly byte[] key_7B0508F0 = { 0xC9, 0x7D, 0x3E, 0x0A, 0x54, 0x81, 0x6E, 0xC7, 0x13, 0x74, 0x99, 0x74, 0x62, 0x18, 0xE7, 0xDD };
        private static readonly byte[] key_7B0506F0 = { 0x78, 0x1A, 0xD2, 0x87, 0x24, 0xBD, 0xA2, 0x96, 0x18, 0x3F, 0x89, 0x36, 0x72, 0x90, 0x92, 0x85 };
        private static readonly byte[] key_7B0505F0 = { 0x2D, 0x86, 0x77, 0x3A, 0x56, 0xA4, 0x4F, 0xDD, 0x3C, 0x16, 0x71, 0x93, 0xAA, 0x8E, 0x11, 0x43 };
        private static readonly byte[] key_0A35EA03 = { 0xF9, 0x48, 0x38, 0x0C, 0x96, 0x88, 0xA7, 0x74, 0x4F, 0x65, 0xA0, 0x54, 0xC2, 0x76, 0xD9, 0xB8 };
        private static readonly byte[] key_BB67C59F =
        {
            0x82, 0x78, 0x73, 0x69, 0x0D, 0x87, 0x1F, 0xE2, 0x49, 0xD6, 0x1F, 0xA0, 0x58, 0xDA, 0xF8, 0x47,
            0x7E, 0x67, 0x2A, 0xE9, 0xE1, 0x01, 0xBE, 0xC0, 0x6F, 0x54, 0x8D, 0x35, 0x9D, 0xA3, 0x73, 0x0E,
            0xF9, 0x20, 0x47, 0x84, 0xD3, 0xDA, 0xD3, 0x40, 0xF6, 0x1A, 0x37, 0x71, 0x9C, 0xB6, 0x71, 0xBD,
            0x38, 0x15, 0x8D, 0x19, 0x50, 0xE0, 0x35, 0x51, 0xC8, 0xDD, 0x4D, 0x3F, 0xFE, 0xFE, 0x48, 0xDF,
            0xB0, 0xC2, 0xCA, 0x0A, 0x47, 0x2A, 0x1D, 0x6E, 0xFA, 0x64, 0x14, 0x98, 0xED, 0x38, 0xDC, 0x4F,
            0x2D, 0x52, 0x60, 0x44, 0x79, 0x99, 0x79, 0x63, 0xB6, 0x76, 0x90, 0x69, 0x6A, 0x95, 0x42, 0x20,
            0x8B, 0x12, 0x7D, 0xBA, 0x32, 0xAB, 0x36, 0x81, 0x1F, 0x3F, 0xA3, 0xEB, 0x2F, 0x5A, 0x7A, 0x06,
            0x42, 0xC5, 0x24, 0x0B, 0x2C, 0xE2, 0x9D, 0x6F, 0x6B, 0x3B, 0x32, 0x59, 0x90, 0x42, 0x75, 0xE6,
            0xF9, 0x96, 0x16, 0x51, 0x68, 0x7A, 0x45, 0xDE, 0xE8, 0x41, 0x5A, 0xEC, 0x35, 0x7B, 0x6F, 0x05
        };
        private static readonly byte[] key_09000000 =
        {
            0x88, 0xD7, 0x76, 0xF5, 0xB7, 0x6D, 0xEC, 0x0E, 0x00, 0x0B, 0x85, 0xE6, 0x4A, 0x37, 0x9B, 0xC9,
            0x00, 0x95, 0xEA, 0x7A, 0xAD, 0x66, 0x67, 0x09, 0x05, 0xF2, 0x09, 0x52, 0x47, 0x27, 0x8E, 0x4E,
            0x57, 0x83, 0xF7, 0x5D, 0x1F, 0x90, 0x11, 0x0E, 0xEF, 0xF6, 0x97, 0x39, 0x83, 0xA1, 0x65, 0xD0,
            0x11, 0xCA, 0x64, 0x96, 0x7A, 0xEF, 0xC9, 0x18, 0x6B, 0x7D, 0x97, 0x7A, 0xF0, 0x32, 0x28, 0xA6,
            0xE2, 0xE3, 0x86, 0x64, 0xDF, 0xAE, 0x03, 0xD2, 0xAD, 0x7E, 0x7F, 0x46, 0x2E, 0x5F, 0x02, 0x95,
            0xE9, 0x35, 0x5C, 0x17, 0xAD, 0x42, 0x54, 0x62, 0x84, 0x66, 0xC9, 0x35, 0x9C, 0x7B, 0x2A, 0xFA,
            0xD7, 0xDC, 0xD0, 0x2B, 0xDA, 0x89, 0x32, 0x7C, 0xD0, 0x77, 0xE6, 0xEE, 0xFE, 0xCD, 0x2B, 0xE0,
            0x87, 0xBB, 0xE1, 0xB5, 0xCE, 0x29, 0x11, 0x9A, 0x79, 0xF1, 0xA8, 0x71, 0xC0, 0x5E, 0x65, 0x39,
            0xFB, 0x6D, 0xB9, 0x37, 0xD4, 0x52, 0x74, 0x44, 0xE1, 0xC2, 0xBA, 0xBA, 0x3D, 0x2D, 0x4A, 0x35
        };

        // sceKernelWaitSema
        public static int sceMesgLed_driver_4EAB9850(Span<byte> modData, int size, out int newSize)
        {
            var ret = Decrypt(0x7B0528F0, key_7B0528F0, 0x5E, modData, size, out newSize, 0, null, 2, null, null);
            if (ret == -0x12d)
            {
                ret = Decrypt(0x7B051EF0, key_7B051EF0, 0x5E, modData, size, out newSize, 0, null, 2, null, null);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0x7B0510F0, key_7B0510F0, 0x5E, modData, size, out newSize, 0, null, 2, null, null);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0x7B0508F0, key_7B0508F0, 0x5E, modData, size, out newSize, 0, null, 2, null, null);
                        if (ret == -0x12d)
                        {
                            ret = Decrypt(0x7B0506F0, key_7B0506F0, 0x5E, modData, size, out newSize, 0, null, 2, null, null);
                            if (ret == -0x12d)
                            {
                                ret = Decrypt(0x7B0505F0, key_7B0505F0, 0x5E, modData, size, out newSize, 0, null, 2, null, null);
                                if (ret == -0x12d)
                                {
                                    ret = Decrypt(0x0A35EA03, key_0A35EA03, 0x5E, modData, size, out newSize, 0, null, 2, null, null);
                                    if (ret == -0x12d)
                                    {
                                        ret = Decrypt(0xBB67C59F, key_BB67C59F, 0x5E, modData, size, out newSize, 0, null, 1, null, null);
                                        if (ret == -0x12d)
                                        {
                                            ret = Decrypt(0x09000000, key_09000000, 0x4C, modData, size, out newSize, 0, null, 0, null, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }

        // sceKernelPollSema
        static int sceMesgLed_driver_4BE02A12(Span<byte> modData, int size, out int newSize) => sceMesgLed_driver_4EAB9850(modData, size, out newSize);

        private static readonly byte[] key_ADF328F0 = { 0xE3, 0xB1, 0xC9, 0xB3, 0x4E, 0x07, 0x60, 0x49, 0xA7, 0x47, 0xFF, 0x7D, 0x19, 0xFA, 0x56, 0xA1 };
        private static readonly byte[] key_ADF31EF0 = { 0xF1, 0x5F, 0xE7, 0x23, 0xDB, 0x34, 0xE9, 0x65, 0xBC, 0x50, 0x93, 0x85, 0xDA, 0x7D, 0xC2, 0x1F };
        private static readonly byte[] key_ADF310F0 = { 0x23, 0x12, 0x46, 0x61, 0x9B, 0xE8, 0x3D, 0x3C, 0x4D, 0xD8, 0x58, 0xDE, 0xFA, 0x46, 0xC2, 0xDB };
        private static readonly byte[] key_ADF308F0 = { 0xF6, 0x62, 0x39, 0x6E, 0x26, 0x22, 0x4D, 0xCA, 0x02, 0x64, 0x16, 0x99, 0x7B, 0x9A, 0xE7, 0xB8 };
        private static readonly byte[] key_ADF306F0 = { 0x47, 0x05, 0xD5, 0xE3, 0x56, 0x1E, 0x81, 0x9B, 0x09, 0x2F, 0x06, 0xDB, 0x6B, 0x12, 0x92, 0xE0 };
        private static readonly byte[] key_ADF305F0 = { 0x12, 0x99, 0x70, 0x5E, 0x24, 0x07, 0x6C, 0xD0, 0x2D, 0x06, 0xFE, 0x7E, 0xB3, 0x0C, 0x11, 0x26 };
        private static readonly byte[] key_D67B3303 = { 0xC9, 0x03, 0x4F, 0x3C, 0xDD, 0x4F, 0xE8, 0xD0, 0x9A, 0xDD, 0xED, 0x74, 0x64, 0xDC, 0x5C, 0x35 };
        private static readonly byte[] key_7F24BDCD =
        {
            0xB2, 0x97, 0x43, 0x75, 0x5E, 0x0C, 0xA7, 0x5E, 0x38, 0x53, 0x7D, 0x80, 0xD1, 0x6B, 0xEA, 0x68,
            0x86, 0x8D, 0x2B, 0xAD, 0x3B, 0x2D, 0x46, 0x47, 0x84, 0xF3, 0x1E, 0x56, 0xBB, 0xEE, 0x3A, 0x3C,
            0xE3, 0x7A, 0xF3, 0x6E, 0xAE, 0x77, 0xF6, 0x23, 0x16, 0xF3, 0x25, 0xCD, 0xFB, 0x8A, 0x01, 0x6F,
            0xB0, 0xDD, 0x3C, 0x44, 0x2F, 0x01, 0xE1, 0x00, 0xA1, 0x5F, 0xB8, 0x86, 0x86, 0x20, 0xB9, 0xD1,
            0xCE, 0x3D, 0xC5, 0x6E, 0x41, 0xB9, 0x11, 0x10, 0x57, 0x2F, 0x29, 0xE2, 0xC9, 0x4C, 0xFD, 0x08,
            0x3D, 0xC2, 0x18, 0x7E, 0x5C, 0xE2, 0xA2, 0x24, 0xD9, 0x01, 0x4B, 0xA1, 0x28, 0x4C, 0xC4, 0xAC,
            0xE2, 0x0E, 0x0A, 0xC4, 0xD2, 0x7F, 0xAA, 0x3A, 0x08, 0x62, 0x45, 0xF7, 0xCA, 0x8D, 0xC6, 0x18,
            0xF0, 0xDE, 0x12, 0x17, 0xAD, 0xCB, 0xB7, 0xF2, 0xCA, 0xC5, 0xA8, 0x56, 0xC1, 0xB3, 0x20, 0xB6,
            0x02, 0xE9, 0x31, 0x08, 0x3B, 0x44, 0x7C, 0xDE, 0x56, 0xB4, 0x3F, 0x12, 0x5D, 0xF0, 0x4F, 0x97
        };

        private static readonly byte[] key_0C000000 =
        {
            0x82, 0x4C, 0xA5, 0x18, 0xD3, 0xC8, 0x6E, 0xEA, 0x17, 0x41, 0x04, 0xDC, 0xEA, 0xC5, 0x01, 0xFC,
            0x97, 0xB1, 0x94, 0x54, 0x71, 0x19, 0x22, 0xEE, 0xE0, 0x2D, 0xE9, 0x83, 0x3D, 0x64, 0x30, 0xE6,
            0x42, 0x5C, 0x30, 0x5F, 0xEB, 0x41, 0xA0, 0xE0, 0x62, 0xC6, 0x63, 0xEE, 0x5D, 0xA5, 0x0D, 0x1E,
            0xC2, 0x10, 0x14, 0x49, 0x06, 0xC6, 0x93, 0x84, 0x71, 0xA5, 0x42, 0x63, 0x13, 0xF0, 0xB6, 0xD5,
            0x43, 0x51, 0x9E, 0xFA, 0x91, 0x0A, 0x7C, 0xE1, 0x58, 0x1B, 0x95, 0x25, 0x40, 0x11, 0xF1, 0x8D,
            0xB1, 0x01, 0x8D, 0x04, 0x09, 0x54, 0x5C, 0x54, 0xF5, 0x53, 0x08, 0xB0, 0x53, 0x85, 0xB4, 0xCE,
            0x0B, 0xF5, 0xC3, 0xFB, 0xC6, 0x55, 0x24, 0x0B, 0xF2, 0xC6, 0x2C, 0xE4, 0x0C, 0xF0, 0x05, 0x3C,
            0xD7, 0x6C, 0x39, 0xD5, 0x87, 0x22, 0x09, 0xF7, 0x3D, 0xC5, 0xA2, 0xFD, 0x55, 0x92, 0x3F, 0xB1,
            0xF6, 0xFE, 0xC8, 0x18, 0x1D, 0x6B, 0x04, 0x52, 0x5F, 0x8C, 0xE8, 0xE7, 0x26, 0x5A, 0x6E, 0x5A
        };

        // sceKernelWaitSema
        public static int sceMesgLed_driver_21AFFAAC(Span<byte> modData, int size, out int newSize)
        {
            var ret = Decrypt(0xADF328F0, key_ADF328F0, 0x60, modData, size, out newSize, 0, null, 4, null, null);
            if (ret == -0x12d)
            {
                ret = Decrypt(0xADF31EF0, key_ADF31EF0, 0x60, modData, size, out newSize, 0, null, 4, null, null);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0xADF310F0, key_ADF310F0, 0x60, modData, size, out newSize, 0, null, 4, null, null);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0xADF308F0, key_ADF308F0, 0x60, modData, size, out newSize, 0, null, 4, null, null);
                        if (ret == -0x12d)
                        {
                            ret = Decrypt(0xADF306F0, key_ADF306F0, 0x60, modData, size, out newSize, 0, null, 4, null, null);
                            if (ret == -0x12d)
                            {
                                ret = Decrypt(0xADF305F0, key_ADF305F0, 0x60, modData, size, out newSize, 0, null, 4, null, null);
                                if (ret == -0x12d)
                                {
                                    ret = Decrypt(0xD67B3303, key_D67B3303, 0x60, modData, size, out newSize, 0, null, 2, null, null);
                                    if (ret == -0x12d)
                                    {
                                        ret = Decrypt(0x7F24BDCD, key_7F24BDCD, 0x60, modData, size, out newSize, 0, null, 1, null, null);
                                        if (ret == -0x12d)
                                        {
                                            ret = Decrypt(0x0C000000, key_0C000000, 0x4F, modData, size, out newSize, 0, null, 0, null, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ret;
        }

        // sceKernelPollSema
        static int sceMesgLed_driver_52B6E552(Span<byte> modData, int size, out int newSize) => sceMesgLed_driver_21AFFAAC(modData, size, out newSize);

        private static readonly byte[] key_279D28F0 = { 0xD2, 0xF4, 0x82, 0x58, 0xC3, 0x82, 0xDA, 0x73, 0xE9, 0xE2, 0x6F, 0x28, 0x81, 0x1F, 0xA6, 0xD3 };
        private static readonly byte[] key_279D1EF0 = { 0xC0, 0x1A, 0xAC, 0xC8, 0x56, 0xB1, 0x53, 0x5F, 0xF2, 0xF5, 0x03, 0xD0, 0x42, 0x98, 0x32, 0x6D };
        private static readonly byte[] key_279D10F0 = { 0x12, 0x57, 0x0D, 0x8A, 0x16, 0x6D, 0x87, 0x06, 0x03, 0x7D, 0xC8, 0x8B, 0x62, 0xA3, 0x32, 0xA9 };
        private static readonly byte[] key_279D08F0 = { 0xC7, 0x27, 0x72, 0x85, 0xAB, 0xA7, 0xF7, 0xF0, 0x4C, 0xC1, 0x86, 0xCC, 0xE3, 0x7F, 0x17, 0xCA };
        private static readonly byte[] key_279D06F0 = { 0x76, 0x40, 0x9E, 0x08, 0xDB, 0x9B, 0x3B, 0xA1, 0x47, 0x8A, 0x96, 0x8E, 0xF3, 0xF7, 0x62, 0x92 };
        private static readonly byte[] key_279D05F0 = { 0x23, 0xDC, 0x3B, 0xB5, 0xA9, 0x82, 0xD6, 0xEA, 0x63, 0xA3, 0x6E, 0x2B, 0x2B, 0xE9, 0xE1, 0x54 };
        private static readonly byte[] key_D66DF703 = { 0x22, 0x43, 0x57, 0x68, 0x2F, 0x41, 0xCE, 0x65, 0x4C, 0xA3, 0x7C, 0xC6, 0xC4, 0xAC, 0xF3, 0x60 };
        private static readonly byte[] key_1BC8D12B =
        {
            0xAE, 0x11, 0x18, 0xE6, 0xED, 0xB2, 0xF1, 0xB2, 0xA8, 0xCC, 0xCA, 0xBF, 0xA9, 0x14, 0x80, 0x87,
            0x66, 0xF1, 0x2D, 0x87, 0xDA, 0xD3, 0x5E, 0xD3, 0xBF, 0xAF, 0xDF, 0xB5, 0x11, 0xC2, 0xDA, 0x4E,
            0x85, 0x09, 0x7C, 0x19, 0xA9, 0xB1, 0xF4, 0x80, 0x35, 0x10, 0xF7, 0x27, 0xA4, 0xB1, 0x52, 0x66,
            0x96, 0xA2, 0x68, 0x77, 0xDB, 0x9C, 0x76, 0x04, 0x09, 0x44, 0x87, 0x91, 0x06, 0xBA, 0xA4, 0x24,
            0x68, 0xA5, 0xBE, 0xD7, 0x4F, 0x73, 0x83, 0x7F, 0x0E, 0x60, 0x13, 0xAD, 0x60, 0xAA, 0xA5, 0xD7,
            0x80, 0x44, 0x3A, 0x3F, 0x5D, 0xE1, 0x6B, 0xD3, 0xD9, 0xE8, 0x4A, 0xF6, 0x57, 0xEB, 0x95, 0x5A,
            0x10, 0xA9, 0x9F, 0x4D, 0xDE, 0x2F, 0xE5, 0xCE, 0xE4, 0xDD, 0x82, 0x5D, 0x05, 0x57, 0x22, 0xCA,
            0xA6, 0xE3, 0xA2, 0xCD, 0x81, 0x16, 0xE4, 0x5A, 0x7D, 0x0E, 0x9B, 0xC0, 0x26, 0x0D, 0xC6, 0x82,
            0x02, 0xC0, 0x9B, 0x0C, 0xE3, 0x7C, 0xD3, 0xCD, 0xF4, 0x33, 0x72, 0x4D, 0xFC, 0x2C, 0xAA, 0x48
        };

        private static readonly byte[] key_0D000000 =
        {
            0xF5, 0x07, 0xA4, 0x47, 0xD7, 0xE0, 0x29, 0x25, 0xB2, 0xC8, 0xE3, 0x95, 0x3E, 0x5E, 0x69, 0xE0,
            0x6F, 0xA5, 0x0D, 0xCF, 0x2D, 0x6A, 0xB5, 0x6C, 0xF1, 0x7D, 0x65, 0xD2, 0x81, 0xD5, 0xBE, 0xDA,
            0xFC, 0xE9, 0xBE, 0x4F, 0xCE, 0x25, 0x95, 0x84, 0xE7, 0x94, 0x23, 0x4A, 0x99, 0x26, 0x48, 0x6F,
            0xFC, 0xCB, 0xC7, 0xDB, 0x99, 0x7A, 0x54, 0x5D, 0x06, 0x07, 0xDF, 0xD8, 0x50, 0x97, 0x48, 0x19,
            0x49, 0x72, 0x0F, 0x16, 0x0F, 0xE6, 0x94, 0x41, 0x70, 0x64, 0xC2, 0x69, 0xCD, 0x22, 0x97, 0xA4,
            0xB7, 0xBD, 0x3F, 0xF8, 0xD9, 0x60, 0xBA, 0x3E, 0x7C, 0xCE, 0x5E, 0x98, 0xBF, 0xAC, 0x5B, 0x91,
            0x01, 0xF3, 0xF1, 0x42, 0xB0, 0xE9, 0xDA, 0x2C, 0xE1, 0x62, 0x75, 0xA1, 0x35, 0x3C, 0xCE, 0x57,
            0x1D, 0xB0, 0xC1, 0x4A, 0xD5, 0xC2, 0xD0, 0x98, 0xAF, 0x64, 0x78, 0x84, 0xB7, 0xDF, 0x5B, 0x57,
            0x67, 0x74, 0xF1, 0x20, 0x57, 0x7A, 0x5F, 0xFF, 0x20, 0x8A, 0xE4, 0x29, 0xEB, 0x12, 0x5E, 0xF4
        };

        // sceKernelWaitSema
        public static int sceMesgLed_driver_C00DAD75(Span<byte> modData, int size, out int newSize)
        {
            var ret = Decrypt(0x279D28F0, key_279D28F0, 0x61, modData, size, out newSize, 0, null, 4, null, null);
            if (ret == -0x12d)
            {
                ret = Decrypt(0x279D1EF0, key_279D1EF0, 0x61, modData, size, out newSize, 0, null, 4, null, null);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0x279D10F0, key_279D10F0, 0x61, modData, size, out newSize, 0, null, 4, null, null);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0x279D08F0, key_279D08F0, 0x61, modData, size, out newSize, 0, null, 4, null, null);
                        if (ret == -0x12d)
                        {
                            ret = Decrypt(0x279D06F0, key_279D06F0, 0x61, modData, size, out newSize, 0, null, 4, null, null);
                            if (ret == -0x12d)
                            {
                                ret = Decrypt(0x279D05F0, key_279D05F0, 0x61, modData, size, out newSize, 0, null, 4, null, null);
                                if (ret == -0x12d)
                                {
                                    ret = Decrypt(0xD66DF703, key_D66DF703, 0x61, modData, size, out newSize, 0, null, 2, null, null);
                                    if (ret == -0x12d)
                                    {
                                        ret = Decrypt(0x1BC8D12B, key_1BC8D12B, 0x61, modData, size, out newSize, 0, null, 1, null, null);
                                        if (ret == -0x12d)
                                        {
                                            ret = Decrypt(0x0D000000, key_0D000000, 0x50, modData, size, out newSize, 0, null, 0, null, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }

        // sceKernelPollSema
        static int sceMesgLed_driver_F8485C9C(Span<byte> modData, int size, out int newSize) => sceMesgLed_driver_C00DAD75(modData, size, out newSize);

        private static readonly byte[] key_E92428F0 = { 0xAA, 0x14, 0x26, 0xB4, 0x4C, 0xB9, 0xF4, 0x58, 0xC1, 0x6F, 0xCD, 0x42, 0x70, 0x2E, 0x12, 0x6A };
        private static readonly byte[] key_E92428F0_xor = { 0xCB, 0x81, 0xEE, 0x3B, 0xDC, 0x87, 0x1E, 0xA1, 0xC8, 0x14, 0xB8, 0xFF, 0x92, 0x3F, 0xB7, 0xC0 };
        private static readonly byte[] key_E9241EF0 = { 0xEF, 0x4A, 0x8E, 0x6B, 0x24, 0x1A, 0xD5, 0xDC, 0xE0, 0xE5, 0x9D, 0xAD, 0xE6, 0x7F, 0xBD, 0x0E };
        private static readonly byte[] key_E9241EF0_xor = { 0x02, 0x99, 0xCE, 0xA6, 0x38, 0x38, 0x32, 0x84, 0x0E, 0xCF, 0x86, 0x6B, 0xB4, 0xEE, 0x3C, 0x77 };
        private static readonly byte[] key_E92410F0 = { 0x71, 0xBE, 0x93, 0xCD, 0x96, 0x65, 0xBC, 0x57, 0xF6, 0xE5, 0xE9, 0xD7, 0x1C, 0x6A, 0xD5, 0xAA };
        private static readonly byte[] key_E92410F0_xor = { 0x36, 0xEF, 0x82, 0x4E, 0x74, 0xFB, 0x17, 0x5B, 0x14, 0x14, 0x05, 0xF3, 0xB3, 0x8A, 0x76, 0x18 };
        private static readonly byte[] key_E92408F0 = { 0x24, 0x84, 0xBE, 0x35, 0xF0, 0xC5, 0x91, 0xA3, 0x3D, 0xA5, 0x94, 0x12, 0x8F, 0xD0, 0x4C, 0x01 };
        private static readonly byte[] key_E92408F0_xor = { 0x2A, 0x1B, 0xF2, 0xD5, 0x11, 0xF8, 0x93, 0x04, 0x9B, 0xF7, 0xB1, 0x7F, 0xC7, 0x8F, 0x6A, 0x11 };
        private static readonly byte[] key_89742B04 = { 0xD7, 0xEB, 0xC9, 0x24, 0x7E, 0x23, 0x3D, 0x89, 0x46, 0xE7, 0x2E, 0x47, 0xAD, 0xDB, 0x0D, 0x09 };
        private static readonly byte[] key_89742B04_xor = { 0xFF, 0x5E, 0xF1, 0xE9, 0xB1, 0xC9, 0x3E, 0xC5, 0xDB, 0xE0, 0x67, 0x82, 0x95, 0x3A, 0x8E, 0xA5 };

        public static int sceMesgLed_driver_CED2C075(Span<byte> modData, int size, out int newSize, ReadOnlySpan<byte> versionKey)
        {
            var ret = Decrypt(0xE92428F0, key_E92428F0, 0x65, modData, size, out newSize, 0, null, 3, key_E92428F0_xor, versionKey);
            if (ret == -0x12d)
            {
                ret = Decrypt(0xE9241EF0, key_E9241EF0, 0x65, modData, size, out newSize, 0, null, 3, key_E9241EF0_xor, versionKey);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0xE92410F0, key_E92410F0, 0x65, modData, size, out newSize, 0, null, 3, key_E92410F0_xor, versionKey);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0xE92408F0, key_E92408F0, 0x65, modData, size, out newSize, 0, null, 3, key_E92408F0_xor, versionKey);
                        if (ret == -0x12d)
                        {
                            ret = Decrypt(0x89742B04, key_89742B04, 0x65, modData, size, out newSize, 0, null, 3, key_89742B04_xor, versionKey);
                        }
                    }
                }
            }
            return ret;
        }

        private static readonly byte[] key_0DAA28F0 = { 0x81, 0x79, 0xDB, 0x6B, 0xC1, 0x04, 0x71, 0xE7, 0x64, 0x90, 0xAA, 0x71, 0xC2, 0x94, 0x92, 0x76 };
        private static readonly byte[] key_0DAA28F0_xor = { 0x3C, 0x8B, 0xAB, 0xB0, 0x07, 0x92, 0xAC, 0x1B, 0x2B, 0xCF, 0x10, 0xAA, 0xBD, 0x9F, 0x5B, 0xD8 };
        private static readonly byte[] key_0DAA1EF0 = { 0x1C, 0x3B, 0xD7, 0xA4, 0xA6, 0x41, 0x62, 0x98, 0xA7, 0xDF, 0x5B, 0x16, 0xDA, 0x53, 0x62, 0xF1 };
        private static readonly byte[] key_0DAA1EF0_xor = { 0x87, 0x66, 0x42, 0x6E, 0x6D, 0xB8, 0xC6, 0x28, 0x10, 0x7F, 0xFD, 0xBD, 0x10, 0x7E, 0x7E, 0x31 };
        private static readonly byte[] key_0DAA10F0 = { 0xA9, 0x81, 0x71, 0x9D, 0x92, 0x2D, 0xCC, 0xEE, 0x44, 0x1C, 0x0E, 0x37, 0x7A, 0xF6, 0xE2, 0x3E };
        private static readonly byte[] key_0DAA10F0_xor = { 0xD3, 0x43, 0xA8, 0x49, 0x79, 0x61, 0x82, 0x63, 0x40, 0xBF, 0xA3, 0xEF, 0xB0, 0x99, 0xED, 0x48 };
        private static readonly byte[] key_0DAA06F0 = { 0xCA, 0x26, 0x7D, 0xA2, 0xB9, 0xCE, 0x24, 0x6E, 0xFD, 0x32, 0xA8, 0x97, 0xF4, 0x7C, 0x19, 0x19 };
        private static readonly byte[] key_0DAA06F0_xor = { 0x77, 0x32, 0x20, 0x31, 0xDF, 0x7F, 0x4B, 0x1C, 0x8D, 0xD7, 0xD2, 0xC3, 0x23, 0xA9, 0xF8, 0xA9 };

        public static int sceMesgLed_driver_EBB4613D(Span<byte> modData, int size, out int newSize, ReadOnlySpan<byte> versionKey)
        {
            var ret = Decrypt(0x0DAA28F0, key_0DAA28F0, 0x65, modData, size, out newSize, 0, null, 5, key_0DAA28F0_xor, versionKey);
            if (ret == -0x12d)
            {
                ret = Decrypt(0x0DAA1EF0, key_0DAA1EF0, 0x65, modData, size, out newSize, 0, null, 5, key_0DAA1EF0_xor, versionKey);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0x0DAA10F0, key_0DAA10F0, 0x65, modData, size, out newSize, 0, null, 5, key_0DAA10F0_xor, versionKey);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0x0DAA06F0, key_0DAA06F0, 0x65, modData, size, out newSize, 0, null, 5, key_0DAA06F0_xor, versionKey);
                    }
                }
            }

            return ret;
        }

        private static readonly byte[] key_692828F0 = { 0x49, 0x85, 0x86, 0x1E, 0xB9, 0x99, 0xBD, 0xA5, 0x92, 0xE9, 0xF9, 0xD1, 0x26, 0x43, 0x7E, 0xB5 };
        private static readonly byte[] key_692828F0_xor = { 0x88, 0xD5, 0x04, 0xD5, 0xF8, 0x27, 0x24, 0x13, 0x62, 0x4B, 0xBB, 0x16, 0x44, 0x1E, 0x43, 0x50 };
        private static readonly byte[] key_69281EF0 = { 0x05, 0x94, 0x7F, 0xE2, 0x80, 0x5C, 0x7E, 0xAB, 0x03, 0x66, 0x40, 0x85, 0x3C, 0xD1, 0x2C, 0xFA };
        private static readonly byte[] key_69281EF0_xor = { 0x10, 0xCD, 0x0D, 0xD5, 0x25, 0xC6, 0x28, 0x87, 0x34, 0xC6, 0x0E, 0xBE, 0x6D, 0xE7, 0x19, 0x7D };
        private static readonly byte[] key_692810F0 = { 0xB8, 0xE7, 0xAC, 0xEE, 0x3F, 0x50, 0xB9, 0xA0, 0x66, 0xC8, 0xBD, 0x5E, 0x21, 0x53, 0xF1, 0xD5 };
        private static readonly byte[] key_692810F0_xor = { 0x21, 0x52, 0x5D, 0x76, 0xF6, 0x81, 0x0F, 0x15, 0x2F, 0x4A, 0x40, 0x89, 0x63, 0xA0, 0x10, 0x55 };
        private static readonly byte[] key_692808F0 = { 0x77, 0x66, 0xAD, 0xF8, 0x69, 0x1D, 0x04, 0x6A, 0x37, 0xFE, 0x46, 0x4C, 0xEB, 0xE2, 0x4C, 0xDC };
        private static readonly byte[] key_692808F0_xor = { 0xB6, 0x67, 0x15, 0x92, 0x49, 0x3D, 0x4D, 0x8A, 0x21, 0xE2, 0xF9, 0x0B, 0x7E, 0x24, 0x64, 0xF3 };
        private static readonly byte[] key_F5F12304 = { 0xC0, 0xF0, 0x2D, 0x65, 0xC6, 0xA6, 0x56, 0x9B, 0xB8, 0xE8, 0x0E, 0x82, 0x3B, 0x56, 0xE2, 0xA9 };
        private static readonly byte[] key_F5F12304_xor = { 0x21, 0xEA, 0xBE, 0x48, 0x63, 0xDE, 0x22, 0x4B, 0x3A, 0xDB, 0x81, 0x53, 0x30, 0x03, 0x54, 0x92 };

        public static int sceMesgLed_driver_C7D1C16B(Span<byte> modData, int size, out int newSize, ReadOnlySpan<byte> versionKey)
        {
            var ret = Decrypt(0x692828F0, key_692828F0, 0x66, modData, size, out newSize, 0, null, 3, key_692828F0_xor, versionKey);
            if (ret == -0x12d)
            {
                ret = Decrypt(0x69281EF0, key_69281EF0, 0x66, modData, size, out newSize, 0, null, 3, key_69281EF0_xor, versionKey);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0x692810F0, key_692810F0, 0x66, modData, size, out newSize, 0, null, 3, key_692810F0_xor, versionKey);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0x692808F0, key_692808F0, 0x66, modData, size, out newSize, 0, null, 3, key_692808F0_xor, versionKey);
                        if (ret == -0x12d)
                        {
                            ret = Decrypt(0xF5F12304, key_F5F12304, 0x66, modData, size, out newSize, 0, null, 3, key_F5F12304_xor, versionKey);
                        }
                    }
                }
            }
            return ret;
        }

        private static readonly byte[] key_E1ED28F0 = { 0xF2, 0x1A, 0x92, 0x0C, 0x33, 0xC2, 0x5C, 0xFF, 0x30, 0x73, 0xF6, 0x94, 0x50, 0xAD, 0x33, 0x34 };
        private static readonly byte[] key_E1ED28F0_xor = { 0xE2, 0x04, 0x7A, 0xE7, 0x51, 0x69, 0xE2, 0xF5, 0xC5, 0x94, 0xDF, 0xC9, 0x18, 0x90, 0x43, 0xC8 };
        private static readonly byte[] key_E1ED1EF0 = { 0x50, 0xAC, 0x80, 0x31, 0x36, 0x27, 0xCE, 0x39, 0x43, 0xDA, 0xC7, 0x77, 0x6A, 0x1F, 0x8B, 0x1D };
        private static readonly byte[] key_E1ED1EF0_xor = { 0x0A, 0x97, 0x2A, 0x3C, 0xAD, 0x20, 0x09, 0xAC, 0xB0, 0x72, 0xB8, 0xBF, 0x1A, 0x01, 0x42, 0x8B };
        private static readonly byte[] key_E1ED10F0 = { 0x2A, 0xCE, 0x63, 0xF9, 0xA7, 0x93, 0x2A, 0x6B, 0xF1, 0xDB, 0x41, 0x70, 0x21, 0xB7, 0x21, 0x77 };
        private static readonly byte[] key_E1ED10F0_xor = { 0xE8, 0x21, 0xA6, 0x81, 0xBC, 0xC8, 0x4A, 0x09, 0x88, 0x92, 0x78, 0x65, 0x3A, 0x3B, 0x3C, 0x4E };
        private static readonly byte[] key_E1ED06F0 = { 0x2D, 0xB6, 0x4D, 0x66, 0xCB, 0xA3, 0x8E, 0x4D, 0x13, 0x6F, 0xB1, 0x63, 0x4C, 0xCC, 0x21, 0xF2 };
        private static readonly byte[] key_E1ED06F0_xor = { 0xA5, 0xAC, 0x61, 0x8A, 0x6B, 0xD2, 0x4A, 0xC4, 0x96, 0x75, 0x3B, 0x5A, 0x8C, 0xF6, 0x46, 0x2F };

        public static int sceMesgLed_driver_66B348B2(Span<byte> modData, int size, out int newSize, ReadOnlySpan<byte> versionKey)
        {
            var ret = Decrypt(0xE1ED28F0, key_E1ED28F0, 0x66, modData, size, out newSize, 0, null, 5, key_E1ED28F0_xor, versionKey);
            if (ret == -0x12d)
            {
                ret = Decrypt(0xE1ED1EF0, key_E1ED1EF0, 0x66, modData, size, out newSize, 0, null, 5, key_E1ED1EF0_xor, versionKey);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0xE1ED10F0, key_E1ED10F0, 0x66, modData, size, out newSize, 0, null, 5, key_E1ED10F0_xor, versionKey);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0xE1ED06F0, key_E1ED06F0, 0x66, modData, size, out newSize, 0, null, 5, key_E1ED06F0_xor, versionKey);
                    }
                }
            }

            return ret;
        }

        private static readonly byte[] key_3C2A28F0 = { 0x0B, 0xFD, 0xC8, 0x94, 0xB2, 0xF1, 0x3B, 0x8B, 0x82, 0x0D, 0x1A, 0x58, 0x55, 0x15, 0x31, 0x8A };
        private static readonly byte[] key_3C2A1EF0 = { 0x19, 0x13, 0xE6, 0x04, 0x27, 0xC2, 0xB2, 0xA7, 0x99, 0x1A, 0x76, 0xA0, 0x96, 0x92, 0xA5, 0x34 };
        private static readonly byte[] key_3C2A10F0 = { 0xCB, 0x5E, 0x47, 0x46, 0x67, 0x1E, 0x66, 0xFE, 0x68, 0x92, 0xBD, 0xFB, 0xB6, 0xA9, 0xA5, 0xF0 };
        private static readonly byte[] key_3C2A08F0 = { 0x1E, 0x2E, 0x38, 0x49, 0xDA, 0xD4, 0x16, 0x08, 0x27, 0x2E, 0xF3, 0xBC, 0x37, 0x75, 0x80, 0x93 };

        public static int sceMesgLed_driver_B2D95FDF(Span<byte> modData, int size, out int newSize)
        {
            var ret = Decrypt(0x3C2A28F0, key_3C2A28F0, 0x67, modData, size, out newSize, 0, null, 2, null, null);
            if (ret == -0x12d)
            {
                ret = Decrypt(0x3C2A1EF0, key_3C2A1EF0, 0x67, modData, size, out newSize, 0, null, 2, null, null);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0x3C2A10F0, key_3C2A10F0, 0x67, modData, size, out newSize, 0, null, 2, null, null);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0x3C2A08F0, key_3C2A08F0, 0x67, modData, size, out newSize, 0, null, 2, null, null);
                    }
                }
            }

            return ret;
        }

        static int sceMesgLed_driver_C9ABD2F2(Span<byte> modData, int size, out int newSize) => sceMesgLed_driver_B2D95FDF(modData, size, out newSize);

        private static readonly byte[] key_407810F0 = { 0xAF, 0xAD, 0xCA, 0xF1, 0x95, 0x59, 0x91, 0xEC, 0x1B, 0x27, 0xD0, 0x4E, 0x8A, 0xF3, 0x3D, 0xE7 };
        private static readonly byte[] key_407810F0_xor = { 0x84, 0x7B, 0xF5, 0xFE, 0xE8, 0x4D, 0xAD, 0x7A, 0xB5, 0x06, 0x28, 0x0E, 0x09, 0xFA, 0x81, 0xE1 };

        public static int sceMesgLed_driver_91E0A9AD(Span<byte> modData, int size, out int newSize, ReadOnlySpan<byte> versionKey)
        {
            return Decrypt(0x407810F0, key_407810F0, 0x6A, modData, size, out newSize, 0, null, 5, key_407810F0_xor, versionKey);
        }

        #endregion

        #region sceResmap_driver

        private static readonly byte[] key_628928F0 = { 0x7F, 0x97, 0xE8, 0x15, 0xFB, 0x5D, 0x9A, 0xAE, 0x06, 0x2D, 0xB1, 0xD4, 0xF6, 0x28, 0x53, 0xBC };
        private static readonly byte[] key_62891EF0 = { 0x6D, 0x79, 0xC6, 0x85, 0x6E, 0x6E, 0x13, 0x82, 0x1D, 0x3A, 0xDD, 0x2C, 0x35, 0xAF, 0xC7, 0x02 };
        private static readonly byte[] key_628910F0 = { 0xBF, 0x34, 0x67, 0xC7, 0x2E, 0xB2, 0xC7, 0xDB, 0xEC, 0xB2, 0x16, 0x77, 0x15, 0x94, 0xC7, 0xC6 };

        private static readonly byte[] key_04000000 =
        {
            0xA3, 0xA0, 0x8E, 0x41, 0x82, 0xE0, 0xBC, 0xC9, 0x9D, 0x22, 0x7F, 0xC5, 0x9C, 0x3C, 0x9C, 0xBE,
            0xC3, 0x72, 0x4F, 0x60, 0x69, 0x48, 0xD2, 0x8C, 0xAD, 0x3A, 0x4A, 0xCF, 0xAF, 0x3F, 0x80, 0x31,
            0x3C, 0x8D, 0x55, 0x2A, 0xD4, 0xBF, 0x3B, 0x3A, 0x5C, 0x2F, 0xF3, 0x57, 0x20, 0xAD, 0xFF, 0x35,
            0x71, 0x4F, 0xD2, 0xD4, 0xE8, 0xC9, 0x52, 0x81, 0xA5, 0x14, 0x0E, 0xB9, 0x8E, 0xED, 0x07, 0x2F,
            0xAE, 0x7F, 0x6B, 0x54, 0x0C, 0xCD, 0x86, 0x91, 0x87, 0x84, 0x48, 0xDA, 0xED, 0xA4, 0x6B, 0xA5,
            0xB5, 0x44, 0x5A, 0x4E, 0xD5, 0x44, 0x00, 0x58, 0x4D, 0xE5, 0xE8, 0xC8, 0x62, 0x68, 0xD2, 0x39,
            0xC7, 0x25, 0x86, 0x81, 0x27, 0xFF, 0x4C, 0x5D, 0xFE, 0x50, 0xBB, 0x2D, 0xB7, 0x08, 0x5C, 0x4F,
            0x7E, 0x85, 0x14, 0x03, 0x91, 0x5A, 0x26, 0x84, 0xF5, 0xE3, 0xA2, 0x02, 0x44, 0x49, 0x96, 0x4F,
            0x66, 0x7E, 0xCF, 0xDB, 0xFE, 0xCB, 0xB4, 0x02, 0xED, 0x66, 0x6F, 0x92, 0x7F, 0x4E, 0x80, 0x63
        };

        // sceKernelWaitSema
        public static int sceResmap_driver_E5659590(Span<byte> modData, int size, out int newSize)
        {
            var ret = Decrypt(0x628928F0, key_628928F0, 0x47, modData, size, out newSize, 0, null, 2, null, null);
            if (ret == -0x12d)
            {
                ret = Decrypt(0x62891EF0, key_62891EF0, 0x47, modData, size, out newSize, 0, null, 2, null, null);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0x628910F0, key_628910F0, 0x47, modData, size, out newSize, 0, null, 2, null, null);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0x04000000, key_04000000, 0x47, modData, size, out newSize, 0, null, 0, null, null);
                    }
                }
            }
            return ret;
        }

        // sceKernelPollSema
        static int sceResmap_driver_4434E59F(Span<byte> modData, int size, out int newSize) => sceResmap_driver_E5659590(modData, size, out newSize);
        #endregion

        #region scePauth_driver

        private static readonly byte[] key_2FD312F0 = { 0xC5, 0xFB, 0x69, 0x03, 0x20, 0x7A, 0xCF, 0xBA, 0x2C, 0x90, 0xF8, 0xB8, 0x4D, 0xD2, 0xF1, 0xDE };
        private static readonly byte[] key_2FD312F0_xor = { 0xA9, 0x1E, 0xDD, 0x7B, 0x09, 0xBB, 0x22, 0xB5, 0x9D, 0xA3, 0x30, 0x69, 0x13, 0x6E, 0x0E, 0xD8 };
        private static readonly byte[] key_2FD311F0 = { 0x3A, 0x6B, 0x48, 0x96, 0x86, 0xA5, 0xC8, 0x80, 0x69, 0x6C, 0xE6, 0x4B, 0xF6, 0x04, 0x17, 0x44 };
        private static readonly byte[] key_2FD311F0_xor = { 0xA9, 0x1E, 0xDD, 0x7B, 0x09, 0xBB, 0x22, 0xB5, 0x9D, 0xA3, 0x30, 0x69, 0x13, 0x6E, 0x0E, 0xD8 };

        public static int scePauth_driver_F7AA47F6(Span<byte> modData, int size, out int newSize, ReadOnlySpan<byte> versionKey)
        {
            var ret = Decrypt(0x2FD312F0, key_2FD312F0, 0x47, modData, size, out newSize, 0, null, 5, key_2FD312F0_xor, versionKey);
            if (ret == -0x12d)
            {
                ret = Decrypt(0x2FD311F0, key_2FD311F0, 0x47, modData, size, out newSize, 0, null, 5, key_2FD311F0_xor, versionKey);
            }

            return ret;
        }

        private static readonly byte[] key_2FD313F0 = { 0xB0, 0x24, 0xC8, 0x16, 0x43, 0xE8, 0xF0, 0x1C, 0x8C, 0x30, 0x67, 0x73, 0x3E, 0x96, 0x35, 0xEF };
        private static readonly byte[] key_2FD313F0_xor = { 0xA9, 0x1E, 0xDD, 0x7B, 0x09, 0xBB, 0x22, 0xB5, 0x9D, 0xA3, 0x30, 0x69, 0x13, 0x6E, 0x0E, 0xD8 };

        static int scePauth_driver_98B83B5D(Span<byte> modData, int size, out int newSize, ReadOnlySpan<byte> versionKey)
        {
            return Decrypt(0x2FD313F0, key_2FD313F0, 0x47, modData, size, out newSize, 0, null, 5, key_2FD313F0_xor, versionKey);
        }
        #endregion

        #region sceDbman_driver

        private static readonly byte[] key_8B9B28F0 = { 0x1A, 0x16, 0xB2, 0x22, 0x5A, 0xA4, 0xF9, 0xB3, 0x55, 0x09, 0xDA, 0xA9, 0xF0, 0x4C, 0x35, 0x28 };
        private static readonly byte[] key_8B9B1EF0 = { 0x08, 0xF8, 0x9C, 0xB2, 0xCF, 0x97, 0x70, 0x9F, 0x4E, 0x1E, 0xB6, 0x51, 0x33, 0xCB, 0xA1, 0x96 };
        private static readonly byte[] key_8B9B10F0 = { 0xDA, 0xB5, 0x3D, 0xF0, 0x8F, 0x4B, 0xA4, 0xC6, 0xBF, 0x96, 0x7D, 0x0A, 0x13, 0xF0, 0xA1, 0x52 };
        private static readonly byte[] key_05000000 =
        {
            0xF3, 0x43, 0x5E, 0xDD, 0x3A, 0x41, 0xCA, 0x41, 0xC6, 0x22, 0xBD, 0x56, 0xA5, 0x42, 0x18, 0xB3,
            0x8C, 0xDB, 0xA7, 0x6F, 0x87, 0x41, 0xB2, 0x83, 0x84, 0xAB, 0x07, 0xB0, 0x3D, 0x40, 0x1B, 0xBE,
            0x06, 0x24, 0x6C, 0xD6, 0xAD, 0xEA, 0x47, 0xB6, 0xD0, 0xEE, 0x63, 0xD9, 0x75, 0x0E, 0x3A, 0x1E,
            0x80, 0xAA, 0x4E, 0x5A, 0xD9, 0xED, 0x40, 0x98, 0xD4, 0x14, 0x2D, 0xBF, 0xC5, 0xE7, 0x46, 0xA2,
            0xCD, 0xCD, 0xF2, 0x19, 0x38, 0xDC, 0x74, 0xE4, 0xC5, 0x5D, 0x51, 0xC9, 0xA5, 0xA6, 0x8C, 0x26,
            0x49, 0x36, 0x7A, 0x85, 0x05, 0x7D, 0x5B, 0x4C, 0x24, 0x9F, 0x84, 0x67, 0x23, 0x83, 0xC3, 0xEF,
            0x6C, 0x6A, 0x3C, 0xF9, 0x9F, 0x6E, 0xCA, 0x83, 0xAC, 0x98, 0xC1, 0xDD, 0xE6, 0xD4, 0x91, 0xEA,
            0x77, 0xAA, 0x95, 0xC9, 0x02, 0x03, 0x40, 0xDA, 0x40, 0xF9, 0xC7, 0x8B, 0x95, 0x60, 0xCA, 0x34,
            0x03, 0x17, 0x19, 0x0C, 0xFC, 0xE6, 0x51, 0x3D, 0xC5, 0xF3, 0xC3, 0x72, 0x2D, 0x6B, 0xC4, 0xD5
        };

        // sceKernelWaitSema sceDbmanSelect
        public static int sceDbman_driver_B2B8C3F9(Span<byte> modData, int size, out int newSize)
        {
            var ret = Decrypt(0x8B9B28F0, key_8B9B28F0, 0x48, modData, size, out newSize, 0, null, 2, null, null);
            if (ret == -0x12d)
            {
                ret = Decrypt(0x8B9B1EF0, key_8B9B1EF0, 0x48, modData, size, out newSize, 0, null, 2, null, null);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0x8B9B10F0, key_8B9B10F0, 0x48, modData, size, out newSize, 0, null, 2, null, null);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0x05000000, key_05000000, 0x48, modData, size, out newSize, 0, null, 0, null, null);
                    }
                }
            }
            return ret;
        }

        // sceKernelPollSema
        static int sceDbman_driver_34B53D46(Span<byte> modData, int size, out int newSize) => sceDbman_driver_B2B8C3F9(modData, size, out newSize);
        #endregion

        #region sceNwman_driver

        private static readonly byte[] key_5A5C28F0 = { 0x66, 0xC5, 0x9C, 0x07, 0x50, 0x73, 0xE4, 0x54, 0xE9, 0x13, 0x42, 0x47, 0xEF, 0xD1, 0x2D, 0x2A };
        private static readonly byte[] key_5A5C1EF0 = { 0x74, 0x2B, 0xB2, 0x97, 0xC5, 0x40, 0x6D, 0x78, 0xF2, 0x04, 0x2E, 0xBF, 0x2C, 0x56, 0xB9, 0x94 };
        private static readonly byte[] key_5A5C10F0 = { 0xA6, 0x66, 0x13, 0xD5, 0x85, 0x9C, 0xB9, 0x21, 0x03, 0x8C, 0xE5, 0xE4, 0x0C, 0x6D, 0xB9, 0x50 };
        private static readonly byte[] key_E42C2303 = { 0x6D, 0x79, 0xF2, 0xF6, 0x37, 0x3D, 0xB7, 0xBE, 0xA2, 0x73, 0xA1, 0xAE, 0x88, 0x70, 0xC9, 0xA3 };
        private static readonly byte[] key_06000000 =
        {
            0x84, 0x15, 0x12, 0x8C, 0xA8, 0x83, 0xD7, 0x80, 0xEF, 0x1E, 0x88, 0xDB, 0xBC, 0x61, 0x96, 0x23,
            0x2B, 0xF3, 0x88, 0xFC, 0xE5, 0x3F, 0xB5, 0xDE, 0x98, 0x5A, 0xA0, 0x6B, 0xDE, 0x0A, 0x55, 0xDB,
            0xF2, 0xF8, 0x44, 0x36, 0xEB, 0xD1, 0x94, 0x55, 0x4A, 0x39, 0x3E, 0x93, 0x7C, 0x3D, 0xE3, 0x02,
            0x12, 0x88, 0xE7, 0xF5, 0xF8, 0xF0, 0xC1, 0xEB, 0x25, 0x1B, 0x8D, 0xC6, 0xB8, 0x1E, 0x2B, 0x44,
            0xA5, 0xB7, 0x6A, 0x7E, 0xD0, 0x39, 0x46, 0x92, 0x6D, 0x71, 0xDE, 0x07, 0x97, 0xB8, 0x2F, 0x10,
            0x18, 0xBA, 0xDD, 0x53, 0xC6, 0x07, 0x2B, 0x98, 0x24, 0x8A, 0x74, 0x0D, 0x5C, 0x64, 0x5D, 0xFE,
            0x8E, 0xE7, 0x67, 0x43, 0x93, 0x96, 0xB3, 0xA1, 0xA1, 0xA8, 0xEC, 0x12, 0xC4, 0xFB, 0x58, 0x44,
            0xFC, 0x55, 0x0A, 0x9C, 0x1E, 0x30, 0x37, 0xFA, 0x54, 0x24, 0xD3, 0x03, 0xE2, 0x92, 0xBD, 0x31,
            0x23, 0x03, 0xEA, 0xF7, 0xE7, 0x73, 0xDE, 0x09, 0x3B, 0xB3, 0x83, 0x79, 0xDA, 0x17, 0x85, 0x0E
        };

        // sceKernelWaitSema
        public static int sceNwman_driver_9555D68D(Span<byte> modData, int size, out int newSize)
        {
            var ret = Decrypt(0x5A5C28F0, key_5A5C28F0, 0x49, modData, size, out newSize, 0, null, 2, null, null);
            if (ret == -0x12d)
            {
                ret = Decrypt(0x5A5C1EF0, key_5A5C1EF0, 0x49, modData, size, out newSize, 0, null, 2, null, null);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0x5A5C10F0, key_5A5C10F0, 0x49, modData, size, out newSize, 0, null, 2, null, null);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0xE42C2303, key_E42C2303, 0x49, modData, size, out newSize, 0, null, 2, null, null);
                        if (ret == -0x12d)
                        {
                            ret = Decrypt(0x06000000, key_06000000, 0x49, modData, size, out newSize, 0, null, 0, null, null);
                        }
                    }
                }
            }
            return ret;
        }
        #endregion

        #region sceMesgd_driver

        private static readonly byte[] key_D82328F0 = { 0x5D, 0xAA, 0x72, 0xF2, 0x26, 0x60, 0x4D, 0x1C, 0xE7, 0x2D, 0xC8, 0xA3, 0x2F, 0x79, 0xC5, 0x54 };
        private static readonly byte[] key_D8231EF0 = { 0x4F, 0x44, 0x5C, 0x62, 0xB3, 0x53, 0xC4, 0x30, 0xFC, 0x3A, 0xA4, 0x5B, 0xEC, 0xFE, 0x51, 0xEA };
        private static readonly byte[] key_D82310F0 = { 0x9D, 0x09, 0xFD, 0x20, 0xF3, 0x8F, 0x10, 0x69, 0x0D, 0xB2, 0x6F, 0x00, 0xCC, 0xC5, 0x51, 0x2E };
        private static readonly byte[] key_63BAB403 = { 0x02, 0x2B, 0x67, 0x21, 0xE7, 0x86, 0xAD, 0x91, 0x73, 0xBC, 0xC9, 0xDE, 0xC5, 0x7A, 0x13, 0xA4 };
        private static readonly byte[] key_0E000000 =
        {
            0xDE, 0x57, 0xB7, 0x77, 0x17, 0xDD, 0x62, 0xEE, 0x7B, 0x78, 0x03, 0x5D, 0x44, 0x86, 0xCA, 0x59,
            0x20, 0x8D, 0xF6, 0x93, 0x28, 0x93, 0x81, 0x21, 0x71, 0x4E, 0xA7, 0x86, 0xCA, 0x82, 0x24, 0x1B,
            0x58, 0xAE, 0x74, 0x5F, 0x6C, 0x01, 0x8D, 0x56, 0x32, 0x88, 0x4D, 0x9A, 0x72, 0x43, 0xA2, 0x2E,
            0x84, 0xF4, 0x0C, 0x82, 0xB9, 0x06, 0xFC, 0xFC, 0x6A, 0xFB, 0x5B, 0x8A, 0xD7, 0x9C, 0x9F, 0xBF,
            0x01, 0x0D, 0x85, 0x15, 0xBA, 0x5F, 0xED, 0x39, 0x93, 0x83, 0xC3, 0x4C, 0xAF, 0xDE, 0x3A, 0xED,
            0xBF, 0x68, 0xA7, 0x1A, 0x77, 0x8A, 0xBD, 0x89, 0x65, 0x41, 0x56, 0x46, 0xD9, 0xDB, 0x33, 0x73,
            0x81, 0x6C, 0xE8, 0x62, 0x96, 0x9B, 0x29, 0x03, 0x5A, 0xAE, 0xAF, 0x73, 0x20, 0x53, 0xA0, 0x40,
            0xE8, 0x4B, 0x66, 0x10, 0x99, 0x6A, 0xB7, 0xE5, 0x70, 0xDD, 0xE0, 0x29, 0x28, 0x24, 0x60, 0xEA,
            0x30, 0xAE, 0x42, 0x20, 0x32, 0x8D, 0x6F, 0x94, 0x71, 0x5F, 0x9E, 0xA2, 0xD5, 0x7F, 0x0C, 0x7C
        };

        // sceKernelWaitSema
        public static int sceMesgd_driver_102DC8AF(Span<byte> modData, int size, out int newSize)
        {
            var ret = Decrypt(0xD82328F0, key_D82328F0, 0x51, modData, size, out newSize, 0, null, 2, null, null);
            if (ret == -0x12d)
            {
                ret = Decrypt(0xD8231EF0, key_D8231EF0, 0x51, modData, size, out newSize, 0, null, 2, null, null);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0xD82310F0, key_D82310F0, 0x51, modData, size, out newSize, 0, null, 2, null, null);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0x63BAB403, key_63BAB403, 0x51, modData, size, out newSize, 0, null, 2, null, null);
                        if (ret == -0x12d)
                        {
                            ret = Decrypt(0x0E000000, key_0E000000, 0x51, modData, size, out newSize, 0, null, 0, null, null);
                        }
                    }
                }
            }
            return ret;
        }

        // sceKernelPollSema
        static int sceMesgd_driver_ADD0CB66(Span<byte> modData, int size, out int newSize) => sceMesgd_driver_102DC8AF(modData, size, out newSize);
        #endregion

        #region sceWmd_driver

        private static readonly byte[] key_D13B28F0 = { 0x16, 0x6F, 0x8A, 0x89, 0x93, 0x67, 0xF2, 0x47, 0xEB, 0x3D, 0xE5, 0x05, 0xB9, 0x96, 0xEA, 0xEA };
        private static readonly byte[] key_D13B1EF0 = { 0x04, 0x81, 0xA4, 0x19, 0x06, 0x54, 0x7B, 0x6B, 0xF0, 0x2A, 0x89, 0xFD, 0x7A, 0x11, 0x7E, 0x54 };
        private static readonly byte[] key_D13B10F0 = { 0xD6, 0xCC, 0x05, 0x5B, 0x46, 0x88, 0xAF, 0x32, 0x01, 0xA2, 0x42, 0xA6, 0x5A, 0x2A, 0x7E, 0x90 };
        private static readonly byte[] key_D13B08F0 = { 0x03, 0xBC, 0x7A, 0x54, 0xFB, 0x42, 0xDF, 0xC4, 0x4E, 0x1E, 0x0C, 0xE1, 0xDB, 0xF6, 0x5B, 0xF3 };
        private static readonly byte[] key_D13B06F0 = { 0xB2, 0xDB, 0x96, 0xD9, 0x8B, 0x7E, 0x13, 0x95, 0x45, 0x55, 0x1C, 0xA3, 0xCB, 0x7E, 0x2E, 0xAB };
        private static readonly byte[] key_D13B05F0 = { 0xE7, 0x47, 0x33, 0x64, 0xF9, 0x67, 0xFE, 0xDE, 0x61, 0x7C, 0xE4, 0x06, 0x13, 0x60, 0xAD, 0x6D };
        private static readonly byte[] key_1B11FD03 = { 0x71, 0x39, 0xAD, 0x80, 0xA1, 0x07, 0xDC, 0xA1, 0xE4, 0xE5, 0x59, 0x97, 0xEB, 0xB3, 0xFF, 0x48 };
        private static readonly byte[] key_862648D1 =
        {
            0x35, 0x2E, 0x7E, 0x08, 0xD6, 0x0A, 0xA8, 0xD0, 0xC2, 0xCF, 0xAB, 0x01, 0x46, 0x6C, 0x5C, 0x81,
            0xF9, 0xE5, 0xA0, 0xAD, 0x38, 0x06, 0x49, 0x19, 0x10, 0xAE, 0x2F, 0xDB, 0xE2, 0x3B, 0xAB, 0x97,
            0xD1, 0x56, 0x70, 0x9F, 0x54, 0x22, 0x07, 0x40, 0xAA, 0x37, 0x11, 0x1F, 0x78, 0x39, 0x42, 0xF2,
            0x1C, 0x44, 0xE5, 0x12, 0xA3, 0x31, 0xE9, 0xEB, 0xFC, 0xE8, 0x5E, 0x8A, 0xE7, 0xC3, 0x18, 0x7B,
            0xEC, 0xA0, 0xBB, 0xD8, 0x11, 0xFB, 0x9C, 0xFD, 0xB8, 0x17, 0xC0, 0xB6, 0x3B, 0x54, 0xAD, 0x2E,
            0xEC, 0xFA, 0x13, 0xF2, 0xFC, 0x8B, 0x91, 0x27, 0xB1, 0x43, 0x93, 0xF0, 0x72, 0x80, 0xE2, 0x50,
            0x18, 0x69, 0xF9, 0x23, 0x69, 0xDA, 0xF8, 0xC0, 0x54, 0xAA, 0x56, 0x80, 0xEE, 0x03, 0x41, 0xD0,
            0xE7, 0xF9, 0x9D, 0xDD, 0x76, 0x09, 0xBC, 0xBF, 0x96, 0xAC, 0x3E, 0x5E, 0x83, 0xA3, 0xEC, 0xCB,
            0x0F, 0xAB, 0x86, 0x9B, 0x02, 0xFC, 0x34, 0x1A, 0x06, 0x3F, 0xC8, 0xD9, 0xF0, 0x00, 0x4C, 0x17
        };
        private static readonly byte[] key_0F000000 =
        {
            0x60, 0x73, 0xFD, 0xA2, 0x2D, 0xCE, 0xF1, 0x11, 0xE3, 0x82, 0x78, 0xF5, 0x34, 0xAA, 0x9D, 0xE6,
            0xD2, 0x2D, 0x34, 0xBE, 0x55, 0xBB, 0x57, 0x7F, 0x9B, 0x63, 0x70, 0x21, 0x94, 0x31, 0xEB, 0x4F,
            0xDB, 0x97, 0xB7, 0xA3, 0x1D, 0x64, 0xE4, 0x19, 0xF7, 0x13, 0x16, 0x5E, 0xB3, 0xC9, 0x0F, 0x3E,
            0xBE, 0xC6, 0x41, 0xB5, 0x13, 0xD0, 0x7F, 0xB4, 0x55, 0x16, 0x46, 0x95, 0x22, 0x9A, 0xE6, 0xF7,
            0xAA, 0x67, 0xCB, 0xC4, 0xDD, 0xB7, 0x70, 0x67, 0x52, 0x48, 0xB3, 0x26, 0x5E, 0xA9, 0x38, 0xFF,
            0x5F, 0x62, 0xEA, 0xD4, 0x47, 0xAC, 0xD0, 0x49, 0xAB, 0xC7, 0x7C, 0x48, 0x1B, 0x83, 0x02, 0x83,
            0x30, 0x1B, 0x33, 0xC1, 0x7D, 0x0B, 0x52, 0xD4, 0xBB, 0xEA, 0x10, 0x39, 0x59, 0x3D, 0xF6, 0x96,
            0x2F, 0xF0, 0x50, 0x42, 0xF4, 0x87, 0xC4, 0xEE, 0x29, 0x98, 0x4A, 0xA7, 0x77, 0x36, 0x11, 0xAF,
            0xE7, 0xF9, 0x9C, 0x42, 0xB6, 0x3A, 0x2A, 0x78, 0x0C, 0xFE, 0x8E, 0x55, 0x82, 0x66, 0x11, 0x4A
        };

        // sceKernelWaitSema
        public static int sceWmd_driver_7A0E484C(Span<byte> modData, int size, out int newSize)
        {
            var ret = Decrypt(0xD13B28F0, key_D13B28F0, 0x52, modData, size, out newSize, 0, null, 2, null, null);
            if (ret == -0x12d)
            {
                ret = Decrypt(0xD13B1EF0, key_D13B1EF0, 0x52, modData, size, out newSize, 0, null, 2, null, null);
                if (ret == -0x12d)
                {
                    ret = Decrypt(0xD13B10F0, key_D13B10F0, 0x52, modData, size, out newSize, 0, null, 2, null, null);
                    if (ret == -0x12d)
                    {
                        ret = Decrypt(0xD13B08F0, key_D13B08F0, 0x52, modData, size, out newSize, 0, null, 2, null, null);
                        if (ret == -0x12d)
                        {
                            ret = Decrypt(0xD13B06F0, key_D13B06F0, 0x52, modData, size, out newSize, 0, null, 2, null, null);
                            if (ret == -0x12d)
                            {
                                ret = Decrypt(0xD13B05F0, key_D13B05F0, 0x52, modData, size, out newSize, 0, null, 2, null, null);
                                if (ret == -0x12d)
                                {
                                    ret = Decrypt(0x1B11FD03, key_1B11FD03, 0x52, modData, size, out newSize, 0, null, 2, null, null);
                                    if (ret == -0x12d)
                                    {
                                        ret = Decrypt(0x862648D1, key_862648D1, 0x52, modData, size, out newSize, 0, null, 1, null, null);
                                        if (ret == -0x12d)
                                        {
                                            ret = Decrypt(0x0F000000, key_0F000000, 0x52, modData, size, out newSize, 0, null, 0, null, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }

        // sceKernelPollSema
        static int sceWmd_driver_B7CE9041(Span<byte> modData, int size, out int newSize) => sceWmd_driver_7A0E484C(modData, size, out newSize);
        #endregion


        static readonly Memory<byte> mem88134300 = new byte[0x1000];
        static readonly Memory<byte> mem881343a4 = mem88134300.Slice(0xa4, 0x78);
        static readonly Memory<byte> mem881343b4 = mem88134300.Slice(0xb4, 0x30);
        static readonly Memory<byte> mem881343e4 = mem88134300.Slice(0xe4);
        static readonly Memory<byte> mem881343f4 = mem88134300.Slice(0xf4);
        static readonly Memory<byte> mem881345c0 = mem88134300.Slice(0x2c0);
        static readonly Memory<byte> mem881345c4 = mem88134300.Slice(0x2c4);
        static readonly Memory<byte> mem881345d4 = mem88134300.Slice(0x2d4);
        static readonly Memory<byte> mem881345d8 = mem88134300.Slice(0x2d8);
        static readonly Memory<byte> mem881345fc = mem88134300.Slice(0x2fc);
        static readonly Memory<byte> mem881345e8 = mem88134300.Slice(0x2e8);
        static readonly Memory<byte> mem88134604 = mem88134300.Slice(0x304);
        static readonly Memory<byte> mem88134740 = mem88134300.Slice(0x440);
        static readonly byte[] key7D20 = { 0xAC, 0x84, 0x69, 0xEB, 0x19, 0x51, 0xEE, 0xE3, 0xFD, 0x5D, 0x93, 0xF8, 0xAB, 0x97, 0x47, 0x56 };
        static readonly byte[] key7D30 = { 0x00, 0x77, 0x3A, 0x39, 0x2A, 0xFC, 0x47, 0x6D, 0x16, 0x3D, 0x43, 0x7E, 0xEF, 0xEF, 0xC3, 0x50 };
        static readonly byte[] key7D40 =
        {
            0x2E, 0x2C, 0x67, 0x81, 0xB2, 0x0D, 0xB6, 0x80, 0x7D, 0x99, 0xEB, 0x04, 0x5F, 0xC3, 0x37, 0xB0,
            0xF5, 0x44, 0x2F, 0xEF, 0xDE, 0x1F, 0x9A, 0x4A, 0x01, 0x8A, 0x2E, 0xBF, 0x82, 0x4A, 0x74, 0x95,
            0x71, 0x1F, 0xF2, 0x29, 0xCB, 0x23, 0xC2, 0x6D
        };

        public static int sceResmgr_8E6C62C8(ReadOnlySpan<byte> keyBuf)
        {
            keyBuf.CopyTo(mem881343a4.Span);
            mem881343a4[..0x40].CopyTo(mem881345c4);
            int len = 0x50;
            MemoryMarshal.Write(mem881345c0.Span, ref len);
            key7D20.CopyTo(mem88134604);
            KIRKEngine.sceUtilsBufferCopyWithRange(mem881345c0.Span, 0x54, mem881345c0.Span, 0x54, KIRKEngine.KIRK_CMD_SHA1_HASH);
            mem881345c0[..0x14].CopyTo(mem88134740);
            mem881343e4[..0x10].CopyTo(mem881345d4);
            Kirk7(mem881345c0.Span, 0x10, 0x56, 0);
            if (!mem881345c0.Span.Slice(0, 0x10).SequenceEqual(mem88134740.Span.Slice(0, 0x10)))
            {
                return -0x12e;
            }
            key7D40.CopyTo(mem881345c0);
            mem88134740[..0x14].CopyTo(mem881345e8);
            mem881343f4[..0x28].CopyTo(mem881345fc);
            var ret = KIRKEngine.sceUtilsBufferCopyWithRange(null, 0, mem881345c0.Span, 100, KIRKEngine.KIRK_CMD_ECDSA_VERIFY);
            if (ret != 0)
            {
                return -0x12f;
            }
            mem881343b4.CopyTo(mem881345d4);
            for (int i = 0; i < 0x30; i++)
            {
                mem881345d4.Span[i] ^= key7D30[i & 0xF];
            }
            ret = Kirk7(mem881345c0.Span, 0x30, 0x56, 0);
            if (ret != 0)
            {
                return -0x67;
            }
            for (int i = 0; i < 0x30; i++)
            {
                mem881345c0.Span[i] ^= key7D30[i & 0xF];
            }
            int type = 2;
            uint oldTag1, oldTag2, keyTag1, keyTag2;
            Memory<byte> key1, key2;
            if (mem881345c0.Span[0] == 0xF0 && mem881345c0.Span[1] > 0x8f)
            {
                // type4
                type = 4;
                // D91690F0
                oldTag1 = 0xD91690F0;
                keyTag1 = MemoryMarshal.Read<uint>(mem881345c0.Span);
                key1 = mem881345c4.Slice(0, 0x10);
                // 2E5E90F0
                oldTag2 = 0x2E5E90F0;
                keyTag2 = MemoryMarshal.Read<uint>(mem881345d4.Span);
                key2 = mem881345d8.Slice(0, 0x10);
            }
            else if (mem881345c0.Span[0] == 0xF0 && (sbyte)mem881345c0.Span[1] < 0)
            {
                // type3
                type = 3;
                // D91680F0
                oldTag1 = 0xD91680F0;
                keyTag1 = MemoryMarshal.Read<uint>(mem881345c0.Span);
                key1 = mem881345c4.Slice(0, 0x10);
                // 2E5E80F0
                oldTag2 = 0x2E5E80F0;
                keyTag2 = MemoryMarshal.Read<uint>(mem881345d4.Span);
                key2 = mem881345d8.Slice(0, 0x10);
            }
            else
            {
                // type1
                type = 1;
                // D91611F0
                oldTag1 = 0xD91611F0;
                keyTag1 = MemoryMarshal.Read<uint>(mem881345c0.Span);
                key1 = mem881345c4.Slice(0, 0x10);
                // 2E5E11F0
                oldTag2 = 0x2E5E11F0;
                keyTag2 = MemoryMarshal.Read<uint>(mem881345d4.Span);
                key2 = mem881345d8.Slice(0, 0x10);
            }
            Console.WriteLine($"Type: {type}");
            Console.WriteLine($"Old Tag: 0x{oldTag1:X8}, New Tag: 0x{keyTag1:X8}");
            Console.WriteLine($"Key {string.Join(", ", key1.ToArray().Select(b => $"0x{b:X2}"))}");
            Console.WriteLine($"Old Tag: 0x{oldTag2:X8}, New Tag: 0x{keyTag2:X8}");
            Console.WriteLine($"Key {string.Join(", ", key2.ToArray().Select(b => $"0x{b:X2}"))}");

            return 0;
        }

        static int Kirk4(Span<byte> data, int size, int seed, int use_polling)
        {
            KIRKEngine.KIRK_AES128CBC_HEADER hdr = new KIRKEngine.KIRK_AES128CBC_HEADER
            {
                mode = KIRKEngine.KIRK_MODE_ENCRYPT_CBC,
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
                    KIRKEngine.KIRK_CMD_ENCRYPT_IV_0);
            }
            return 0;
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


        static int BuildKeyData(int keySeed, int use_polling, int type, ReadOnlySpan<byte> key,
            ReadOnlySpan<byte> versionKey)
        {
            //Span<byte> kirkBuf = stackalloc byte[20 + 0x90];
            if ((1 < type && type < 8) || type == 9 || type == 10)
            {
                for (byte i = 0; i < 9; i++)
                {
                    key[..16].CopyTo(KirkMemory[(20 + i * 16)..].Span);
                    KirkMemory.Span[20 + i * 16] = i;
                }
            }
            else
            {
                key[..144].CopyTo(KirkMemory[20..].Span);
            }

            var ret = Kirk7(KirkMemory.Span, 0x90, keySeed, use_polling);
            if (ret != 0)
            {
                ret = -0x68;
                return ret;
            }

            if ((type == 3 || type == 5 || type == 7 || type == 10) && versionKey != null)
            {
                XorKey(KirkMemory.Span, 0x90, versionKey);
            }
            KirkMemory[..0x90].CopyTo(KeyData);
            return ret;
        }
        static int CheckBlackList(PSPHeader2 prxHdr, ReadOnlySpan<byte> blacklist)
        {
            return 0;
        }
        static void XorKeyLarge(Span<byte> buffer, int size, ReadOnlySpan<byte> key)
        {
            for (int i = 0; i < size; i++)
            {
                buffer[i] ^= key[i];
            }
        }

        static void XorKey(Span<byte> buffer, int size, ReadOnlySpan<byte> key)
        {
            XorKey(buffer, size, key, null);
        }

        static void XorKey(Span<byte> buffer, int size, ReadOnlySpan<byte> key1, ReadOnlySpan<byte> key2)
        {
            for (int i = 0; i < size; i++)
            {
                if (key2 != null)
                {
                    buffer[i] ^= key2[i & 0xf];
                }

                buffer[i] ^= key1[i & 0xf];
            }
        }

        static void XorKeyInto(Span<byte> dst, int size, ReadOnlySpan<byte> src, ReadOnlySpan<byte> key)
        {
            for (int i = 0; i < size; i++)
            {
                dst[i] = (byte)(src[i] ^ key[i]);
            }
        }

        private static readonly Memory<byte> KirkMemory = new byte[0x150]; // DAT_00009dc0
        private static readonly Memory<byte> KeyData = new byte[0x90]; // DAT_00009d00
        private static readonly Memory<byte> KirkCmd1Memory = new byte[0xb4]; // DAT_00009f40
        private static readonly Memory<byte> KirkEcdsaMemory = new byte[0x48]; // DAT_0000a040 DAT_0000a060

        private static readonly byte[] key7AE0 =
        {
            0xE3, 0x5E, 0x4E, 0x7E, 0x2F, 0xA3, 0x20, 0x96, 0x75, 0x43, 0x94, 0xA9, 0x92, 0x01, 0x83, 0xA7,
            0x85, 0xBD, 0xF6, 0x19, 0x1F, 0x44, 0x8F, 0x95, 0xE0, 0x43, 0x35, 0xA3, 0xF5, 0xE5, 0x05, 0x65,
            0x5E, 0xD7, 0x59, 0x3F, 0xC6, 0xDB, 0xAF, 0x39
        };

        private static readonly byte[] key7AB8 =
        {
            0x25, 0xDC, 0xFD, 0xE2, 0x12, 0x79, 0x89, 0x54, 0x79, 0x37, 0x13, 0x24, 0xEC, 0x25, 0x08, 0x81,
            0x57, 0xAA, 0xF1, 0xD0, 0xA4, 0x64, 0x8C, 0x15, 0x42, 0x25, 0xF6, 0x90, 0x3F, 0x44, 0xE3, 0x6A,
            0xE6, 0x64, 0x12, 0xFC, 0x80, 0x68, 0xBD, 0xC1
        };

        private static readonly byte[] key7A90 =
        {
            0x77, 0x3F, 0x4B, 0xE1, 0x4C, 0x0A, 0xB4, 0x52, 0x67, 0x2B, 0x67, 0x56, 0x82, 0x4C, 0xCF, 0x42,
            0xAA, 0x37, 0xFF, 0xC0, 0x89, 0x41, 0xE5, 0x63, 0x5E, 0x84, 0xE9, 0xFB, 0x53, 0xDA, 0x94, 0x9E,
            0x9B, 0xB7, 0xC2, 0xA4, 0x22, 0x9F, 0xDF, 0x1F
        };

        public static int Decrypt(Span<byte> modData, out int newSize, int use_polling, ReadOnlySpan<byte> xorKey2)
        {
            var hdr = MemoryMarshal.AsRef<PSPHeader2>(modData);
            if (!Ciphers.ContainsKey(hdr.tag))
            {
                newSize = 0;
                return -1;
            }
            var cipher = Ciphers[hdr.tag];
            return Decrypt(hdr.tag, cipher.Key, cipher.Seed, modData, hdr.pspSize, out newSize, use_polling, null, cipher.Type, cipher.XorKey, xorKey2);
        }

        static int Decrypt(uint tag, ReadOnlySpan<byte> key, int keySeed, Span<byte> modData, int size, out int newSize, int use_polling, ReadOnlySpan<byte> blacklist, int type, ReadOnlySpan<byte> xorKey, ReadOnlySpan<byte> xorKey2)
        {
            newSize = 0;
            int ret = -201;

            PSPHeader2 hdr; // DAT_00009bb0
                            // Span<byte> buf2 = stackalloc byte[0x150]; // DAT_00009dc0
                            // Span<byte> buf3 = stackalloc byte[0x90]; // DAT_00009d00
                            // Span<byte> buf4 = stackalloc byte[0xb4]; // DAT_00009f40
                            // Span<byte> buf5 = stackalloc byte[0x20]; // DAT_0000a040
                            // Span<byte> buf6 = stackalloc byte[0x28]; // DAT_0000a060
            KirkMemory.Span.Clear();
            KirkCmd1Memory.Span.Clear();
            KirkEcdsaMemory.Span.Clear();
            byte b_0xd4 = 0;
            if (modData == null || size == 0)
            {
                return ret; // 0xffffff37
            }
            ret = -202; // 0xffffff36
            if (size < 0x160)
            {
                return ret;
            }

            hdr = MemoryMarshal.Read<PSPHeader2>(modData);
            // DAT_00009c80
            var encTag = hdr.tag;
            if (tag != encTag)
            {
                ret = -0x12d;
                return ret;
            }

            if (type - 9 < 2 && size < 0x160)
            {
                ret = 0;
                return ret;
            }

            if ((type == 9 || type == 10) && size < 0x160)
            {
                ret = -0xca;
                return ret;
            }

            switch (type)
            {
                case 3:
                    {
                        for (int i = 0; i < 0x18; i++)
                        {
                            if (hdr.sCheck[i] != 0)
                            {
                                return -0x12e;
                            }
                        }

                        break;
                    }
                case 2:
                    {
                        for (int i = 0; i < 0x58; i++)
                        {
                            if (hdr.sCheck[i] != 0)
                            {
                                return -0x12e;
                            }
                        }
                        break;
                    }
                case 5:
                    {
                        for (int i = 1; i < 0x58; i++)
                        {
                            if (hdr.sCheck[i] != 0)
                            {
                                return -0x12e;
                            }
                        }

                        b_0xd4 = hdr.sCheck[0]; // DAT_00009c84
                        break;
                    }
                case 6:
                    {
                        for (int i = 0; i < 0x38; i++)
                        {
                            if (hdr.sCheck[i] != 0)
                            {
                                return -0x12e;
                            }
                        }

                        break;
                    }
                case 7:
                    {
                        for (int i = 1; i < 0x38; i++)
                        {
                            if (hdr.sCheck[i] != 0)
                            {
                                return -0x12e;
                            }
                        }
                        b_0xd4 = hdr.sCheck[0]; // DAT_00009c84
                        break;
                    }
                case 9:
                    {
                        for (int i = 0; i < 0x30; i++)
                        {
                            if (hdr.sCheck[i] != 0)
                            {
                                return -0x12e;
                            }
                        }

                        break;
                    }
                case 10:
                    {
                        for (int i = 1; i < 0x30; i++)
                        {
                            if (hdr.sCheck[i] != 0)
                            {
                                return -0x12e;
                            }
                        }
                        b_0xd4 = hdr.sCheck[0]; // DAT_00009c84

                        break;
                    }
            }

            if (blacklist != null && blacklist.Length > 0)
            {
                ret = CheckBlackList(hdr, blacklist);
                if (ret == 1)
                {
                    ret = -0x131;
                    return ret;
                }
            }

            var elfSize = hdr.dataSize; // DAT_00009c60 - 9bb0
            newSize = elfSize;
            if (size - 0x150 < elfSize)
            {
                ret = -0xce;
                return ret;
            }

            ret = BuildKeyData(keySeed, use_polling, type, key, xorKey2);
            if (ret != 0)
            {
                return ret;
            }


            if (type == 9 || type == 10)
            {
                Span<byte> sigSpan = stackalloc byte[48];
                Span<byte> sha1Span = stackalloc byte[32];
                modData.Slice(0x104, 0x28).CopyTo(KirkEcdsaMemory.Slice(0x20).Span); // DAT_0000a060
                modData.Slice(0x104, 0x28).Clear();
                KirkEcdsaMemory.Slice(0x20).Span.CopyTo(sigSpan);
                var tmpSize = size - 4;
                MemoryMarshal.Write(modData, ref tmpSize);
                if (use_polling == 0)
                {
                    ret = KIRKEngine.sceUtilsBufferCopyWithRange(modData, size, modData, size,
                        KIRKEngine.KIRK_CMD_SHA1_HASH);
                }
                else
                {
                    ret = -13;
                    return ret;
                }

                if (ret != 0)
                {
                    ret = -13;
                    return ret;
                }
                modData[..0x14].CopyTo(sha1Span);
                hdr.RawHdr.Slice(0, 0x20).CopyTo(modData);
                var tagBytes = BitConverter.GetBytes(hdr.tag);
                ref var ecdsaHdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_CMD17_BUFFER>(KirkCmd1Memory.Span);
                if (tagBytes[2] == 0x16)
                {
                    // key7AE0.CopyTo(buf4); // DAT_00007ae0 DAT_00009f40
                    key7AE0.CopyTo(ecdsaHdr.public_key.point); // DAT_00007ae0 DAT_00009f40
                }
                else if (tagBytes[2] == 0x5e)
                {
                    // key7AB8.CopyTo(buf4); // DAT_00007ab DAT_00009f40
                    key7AB8.CopyTo(ecdsaHdr.public_key.point); // DAT_00007ab DAT_00009f40
                }
                else
                {
                    // key7A90.CopyTo(buf4); // DAT_00007a90 DAT_00009f40
                    key7A90.CopyTo(ecdsaHdr.public_key.point);
                }
                // sha1Span.Slice(0, 0x14).CopyTo(buf4.Slice(0x28)); // DAT_00009f68 - 9f40
                sha1Span[..0x14].CopyTo(ecdsaHdr.message_hash); // DAT_00009f68 - 9f40
                                                                // sigSpan.Slice(0, 0x28).CopyTo(buf4.Slice(0x3c)); // DAT_00009f7c - 9f40
                sigSpan[..0x28].CopyTo(ecdsaHdr.signature.sig); // DAT_00009f68 - 9f40
                if (use_polling == 0)
                {
                    ret = KIRKEngine.sceUtilsBufferCopyWithRange(null, 0, KirkCmd1Memory.Span, 0x64,
                        KIRKEngine.KIRK_CMD_ECDSA_VERIFY);
                }
                else
                {
                    ret = -13;
                    return ret;
                }

                if (ret != 0)
                {
                    ret = -0x132;
                    return ret;
                }
            }

            if (type == 3)
            {
                hdr.sCheck.Slice(24, 0x40).CopyTo(KirkMemory.Span); // DAT_00009c9c - 9bb0 buf1 0xec
                KirkMemory.Slice(0x40, 0x50).Span.Clear(); // DAT_00009e00 - 9dc0 0x40
                KirkMemory.Span[0x60] = 3; // DAT_00009e20 - 9dc0 0x60
                KirkMemory.Span[0x70] = 0x50; // DAT_00009e30 - 9dc0 0x70
                hdr.keyData.CopyTo(KirkMemory.Slice(0x90).Span); // DAT_00009c30 - 9bb0 0x80 DAT_00009e50 - 9dc0 0x90
                hdr.cmacDataHash.CopyTo(KirkMemory.Slice(0xc0).Span); // DAT_00009c70 - 9bb0 0xc0 DAT_00009e80 - 9dc0 0xc0
                hdr.sha1Hash.Slice(0, 0x10).CopyTo(KirkMemory.Slice(0xd0).Span); // DAT_00009cdc - 9bb0 0x12c DAT_00009e90 - 9dc0 0xd0

                XorKey(KirkMemory.Slice(0x90).Span, 0x50, xorKey, xorKey2); // DAT_00009e50 - 9dc0 0x90
                ret = KIRKEngine.sceUtilsBufferCopyWithRange(KirkCmd1Memory.Span, 0xb4, KirkMemory.Span, 0x150, KIRKEngine.KIRK_CMD_3);
                if (ret != 0)
                {
                    ret = -14;
                    return ret;
                }

                MemoryMarshal.Write(KirkMemory.Span, ref hdr.tag);
                KirkMemory.Slice(0x04, 0x58).Span.Clear();
                hdr.keyData4.CopyTo(KirkMemory.Slice(0x5c).Span);
                hdr.sha1Hash.CopyTo(KirkMemory.Slice(0x6c).Span);
                KirkCmd1Memory[..0x10].CopyTo(KirkMemory[0x6c..]);
                KirkCmd1Memory[..0x30].CopyTo(KirkMemory[0x80..]);
                KirkCmd1Memory.Slice(0x30, 0x10).CopyTo(KirkMemory.Slice(0xb0));
                hdr.sizeInfo.CopyTo(KirkMemory.Slice(0xc0).Span);
                var hdrSpan = MemoryMarshal.CreateSpan(ref hdr, 1);
                MemoryMarshal.AsBytes(hdrSpan).Slice(0, 0x80).CopyTo(KirkMemory.Slice(0xd0).Span);
            }
            else if (type == 5 || type == 7 || type == 10)
            {
                hdr.keyData.CopyTo(KirkMemory.Slice(0x14).Span); // DAT_00009c30 - 9bb0 0x80 DAT_00009dd4 - 9dc0 0x14
                hdr.cmacDataHash.CopyTo(KirkMemory.Slice(0x44).Span); // DAT_00009c70 - 9bb0 0xC0 DAT_00009e04 - 9dc0 0x44
                hdr.sha1Hash.Slice(0, 0x10).CopyTo(KirkMemory.Slice(0x54).Span); // DAT_00009cdc - 9bb0 0x12C DAT_00009e14 - 9dc0 0x54
                XorKey(KirkMemory.Slice(0x14).Span, 0x50, xorKey, xorKey2);
                ret = Kirk7(KirkMemory.Span, 0x50, keySeed, use_polling);
                if (ret != 0)
                {
                    return -13;
                }
                KirkMemory[..0x50].CopyTo(KirkCmd1Memory); // DAT_00009dc0 DAT_00009f40
                MemoryMarshal.Write(KirkMemory.Span, ref hdr.tag); // DAT_00009c80 - 9bb0 DAT_00009dc0
                KirkMemory.Slice(4, 0x58).Span.Fill(0); // DAT_00009dc4 - 9dc0 4
                if (type == 7)
                {
                    hdr.sCheck.Slice(56, 0x20).CopyTo(KirkMemory.Slice(0x3c).Span); // DAT_00009cbc - 9bb0 0x10c DAT_00009dfc - 9dc0 0x3C
                }

                hdr.keyData4.CopyTo(KirkMemory.Slice(0x5c).Span);  // DAT_00009cf0 - 9bb0 0x140 DAT_00009e1c - 9dc0 0x5c
                hdr.sha1Hash.CopyTo(KirkMemory.Slice(0x6c).Span); // DAT_00009cdc - 9bb0 0x12C DAT_00009e2c - 9dc0 0x6c
                KirkCmd1Memory.Slice(0x40, 0x10).CopyTo(KirkMemory.Slice(0x6c)); // DAT_00009f80 - 9f40 0x40 DAT_00009e2c - 9dc0 0x6c
                KirkCmd1Memory[..0x30].CopyTo(KirkMemory[0x80..]);
                KirkCmd1Memory.Slice(0x30, 0x10).CopyTo(KirkMemory.Slice(0xb0));
                hdr.sizeInfo.CopyTo(KirkMemory.Slice(0xc0).Span); // DAT_00009c60 - 9bb0 0xB0 DAT_00009e80 - 9dc0 0xC0
                hdr.RawHdr.CopyTo(KirkMemory.Slice(0xd0).Span); // DAT_00009bb0 DAT_00009e90 - 9dc0 0xd0

                if (type == 10)
                {
                    KirkMemory.Slice(0x34, 0x28).Span.Fill(0);
                }
            }
            else if (type == 2 || type == 4 || type == 6 || type == 9)
            {
                MemoryMarshal.Write(KirkMemory.Span, ref hdr.tag);
                hdr.keyData4.CopyTo(KirkMemory.Slice(0x5c).Span);
                hdr.sha1Hash.CopyTo(KirkMemory.Slice(0x6c).Span);
                hdr.keyData.CopyTo(KirkMemory.Slice(0x80).Span);
                hdr.cmacDataHash.CopyTo(KirkMemory.Slice(0xb0).Span);
                hdr.sizeInfo.CopyTo(KirkMemory.Slice(0xc0).Span);
                hdr.RawHdr.CopyTo(KirkMemory.Slice(0xd0).Span);
                if (type == 9)
                {
                    KirkMemory.Slice(0x34, 0x28).Span.Fill(0); // DAT_00009df4 - 9dc0
                }
            }
            else
            {
                hdr.CheckData.CopyTo(KirkMemory.Span); // DAT_00009c80 DAT_00009dc0
                hdr.keyData50.CopyTo(KirkMemory.Slice(0x80).Span); // DAT_00009c30 DAT_00009e40
                hdr.RawHdr.CopyTo(KirkMemory.Slice(0xd0).Span); // DAT_00009bb0  DAT_00009e90
            }

            if (type == 1)
            {
                KirkMemory.Slice(0x10, 0xa0).CopyTo(KirkCmd1Memory.Slice(0x14)); // DAT_00009dd0 DAT_00009f54
                ret = Kirk7(KirkCmd1Memory.Span, 0xa0, keySeed, use_polling);
                if (ret != 0)
                {
                    return -15;
                }
                KirkCmd1Memory[..0xa0].CopyTo(KirkMemory[0x10..]); // DAT_00009f40 DAT_00009dd0
            }
            else if ((1 < type && type < 8) || type == 9 || type == 10)
            {
                KirkMemory.Slice(0x5c, 0x60).CopyTo(KirkCmd1Memory[0x14..]); // DAT_00009e1c DAT_00009f54

                if (type == 3 || type == 5 || type == 7 || type == 10)
                {
                    XorKey(KirkCmd1Memory.Span[0x14..], 0x60, xorKey); // DAT_00009f54
                }

                unsafe
                {
                    fixed (byte* ptr = &MemoryMarshal.GetReference(KirkCmd1Memory.Span))
                    {

                    }
                }


                ret = Kirk7(KirkCmd1Memory.Span, 0x60, keySeed, use_polling);
                if (ret != 0)
                {
                    return -5;
                }
                KirkCmd1Memory[..0x60].CopyTo(KirkMemory[0x5c..]); // DAT_00009f40 DAT_00009e1c
            }

            if ((1 < type && type < 8) || type == 9 || type == 10)
            {
                KirkMemory.Slice(0x6c, 0x14).CopyTo(KirkCmd1Memory);
                if (type == 4)
                {
                    KirkMemory[..0x67].CopyTo(KirkMemory[0x18..]); // DAT_00009dc4 DAT_00009dd8
                }
                else
                {
                    KirkMemory.Slice(0x5c, 0x10).CopyTo(KirkMemory[0x70..]); // DAT_00009e1c DAT_00009e30
                    if (type == 6 || type == 7)
                    {
                        KirkMemory.Slice(0x3c, 0x20).CopyTo(KirkEcdsaMemory); // DAT_00009dfc DAT_0000a040
                        KirkEcdsaMemory[..0x20].CopyTo(KirkMemory[0x50..]); // DAT_0000a040 DAT_00009e10
                        KirkMemory.Slice(0x18, 0x38).Span.Fill(0); // DAT_00009dd8
                    }
                    else
                    {
                        KirkMemory.Span.Slice(0x18, 0x58).Fill(0); // DAT_00009dd8
                    }

                    if (b_0xd4 == 0x80)
                    {
                        KirkMemory.Span[0x18] = 0x80; // DAT_00009dd8
                    }

                    KirkMemory[..0x4].CopyTo(KirkMemory[0x04..]); // DAT_00009dc0
                    var value = 0x14c; // sha1 hdr size 0x150 - 4
                    MemoryMarshal.Write(KirkMemory.Span, ref value);

                    KeyData[..0x10].CopyTo(KirkMemory[8..]); // DAT_00009d00 DAT_00009dc8
                }
            }
            else
            {
                KirkMemory.Slice(0x04, 0x14).CopyTo(KirkCmd1Memory); // DAT_00009dc4 DAT_00009f40
                var value = 0x14c;  // sha1 hdr size 0x150 - 4
                MemoryMarshal.Write(KirkMemory.Span, ref value);

                KeyData[..0x14].CopyTo(KirkMemory[0x4..]); // DAT_00009d00 DAT_00009dc4
            }

            if (use_polling == 0)
            {
                ret = KIRKEngine.sceUtilsBufferCopyWithRange(KirkMemory.Span, 0x150, KirkMemory.Span, 0x150,
                    KIRKEngine.KIRK_CMD_SHA1_HASH);
            }
            else
            {
                ret = -1;
                // ignore
            }
            if (ret != 0)
            {
                ret = -6;
                return ret;
            }

            if (!KirkMemory.Slice(0, 0x14).Span.SequenceEqual(KirkCmd1Memory.Slice(0, 0x14).Span))
            {
                ret = 0x12e;
                return ret;
            }

            ref var cmd1Hdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_CMD1_HEADER>(modData.Slice(0x40));
            ref var cmd1ecdsaHdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_CMD1_ECDSA_HEADER>(modData.Slice(0x40));

            if ((1 < type && type < 8) || type == 9 || type == 10)
            {
                XorKeyLarge(KirkMemory[0x80..].Span, 0x40, KeyData[16..].Span); // DAT_00009e40 - 9dc0 0x80 DAT_00009d10 - 9d00 0x10
                var k4 = KirkMemory.Slice(0x70, 0x10).Span;
                unsafe
                {
                    fixed (byte* ptr = &MemoryMarshal.GetReference(KirkMemory[0x6c..].Span))
                    {

                    }
                }
                ret = Kirk7(KirkMemory[0x6c..].Span, 0x40, keySeed, use_polling);
                if (ret != 0)
                {
                    ret = -7;
                    return ret;
                }
                unsafe
                {
                    fixed (byte* ptr = &MemoryMarshal.GetReference(modData[0x40..]))
                    {

                    }
                }
                XorKeyInto(modData[0x40..], 0x40, KirkMemory[108..].Span, KeyData[80..].Span);


                if (type == 6 || type == 7)
                {
                    KirkEcdsaMemory[..0x20].Span.CopyTo(modData[0x80..]); // DAT_0000a040
                    modData.Slice(0xa0, 0x10).Clear();
                    // modData[0xa4] = 1; // cmd1 ecdsa_hash
                    cmd1ecdsaHdr.ecdsa_hash = 1;
                    // modData[0xa0] = 1; // cmd1 mode
                    cmd1ecdsaHdr.mode = 1;
                    KirkMemory.Slice(0xc0, 0x10).Span.CopyTo(modData.Slice(0xb0));
                    modData.Slice(0xc0, 0x10).Clear(); // DAT_00009e80
                    KirkMemory.Slice(0xd0, 0x80).Span.CopyTo(modData.Slice(0xd0)); // DAT_00009e90
                }
                else
                {
                    // modData.Slice(0x80, 0x30).Fill(0);
                    cmd1Hdr.content.Slice(0, 0x30).Clear();
                    // modData[0xa0] = 1; // cmd1 mode
                    cmd1Hdr.mode = 1; // cmd1 mode
                                      // KirkMemory.Slice(0xc0, 0x10).Span.CopyTo(modData.Slice(0xb0)); // DAT_00009e80
                    KirkMemory.Slice(0xc0, 0x10).Span.CopyTo(cmd1Hdr.off70);  // DAT_00009e80
                    modData.Slice(0xc0, 0x10).Clear();
                    KirkMemory.Slice(0xd0, 0x80).Span.CopyTo(modData.Slice(0xd0)); // psp hdr size 0x80
                }
            }
            else
            {
                XorKeyLarge(KirkMemory.Slice(0x40).Span, 0x70, KeyData.Slice(0x14).Span); // DAT_00009e00 DAT_00009d14
                ret = Kirk7(KirkMemory.Slice(0x2c).Span, 0x70, keySeed, use_polling); // DAT_00009dec
                if (ret != 0)
                {
                    ret = -16;
                    return ret;
                }
                XorKeyInto(modData.Slice(0x40), 0x70, KirkMemory.Slice(0x2c).Span, KeyData.Slice(0x20).Span); // DAT_00009dec DAT_00009d20
                KirkMemory.Slice(0xb0, 0xa0).Span.CopyTo(modData.Slice(0xb0)); // DAT_00009e70
                if (type == 8 && cmd1ecdsaHdr.ecdsa_hash != 1)
                {
                    ret = -0x12f;
                    return ret;
                }
            }

            if (b_0xd4 == 0x80)
            {
                if (modData[0x590] != 0)
                {
                    ret = -0x12e;
                    return ret;
                }
                modData[0x590] = 0x80;
            }

            if (use_polling == 0)
            {
                ret = KIRKEngine.sceUtilsBufferCopyWithRange(modData, size, modData[0x40..],
                    size - 0x40, KIRKEngine.KIRK_CMD_DECRYPT_PRIVATE);
            }
            else
            {
                //ignore 
                ret = -5;
                return ret;
            }

            return ret;
        }



        private const int EI_NIDENT = 16;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct Elf32_Ehdr
        {
            public fixed byte e_ident[EI_NIDENT];
            public Elf32_Half e_type;
            public Elf32_Half e_machine;
            public Elf32_Word e_version;
            public Elf32_Addr e_entry;  /* Entry point */
            public Elf32_Off e_phoff;
            public Elf32_Off e_shoff;
            public Elf32_Word e_flags;
            public Elf32_Half e_ehsize;
            public Elf32_Half e_phentsize;
            public Elf32_Half e_phnum;
            public Elf32_Half e_shentsize;
            public Elf32_Half e_shnum;
            public Elf32_Half e_shstrndx;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Elf32_Shdr
        {
            public Elf32_Word sh_name;
            public Elf32_Word sh_type;
            public Elf32_Word sh_flags;
            public Elf32_Addr sh_addr;
            public Elf32_Off sh_offset;
            public Elf32_Word sh_size;
            public Elf32_Word sh_link;
            public Elf32_Word sh_info;
            public Elf32_Word sh_addralign;
            public Elf32_Word sh_entsize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Elf32_Phdr
        {
            public Elf32_Word p_type;
            public Elf32_Off p_offset;
            public Elf32_Addr p_vaddr;
            public Elf32_Addr p_paddr;
            public Elf32_Word p_filesz;
            public Elf32_Word p_memsz;
            public Elf32_Word p_flags;
            public Elf32_Word p_align;
        }

        private const int SCE_MODULE_NAME_LEN = 27;
        private const int SCE_MODULE_MAX_SEGMENTS = 4;
        private const uint PT_LOAD = 1;

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct SceModuleInfo
        {
            public ushort modattribute;
            private fixed byte _modversion[2];

            public Span<byte> modversion
            {
                get
                {
                    fixed (byte* ptr = _modversion)
                    {
                        return new Span<byte>(ptr, 2);
                    }
                }
            }
            private fixed byte _modname[SCE_MODULE_NAME_LEN];

            public Span<byte> modname
            {
                get
                {
                    fixed (byte* ptr = _modname)
                    {
                        return new Span<byte>(ptr, SCE_MODULE_NAME_LEN);
                    }
                }
            }
            public byte terminal;
            public uint gp_value;
            public uint ent_top;
            public uint ent_end;
            public uint stub_top;
            public uint stub_end;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PspModuleExport
        {
            public uint name;
            public uint flags;
            public byte entry_len;
            public byte var_count;
            public ushort func_count;
            public uint exports;
        }

        private static Elf32_Shdr? FindSection(ReadOnlySpan<Elf32_Shdr> selections, ReadOnlySpan<byte> strTable, string name)
        {
            var nameSpan = Encoding.UTF8.GetBytes(name).AsSpan();
            foreach (var selection in selections)
            {
                if (nameSpan.SequenceEqual(strTable.Slice((int)selection.sh_name, name.Length)))
                {
                    return selection;
                }
            }

            return null;
        }

        private static void GenDecrypt(Span<byte> buff)
        {
            var hdr = MemoryMarshal.AsRef<PSPHeader2>(buff);
            switch (hdr.decryptMode)
            {
                case SceExecFileDecryptMode.DECRYPT_MODE_NO_EXEC:
                    break;
                case SceExecFileDecryptMode.DECRYPT_MODE_DEMO_EXEC:
                case SceExecFileDecryptMode.DECRYPT_MODE_KERNEL_MODULE:
                    break;
            }
        }

        struct Cipher
        {
            public byte[] Key;
            public int Seed;
            public int Type;
            public byte[] XorKey;
        }


        private static readonly Dictionary<uint, Cipher> Ciphers = new Dictionary<uint, Cipher>
        {
            {0x00000000, new Cipher {Key = SceMemlmd.key_00000000, Seed = 0x42, Type = 0} },
            {0x01000000, new Cipher {Key = SceMemlmd.key_01000000, Seed = 0x43, Type = 0} },
            {0x4C940AF0, new Cipher {Key = SceMemlmd.key_4C940AF0, Seed = 0x43, Type = 2} },
            {0x4C940BF0, new Cipher {Key = SceMemlmd.key_4C940BF0, Seed = 0x43, Type = 2} },
            {0x4C9410F0, new Cipher {Key = SceMemlmd.key_4C9410F0, Seed = 0x43, Type = 2} },
            {0x4C9412F0, new Cipher {Key = SceMemlmd.key_4C9412F0, Seed = 0x43, Type = 2} },
            {0x4C9413F0, new Cipher {Key = SceMemlmd.key_4C9413F0, Seed = 0x43, Type = 2} },
            {0x4C9414F0, new Cipher {Key = SceMemlmd.key_4C9414F0, Seed = 0x43, Type = 2} },
            {0x4C9415F0, new Cipher {Key = SceMemlmd.key_4C9415F0, Seed = 0x43, Type = 2} },
            {0x4C9490F0, new Cipher {Key = SceMemlmd.key_4C9490F0, Seed = 0x43, Type = 9} },
            {0x4C9491F0, new Cipher {Key = SceMemlmd.key_4C9491F0, Seed = 0x43, Type = 9} },
            {0x4C9494F0, new Cipher {Key = SceMemlmd.key_4C9494F0, Seed = 0x43, Type = 9} },
            {0x4C949AF0, new Cipher {Key = SceMemlmd.key_4C949AF0, Seed = 0x43, Type = 9} },
            {0x4C949BF0, new Cipher {Key = SceMemlmd.key_4C949BF0, Seed = 0x43, Type = 9} },
            {0x4C949CF0, new Cipher {Key = SceMemlmd.key_4C949CF0, Seed = 0x43, Type = 9} },
            {0x4C949DF0, new Cipher {Key = SceMemlmd.key_4C949DF0, Seed = 0x43, Type = 9} },
            {0x4C949EF0, new Cipher {Key = SceMemlmd.key_4C949EF0, Seed = 0x43, Type = 9} },
            {0x4C949FF0, new Cipher {Key = SceMemlmd.key_4C949FF0, Seed = 0x43, Type = 9} },
            {0x4C94A0F0, new Cipher {Key = SceMemlmd.key_4C94A0F0, Seed = 0x43, Type = 9} },
            {0x4C94A1F0, new Cipher {Key = SceMemlmd.key_4C94A1F0, Seed = 0x43, Type = 9} },

            {0x04000000, new Cipher {Key = key_04000000, Seed = 0x47, Type = 0} },
            {0x628910F0, new Cipher {Key = key_628910F0, Seed = 0x47, Type = 2} },
            {0x62891EF0, new Cipher {Key = key_62891EF0, Seed = 0x47, Type = 2} },
            {0x628928F0, new Cipher {Key = key_628928F0, Seed = 0x47, Type = 2} },

            {0x2FD311F0, new Cipher {Key = key_2FD311F0, Seed = 0x47, Type = 5, XorKey = key_2FD311F0_xor} },
            {0x2FD312F0, new Cipher {Key = key_2FD312F0, Seed = 0x47, Type = 5, XorKey = key_2FD312F0_xor} },
            {0x2FD313F0, new Cipher {Key = key_2FD313F0, Seed = 0x47, Type = 5, XorKey = key_2FD313F0_xor} },

            {0x05000000, new Cipher {Key = key_05000000, Seed = 0x48, Type = 0} },
            {0x8B9B10F0, new Cipher {Key = key_8B9B10F0, Seed = 0x48, Type = 2} },
            {0x8B9B1EF0, new Cipher {Key = key_8B9B1EF0, Seed = 0x48, Type = 2} },
            {0x8B9B28F0, new Cipher {Key = key_8B9B28F0, Seed = 0x48, Type = 2} },

            {0x2E5E10F0, new Cipher {Key = key_2E5E10F0, Seed = 0x48, Type = 5, XorKey = key_2E5E10F0_xor} },
            {0x2E5E11F0, new Cipher {Key = key_2E5E11F0, Seed = 0x48, Type = 5, XorKey = key_2E5E11F0_xor} },
            {0x2E5E12F0, new Cipher {Key = key_2E5E12F0, Seed = 0x48, Type = 5, XorKey = key_2E5E11F0_xor} },
            {0x2E5E13F0, new Cipher {Key = key_2E5E13F0, Seed = 0x48, Type = 5, XorKey = key_2E5E11F0_xor} },
            {0x2E5E80F0, new Cipher {Key = key_2E5E80F0, Seed = 0x48, Type = 7, XorKey = key_2E5E80F0_xor} },
            {0x2E5E90F0, new Cipher {Key = key_2E5E90F0, Seed = 0x48, Type = 10, XorKey = key_2E5E90F0_xor} },

            {0x06000000, new Cipher {Key = key_06000000, Seed = 0x49, Type = 0} },
            {0xE42C2303, new Cipher {Key = key_E42C2303, Seed = 0x49, Type = 2} },
            {0x5A5C10F0, new Cipher {Key = key_5A5C10F0, Seed = 0x49, Type = 2} },
            {0x5A5C1EF0, new Cipher {Key = key_5A5C1EF0, Seed = 0x49, Type = 2} },
            {0x5A5C28F0, new Cipher {Key = key_5A5C28F0, Seed = 0x49, Type = 2} },

            {0x0A000000, new Cipher {Key = key_0A000000, Seed = 0x4D, Type = 0} },
            {0xEFD210F0, new Cipher {Key = key_EFD210F0, Seed = 0x4D, Type = 2} },
            {0xEFD21EF0, new Cipher {Key = key_EFD21EF0, Seed = 0x4D, Type = 2} },
            {0xEFD228F0, new Cipher {Key = key_EFD228F0, Seed = 0x4D, Type = 2} },

            {0x0B000000, new Cipher {Key = key_0B000000, Seed = 0x4E, Type = 8} },

            {0x0E000000, new Cipher {Key = key_0E000000, Seed = 0x51, Type = 0} },
            {0x63BAB403, new Cipher {Key = key_63BAB403, Seed = 0x51, Type = 2} },
            {0xD82310F0, new Cipher {Key = key_D82310F0, Seed = 0x51, Type = 2} },
            {0xD8231EF0, new Cipher {Key = key_D8231EF0, Seed = 0x51, Type = 2} },
            {0xD82328F0, new Cipher {Key = key_D82328F0, Seed = 0x51, Type = 2} },

            {0x0F000000, new Cipher {Key = key_0F000000, Seed = 0x52, Type = 0} },
            {0x862648D1, new Cipher {Key = key_862648D1, Seed = 0x52, Type = 1} },
            {0x1B11FD03, new Cipher {Key = key_1B11FD03, Seed = 0x52, Type = 2} },
            {0xD13B05F0, new Cipher {Key = key_D13B05F0, Seed = 0x52, Type = 2} },
            {0xD13B06F0, new Cipher {Key = key_D13B06F0, Seed = 0x52, Type = 2} },
            {0xD13B08F0, new Cipher {Key = key_D13B08F0, Seed = 0x52, Type = 2} },
            {0xD13B10F0, new Cipher {Key = key_D13B10F0, Seed = 0x52, Type = 2} },
            {0xD13B1EF0, new Cipher {Key = key_D13B1EF0, Seed = 0x52, Type = 2} },
            {0xD13B28F0, new Cipher {Key = key_D13B28F0, Seed = 0x52, Type = 2} },

            {0x4467415D, new Cipher {Key = SceMemlmd.key_4467415D, Seed = 0x59, Type = 1} },

            {0x02000000, new Cipher {Key = key_02000000, Seed = 0x45, Type = 0} },
            {0x380290F0, new Cipher {Key = key_380290F0, Seed = 0x5A, Type = 9} },
            {0x380291F0, new Cipher {Key = key_380291F0, Seed = 0x5A, Type = 9} },
            {0x380292F0, new Cipher {Key = key_380292F0, Seed = 0x5A, Type = 9} },
            {0x380293F0, new Cipher {Key = key_380293F0, Seed = 0x5A, Type = 9} },
            {0x38029AF0, new Cipher {Key = key_38029AF0, Seed = 0x5A, Type = 9} },

            {0x03000000, new Cipher {Key = key_03000000, Seed = 0x46, Type = 0} },
            {0x3ACE4DCE, new Cipher {Key = key_3ACE4DCE, Seed = 0x5B, Type = 1} },
            {0x76202403, new Cipher {Key = key_76202403, Seed = 0x5B, Type = 2} },
            {0x457B05F0, new Cipher {Key = key_457B05F0, Seed = 0x5B, Type = 2} },
            {0x457B06F0, new Cipher {Key = key_457B06F0, Seed = 0x5B, Type = 2} },
            {0x457B08F0, new Cipher {Key = key_457B08F0, Seed = 0x5B, Type = 2} },
            {0x457B0AF0, new Cipher {Key = key_457B0AF0, Seed = 0x5B, Type = 2} },
            {0x457B0BF0, new Cipher {Key = key_457B0BF0, Seed = 0x5B, Type = 2} },
            {0x457B0CF0, new Cipher {Key = key_457B0CF0, Seed = 0x5B, Type = 2} },
            {0x457B10F0, new Cipher {Key = key_457B10F0, Seed = 0x5B, Type = 2} },
            {0x457B1EF0, new Cipher {Key = key_457B1EF0, Seed = 0x5B, Type = 2} },
            {0x457B28F0, new Cipher {Key = key_457B28F0, Seed = 0x5B, Type = 2} },
            {0x457B80F0, new Cipher {Key = key_457B80F0, Seed = 0x5B, Type = 6} },
            {0x457B8AF0, new Cipher {Key = key_457B8AF0, Seed = 0x5B, Type = 6} },
            {0x457B90F0, new Cipher {Key = key_457B90F0, Seed = 0x5B, Type = 9} },
            {0x457B91F0, new Cipher {Key = key_457B91F0, Seed = 0x5B, Type = 9} },
            {0x457B92F0, new Cipher {Key = key_457B92F0, Seed = 0x5B, Type = 9} },
            {0x457B93F0, new Cipher {Key = key_457B93F0, Seed = 0x5B, Type = 9} },
            {0x457B9AF0, new Cipher {Key = key_457B9AF0, Seed = 0x5B, Type = 9} },

            {0x08000000, new Cipher {Key = key_08000000, Seed = 0x4B, Type = 0} },
            {0xC0CB167C, new Cipher {Key = key_C0CB167C, Seed = 0x5D, Type = 1} },
            {0x8004FD03, new Cipher {Key = key_8004FD03, Seed = 0x5D, Type = 2} },
            {0xD91605F0, new Cipher {Key = key_D91605F0, Seed = 0x5D, Type = 2} },
            {0xD91606F0, new Cipher {Key = key_D91606F0, Seed = 0x5D, Type = 2} },
            {0xD9160AF0, new Cipher {Key = key_D9160AF0, Seed = 0x5D, Type = 2} },
            {0xD9160BF0, new Cipher {Key = key_D9160BF0, Seed = 0x5D, Type = 2} },
            {0xD91610F0, new Cipher {Key = key_D91610F0, Seed = 0x5D, Type = 2} },
            {0xD91611F0, new Cipher {Key = key_D91611F0, Seed = 0x5D, Type = 2} },
            {0xD91612F0, new Cipher {Key = key_D91612F0, Seed = 0x5D, Type = 2} },
            {0xD91613F0, new Cipher {Key = key_D91613F0, Seed = 0x5D, Type = 2} },
            {0xD91614F0, new Cipher {Key = key_D91614F0, Seed = 0x5D, Type = 2} },
            {0xD91617F0, new Cipher {Key = key_D91617F0, Seed = 0x5D, Type = 2} },
            {0xD91618F0, new Cipher {Key = key_D91618F0, Seed = 0x5D, Type = 2} },
            {0xD9161AF0, new Cipher {Key = key_D9161AF0, Seed = 0x5D, Type = 2} },
            {0xD9161EF0, new Cipher {Key = key_D9161EF0, Seed = 0x5D, Type = 2} },
            {0xD91628F0, new Cipher {Key = key_D91628F0, Seed = 0x5D, Type = 2} },
            {0xD91680F0, new Cipher {Key = key_D91680F0, Seed = 0x5D, Type = 6} },
            {0xD91681F0, new Cipher {Key = key_D91681F0, Seed = 0x5D, Type = 6} },
            {0xD91690F0, new Cipher {Key = key_D91690F0, Seed = 0x5D, Type = 9} },

            {0x09000000, new Cipher {Key = key_09000000, Seed = 0x4C, Type = 0} },
            {0xBB67C59F, new Cipher {Key = key_BB67C59F, Seed = 0x5E, Type = 1} },
            {0x0A35EA03, new Cipher {Key = key_0A35EA03, Seed = 0x5E, Type = 2} },
            {0x7B0505F0, new Cipher {Key = key_7B0505F0, Seed = 0x5E, Type = 2} },
            {0x7B0506F0, new Cipher {Key = key_7B0506F0, Seed = 0x5E, Type = 2} },
            {0x7B0508F0, new Cipher {Key = key_7B0508F0, Seed = 0x5E, Type = 2} },
            {0x7B0510F0, new Cipher {Key = key_7B0510F0, Seed = 0x5E, Type = 2} },
            {0x7B051EF0, new Cipher {Key = key_7B051EF0, Seed = 0x5E, Type = 2} },
            {0x7B0528F0, new Cipher {Key = key_7B0528F0, Seed = 0x5E, Type = 2} },

            {0x0C000000, new Cipher {Key = key_0C000000, Seed = 0x4F, Type = 0} },
            {0x7F24BDCD, new Cipher {Key = key_7F24BDCD, Seed = 0x60, Type = 1} },
            {0xD67B3303, new Cipher {Key = key_D67B3303, Seed = 0x60, Type = 2} },
            {0xADF305F0, new Cipher {Key = key_ADF305F0, Seed = 0x60, Type = 4} },
            {0xADF306F0, new Cipher {Key = key_ADF306F0, Seed = 0x60, Type = 4} },
            {0xADF308F0, new Cipher {Key = key_ADF308F0, Seed = 0x60, Type = 4} },
            {0xADF310F0, new Cipher {Key = key_ADF310F0, Seed = 0x60, Type = 4} },
            {0xADF31EF0, new Cipher {Key = key_ADF31EF0, Seed = 0x60, Type = 4} },
            {0xADF328F0, new Cipher {Key = key_ADF328F0, Seed = 0x60, Type = 4} },

            {0x0D000000, new Cipher {Key = key_0D000000, Seed = 0x50, Type = 0} },
            {0x1BC8D12B, new Cipher {Key = key_1BC8D12B, Seed = 0x61, Type = 1} },
            {0xD66DF703, new Cipher {Key = key_D66DF703, Seed = 0x61, Type = 2} },
            {0x279D05F0, new Cipher {Key = key_279D05F0, Seed = 0x61, Type = 4} },
            {0x279D06F0, new Cipher {Key = key_279D06F0, Seed = 0x61, Type = 4} },
            {0x279D08F0, new Cipher {Key = key_279D08F0, Seed = 0x61, Type = 4} },
            {0x279D10F0, new Cipher {Key = key_279D10F0, Seed = 0x61, Type = 4} },
            {0x279D1EF0, new Cipher {Key = key_279D1EF0, Seed = 0x61, Type = 4} },
            {0x279D28F0, new Cipher {Key = key_279D28F0, Seed = 0x61, Type = 4} },

            {0x16D59E03, new Cipher {Key = SceMemlmd.key_16D59E03, Seed = 0x62, Type = 2} },
            {0xCFEF05F0, new Cipher {Key = SceMemlmd.key_CFEF05F0, Seed = 0x62, Type = 2} },
            {0xCFEF06F0, new Cipher {Key = SceMemlmd.key_CFEF06F0, Seed = 0x62, Type = 2} },
            {0xCFEF08F0, new Cipher {Key = SceMemlmd.key_CFEF08F0, Seed = 0x62, Type = 2} },

            {0x0DAA06F0, new Cipher {Key = key_0DAA06F0, Seed = 0x65, Type = 5, XorKey = key_0DAA06F0_xor} },
            {0x0DAA10F0, new Cipher {Key = key_0DAA10F0, Seed = 0x65, Type = 5, XorKey = key_0DAA10F0_xor} },
            {0x0DAA1EF0, new Cipher {Key = key_0DAA1EF0, Seed = 0x65, Type = 5, XorKey = key_0DAA1EF0_xor} },
            {0x0DAA28F0, new Cipher {Key = key_0DAA28F0, Seed = 0x65, Type = 5, XorKey = key_0DAA28F0_xor} },

            {0x89742B04, new Cipher {Key = key_89742B04, Seed = 0x65, Type = 3, XorKey = key_89742B04_xor} },
            {0xE92408F0, new Cipher {Key = key_E92408F0, Seed = 0x65, Type = 3, XorKey = key_E92408F0_xor} },
            {0xE92410F0, new Cipher {Key = key_E92410F0, Seed = 0x65, Type = 3, XorKey = key_E92410F0_xor} },
            {0xE9241EF0, new Cipher {Key = key_E9241EF0, Seed = 0x65, Type = 3, XorKey = key_E9241EF0_xor} },
            {0xE92428F0, new Cipher {Key = key_E92428F0, Seed = 0x65, Type = 3, XorKey = key_E92428F0_xor} },

            {0xE1ED06F0, new Cipher {Key = key_E1ED06F0, Seed = 0x66, Type = 5, XorKey = key_E1ED06F0_xor} },
            {0xE1ED10F0, new Cipher {Key = key_E1ED10F0, Seed = 0x66, Type = 5, XorKey = key_E1ED10F0_xor} },
            {0xE1ED1EF0, new Cipher {Key = key_E1ED1EF0, Seed = 0x66, Type = 5, XorKey = key_E1ED1EF0_xor} },
            {0xE1ED28F0, new Cipher {Key = key_E1ED28F0, Seed = 0x66, Type = 5, XorKey = key_E1ED28F0_xor} },

            {0xF5F12304, new Cipher {Key = key_F5F12304, Seed = 0x66, Type = 3, XorKey = key_F5F12304_xor} },
            {0x692808F0, new Cipher {Key = key_692808F0, Seed = 0x66, Type = 3, XorKey = key_692808F0_xor} },
            {0x692810F0, new Cipher {Key = key_692810F0, Seed = 0x66, Type = 3, XorKey = key_692810F0_xor} },
            {0x69281EF0, new Cipher {Key = key_69281EF0, Seed = 0x66, Type = 3, XorKey = key_69281EF0_xor} },
            {0x692828F0, new Cipher {Key = key_692828F0, Seed = 0x66, Type = 3, XorKey = key_692828F0_xor} },

            {0x3C2A08F0, new Cipher {Key = key_3C2A08F0, Seed = 0x67, Type = 2} },
            {0x3C2A10F0, new Cipher {Key = key_3C2A10F0, Seed = 0x67, Type = 2} },
            {0x3C2A1EF0, new Cipher {Key = key_3C2A1EF0, Seed = 0x67, Type = 2} },
            {0x3C2A28F0, new Cipher {Key = key_3C2A28F0, Seed = 0x67, Type = 2} },

            {0x407810F0, new Cipher {Key = key_407810F0, Seed = 0x6A, Type = 5, XorKey = key_407810F0_xor} },
        };

        public static int Encrypt(Span<byte> encData, ReadOnlySpan<byte> modData, uint tag, SceExecFileDecryptMode cryptType, ReadOnlySpan<byte> versionKey, string contentId) => Encrypt(encData, modData, tag, cryptType, versionKey, contentId, Array.Empty<byte>());

        public static int Encrypt(Span<byte> encData, ReadOnlySpan<byte> modData, uint tag, SceExecFileDecryptMode cryptType, ReadOnlySpan<byte> versionKey, string contentId, ReadOnlySpan<byte> config)
        {
            int ret = -1;
            if (!Ciphers.ContainsKey(tag))
            {
                return ret;
            }
            var elfHdr = MemoryMarshal.AsRef<Elf32_Ehdr>(modData);
            //var pspHdrSize = Marshal.SizeOf<PSPHeader2>();
            const int pspHdrSize = 0xD0;
            var selection =
                MemoryMarshal.Cast<byte, Elf32_Shdr>(modData.Slice((int)elfHdr.e_shoff,
                    elfHdr.e_shentsize * elfHdr.e_shnum));
            var programHeader =
                MemoryMarshal.Cast<byte, Elf32_Phdr>(modData.Slice((int)elfHdr.e_phoff,
                    elfHdr.e_phentsize * elfHdr.e_phnum));
            ref var pspHdr = ref MemoryMarshal.AsRef<PSPHeader2>(encData);
            pspHdr.magic = 0x5053507E;
            //pspHdr.modAttribute = 0x0200;
            pspHdr.moduleVerLo = 1;
            pspHdr.moduleVerHi = 1;
            pspHdr.modVersion = 1;
            // pspHdr.nSegments = 2;
            pspHdr.elfSize = modData.Length;
            pspHdr.bootEntry = elfHdr.e_entry;
            // pspHdr.modInfoOffset = 0x2940; 
            pspHdr.decryptMode = cryptType;

            Elf32_Shdr? sh = null;
            SceModuleInfo modInfo;
            if (selection.Length > 0)
            {
                var strSec = selection[elfHdr.e_shstrndx];
                var strTable = modData.Slice((int)strSec.sh_offset, (int)strSec.sh_size);
                sh = FindSection(selection, strTable, ".rodata.sceModuleInfo");
            }
            if (sh.HasValue)
            {
                pspHdr.modInfoOffset = (int)sh.Value.sh_offset;
                modInfo = MemoryMarshal.Read<SceModuleInfo>(modData.Slice((int)sh.Value.sh_offset));
            }
            else
            {
                var ph = programHeader[0];
                pspHdr.modInfoOffset = (int)ph.p_paddr;
                modInfo = MemoryMarshal.Read<SceModuleInfo>(modData.Slice((int)ph.p_paddr));
            }
            var ph0 = programHeader[0];
            var exports = MemoryMarshal.Read<PspModuleExport>(modData.Slice((int)(ph0.p_offset + modInfo.ent_top - ph0.p_vaddr)));
            if (exports.var_count > 0)
            {
                var total = exports.var_count + exports.func_count;
                var expNids = MemoryMarshal.Cast<byte, uint>(modData.Slice((int)(ph0.p_offset + exports.exports - ph0.p_vaddr), total * 4));
                var expPtrs = MemoryMarshal.Cast<byte, uint>(modData.Slice((int)(ph0.p_offset + exports.exports - ph0.p_vaddr + total * 4), total * 4));
                for (var i = exports.func_count; i < total; i++)
                {
                    if (expNids[i] == 0x11B97506)
                    {
                        var devkitVersion = MemoryMarshal.Read<uint>(modData.Slice((int)(ph0.p_offset + expPtrs[i] - ph0.p_vaddr)));
                        if (devkitVersion != 0x0c000031)
                        {
                            pspHdr.devkitVersion = devkitVersion;
                        }
                        break;
                    }
                }
            }

            pspHdr.modAttribute = modInfo.modattribute;

            byte j = 0;


            for (var i = 0; i < elfHdr.e_phnum; i++)
            {
                if (programHeader[i].p_type == PT_LOAD)
                {
                    if (j > SCE_MODULE_MAX_SEGMENTS)
                    {
                        throw new Exception("ERROR: Too many EBOOT PH segments!");
                    }
                    pspHdr.segAlign[j] = (ushort)programHeader[i].p_align;
                    pspHdr.segAddress[j] = programHeader[i].p_vaddr;
                    pspHdr.segSize[j] = programHeader[i].p_memsz;
                    pspHdr.bssSize = programHeader[i].p_memsz - programHeader[i].p_filesz;
                    j++;
                }
            }

            pspHdr.nSegments = j;

            bool compress = false;
            switch (cryptType)
            {
                case SceExecFileDecryptMode.DECRYPT_MODE_UMD_GAME_EXEC:
                    modInfo.modname.CopyTo(pspHdr.modName);
                    pspHdr.compAttribute = 0;
                    pspHdr.dataOffset = 0x80;
                    break;
                case SceExecFileDecryptMode.DECRYPT_MODE_POPS_EXEC:
                    compress = true;
                    pspHdr.modName[0] = 0x20;
                    pspHdr.modAttribute = 0x0200;
                    pspHdr.compAttribute = 1;
                    pspHdr.dataOffset = 0x890;
                    break;
            }


            if (compress)
            {
                using var ms = new MemoryStream();
                using (var gs = new Ionic.Zlib.GZipStream(ms, Ionic.Zlib.CompressionMode.Compress, true))
                {
                    gs.LastModified = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    gs.Write(modData);
                }
                var tmp = ms.ToArray();
                tmp[9] = 3;
                modData = tmp.AsSpan();
            }
            modData.CopyTo(encData.Slice(pspHdrSize + pspHdr.dataOffset));
            var chk_size = (modData.Length + 15) / 16 * 16;
            pspHdr.dataSize = modData.Length;
            pspHdr.pspSize = pspHdrSize + pspHdr.dataOffset + chk_size;

            // MemoryMarshal.Write(encData, ref pspHdr);

            //var pspHdrSpan = MemoryMarshal.CreateReadOnlySpan(ref pspHdr, 1);
            //var pspHdrBuffer = MemoryMarshal.AsBytes(pspHdrSpan);
            Span<byte> randomSpan = null;

            if (cryptType == SceExecFileDecryptMode.DECRYPT_MODE_POPS_EXEC)
            {
                //randomSpan = new byte[]
                //{
                //0x01, 0x94, 0xB1, 0x16, 0x3D, 0x01, 0x42, 0x0F, 0xC7, 0x03, 0xDE, 0x10, 0x1C, 0x50, 0x91, 0x52,
                //0x73, 0x31, 0x52, 0xD7, 0xAF, 0xB4, 0x0A, 0xBF, 0x1C, 0xA5, 0x39, 0xCA, 0x1F, 0xBF, 0xA1, 0x0C,
                //0xF6, 0x1A, 0x52, 0x53, 0x7E, 0x68, 0x2F, 0xDE, 0x66, 0xC3, 0x82, 0x31, 0x08, 0x1A, 0x18, 0xC3,
                //0xFC, 0xE5, 0x83, 0xD5, 0xA6, 0x8F, 0xBC, 0x36, 0xD9, 0x17, 0x8E, 0x00, 0xEC, 0x5A, 0x49, 0x63,
                //0xEF, 0xA2, 0x7E, 0x89, 0x35, 0x10, 0x0D, 0x28, 0xFE, 0x67, 0xF1, 0xD9, 0xA5, 0x73, 0x17, 0x73,
                //0x86, 0x5B, 0xF1, 0x86, 0x06, 0x73, 0xCB, 0x18, 0xB8, 0xF9, 0x0B, 0x61, 0xDD, 0xB0, 0xC0, 0x56,
                //0x3A, 0x96, 0xED, 0x1E, 0xE6, 0x8C, 0x37, 0xBE, 0xA2, 0xA1, 0x61, 0xDE, 0x7D, 0x48, 0xA3, 0x21,
                //0x3C, 0x06, 0xBF, 0xB0, 0xB2, 0x40, 0x8C, 0x46, 0xED, 0x7B, 0x52, 0x01, 0x4F, 0xDD, 0x85, 0x43,
                //0x09, 0x61, 0x65, 0x8F, 0xCC, 0x53, 0x29, 0xCC, 0xDC, 0x48, 0x99, 0x8E, 0x73, 0x09, 0x67, 0x68,
                //0x93, 0x0A, 0xF5, 0x04, 0xFC, 0x3E, 0x51, 0xBB, 0x04, 0xD3, 0x70, 0xF9, 0xD0, 0xB0, 0xEA, 0x6D,
                //0xC8, 0xD7, 0xDE, 0x29, 0x91, 0xB0, 0xC9, 0xA0, 0xE9, 0x25, 0x17, 0x2A, 0x61, 0xF4, 0xC0, 0x9B,
                //0xEE, 0xD4, 0xCF, 0x55, 0x9F, 0x0C, 0xA9, 0x06, 0xE3, 0x79, 0xF7, 0x80, 0x85, 0xB5, 0x3C, 0x0E,
                //0x6D, 0xC1, 0x5D, 0x38, 0x2F, 0x99, 0x74, 0xCA, 0xC0, 0xAC, 0x7F, 0x3C, 0x7C, 0xF5, 0xB5, 0xA9,
                //0x85, 0x27, 0xC8, 0xF2, 0xEE, 0x7D, 0xCF, 0xE6, 0x53, 0x99, 0xF4, 0x14, 0x28, 0x20, 0x42, 0x3E,
                //0x13, 0x7F, 0xB9, 0x60, 0x9E, 0xC4, 0xAC, 0xE5, 0x02, 0x8B, 0x36, 0x9A, 0xB3, 0x89, 0x46, 0x33,
                //0xBC, 0x4D, 0x79, 0xD5, 0x1F, 0x8D, 0x36, 0xB5, 0xAB, 0x01, 0x0F, 0xFE, 0xC2, 0x09, 0xFD, 0xD8,
                //0x4B, 0x12, 0x9F, 0x05, 0xF0, 0x6B, 0x3C, 0xBD, 0x3F, 0x65, 0xF5, 0x5B, 0x86, 0x19, 0x11, 0x20,
                //0x53, 0xB1, 0x80, 0x32, 0x62, 0x3E, 0x00, 0x11, 0x9E, 0x8D, 0x4B, 0x57, 0x56, 0x48, 0xBC, 0x5B,
                //0x32, 0x90, 0xA1, 0x9E, 0x05, 0xD1, 0xEB, 0x8C, 0x12, 0xA7, 0x4D, 0x33, 0x15, 0xCB, 0xC0, 0xD2,
                //0x8F, 0x44, 0x98, 0x6F, 0x4B, 0x81, 0x44, 0x58, 0xD0, 0x5F, 0x88, 0xA5, 0x22, 0x06, 0x76, 0x9F,
                //0x15, 0xB2, 0x40, 0xF2, 0x3C, 0x2B, 0x55, 0x6E, 0x1B, 0x9B, 0xCE, 0x11, 0x37, 0x75, 0xA6, 0xBE,
                //0xE3, 0xA7, 0xFF, 0x30, 0xE8, 0x5C, 0x7B, 0xAF, 0x1C, 0xFC, 0x63, 0x4A, 0x5C, 0xE4, 0xCF, 0xBD,
                //0x68, 0x89, 0xC8, 0xDF, 0x89, 0x63, 0x61, 0x71, 0x8C, 0xCD, 0xB2, 0x21, 0x23, 0x55, 0x5E, 0x8E,
                //0x6F, 0x04, 0x89, 0xC0, 0x4E, 0xF6, 0x3A, 0x74, 0x89, 0xBA, 0xF4, 0x51, 0x9A, 0x5D, 0x09, 0x2C,
                //0x76, 0x48, 0x9F, 0x86, 0xCD, 0xF3, 0x66, 0xA6, 0xBF, 0x78, 0xC7, 0xD0, 0x95, 0x48, 0x4E, 0x31,
                //0x2B, 0xBC, 0x73, 0x70, 0x95, 0x7C, 0xE9, 0x1E, 0x8F, 0x55, 0x35, 0x83, 0x37, 0x65, 0x9C, 0x34,
                //0x98, 0x15, 0x6F, 0x70, 0x31, 0x7C, 0xED, 0x46, 0x2A, 0xC4, 0x3E, 0xAE, 0x55, 0x49, 0x74, 0x37,
                //0x81, 0x02, 0x5C, 0x73, 0x79, 0xA3, 0x6E, 0xCF, 0x37, 0xFB, 0x0F, 0x9F, 0xCC, 0x1E, 0xC1, 0x73,
                //0xF5, 0xE1, 0x8B, 0x84, 0x52, 0xF2, 0x6B, 0xD3, 0xE7, 0x29, 0x69, 0xB1, 0xD9, 0xE9, 0xEB, 0x95,
                //0x6D, 0xD3, 0x00, 0xC2, 0x7C, 0x5D, 0xDF, 0x84, 0xA6, 0x8D, 0xB0, 0x2E, 0xC2, 0x41, 0xC1, 0x49,
                //0x2A, 0x61, 0xAA, 0x3B, 0x40, 0x98, 0x6F, 0x36, 0x3D, 0xAB, 0x31, 0x08, 0x50, 0xD1, 0x5C, 0xCC,
                //0x0E, 0x6F, 0x7C, 0xD4, 0x98, 0x96, 0x73, 0x13, 0x9D, 0xD8, 0xF5, 0x4E, 0x81, 0x08, 0xEA, 0x2A,
                //0x7E, 0x1A, 0x8A, 0x86, 0xC5, 0x70, 0x14, 0x15, 0xC6, 0x3F, 0x00, 0x86, 0x8C, 0x9F, 0x20, 0x29,
                //0x7B, 0xF4, 0xD2, 0xDE, 0xB1, 0x8A, 0x3F, 0xE9, 0x58, 0x00, 0x6B, 0x19, 0x0C, 0xC1, 0xF9, 0x56,
                //0x95, 0x29, 0xCC, 0x95, 0x29, 0x56, 0x16, 0xE3, 0xBA, 0xCE, 0xCA, 0x5C, 0xD5, 0x92, 0xB0, 0x49,
                //0xBA, 0xBD, 0x9B, 0xE1, 0x0E, 0x22, 0x9A, 0x2B, 0x3A, 0x94, 0x1F, 0xB4, 0x74, 0x24, 0x1B, 0xBE,
                //0xF3, 0x07, 0x7A, 0xD0, 0xCF, 0x6F, 0x3C, 0xC2, 0x5B, 0xA1, 0x08, 0x3A, 0xA6, 0x9F, 0xE5, 0xB1,
                //0x95, 0x82, 0x9F, 0x1D, 0xC3, 0xAA, 0xEA, 0x8B, 0x75, 0x1C, 0x63, 0xA5, 0x3D, 0xAF, 0x6B, 0x70,
                //0x21, 0x59, 0x4E, 0x50, 0x83, 0xF0, 0xCD, 0x7D, 0x83, 0x34, 0x68, 0x78, 0x71, 0xB1, 0x47, 0x2E,
                //0x48, 0xB5, 0xCC, 0xE1, 0xAF, 0x8A, 0x04, 0xBD, 0xF4, 0x4B, 0xB7, 0x1F, 0xB5, 0x46, 0x47, 0x02,
                //0xD7, 0xE8, 0x97, 0xDE, 0x53, 0x94, 0x41, 0xE3, 0x6F, 0x26, 0x9B, 0x4F, 0x97, 0x60, 0xAA, 0xA1,
                //0x2F, 0x1C, 0xD3, 0x27, 0x99, 0x92, 0x21, 0x08, 0x32, 0x4F, 0x44, 0x4C, 0xA7, 0x1C, 0x73, 0xC6,
                //0x7A, 0xCF, 0xC4, 0xBD, 0x7B, 0xAB, 0x17, 0x37, 0x33, 0xF2, 0x76, 0x2C, 0x65, 0xD6, 0x85, 0xB8,
                //0x15, 0xBA, 0xC7, 0x82, 0x05, 0x65, 0x60, 0x0C, 0xC8, 0x1D, 0x32, 0xF6, 0xAB, 0x12, 0xD6, 0x59,
                //0xA7, 0xC7, 0x06, 0x5C, 0x2F, 0x2D, 0xEC, 0xE7, 0x82, 0xF5, 0x40, 0x7D, 0x58, 0x80, 0xB2, 0x61,
                //0xA8, 0x95, 0x58, 0xF5, 0xEA, 0x93, 0x5F, 0xEA, 0xAD, 0x77, 0x1C, 0x8E, 0xC3, 0x7A, 0x1B, 0xCF,
                //0x31, 0x53, 0x8D, 0xA9, 0x86, 0xC6, 0x03, 0x09, 0x5D, 0x23, 0xF2, 0x25, 0xE4, 0x62, 0xAB, 0xC4,
                //0x71, 0xA6, 0x3D, 0x0B, 0xDA, 0x3A, 0x24, 0xF6, 0x85, 0x3E, 0xD1, 0x13, 0x28, 0xEC, 0x58, 0xA3,
                //0xC6, 0x18, 0x91, 0x75, 0x0E, 0xD6, 0x06, 0x6C, 0x89, 0xA1, 0xA7, 0xDE, 0xBF, 0x59, 0xD4, 0x2C,
                //0xE4, 0x8C, 0x08, 0x15, 0x13, 0xF5, 0xD2, 0xD6, 0x6D, 0xC8, 0xEB, 0xE9, 0x9F, 0x9E, 0x19, 0x39,
                //0x81, 0x7B, 0xE2, 0x89, 0x07, 0x1B, 0x98, 0xE3, 0x20, 0x18, 0xFE, 0xEF, 0xFD, 0x75, 0xED, 0x6D,
                //0x9E, 0x1B, 0x21, 0x97, 0x17, 0x19, 0x59, 0xD5, 0xCA, 0xD1, 0xA0, 0xED, 0x7F, 0xAE, 0xEF, 0x8E,
                //0x38, 0x8E, 0xAC, 0x1A, 0xC4, 0x0E, 0x4D, 0xDE, 0x03, 0xF5, 0x93, 0x95, 0x9C, 0xC9, 0x5E, 0x00,
                //0x3A, 0x4E, 0x46, 0x8F, 0xA3, 0xAB, 0x6D, 0x61, 0x49, 0x6E, 0xCC, 0x0E, 0x8E, 0x4B, 0xC6, 0x91,
                //0xB2, 0x08, 0x79, 0x57, 0x2F, 0x29, 0x76, 0xFA, 0x09, 0x8A, 0x59, 0xF9, 0x2B, 0x6E, 0x48, 0xF4,
                //0x06, 0xF6, 0x82, 0xE8, 0xFC, 0x3F, 0xFC, 0x5C, 0xD8, 0x3B, 0x02, 0xD3, 0xC5, 0x20, 0x0C, 0xBB,
                //0x53, 0x63, 0x06, 0xEF, 0x67, 0x85, 0xE4, 0xAB, 0x1E, 0x62, 0xEB, 0x20, 0x31, 0xE0, 0x47, 0xCD,
                //0x2A, 0xAE, 0xF1, 0x85, 0x55, 0x69, 0xE5, 0xE2, 0xC1, 0x9A, 0x3D, 0xF3, 0x74, 0x75, 0x4F, 0xCD,
                //0x94, 0x2C, 0xFB, 0x9B, 0x29, 0x81, 0x82, 0x07, 0x79, 0xC4, 0xB2, 0xD7, 0x62, 0x43, 0x78, 0x7C,
                //0xF2, 0xC1, 0xAF, 0x37, 0xAA, 0x9A, 0xDA, 0xDB, 0x7D, 0xCC, 0xA2, 0xC7, 0x54, 0x56, 0xB6, 0x76,
                //0xCF, 0x57, 0xE5, 0x0E, 0x2E, 0xB9, 0xF4, 0x68, 0x99, 0x36, 0xEE, 0xAF, 0x90, 0x21, 0x8F, 0xC8,
                //0x5C, 0x92, 0x0A, 0x99, 0xDD, 0x6A, 0x2A, 0xB5, 0x74, 0xB9, 0xAC, 0xCC, 0x64, 0xA3, 0xA4, 0xC7,
                //0xBD, 0xEE, 0x9A, 0xCD, 0x60, 0x3B, 0xBB, 0xB8, 0xE2, 0xAF, 0xE0, 0x91, 0x69, 0x37, 0x61, 0x63,
                //0x04, 0x97, 0x52, 0x49, 0x86, 0x89, 0xC5, 0x80, 0xAA, 0x72, 0xA9, 0xEA, 0x87, 0xD0, 0x3A, 0xDA,
                //0x7E, 0xAE, 0x4A, 0x84, 0x83, 0xC7, 0x1F, 0x12, 0xB9, 0x2E, 0x6B, 0x8E, 0xAC, 0x62, 0x5A, 0xF8
                //};
                //randomSpan.CopyTo(encData.Slice(0x150));
                config.CopyTo(encData[0x150..]);
                //RandomNumberGenerator.Fill(encData.Slice(0x150, 0x410));

                // JP9000-NPJI90001_00-0000000000000001
                // JP0082-NPJJ00294_00-0000000000000001
                Encoding.ASCII.GetBytes(contentId, encData[(0x150 + 0x410)..]);
                int tmp = 0x01000000;
                MemoryMarshal.Write(encData[0x590..], ref tmp);
            }
            else if (pspHdr.dataOffset > 0x80)
            {
                RandomNumberGenerator.Fill(encData.Slice(0x150, pspHdr.dataOffset - 0x80));
            }

            var cipher = Ciphers[tag];

            var type = cipher.Type;
            var keySeed = cipher.Seed;
            var xorKey = cipher.XorKey.AsSpan();

            // ENCRYPT DATA
            Span<byte> aesKeys = stackalloc byte[32];
            using (var aes = AesHelper.CreateAes())
            {
                // aes.Key = new byte[] { 0x37, 0x49, 0x0A, 0x87, 0xA3, 0xB3, 0x7B, 0x50, 0x47, 0x4E, 0x21, 0x2B, 0x4A, 0x0F, 0xA6, 0xC8 };
                aes.Key.AsSpan().CopyTo(aesKeys);
                AesHelper.AesEncrypt(aes, encData.Slice(pspHdrSize + pspHdr.dataOffset), encData.Slice(pspHdrSize + pspHdr.dataOffset, modData.Length));
            }

            // CMAC HASHES
            ref var cmd1Hdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_CMD1_HEADER>(encData.Slice(0x40));
            using (var aesCmac = AesHelper.CreateAes())
            {
                cmd1Hdr.mode = KIRKEngine.KIRK_MODE_CMD1;
                if (type == 6 || type == 7)
                {
                    cmd1Hdr.ecdsa_hash = 1;
                }
                encData[..0x80].CopyTo(encData.Slice(0x40 + 0x90));

                // aesCmac.Key = new byte[] { 0xBE, 0xAF, 0x2D, 0x80, 0xBF, 0xC1, 0xCC, 0x53, 0x3B, 0xCF, 0x18, 0x7B, 0x95, 0xFA, 0xC7, 0xCC };
                aesCmac.Key.AsSpan().CopyTo(aesKeys.Slice(16));
                AesHelper.Cmac(aesCmac, cmd1Hdr.CMAC_header_hash, encData.Slice(0xA0, 0x30));
                AesHelper.Cmac(aesCmac, cmd1Hdr.CMAC_data_hash, encData.Slice(0xA0, 0x30 + pspHdr.dataOffset + chk_size));
                //cmac_header_hash.CopyTo(pspHdr.cmacHeaderHash);
                //cmac_data_hash.CopyTo(pspHdr.cmacDataHash);
            }

            // ENCRYPT KEYS
            using (var kirkAes = AesHelper.CreateKirkAes())
            {
                AesHelper.AesEncrypt(kirkAes, cmd1Hdr.AESKeys, aesKeys);
            }

            ret = BuildKeyData(keySeed, 0, type, cipher.Key, versionKey);
            if (ret != 0)
            {
                return ret;
            }

            KirkMemory.Span.Fill(0);
            KirkCmd1Memory.Span.Fill(0);
            KirkEcdsaMemory.Span.Fill(0);

            if ((1 < type && type < 8) || type == 9 || type == 10)
            {
                if (type == 6 || type == 7)
                {
                    // modData.Slice(0x80).CopyTo(buf5.Slice(0, 0x20));
                    encData.Slice(0xb0, 0x10).CopyTo(KirkMemory.Slice(0xc0).Span);
                    encData.Slice(0xd0, 0x80).CopyTo(KirkMemory.Slice(0xd0).Span);
                    encData.Slice(0xd0, 0x80).Fill(0);
                }
                else
                {
                    encData.Slice(0xb0, 0x10).CopyTo(KirkMemory.Slice(0xc0).Span);
                    encData.Slice(0xd0, 0x80).CopyTo(KirkMemory.Slice(0xd0).Span);
                    encData.Slice(0xd0, 0x80).Fill(0);
                }

                unsafe
                {
                    fixed (byte* ptr = encData.Slice(0x40))
                    {

                    }

                    fixed (byte* ptr1 = KirkMemory.Slice(0x6c).Span)
                    {

                    }
                }

                XorKeyInto(KirkMemory.Slice(0x80).Span, 0x40, encData.Slice(0x40), KeyData.Slice(80).Span);
                ret = Kirk4(KirkMemory.Slice(0x6c).Span, 0x40, keySeed, 0);
                if (ret != 0)
                {
                    ret = -7;
                    return ret;
                }
                XorKeyLarge(KirkMemory.Slice(0x80).Span, 0x40, KeyData.Slice(16).Span);
                Span<byte> k4 = stackalloc byte[] { 0x4A, 0xB8, 0x0B, 0x41, 0xA1, 0xDF, 0x55, 0x0D, 0xFC, 0xD1, 0xA6, 0xA6, 0x84, 0xFE, 0xCE, 0x6A };
                //if (randomSpan != null)
                //{
                //    using (var aes = AesHelper.CreateAes())
                //    {
                //        aes.Key = k4.ToArray();
                //        AesHelper.AesDecrypt(aes, randomSpan, randomSpan, 0x410);
                //    }
                //}
                RandomNumberGenerator.Fill(k4);
                k4.CopyTo(KirkMemory.Slice(0x70).Span);
                if (type == 4)
                {

                }
                else
                {


                    if (type == 6 || type == 7)
                    {
                        // TODO
                    }
                    else
                    {
                        KirkMemory.Span.Slice(0x18, 0x58).Fill(0);
                    }
                    // if (b_0xd4 == 0x80)
                    // KirkMemory.Span[0x18] = 0x80;

                    var value = 0x14c;
                    MemoryMarshal.Write(KirkMemory.Span, ref value);
                    MemoryMarshal.Write(KirkMemory.Slice(4).Span, ref tag);
                    KeyData[..0x10].CopyTo(KirkMemory[8..]);
                }
            }
            else
            {
                // TODO
            }

            ret = KIRKEngine.sceUtilsBufferCopyWithRange(KirkCmd1Memory.Span, 0x150, KirkMemory.Span, 0x150,
                KIRKEngine.KIRK_CMD_SHA1_HASH);
            if (ret != 0)
            {
                ret = -6;
                return ret;
            }


            if ((1 < type && type < 8) || type == 9 || type == 10)
            {
                if (type == 4)
                {
                    KirkMemory.Slice(0x18, 0x67).CopyTo(KirkMemory);
                }
                else
                {
                    KirkMemory.Slice(0x70, 0x10).CopyTo(KirkMemory.Slice(0x5c));
                    if (type == 6 || type == 7)
                    {
                        KirkMemory.Slice(0x50, 0x20).CopyTo(KirkEcdsaMemory);
                        KirkEcdsaMemory[..0x20].CopyTo(KirkMemory[0x3c..]);
                    }
                }
                KirkCmd1Memory[..0x14].CopyTo(KirkMemory[0x6c..]);
            }
            else
            {
                // TODO
            }

            if (type == 1)
            {
                // TODO
            }
            else
            {
                unsafe
                {
                    fixed (byte* ptr = &MemoryMarshal.GetReference(KirkCmd1Memory.Slice(0x14).Span))
                    {

                    }
                }

                KirkMemory.Slice(0x5c, 0x60).CopyTo(KirkCmd1Memory.Slice(0x14));
                ret = Kirk4(KirkCmd1Memory.Span, 0x60, keySeed, 0);
                if (ret != 0)
                {
                    return -5;
                }

                if (type == 3 || type == 5 || type == 7 || type == 10)
                {
                    XorKey(KirkCmd1Memory.Span.Slice(0x14), 0x60, xorKey);
                }

                if ((1 < type && type < 8) || type == 9 || type == 10)
                {
                    KirkCmd1Memory.Slice(0x14, 0x60).CopyTo(KirkMemory.Slice(0x5c));
                }
            }

            if (type == 3)
            {
                // TODO
            }
            else if (type == 5 || type == 7 || type == 10)
            {
                KirkMemory.Slice(0xd0, 0x80).Span.CopyTo(pspHdr.RawHdr);
                KirkMemory.Slice(0xc0, 0x10).Span.CopyTo(pspHdr.sizeInfo);
                KirkMemory.Slice(0xb0, 0x10).CopyTo(KirkCmd1Memory.Slice(0x30));
                KirkMemory.Slice(0x80, 0x30).CopyTo(KirkCmd1Memory);
                KirkMemory.Slice(0x6c, 0x10).CopyTo(KirkCmd1Memory.Slice(0x40));
                KirkMemory.Slice(0x6c, 0x14).Span.CopyTo(pspHdr.sha1Hash);
                KirkMemory.Slice(0x5c, 0x10).Span.CopyTo(pspHdr.keyData4);

                if (type == 7)
                {
                    KirkMemory.Slice(0x3c).Span.CopyTo(pspHdr.sCheck.Slice(56, 0x20));
                }

                KirkCmd1Memory[..0x50].CopyTo(KirkMemory[0x14..]);
                ret = Kirk4(KirkMemory.Span, 0x50, keySeed, 0);
                if (ret != 0)
                {
                    return -13;
                }
                XorKey(KirkMemory.Slice(0x14).Span, 0x50, xorKey, versionKey);
                KirkMemory.Slice(0x14, 0x30).Span.CopyTo(pspHdr.keyData);
                KirkMemory.Slice(0x44, 0x10).Span.CopyTo(pspHdr.cmacDataHash);
                KirkMemory.Slice(0x54, 0x10).Span.CopyTo(pspHdr.sha1Hash);
                pspHdr.tag = tag;
            }
            else if (type == 2 || type == 4 || type == 6 || type == 9)
            {
                KirkMemory.Slice(0x5c, 0x10).Span.CopyTo(pspHdr.keyData4);
                KirkMemory.Slice(0x6c, 0x14).Span.CopyTo(pspHdr.sha1Hash);
                KirkMemory.Slice(0x80, 0x30).Span.CopyTo(pspHdr.keyData);
                KirkMemory.Slice(0xb0, 0x10).Span.CopyTo(pspHdr.cmacDataHash);
                KirkMemory.Slice(0xc0, 0x10).Span.CopyTo(pspHdr.sizeInfo);
                KirkMemory.Slice(0xd0, 0x80).Span.CopyTo(pspHdr.RawHdr);
                pspHdr.tag = tag;
                pspHdr.sCheck.Clear();
            }
            else
            {
                KirkMemory[..0x80].Span.CopyTo(pspHdr.CheckData);
                KirkMemory.Slice(0x80, 0x30).Span.CopyTo(pspHdr.keyData);
                KirkMemory.Slice(0xd0, 0x80).Span.CopyTo(pspHdr.RawHdr);
            }

            return ret;
        }
    }
}
