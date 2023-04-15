using CommunityToolkit.HighPerformance;
using Ionic.Zlib;
using PopsBuilder.Pops;
using PopsBuilder.Psp;
using PspCrypto;
using PsvImage;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace PbpResign
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PbpHeader
    {
        public int Sig;
        public int Ver;
        public int ParamOff;
        public int Icon0Off;
        public int Icon1Off;
        public int Pic0Off;
        public int Pic1Off;
        public int Snd0Off;
        public int DataPspOff;
        public int DataPsarOff;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct NpUmdImgBody
    {
        public ushort SectorSize;    // 0x0800
        public ushort Unk2;          // 0xE000
        public uint Unk4;
        public uint Unk8;
        public uint Unk12;
        public uint Unk16;
        public uint LbaStart;
        public uint Unk24;
        public uint NSectors;
        public uint Unk32;
        public uint LbaEnd;
        public uint Unk40;
        public uint BlockEntryOffset;
        private fixed byte discId_[0x10];
        public Span<byte> DiscId
        {
            get
            {
                fixed (byte* ptr = discId_)
                {
                    return new Span<byte>(ptr, 0x10);
                }
            }
        }
        public ushort HeaderStartOffset;
        public ushort HeaderStartOffset1;
        public uint ThreadPriority;
        public byte Unk72;
        public byte BBMacParam;
        public byte Unk74;
        public byte Unk75;
        public uint Unk76;
        public uint Unk80;
        public uint Unk84;
        public uint Unk88;
        public uint Unk92;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct NpUmdImgHdr
    {
        public int Magic0;
        public int Magic1;
        public int NpFlags;
        public int BlockBasis;
        private fixed byte contentId_[0x30];
        public Span<byte> ContentId
        {
            get
            {
                fixed (byte* ptr = contentId_)
                {
                    return new Span<byte>(ptr, 0x30);
                }
            }
        }
        public NpUmdImgBody Body;
        private fixed byte headerKey_[0x10];
        public Span<byte> HeaderKey
        {
            get
            {
                fixed (byte* ptr = headerKey_)
                {
                    return new Span<byte>(ptr, 0x10);
                }
            }
        }
        private fixed byte dataKey_[0x10];
        public Span<byte> DataKey
        {
            get
            {
                fixed (byte* ptr = dataKey_)
                {
                    return new Span<byte>(ptr, 0x10);
                }
            }
        }
        private fixed byte headerHash_[0x10];
        public Span<byte> HeaderHash
        {
            get
            {
                fixed (byte* ptr = headerHash_)
                {
                    return new Span<byte>(ptr, 0x10);
                }
            }
        }
        private fixed byte Pad[0x8];
        private fixed byte eCDsaSig_[0x28];
        public Span<byte> ECDsaSig
        {
            get
            {
                fixed (byte* ptr = eCDsaSig_)
                {
                    return new Span<byte>(ptr, 0x28);
                }
            }
        }
    }

    unsafe struct NpBlock
    {
        private fixed byte mac_[0x10];

        public Span<byte> Mac
        {
            get
            {
                fixed (byte* ptr = mac_)
                {
                    return new Span<byte>(ptr, 0x10);
                }
            }
        }

        public int Offset;
        public int Size;
        public int Unk1;
        public int Unk2;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct CDDA_ENTRY
    {
        public int offset;
        public int size;
        public int padding;
        public int key;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct ISO_ENTRY
    {
        public int offset;
        public ushort size;
        public short marker;             // 0x01 or 0x00
        private fixed byte checksum_[0x10]; // First 0x10 bytes of sha1 sum of 0x10 disc sectors

        public Span<byte> checksum
        {
            get
            {
                fixed (byte* ptr = checksum_)
                {
                    return new Span<byte>(ptr, 0x10);
                }
            }
        }
        public fixed byte padding[0x8];
    }

    [StructLayout(LayoutKind.Sequential)]
    struct STARTDAT_HEADER
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] magic;    // STARTDAT
        public uint unk1;       // 0x01
        public uint unk2;       // 0x01
        public int header_size;
        public int data_size;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct SIMPLE_HEADER
    {
        private fixed byte magic_[8];     // SIMPLE__

        public Span<byte> magic
        {
            get
            {
                fixed (byte* ptr = magic_)
                {
                    return new Span<byte>(ptr, 8);
                }
            }
        }
        public uint unk1;        // 0x64
        public uint unk2;        // 0x01
        public int data_size;
        public int unk3;         // 0 or chcksm
        public int unk4;         // 0 or chcksm
    }

    class Program
    {

        private static byte[] Idps = { 0x00, 0x00, 0x00, 0x01, 0x01, 0x03, 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        private static string CId = "JP0177-NPJH50145_00-VALKYRIA2DLC002B";

        private static KeyGen _keyGen;

        private static readonly Memory<byte> VersionKey = new byte[16];
        private static readonly Memory<byte> NewVersionKey = new byte[16];

        //struct VersionKey
        //{
        //    public byte[] Fixed;
        //    public byte[] Type2;
        //    public byte[] Type3;
        //}

        //private static readonly Dictionary<string, VersionKey> Keys = new()
        //{
        //    {
        //        "JP0177-NPJH50145_00-VALKYRIA2DLC002B",
        //        new VersionKey
        //        {
        //            Fixed = new byte[] { 0x38, 0x20, 0xD0, 0x11, 0x07, 0xA3, 0xFF, 0x3E, 0x0A, 0x4C, 0x20, 0x85, 0x39, 0x10, 0xB5, 0x54 },
        //            Type2 = new byte[] { 0x80, 0x2C, 0x03, 0xB8, 0xB9, 0x1E, 0xB6, 0xF8, 0xE8, 0xF6, 0xB8, 0x54, 0xAD, 0x8C, 0x0E, 0x25 },
        //            Type3 = new byte[] { 0x90, 0xC2, 0x03, 0x02, 0x27, 0x90, 0x7C, 0x0C, 0x7A, 0xCD, 0x83, 0x30, 0x28, 0x13, 0x90, 0x83 }
        //        }
        //    },
        //    {
        //        "EP9000-NPEG00005_00-0000000000000001",
        //        new VersionKey
        //        {
        //            Fixed = new byte[] { 0x5A, 0xB0, 0xB5, 0xE2, 0xC3, 0x2E, 0xE3, 0xBA, 0xFE, 0xF8, 0x0A, 0xDE, 0x35, 0xBD, 0x78, 0x88 },
        //            Type2 = new byte[] { 0x5A, 0xB0, 0xB5, 0xE2, 0xC3, 0x2E, 0xE3, 0xBA, 0xFE, 0xF8, 0x0A, 0xDE, 0x35, 0xBD, 0x78, 0x88 },
        //            Type3 = new byte[] { 0x0B, 0x84, 0x50, 0xE0, 0x63, 0x52, 0x36, 0x74, 0x01, 0x1C, 0x6B, 0x2B, 0x94, 0x82, 0x9F, 0x7A }

        //        }
        //    }
        //};


        static readonly byte[] multi_iso_magic = {
            0x50,  // P
            0x53,  // S
            0x54,  // T
            0x49,  // I
            0x54,  // T
            0x4C,  // L
            0x45,  // E
            0x49,  // I
            0x4D,  // M
            0x47,  // G
            0x30,  // 0
            0x30,  // 0
            0x30,  // 0
            0x30,  // 0
            0x30,  // 0
            0x30   // 0
        };

        static readonly byte[] iso_magic = {
            0x50,  // P
            0x53,  // S
            0x49,  // I
            0x53,  // S
            0x4F,  // O
            0x49,  // I
            0x4D,  // M
            0x47,  // G
            0x30,  // 0
            0x30,  // 0
            0x30,  // 0
            0x30   // 0
        };

        static T ReadStruct<T>(BinaryReader reader) where T : struct
        {
            byte[] buff = reader.ReadBytes(Marshal.SizeOf<T>());
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            T t = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();
            return t;
        }

        static T ReadStruct<T>(Stream stream) where T : struct
        {
            Span<byte> buff = stackalloc byte[Unsafe.SizeOf<T>()];
            stream.Read(buff);
            T t = MemoryMarshal.AsRef<T>(buff);
            return t;
        }


        static bool CopyNormalData(Stream input, Stream output, int offset, int size)
        {
            if (size == 0)
            {
                return true;
            }
            if (offset + size < input.Length)
            {
                input.Seek(offset, SeekOrigin.Begin);
                var buff = new byte[size];
                var len = input.Read(buff);
                if (len != size)
                {
                    return false;
                }

                output.Seek(offset, SeekOrigin.Begin);
                output.Write(buff);
                return true;
            }
            return false;
        }

        static void XorTable(Span<uint> tp)
        {
            tp[4] ^= tp[3] ^ tp[2];
            tp[5] ^= tp[2] ^ tp[1];
            tp[6] ^= tp[0] ^ tp[3];
            tp[7] ^= tp[1] ^ tp[0];
        }

        static bool CopyNpUmdImg(Stream input, Stream output, PbpHeader pbpHdr, Span<byte> psarBuff, NpUmdImgHdr npHdr)
        {
            Span<byte> buff = stackalloc byte[0x800]; //
            int len;
            Span<byte> digest = stackalloc byte[0x14];
            SceDdrdb.sceDdrdbHash(psarBuff, 0xd8, digest);
            Span<byte> point = stackalloc byte[Marshal.SizeOf<KIRKEngine.ECDSA_POINT>()];
            KeyVault.Px2.AsSpan().CopyTo(point);
            KeyVault.Py2.AsSpan().CopyTo(point[0x14..]);
            var ret = SceDdrdb.sceDdrdbSigvry(point, digest, npHdr.ECDsaSig);
            if (ret != 0)
            {
                return false;
            }

            var vkey = new byte[0x10];
            Span<byte> mkey = stackalloc byte[Marshal.SizeOf<AMCTRL.MAC_KEY>()];
            AMCTRL.sceDrmBBMacInit(mkey, 3);
            AMCTRL.sceDrmBBMacUpdate(mkey, psarBuff, 0xc0);
            AMCTRL.bbmac_getkey(mkey, npHdr.HeaderHash, vkey);

            AMCTRL.sceDrmBBCipherInit(out var ckey, 1, 2, npHdr.HeaderKey, vkey, 0);
            AMCTRL.sceDrmBBCipherUpdate(ref ckey, psarBuff[0x40..], 0x60);
            AMCTRL.sceDrmBBCipherFinal(ref ckey);
            npHdr = Utils.AsRef<NpUmdImgHdr>(psarBuff);

            var lbasize = npHdr.Body.LbaEnd - npHdr.Body.LbaStart + 1;
            if (npHdr.BlockBasis == 0)
            {
                return false;
            }

            var totalBlocks = (int)(lbasize / npHdr.BlockBasis + (lbasize % npHdr.BlockBasis != 0 ? 1 : 0));
            var tablesize = totalBlocks * 0x20;

            if ((npHdr.Body.Unk40 & (1 << (Idps[5] & 0x1F))) == 0)
            {
                return false;
            }
            //Span<byte> newVkey = npHdr.NpFlags switch
            //{
            //    1 => Keys[CId].Fixed,
            //    2 => _keyGen.GetVersionKey(),
            //    3 => Keys[CId].Type3,
            //    _ => throw new NotSupportedException("unknown np flag")
            //};

            var paramSize = pbpHdr.Icon0Off - pbpHdr.ParamOff;
            var dataPspSize = pbpHdr.DataPsarOff - pbpHdr.DataPspOff;
            var buffSize = paramSize + 0x30;
            if (buffSize > 0x800)
            {
                return false;
            }

            if (pbpHdr.DataPspOff + dataPspSize >= input.Length)
            {
                return false;
            }

            Span<byte> paramSpan = stackalloc byte[paramSize];
            input.Seek(pbpHdr.ParamOff, SeekOrigin.Begin);
            input.Read(paramSpan);
            output.Seek(pbpHdr.ParamOff, SeekOrigin.Begin);
            output.Write(paramSpan);
            paramSpan.CopyTo(buff);
            npHdr.ContentId.CopyTo(buff[paramSize..]);
            input.Seek(pbpHdr.DataPspOff, SeekOrigin.Begin);
            len = input.Read(buff.Slice(paramSize + 0x30, 0x28));
            if (len < 0x28)
            {
                return false;
            }

            SceDdrdb.sceDdrdbHash(buff, paramSize + 0x30, digest);
            // sig.r = buff.Skip(paramSize + 0x30).Take(0x14).ToArray();
            // sig.s = buff.Skip(paramSize + 0x30 + 0x14).Take(0x14).ToArray();

            ret = SceDdrdb.sceDdrdbSigvry(point, digest, buff.Slice(paramSize + 0x30, 0x28));
            if (ret != 0)
            {
                return false;
            }

            var offset = input.Seek(pbpHdr.DataPspOff + 0x560, SeekOrigin.Begin);
            if (offset != pbpHdr.DataPspOff + 0x560)
            {
                return false;
            }
            len = input.Read(buff.Slice(0, 0x34));
            if (len != 0x34)
            {
                return false;
            }

            if (Encoding.ASCII.GetString(npHdr.ContentId).TrimEnd('\0') !=
                Encoding.ASCII.GetString(buff[..0x30]).TrimEnd('\0'))
            {
                return false;
            }

            var bufidx = 0x33;
            var off = Marshal.OffsetOf<NpUmdImgHdr>(nameof(npHdr.NpFlags)).ToInt32();
            for (int i = 0; i < 4; i++)
            {
                if (buff[bufidx - i] != psarBuff[off + i])
                {
                    return false;
                }
            }

            Span<byte> newCid = stackalloc byte[0x30];
            Encoding.ASCII.GetBytes(CId).AsSpan().CopyTo(newCid);

            // Copy PSP.DATA
            var pspDataSize = pbpHdr.DataPsarOff - pbpHdr.DataPspOff;
            Memory<byte> pspData = new byte[pspDataSize];
            input.Seek(pbpHdr.DataPspOff, SeekOrigin.Begin);
            input.Read(pspData.Span);

            paramSpan.CopyTo(buff);
            newCid.CopyTo(buff[paramSize..]);

            ECDsaHelper.SignParamSfo(buff[..(paramSize + 0x30)], pspData.Span);
            newCid.CopyTo(pspData.Span[0x560..]);
            var opnssmpOff = MemoryMarshal.Read<int>(pspData.Span[0x30..]);
            var opnssmpSize = MemoryMarshal.Read<int>(pspData.Span[0x34..]);
            if (opnssmpOff != 0 && opnssmpSize != 0)
            {
                var opnssmp = pspData.Slice(opnssmpOff, opnssmpSize);
                using var ms = opnssmp.AsStream();
                using var dnas = new DNASStream(ms, 0);
                Span<byte> opnssmpData = new byte[dnas.Length];
                dnas.Read(opnssmpData);
                DNASHelper.Encrypt(pspData.Span[opnssmpOff..], opnssmpData, NewVersionKey.Span, opnssmpData.Length, dnas.KeyIndex, 1);
            }

            output.Seek(pbpHdr.DataPspOff, SeekOrigin.Begin);
            output.Write(pspData.Span);


            AMCTRL.sceDrmBBMacInit(mkey, 3);
            var entityOff = pbpHdr.DataPsarOff + npHdr.Body.BlockEntryOffset;
            offset = input.Seek(entityOff, SeekOrigin.Begin);
            if (offset == entityOff)
            {
                if (tablesize > 0)
                {
                    buff = new byte[0x8000];
                    for (int i = 0; i < tablesize; i += 0x8000)
                    {
                        var tmpsize = tablesize - i;
                        if (tmpsize > 0x8000)
                        {
                            tmpsize = 0x8000;
                        }

                        len = input.Read(buff[..0x8000]);
                        if (len < 0x8000)
                        {
                            return false;
                        }

                        // sceAmctrl_driver_9227EA79
                        AMCTRL.sceDrmBBMacUpdate(mkey, buff, tmpsize);

                    }

                    ret = AMCTRL.sceDrmBBMacFinal2(mkey, npHdr.DataKey, vkey);
                    if (ret != 0)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            input.Seek(entityOff, SeekOrigin.Begin);
            Span<byte> table = new byte[tablesize];
            var tp = MemoryMarshal.Cast<byte, uint>(table);
            input.Read(table);

            // Decrypt Table
            for (int i = 0; i < totalBlocks; i++)
                XorTable(tp[(i * 8)..]);


            var blocks = MemoryMarshal.Cast<byte, NpBlock>(table);
            for (int i = 0; i < blocks.Length; i++)
            {
                Span<byte> blockData = new byte[blocks[i].Size];
                input.Seek(pbpHdr.DataPsarOff + blocks[i].Offset, SeekOrigin.Begin);
                input.Read(blockData);

                // Verify MAC
                AMCTRL.sceDrmBBMacInit(mkey, 3);
                AMCTRL.sceDrmBBMacUpdate(mkey, blockData, blocks[i].Size);
                ret = AMCTRL.sceDrmBBMacFinal2(mkey, blocks[i].Mac, vkey);
                if (ret != 0)
                {
                    return false;
                }

                // Decrypt block
                AMCTRL.sceDrmBBCipherInit(out ckey, 1, 2, npHdr.HeaderKey, vkey, blocks[i].Offset >> 4);
                AMCTRL.sceDrmBBCipherUpdate(ref ckey, blockData, blocks[i].Size);
                AMCTRL.sceDrmBBCipherFinal(ref ckey);

                // TODO LzrDecompress

                // Encrypt block
                AMCTRL.sceDrmBBCipherInit(out ckey, 1, 2, npHdr.HeaderKey, NewVersionKey.Span, blocks[i].Offset >> 4);
                AMCTRL.sceDrmBBCipherUpdate(ref ckey, blockData, blocks[i].Size);
                AMCTRL.sceDrmBBCipherFinal(ref ckey);

                // Build Mac
                AMCTRL.sceDrmBBMacInit(mkey, 3);
                AMCTRL.sceDrmBBMacUpdate(mkey, blockData, blocks[i].Size);
                AMCTRL.sceDrmBBMacFinal(mkey, blocks[i].Mac, NewVersionKey.Span);
                Utils.BuildDrmBBMacFinal2(blocks[i].Mac);
                output.Seek(pbpHdr.DataPsarOff + blocks[i].Offset, SeekOrigin.Begin);
                output.Write(blockData);
            }

            // Encrypt Table
            for (int i = 0; i < totalBlocks; i++)
                XorTable(tp[(i * 8)..]);

            output.Seek(entityOff, SeekOrigin.Begin);
            output.Write(table);

            AMCTRL.sceDrmBBMacInit(mkey, 3);
            AMCTRL.sceDrmBBMacUpdate(mkey, table, tablesize);
            AMCTRL.sceDrmBBMacFinal(mkey, npHdr.DataKey, NewVersionKey.Span);
            Utils.BuildDrmBBMacFinal2(npHdr.DataKey);
            newCid.CopyTo(npHdr.ContentId);

            // Encrypt NPUMDIMG body.
            AMCTRL.sceDrmBBCipherInit(out ckey, 1, 2, npHdr.HeaderKey, NewVersionKey.Span, 0);
            AMCTRL.sceDrmBBCipherUpdate(ref ckey, psarBuff[0x40..], 0x60);
            AMCTRL.sceDrmBBCipherFinal(ref ckey);

            // Generate header hash.
            AMCTRL.sceDrmBBMacInit(mkey, 3);
            AMCTRL.sceDrmBBMacUpdate(mkey, psarBuff, 0xC0);
            AMCTRL.sceDrmBBMacFinal(mkey, npHdr.HeaderHash, NewVersionKey.Span);
            Utils.BuildDrmBBMacFinal2(npHdr.HeaderHash);

            ECDsaHelper.SignNpImageHeader(psarBuff);
            output.Seek(pbpHdr.DataPsarOff, SeekOrigin.Begin);
            output.Write(psarBuff);

            return true;
        }

        static int CopyStartData(Stream input, Stream output, int psarOffset)
        {
            int startdat_offset;
            using var br = new BinaryReader(input, new UTF8Encoding(), true);
            startdat_offset = br.ReadInt32();
            if (startdat_offset > 0)
            {
                // Read the STARTDAT header
                br.BaseStream.Seek(psarOffset + startdat_offset, SeekOrigin.Begin);
                var startdat_header = ReadStruct<STARTDAT_HEADER>(br);
                br.BaseStream.Seek(psarOffset + startdat_offset, SeekOrigin.Begin);

                // Read the STARTDAT data.
                int startdat_size = startdat_header.header_size + startdat_header.data_size;
                byte[] startdat_data = br.ReadBytes(startdat_size);
                output.Seek(psarOffset + startdat_offset, SeekOrigin.Begin);
                output.Write(startdat_data, 0, startdat_size);
            }
            return startdat_offset;
        }

        static bool DecryptIsoHeader(Stream input, Span<byte> header, int header_offset, out int block_size)
        {
            // Seek to the ISO header.
            // input.Seek(header_offset, SeekOrigin.Current);

            // Read the ISO header.
            using var dnas = new DNASStream(input, input.Position + header_offset);
            block_size = dnas.BlockSize;
            dnas.VersionKey.CopyTo(VersionKey.Span);
            if (header.Length == dnas.Length)
            {
                dnas.Read(header);
                return true;
            }
            return false;
        }

        static bool CopySimpleData(Stream input, Stream output, PbpHeader pbpHdr, int simple_data_offset)
        {
            if (simple_data_offset > 0)
            {
                using var dnas = new DNASStream(input, pbpHdr.DataPsarOff + simple_data_offset); 
                Span<byte> simpleData = new byte[dnas.Length];
                if (dnas.Read(simpleData) == dnas.Length)
                {
                    var simpleDataEnc = new byte[input.Length - pbpHdr.DataPsarOff - simple_data_offset];
                    DNASHelper.Encrypt(simpleDataEnc, simpleData, NewVersionKey.Span, simpleData.Length, dnas.KeyIndex, 1, blockSize: dnas.BlockSize);
                    output.Seek(pbpHdr.DataPsarOff + simple_data_offset, SeekOrigin.Begin);
                    output.Write(simpleDataEnc);
                    return true;
                }
            }
            return false;
        }

        static bool CopyUnknownData(Stream input, Stream output, PbpHeader pbpHdr, int unknown_data_offset, int startdat_offset)
        {
            if (unknown_data_offset > 0)
            {
                input.Seek(pbpHdr.DataPsarOff, SeekOrigin.Begin);
                using var dnas = new DNASStream(input, input.Position + unknown_data_offset, VersionKey.Span);
                Span<byte> unknownData = new byte[dnas.Length];
                if (dnas.Read(unknownData) == dnas.Length)
                {
                    var unknownDataEnc = new byte[startdat_offset - unknown_data_offset];
                    DNASHelper.Encrypt(unknownDataEnc, unknownData, NewVersionKey.Span, unknownData.Length, dnas.KeyIndex, 1, blockSize: dnas.BlockSize);
                    output.Seek(pbpHdr.DataPsarOff + unknown_data_offset, SeekOrigin.Begin);
                    output.Write(unknownDataEnc);
                    return true;
                }
            }
            return false;
        }

        static bool CopyAudio(Stream input, Stream output, ReadOnlySpan<byte> iso_table, PbpHeader pbpHdr, int base_offset = 0)
        {

            // Set CDDA entry.
            CDDA_ENTRY audio_entry;

            var iso_offset = MemoryMarshal.Read<int>(iso_table[0x7fc..]);

            // Start of the ISO data.
            var iso_base_offset = iso_offset + base_offset;

            // Start the audio track number counter at 2 (data track is always the first one).
            int audio_track_count = 2;



            // Read the audio track table (starts at 0x800 and ends at offset 0xE20).
            for (var audio_offset = 0x800; audio_offset < 0xE20; audio_offset += Unsafe.SizeOf<CDDA_ENTRY>())
            {
                // Read the CDDA entry.
                audio_entry = MemoryMarshal.Read<CDDA_ENTRY>(iso_table[audio_offset..]);

                // Reached the last entry.
                if (audio_entry.offset == 0)
                {
                    break;
                }

                // Locate the block offset in the DATA.PSAR.
                input.Seek(pbpHdr.DataPsarOff + iso_base_offset + audio_entry.offset, SeekOrigin.Begin);
                output.Seek(pbpHdr.DataPsarOff + iso_base_offset + audio_entry.offset, SeekOrigin.Begin);

                // Read the data.
                Span<byte> track_data = new byte[audio_entry.size];
                input.Read(track_data);
                output.Write(track_data);

                // Increment the track counter.
                audio_track_count++;
            }
            return true;
        }

        static bool CopyIso(Stream input, Stream output, ReadOnlySpan<byte> iso_table, Span<byte> header, int base_offset, PbpHeader pbpHdr, int hdrBlockSize)
        {
            var header_offset = base_offset + 0x400;
            var header_size = 0xB6600;

            // Setup buffers.
            int iso_block_size = 0x9300;
            Span<byte> iso_block_comp = new byte[iso_block_size];  // Compressed block.
            byte[] iso_block_decomp = new byte[iso_block_size]; // Decompressed block.

            // Locate the block table.
            int table_offset = 0x3C00;  // Fixed offset.

            int iso_base_offset = 0x100000 + base_offset;  // Start of compressed ISO data.

            // Read the first entry.
            var entry = MemoryMarshal.Read<ISO_ENTRY>(iso_table[table_offset..]);
            var entries = MemoryMarshal.Cast<byte, ISO_ENTRY>(header[0x3C00..]);
            var i = 0;

            Span<byte> mkey = stackalloc byte[Marshal.SizeOf<PspCrypto.AMCTRL.MAC_KEY>()];
            // Keep reading entries until we reach the end of the table.
            while (entry.size > 0)
            {
                // Locate the block offset in the DATA.PSAR.
                var block = iso_block_comp[..entry.size];
                input.Seek(pbpHdr.DataPsarOff + iso_base_offset + entry.offset, SeekOrigin.Begin);
                input.Read(block);
                output.Seek(pbpHdr.DataPsarOff + iso_base_offset + entry.offset, SeekOrigin.Begin);
                output.Write(block);

                PspCrypto.AMCTRL.sceDrmBBMacInit(mkey, 3);
                PspCrypto.AMCTRL.sceDrmBBMacUpdate(mkey, iso_block_comp, entry.size);
                int ret = PspCrypto.AMCTRL.sceDrmBBMacFinal2(mkey, entries[i].checksum, VersionKey.Span);
                if (ret != 0)
                {
                    Console.WriteLine("ERROR: BLOCK CROP");
                    return false;
                }
                PspCrypto.AMCTRL.sceDrmBBMacInit(mkey, 3);
                PspCrypto.AMCTRL.sceDrmBBMacUpdate(mkey, iso_block_comp, entry.size);
                Span<byte> checksum = new byte[20 + 0x10];
                PspCrypto.AMCTRL.sceDrmBBMacFinal(mkey, checksum[20..], NewVersionKey.Span);

                ref var aesHdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_AES128CBC_HEADER>(checksum);
                aesHdr.mode = KIRKEngine.KIRK_MODE_ENCRYPT_CBC;
                aesHdr.keyseed = 0x63;
                aesHdr.data_size = 0x10;
                PspCrypto.KIRKEngine.sceUtilsBufferCopyWithRange(checksum, 0x10, checksum, 0x10,
                    KIRKEngine.KIRK_CMD_ENCRYPT_IV_0);
                checksum.Slice(20, 0x10).CopyTo(entries[i].checksum);
                // Go to next entry.
                table_offset += Marshal.SizeOf<ISO_ENTRY>();
                entry = MemoryMarshal.Read<ISO_ENTRY>(iso_table[table_offset..]);
                i++;
            }
            Span<byte> headerEnc = new byte[header_size];
            DNASHelper.Encrypt(headerEnc, header, NewVersionKey.Span, header.Length, 1, 1, blockSize: hdrBlockSize);
            output.Seek(pbpHdr.DataPsarOff + header_offset, SeekOrigin.Begin);
            output.Write(headerEnc);
            output.Flush();

            return true;
        }

        static bool CopyPsxLoader(Stream input, Stream output, PbpHeader pbpHdr)
        {
            var pspDataSize = pbpHdr.DataPsarOff - pbpHdr.DataPspOff;
            Memory<byte> loader = new byte[pspDataSize];
            input.Seek(pbpHdr.DataPspOff, SeekOrigin.Begin);
            input.Read(loader.Span);

            Span<byte> config = new byte[0x410];
            loader.Span.Slice(0x150, 0x410).CopyTo(config);
            var hdr = MemoryMarshal.AsRef<PSPHeader2>(loader.Span);
            var ret = SceMesgLed.sceMesgLed_driver_EBB4613D(loader.Span, hdr.pspSize, out var newSize, VersionKey.Span);
            if (ret != 0)
            {
                return false;
            }
            Span<byte> decMod;
            if (loader.Span[0] == 0x1f && loader.Span[1] == 0x8b)
            {
                using var ms = loader.AsStream();
                using var gz = new GZipStream(ms, CompressionMode.Decompress);
                using var decMs = new MemoryStream();
                gz.CopyTo(decMs);
                decMod = decMs.ToArray();
            }
            else
            {
                decMod = loader.Span[..newSize];
            }


            Span<byte> loaderEnc = new byte[pspDataSize];
            var key = Convert.ToHexString(NewVersionKey.Span);
            SceMesgLed.Encrypt(loaderEnc, decMod, hdr.tag, SceExecFileDecryptMode.DECRYPT_MODE_POPS_EXEC, NewVersionKey.Span, CId, config);
            output.Seek(pbpHdr.DataPspOff, SeekOrigin.Begin);
            output.Write(loaderEnc);
            return true;
        }

        static bool CopyPsIsoImg(Stream input, Stream output, PbpHeader pbpHdr)
        {
            input.Seek(pbpHdr.DataPsarOff + 12, SeekOrigin.Begin);
            int startdat_offset = CopyStartData(input, output, pbpHdr.DataPsarOff);

            // Decrypt the ISO header and get the block table.
            // NOTE: In a single disc, the ISO header is located at offset 0x400 and has a length of 0xB6600.
            Span<byte> header = new byte[0x400 + 0xb3880];
            input.Seek(pbpHdr.DataPsarOff, SeekOrigin.Begin);
            input.Read(header[..0x400]);

            output.Seek(pbpHdr.DataPsarOff, SeekOrigin.Begin);
            output.Write(header[..0x400]);
            Span<byte> iso_table = header[0x400..];

            input.Seek(pbpHdr.DataPsarOff, SeekOrigin.Begin);
            if (!DecryptIsoHeader(input, iso_table, 0x400, out var hdrBlockSize))
            {
                return false;
            }

            // Save the ISO disc name and title (UTF-8).
            ReadOnlySpan<byte> iso_disc_name = iso_table[..0x10];
            ReadOnlySpan<byte> iso_title = iso_table.Slice(0xE2C, 0x80);
            string iso_disc_name_utf8 = Encoding.UTF8.GetString(iso_disc_name).TrimEnd('\0');
            string iso_title_utf8 = Encoding.UTF8.GetString(iso_title).TrimEnd('\0');
            Console.WriteLine($"ISO disc: {iso_disc_name_utf8}");
            Console.WriteLine($"ISO title: {iso_title_utf8}\n");

            // Seek inside the ISO table to find the SIMPLE data offset.
            int simple_data_offset = MemoryMarshal.Read<int>(iso_table[0xE20..]);

            if (!CopySimpleData(input, output, pbpHdr, simple_data_offset))
            {
                Console.WriteLine("CopySimpleData failed!");
                return false;
            }

            // Seek inside the ISO table to find the unknown data offset.
            int unknown_data_offset = MemoryMarshal.Read<int>(iso_table[0xEDE..]);

            if (unknown_data_offset > 0)
            {
                if (!CopyUnknownData(input, output, pbpHdr, unknown_data_offset, startdat_offset))
                {
                    Console.WriteLine("CopyUnknownData failed");
                    return false;
                }
            }

            // Extract the CDDA tracks.
            if (!CopyAudio(input, output, iso_table, pbpHdr))
            {
                Console.WriteLine("CopyAudio failed");
                return false;
            }

            if (!CopyIso(input, output, iso_table, header[0x400..], 0, pbpHdr, hdrBlockSize))
            {
                Console.WriteLine("CopyIso failed");
                return false;
            }

            if (!CopyPsxLoader(input, output, pbpHdr))
            {
                Console.WriteLine("CopyPsxLoader failed");
                return false;
            }

            return true;
        }


        static bool ReadIsoMap(Stream input, PbpHeader pbpHdr, Span<byte> isoMap)
        {
            var mapOffset = 0x200;
            using var dnas = new DNASStream(input, pbpHdr.DataPsarOff + mapOffset, VersionKey.Span);
            Span<byte> buffer = new byte[dnas.Length];
            if (dnas.Read(buffer) == dnas.Length)
            {
                buffer.CopyTo(isoMap);
                return true;
            }
            return false;
        }

        static void WriteIsoMap(Stream output, PbpHeader pbpHdr, ReadOnlySpan<byte> isoMap)
        {
            var mapOffset = 0x200;
            var mapSize = 0x2A0;
            var isoMapEnc = new byte[mapSize];
            DNASHelper.Encrypt(isoMapEnc, isoMap, NewVersionKey.Span, isoMap.Length, 1, 1);
            output.Seek(pbpHdr.DataPsarOff + mapOffset, SeekOrigin.Begin);
            output.Write(isoMapEnc);
        }

        static bool CopyPsTitleImg(Stream input, Stream output, PbpHeader pbpHdr)
        {
            input.Seek(pbpHdr.DataPsarOff, SeekOrigin.Begin);
            output.Seek(pbpHdr.DataPsarOff, SeekOrigin.Begin);
            Span<byte> psTitle = new byte[200];
            input.Read(psTitle);
            output.Write(psTitle);
            output.Flush();

            input.Seek(pbpHdr.DataPsarOff + 16, SeekOrigin.Begin);
            int startdat_offset = CopyStartData(input, output, pbpHdr.DataPsarOff);
            Span<byte> isoMap = new byte[0x200];
            if (!ReadIsoMap(input, pbpHdr, isoMap))
            {
                Console.WriteLine("ReadIsoMap failed");
                return false;
            }

            Span<int> discOffsets = isoMap[..20].Cast<byte, int>();
            Span<byte> macKeys = isoMap.Slice(20, 16 * 5);

            ReadOnlySpan<byte> iso_disc_name = isoMap.Slice(0x64, 0x20);
            ReadOnlySpan<byte> iso_title = isoMap.Slice(0x10C, 0x80);
            string iso_disc_name_utf8;
            string iso_title_utf8;
            unsafe
            {
                fixed (byte* ptr = iso_disc_name)
                {
                    ReadOnlySpan<byte> disc = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(ptr);
                    iso_disc_name_utf8 = Encoding.UTF8.GetString(disc);
                }
                fixed (byte* ptr = iso_title)
                {
                    ReadOnlySpan<byte> title = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(ptr);
                    iso_title_utf8 = Encoding.UTF8.GetString(title);
                }
            }
            Console.WriteLine($"ISO disc: {iso_disc_name_utf8}");
            Console.WriteLine($"ISO title: {iso_title_utf8}\n");

            int simple_data_offset = MemoryMarshal.Read<int>(isoMap[0x84..]);
            if (!CopySimpleData(input, output, pbpHdr, simple_data_offset))
            {
                Console.WriteLine("CopySimpleData failed");
                return false;
            }


            // Build each valid ISO image.
            Span<byte> mkey = stackalloc byte[Marshal.SizeOf<PspCrypto.AMCTRL.MAC_KEY>()];
            for (int i = 0; i < discOffsets.Length; i++)
            {
                var diskOffset = discOffsets[i];
                if (diskOffset > 0)
                {
                    Span<byte> header = new byte[0x400 + 0xb3880];
                    input.Seek(pbpHdr.DataPsarOff + diskOffset, SeekOrigin.Begin);
                    input.Read(header[..0x400]);
                    output.Seek(pbpHdr.DataPsarOff + diskOffset, SeekOrigin.Begin);
                    output.Write(header[..0x400]);
                    Span<byte> iso_table = header[0x400..];

                    input.Seek(pbpHdr.DataPsarOff + diskOffset, SeekOrigin.Begin);
                    if (!DecryptIsoHeader(input, iso_table, 0x400, out var hdrBlockSize))
                    {
                        return false;
                    }
                    int ret = PspCrypto.AMCTRL.sceDrmBBMacInit(mkey, 3);
                    ret = PspCrypto.AMCTRL.sceDrmBBMacUpdate(mkey, header, 0xb3c80);
                    ret = PspCrypto.AMCTRL.sceDrmBBMacFinal2(mkey, macKeys[(i * 0x10)..], VersionKey.Span);
                    if (ret != 0)
                    {
                        Console.WriteLine("ERROR: Header miss match");
                        return false;
                    }

                    var unknown = MemoryMarshal.Read<int>(iso_table[0xEDE..]);
                    Console.WriteLine($"unknown {unknown}");

                    // Extract the CDDA tracks.
                    if (!CopyAudio(input, output, iso_table, pbpHdr, diskOffset))
                    {
                        Console.WriteLine("CopyAudio failed");
                        return false;
                    }

                    if (!CopyIso(input, output, iso_table, header[0x400..], diskOffset, pbpHdr, hdrBlockSize))
                    {
                        Console.WriteLine($"CopyIso disc{i} failed");
                        return false;
                    }
                    ret = PspCrypto.AMCTRL.sceDrmBBMacInit(mkey, 3);
                    ret = PspCrypto.AMCTRL.sceDrmBBMacUpdate(mkey, header, 0xb3c80);
                    Span<byte> newKey = new byte[20 + 0x10];
                    ret = PspCrypto.AMCTRL.sceDrmBBMacFinal(mkey, newKey[20..], NewVersionKey.Span);
                    ref var aesHdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_AES128CBC_HEADER>(newKey);
                    aesHdr.mode = KIRKEngine.KIRK_MODE_ENCRYPT_CBC;
                    aesHdr.keyseed = 0x63;
                    aesHdr.data_size = 0x10;
                    PspCrypto.KIRKEngine.sceUtilsBufferCopyWithRange(newKey, 0x10, newKey, 0x10,
                        KIRKEngine.KIRK_CMD_ENCRYPT_IV_0);
                    newKey.Slice(20, 0x10).CopyTo(macKeys[(i * 0x10)..]);
                }
            }
            WriteIsoMap(output, pbpHdr, isoMap);

            if (!CopyPsxLoader(input, output, pbpHdr))
            {
                Console.WriteLine("CopyPsxLoader failed");
                return false;
            }

            return true;
        }

        static int GetKeyType(Stream input, PbpHeader pbpHdr)
        {
            input.Seek(pbpHdr.DataPspOff + 0x560, SeekOrigin.Begin);
            Span<byte> contentInfo = new byte[0x34];
            input.Read(contentInfo);
            var keyType = BinaryPrimitives.ReadInt32BigEndian(contentInfo[0x30..]);
            return keyType;
        }

        static bool CopyData(Stream input, Stream output, PbpHeader pbpHdr, out int type)
        {
            type = -1;
            input.Seek(pbpHdr.DataPsarOff, SeekOrigin.Begin);
            Span<byte> psarBuff = stackalloc byte[0x100];
            var len = input.Read(psarBuff);
            if (len != 0x100)
            {
                return false;
            }

            ref var npHdr = ref Utils.AsRef<NpUmdImgHdr>(psarBuff);

            if (npHdr.Magic0 == 0x4d55504e && npHdr.Magic1 == 0x474d4944)
            {
                int ret = _keyGen.GetVersionKey(NewVersionKey.Span, npHdr.NpFlags);
                if (ret != 0)
                {
                    Console.WriteLine($"GetVersionKey {npHdr.NpFlags} failed");
                    return false;
                }
                type = 0;

                Console.WriteLine("VersionKey: " + BitConverter.ToString(NewVersionKey.ToArray()));

                NpUmdImg npumd = new NpUmdImg(new NpDrmInfo(NewVersionKey.ToArray(), CId, npHdr.NpFlags),
                                                "fft.iso", "ULUS10297", File.ReadAllBytes("TEST\\PARAM.SFO"), false);

                npumd.CreatePsar();
                byte[] paramFile = File.ReadAllBytes("TEST\\PARAM.SFO");

                PbpBuilder.CreatePbp(paramFile, File.ReadAllBytes("TEST\\ICON0.PNG"), null, File.ReadAllBytes("TEST\\PIC0.PNG"), File.ReadAllBytes("TEST\\PIC1.PNG"), null, npumd, "FFT.PBP");
                
                return CopyNpUmdImg(input, output, pbpHdr, psarBuff, npHdr);
            }

            if (psarBuff[..12].SequenceEqual(iso_magic))
            {
                var keyType = GetKeyType(input, pbpHdr);
                int ret = _keyGen.GetVersionKey(NewVersionKey.Span, keyType);
                if (ret != 0)
                {
                    Console.WriteLine("GetVersionKey 1 failed");
                    return false;
                }
                type = 1;
                /*DiscInfo[] discs = new DiscInfo[2];
                discs[0] = new DiscInfo("ABEE\\D1.CUE", "Oddworld: Abe's Exoddus", "SLES01480");
                discs[1] = new DiscInfo("ABEE\\D2.CUE", "Oddworld: Abe's Exoddus", "SLES11480");
                PsTitleImg title = new PsTitleImg(NewVersionKey.ToArray(), CId, discs);
                title.CreatePsar();
                PbpBuilder.CreatePbp(File.ReadAllBytes("TEST\\PARAM.SFO"), File.ReadAllBytes("TEST\\ICON0.PNG"), null,
                                     File.ReadAllBytes("TEST\\PIC0.PNG"), File.ReadAllBytes("TEST\\PIC1.PNG"), null,
                                     title.GenerateDataPsp(), title, "ABE-EBOOT.PBP");
                //PsIsoImg i = new PsIsoImg("ROLLCAGE\\ROLLCAGE.CUE", "SLUS00800", "ROLLCAGE", CId, NewVersionKey.ToArray(),
                //     File.ReadAllBytes("TEST\\PARAM.SFO"), File.ReadAllBytes("TEST\\ICON0.PNG"), null,
                //     File.ReadAllBytes("TEST\\PIC0.PNG"), File.ReadAllBytes("TEST\\PIC1.PNG"), null);
                //File.WriteAllBytes("TEST.BIN", i.GetIsoHeader());
                //File.WriteAllBytes("TEST.ISOc", i.GetIso());*/


                 return CopyPsIsoImg(input, output, pbpHdr);
            }

            if (psarBuff[..16].SequenceEqual(multi_iso_magic))
            {
                var keyType = GetKeyType(input, pbpHdr);
                int ret = _keyGen.GetVersionKey(NewVersionKey.Span, keyType);
                if (ret != 0)
                {
                    Console.WriteLine("GetVersionKey 1 failed");
                    return false;
                }
                type = 2;
                return CopyPsTitleImg(input, output, pbpHdr);
            }

            return false;
        }

        class KeyGen
        {
            private readonly byte[] _actData;
            private readonly byte[] _rifData;

            public KeyGen(string rifName)
            {
                _actData = File.ReadAllBytes("act.dat");
                _rifData = File.ReadAllBytes(rifName);
            }

            public int GetVersionKey(Span<byte> versionKey, int type) => SceNpDrm.sceNpDrmGetVersionKey(versionKey, _actData, _rifData, type);
        }

        static bool CopyDocument(string src, string dist)
        {
            using var fs = File.OpenRead(src);

            return true;
        }

        

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.Write("Usage PbpResign.exe accountId contentId input");
                return;
            }

            if (!File.Exists("act.dat"))
            {
                Console.WriteLine("act.dat not exist");
                return;
            }
            if (!File.Exists("psid"))
            {
                Console.WriteLine("psid not exist");
                return;
            }

            var aid = args[0];
            if (aid.Length < 16)
            {
                aid = aid.PadLeft(16, '0');
            }
            var aidData = Convert.FromHexString(aid);
            SceNpDrm.Aid = BitConverter.ToUInt64(aidData);
            var cmaKey = CmaKeys.GenerateKey(aid);
            Console.WriteLine(Convert.ToHexString(cmaKey));

            var srcPbp = args[2];
            if (!File.Exists(srcPbp))
            {
                Console.WriteLine($"{srcPbp} not exist");
                return;
            }

            Idps = File.ReadAllBytes("psid");
            SceNpDrm.SetPSID(Idps);
            CId = args[1];
            var rifName = $"{CId}.rif";
            if (!File.Exists(rifName))
            {
                Console.WriteLine($"{rifName} not exist");
                return;
            }

            _keyGen = new KeyGen(rifName);


            var srcPath = Path.GetDirectoryName(srcPbp);

           // try
           // {
                using var input = File.OpenRead(srcPbp);
                var hdr = new byte[Marshal.SizeOf<PbpHeader>()];
                var len = input.Read(hdr);
                if (len != hdr.Length)
                {
                    Console.WriteLine("Wrong input");
                    return;
                }

                var pbpHdr = Utils.AsRef<PbpHeader>(hdr);
                if (pbpHdr.Sig != 0x50425000)
                {
                    Console.WriteLine("Wrong pbp sig");
                    return;
                }

                input.Seek(pbpHdr.ParamOff, SeekOrigin.Begin);
                var paramSize = pbpHdr.Icon0Off - pbpHdr.ParamOff;
                Span<byte> param = new byte[paramSize];
                input.Read(param);
                var sfoDic = Sfo.ReadSfo(param);
                string distPath;
                if (sfoDic["DISC_ID"] is string diskId)
                {
                    distPath = Path.Combine(srcPath, $"signed\\game\\ux0_pspemu_temp_game_PSP_GAME_{diskId}");
                }
                else
                {
                    Console.WriteLine("Can't get diskid");
                    return;
                }
                if (!Directory.Exists(distPath))
                {
                    Directory.CreateDirectory(distPath);
                }

                var psxDoc = Path.Combine(srcPath, "DOCUMENT.DAT");
                var psxDistDoc = Path.Combine(distPath, "DOCUMENT.DAT");
                if (File.Exists(psxDoc))
                {
                    File.Copy(psxDoc, psxDistDoc, true);
                }

                var vitaPath = Path.Combine(distPath, "VITA_PATH.TXT");
                var vitaPathData = $"ux0:pspemu/temp/game/PSP/GAME/{diskId}\0";
                File.WriteAllText(vitaPath, vitaPathData);
                var licensePath = Path.Combine(srcPath, $"signed\\license\\ux0_pspemu_temp_game_PSP_LICENSE");
                if (!Directory.Exists(licensePath))
                {
                    Directory.CreateDirectory(licensePath);
                }
                var distLicense = Path.Combine(licensePath, rifName);
                File.Copy(rifName, distLicense, true);
                vitaPath = Path.Combine(licensePath, "VITA_PATH.TXT");
                vitaPathData = "ux0:pspemu/temp/game/PSP/LICENSE\0";
                File.WriteAllText(vitaPath, vitaPathData);

                var sceSysPath = Path.Combine(srcPath, "signed\\sce_sys");
                if (!Directory.Exists(sceSysPath))
                {
                    Directory.CreateDirectory(sceSysPath);
                }

                var paramPath = Path.Combine(sceSysPath, "param.sfo");
                using (var fs = File.Create(paramPath))
                {
                    fs.Write(param);
                    fs.Flush();
                }


                var distPbp = Path.Combine(distPath, "EBOOT.PBP");

                using var output = File.OpenWrite(distPbp);
                output.SetLength(input.Length);
                output.Write(hdr);

                if (!CopyNormalData(input, output, pbpHdr.ParamOff, paramSize))
                {
                    Console.WriteLine("Wrong PARAM.SFO data");
                    return;
                }


                var icon0Size = pbpHdr.Icon1Off - pbpHdr.Icon0Off;
                if (!CopyNormalData(input, output, pbpHdr.Icon0Off, icon0Size))
                {
                    Console.WriteLine("Wrong ICON0.PNG data");
                    return;
                }
                Span<byte> icon0 = new byte[icon0Size];
                input.Seek(pbpHdr.Icon0Off, SeekOrigin.Begin);
                input.Read(icon0);
                var icon0Path = Path.Combine(sceSysPath, "icon0.png");
                using (var fs = File.Create(icon0Path))
                {
                    fs.Write(icon0);
                    fs.Flush();
                }


                var icon1Size = pbpHdr.Pic0Off - pbpHdr.Icon1Off;
                if (!CopyNormalData(input, output, pbpHdr.Icon1Off, icon1Size))
                {
                    Console.WriteLine("Wrong ICON1.PMF/ICON1.PNG data");
                    return;
                }

                var pic0Size = pbpHdr.Pic1Off - pbpHdr.Pic0Off;
                if (!CopyNormalData(input, output, pbpHdr.Pic0Off, pic0Size))
                {
                    Console.WriteLine("Wrong PIC0.PNG/UNKNOWN.PNG data");
                    return;
                }

                var pic1Size = pbpHdr.Snd0Off - pbpHdr.Pic1Off;
                if (!CopyNormalData(input, output, pbpHdr.Pic1Off, pic1Size))
                {
                    Console.WriteLine("Wrong PIC1.PNG/PICT1.PNG data");
                    return;
                }

                var snd0Size = pbpHdr.DataPspOff - pbpHdr.Snd0Off;
                if (!CopyNormalData(input, output, pbpHdr.Snd0Off, snd0Size))
                {
                    Console.WriteLine("Wrong SND0.AT3 data");
                    return;
                }

                if (!CopyData(input, output, pbpHdr, out var type))
                {
                    Console.WriteLine("Wrong DATA.PSP/DATA.PSAR data");
                }
                else
                {
                    Console.WriteLine($"Resign {srcPbp} to {distPbp} Successful");
                    var sigPath = Path.Combine(distPath, "__sce_ebootpbp");
                    input.Close();
                    output.Close();
                    Span<byte> ebootsig = stackalloc byte[0x200];
                    switch (type)
                    {
                        case 0:
                            SceNpDrm.KsceNpDrmEbootSigGenPsp(distPbp, ebootsig, 0x3600000);
                            File.WriteAllBytes(sigPath, ebootsig.ToArray());
                            break;
                        case 1:
                            SceNpDrm.KsceNpDrmEbootSigGenPs1(distPbp, ebootsig, 0x3600000);
                            File.WriteAllBytes(sigPath, ebootsig.ToArray());
                            break;
                        case 2:
                            SceNpDrm.KsceNpDrmEbootSigGenPs1(distPbp, ebootsig, 0x3600000);
                            File.WriteAllBytes(sigPath, ebootsig.ToArray());
                            break;
                    }
                    //var srcDir = Path.GetDirectoryName(srcPbp);
                    //var documentPath = Path.Combine(srcDir, "DOCUMENT.DAT");
                    //var newDocumentPath = Path.Combine(distDir, "DOCUMENT_MOD.DAT");
                    //if (File.Exists(documentPath))
                    //{
                    //    CopyDocument(documentPath, newDocumentPath);
                    //}
                }

            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //}

        }
    }
}
