using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PsvImage
{
    internal class PSVIMGConstants
    {
        public const int AES_BLOCK_SIZE = 0x10;
        public const int SHA256_BLOCK_SIZE = 0x20;
        public const int PSVIMG_BLOCK_SIZE = 0x8000;
        public const int PSVIMG_ENTRY_ALIGN = 0x400;

        public const string PSVIMG_HEADER_END = "EndOfHeader\n";
        public const string PSVIMG_TAILOR_END = "EndOfTailer\n";
        public const string PSVIMG_PADDING_END = "\n";

        public const int FULL_PSVIMG_SIZE = PSVIMG_BLOCK_SIZE + SHA256_BLOCK_SIZE;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SceDateTime
    {
        public ushort Year;
        public ushort Month;
        public ushort Day;
        public ushort Hour;
        public ushort Minute;
        public ushort Second;
        public uint Microsecond;
    }


    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct SceIoStat
    {
        [Flags]
        public enum Modes
        {
            /** Format bits mask */
            FormatBits = 0xF000,
            /** Symbolic link */
            SymbLink = 0x4000,
            /** Directory */
            Directory = 0x1000,
            /** Regular file */
            File = 0x2000,

            /** Set UID */
            SetUid = 0x0800,
            /** Set GID */
            SetGid = 0x0400,
            /** Sticky */
            Sticky = 0x0200,

            /** Others access rights mask */
            OthersAcesssMask = 0x01C0,
            /** Others read permission */
            OthersRead = 0x0100,
            /** Others write permission */
            OthersWrite = 0x0080,
            /** Others execute permission */
            OthersExecute = 0x0040,

            /** Group access rights mask */
            GroupAcessMask = 0x0038,
            /** Group read permission */
            GroupRead = 0x0020,
            /** Group write permission */
            GroupWrite = 0x0010,
            /** Group execute permission */
            GroupExecute = 0x0008,

            /** User access rights mask */
            UserAcessMask = 0x0007,
            /** User read permission */
            UserRead = 0x0004,
            /** User write permission */
            UserWrite = 0x0002,
            /** User execute permission */
            UserExecute = 0x0001,
        };
        public enum AttributesEnum
        {
            /** Format mask */
            FormatMask = 0x0038,               // Format mask
            /** Symlink */
            SymbLink = 0x0008,               // Symbolic link
            /** Directory */
            Directory = 0x0010,               // Directory
            /** Regular file */
            File = 0x0020,               // Regular file

            /** Hidden read permission */
            Read = 0x0004,               // read
            /** Hidden write permission */
            Write = 0x0002,               // write
            /** Hidden execute permission */
            Execute = 0x0001,               // execute
        };

        public Modes Mode;
        public AttributesEnum Attributes;
        /** Size of the file in bytes. */
        public UInt64 Size;
        /** Creation time. */
        public SceDateTime CreationTime;
        /** Access time. */
        public SceDateTime AccessTime;
        /** Modification time. */
        public SceDateTime ModificaionTime;
        /** Device-specific data. */
        public fixed uint Private[6];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct PsvImgTailer
    {

        public ulong Flags;
        public fixed byte Padding[1004];
        public fixed byte bEnd[12];
    }

    internal class PSVIMGPadding
    {
        public static long GetPadding(long size)
        {
            long padding;
            if ((size & (PSVIMGConstants.PSVIMG_ENTRY_ALIGN - 1)) >= 1)
            {
                padding = (PSVIMGConstants.PSVIMG_ENTRY_ALIGN - (size & (PSVIMGConstants.PSVIMG_ENTRY_ALIGN - 1)));
            }
            else
            {
                padding = 0;
            }
            return padding;
        }
    }

    internal unsafe struct PsvImgHeader
    {
        public ulong SysTime;
        public ulong Flags;
        public SceIoStat Statistics;
        public fixed byte bParentPath[256];
        public uint unk_16C;
        public fixed byte bPath[256];
        public fixed byte Padding[904];
        public fixed byte bEnd[12];
    }
}
