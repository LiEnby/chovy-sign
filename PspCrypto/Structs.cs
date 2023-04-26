using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PspCrypto
{
    public enum SceExecFileDecryptMode : byte
    {
        /* Not an executable. */
        DECRYPT_MODE_NO_EXEC = 0,
        /* 1.50 Kernel module. */
        DECRYPT_MODE_BOGUS_MODULE = 1,
        DECRYPT_MODE_KERNEL_MODULE = 2,
        DECRYPT_MODE_VSH_MODULE = 3,
        DECRYPT_MODE_USER_MODULE = 4,
        DECRYPT_MODE_UMD_GAME_EXEC = 9,
        DECRYPT_MODE_GAMESHARING_EXEC = 10,
        /* USB/WLAN module. */
        DECRYPT_MODE_UNKNOWN_11 = 11,
        DECRYPT_MODE_MS_UPDATER = 12,
        DECRYPT_MODE_DEMO_EXEC = 13,
        DECRYPT_MODE_APP_MODULE = 14,
        DECRYPT_MODE_UNKNOWN_18 = 18,
        DECRYPT_MODE_UNKNOWN_19 = 19,
        DECRYPT_MODE_POPS_EXEC = 20,
        /* MS module. */
        DECRYPT_MODE_UNKNOWN_21 = 21,
        /* APP module. */
        DECRYPT_MODE_UNKNOWN_22 = 22,
        /* USER module. */
        DECRYPT_MODE_UNKNOWN_23 = 23,
        /* USER module. */
        DECRYPT_MODE_UNKNOWN_25 = 25,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PSPHeader2
    {
        public Span<byte> RawHdr
        {
            get
            {
                fixed (uint* ptr = &magic)
                {
                    return new Span<byte>(ptr, 0x80);
                }
            }
        }
        public uint magic;
        public ushort modAttribute;
        public ushort compAttribute;
        public byte moduleVerLo;
        public byte moduleVerHi;
        private fixed byte _modName[27];

        public Span<byte> modName
        {
            get
            {
                fixed (byte* ptr = _modName)
                {
                    return new Span<byte>(ptr, 27);
                }
            }
        }
        public byte terminal;
        public byte modVersion;
        public byte nSegments;
        public int elfSize;
        public int pspSize;
        public uint bootEntry;
        public int modInfoOffset;
        public uint bssSize;
        private fixed ushort _segAlign[4];

        public Span<ushort> segAlign
        {
            get
            {
                fixed (ushort* ptr = _segAlign)
                {
                    return new Span<ushort>(ptr, 4);
                }
            }
        }
        private fixed uint _segAddress[4];

        public Span<uint> segAddress
        {
            get
            {
                fixed (uint* ptr = _segAddress)
                {
                    return new Span<uint>(ptr, 4);
                }
            }
        }
        private fixed uint _segSize[4];

        public Span<uint> segSize
        {
            get
            {
                fixed (uint* ptr = _segSize)
                {
                    return new Span<uint>(ptr, 4);
                }
            }
        }
        public fixed uint reserved[5];
        public uint devkitVersion;
        public SceExecFileDecryptMode decryptMode;
        public byte padding;
        public ushort overlapSize;
        private fixed byte _aesKey[16];
        public Span<byte> aesKey
        {
            get
            {
                fixed (byte* ptr = _aesKey)
                {
                    return new Span<byte>(ptr, 16);
                }
            }
        }

        public Span<byte> keyData
        {
            get
            {
                fixed (byte* ptr = _aesKey)
                {
                    return new Span<byte>(ptr, 0x30);
                }
            }
        }

        public Span<byte> keyData50
        {
            get
            {
                fixed (byte* ptr = _aesKey)
                {
                    return new Span<byte>(ptr, 0x50);
                }
            }
        }

        private fixed byte _cmacKey[16];
        public Span<byte> cmacKey
        {
            get
            {
                fixed (byte* ptr = _cmacKey)
                {
                    return new Span<byte>(ptr, 16);
                }
            }
        }
        private fixed byte _cmacHeaderHash[16];
        public Span<byte> cmacHeaderHash
        {
            get
            {
                fixed (byte* ptr = _cmacHeaderHash)
                {
                    return new Span<byte>(ptr, 16);
                }
            }
        }

        public Span<byte> sizeInfo
        {
            get
            {
                fixed (int* ptr = &dataSize)
                {
                    return new Span<byte>(ptr, 0x10);
                }
            }
        }
        public int dataSize;
        public int dataOffset;
        public uint unk184;
        public uint unk188;
        private fixed byte _cmacDataHash[16];

        public Span<byte> cmacDataHash
        {
            get
            {
                fixed (byte* ptr = _cmacDataHash)
                {
                    return new Span<byte>(ptr, 16);
                }
            }
        }

        public Span<byte> CheckData
        {
            get
            {
                fixed (uint* ptr = &tag)
                {
                    return new Span<byte>(ptr, 0x80);
                }
            }
        }
        public uint tag;
        private fixed byte _sCheck[0x58];

        public Span<byte> sCheck
        {
            get
            {
                fixed (byte* ptr = _sCheck)
                {
                    return new Span<byte>(ptr, 0x58);
                }
            }
        }
        private fixed byte _sha1Hash[20];

        public Span<byte> sha1Hash
        {
            get
            {
                fixed (byte* ptr = _sha1Hash)
                {
                    return new Span<byte>(ptr, 20);
                }
            }
        }
        private fixed byte _keyData4[16];

        public Span<byte> keyData4
        {
            get
            {
                fixed (byte* ptr = _keyData4)
                {
                    return new Span<byte>(ptr, 16);
                }
            }
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PbpHeader
    {
        public int Sig;
        public int Version;
        public int ParamOff;
        public int Icon0Off;
        public int Icon1Off;
        public int Pic0Off;
        public int Pic1Off;
        public int Snd0Off;
        public int DataPspOff;
        public int DataPsarOff;
    }
}
