using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using PspCrypto.Security.Cryptography;

namespace PspCrypto
{
    public static class SceNpDrm
    {
        unsafe struct SceEbootPbp
        {
            public ulong Magic;
            public int KeyType;
            public int Type;
            private fixed byte _contentId[0x30];

            public Span<byte> ContentId
            {
                get
                {
                    fixed (byte* ptr = _contentId)
                    {
                        return new Span<byte>(ptr, 0x30);
                    }
                }
            }

            public ulong Aid;
            public ulong SecureTick;
            public long PbpSize;
            public int SwVer;
            public int DiscCount;
            private fixed long _discOffsets[6];

            public Span<long> DiscOffsets
            {
                get
                {
                    fixed (long* ptr = _discOffsets)
                    {
                        return new Span<long>(ptr, 6);
                    }
                }
            }
            private fixed byte _padding[0xC8];
            private fixed byte _pbpHdrSig[0x38];

            public Span<byte> PbpHdrSig
            {
                get
                {
                    fixed (byte* ptr = _pbpHdrSig)
                    {
                        return new Span<byte>(ptr, 0x38);
                    }
                }
            }
            private fixed byte _npUmdImgSig[0x38];

            public Span<byte> NpUmdImgSig
            {
                get
                {
                    fixed (byte* ptr = _npUmdImgSig)
                    {
                        return new Span<byte>(ptr, 0x38);
                    }
                }
            }
            private fixed byte _sig[0x38];

            public Span<byte> Sig
            {
                get
                {
                    fixed (byte* ptr = _sig)
                    {
                        return new Span<byte>(ptr, 0x38);
                    }
                }
            }
        }
        unsafe struct SceEbootPbp100
        {
            public ulong Magic;
            public int KeyType;
            public int Type;
            private fixed byte _contentId[0x30];

            public Span<byte> ContentId
            {
                get
                {
                    fixed (byte* ptr = _contentId)
                    {
                        return new Span<byte>(ptr, 0x30);
                    }
                }
            }
            private fixed byte _padding[0x18];
            private fixed byte _pbpHdrSig[0x38];

            public Span<byte> PbpHdrSig
            {
                get
                {
                    fixed (byte* ptr = _pbpHdrSig)
                    {
                        return new Span<byte>(ptr, 0x38);
                    }
                }
            }
            private fixed byte _npUmdImgSig[0x38];

            public Span<byte> NpUmdImgSig
            {
                get
                {
                    fixed (byte* ptr = _npUmdImgSig)
                    {
                        return new Span<byte>(ptr, 0x38);
                    }
                }
            }
            private fixed byte _sig[0x38];

            public Span<byte> Sig
            {
                get
                {
                    fixed (byte* ptr = _sig)
                    {
                        return new Span<byte>(ptr, 0x38);
                    }
                }
            }
        }

        unsafe struct sceDiscInfo
        {
            private fixed byte _id[0x30];

            public Span<byte> Id
            {
                get
                {
                    fixed (byte* ptr = _id)
                    {
                        return new Span<byte>(ptr, 0x30);
                    }
                }
            }

            public int Version;
            public int DiscCount;
            public long FileSize;
            private fixed long _diskOffsets[6];

            public Span<long> Offsets
            {
                get
                {
                    fixed (long* ptr = _diskOffsets)
                    {
                        return new Span<long>(ptr, 6);
                    }
                }
            }

            private fixed byte _pad[0x20];
            private fixed byte _discsSig[0x38];

            public Span<byte> DiscsSig
            {
                get
                {
                    fixed (byte* ptr = _discsSig)
                    {
                        return new Span<byte>(ptr, 0x38);
                    }
                }
            }
            private fixed byte _sig[0x38];

            public Span<byte> Sig
            {
                get
                {
                    fixed (byte* ptr = _sig)
                    {
                        return new Span<byte>(ptr, 0x38);
                    }
                }
            }

        }

        public static ulong Aid { get; set; }

        public static byte[] SceDiskInfo => Resource.__sce_discinfo;

        private const int BLK_SIZE = 0x7C0;

        private static readonly Memory<byte> _memory = new byte[0x1000];

        private static byte[] _psId;

        private static long _fuseId;

        public static void SetPSID(byte[] psid)
        {
            _psId = psid;
        }

        public static void SetFuseId(long fuseId)
        {
            _fuseId = fuseId;
        }

        public static void SetFuseId(Span<byte> fuseId)
        {
            SetFuseId(MemoryMarshal.Read<long>(fuseId));
        }

        public unsafe struct SceNpDrmKey
        {
            private fixed byte _KeyData[16];
            public Span<byte> KeyData
            {
                get
                {
                    fixed (byte* ptr = _KeyData)
                    {
                        return new Span<byte>(ptr, 16);
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct Rif
        {
            private short _version;
            private short _versionFlag;
            private int _drmType;
            private fixed byte _accountId[8];
            private fixed byte _contentId[0x30];
            private fixed byte _encKey1[0x10];
            private fixed byte _encKey2[0x10];
            private long _startTime;
            private long _endTime;
            private fixed byte _ecdsaSig[0x28];
            public short Version => _version;
            public short VersionFlag => _versionFlag;
            public int DrmType => _drmType;
            public Span<byte> AccountId
            {
                get
                {
                    fixed (byte* ptr = _accountId)
                    {
                        return new Span<byte>(ptr, 8);
                    }
                }
            }
            public Span<byte> ContentId
            {
                get
                {
                    fixed (byte* ptr = _contentId)
                    {
                        return new Span<byte>(ptr, 0x30);
                    }
                }
            }
            public Span<byte> EncKey1
            {
                get
                {
                    fixed (byte* ptr = _encKey1)
                    {
                        return new Span<byte>(ptr, 0x10);
                    }
                }
            }
            public Span<byte> EncKey2
            {
                get
                {
                    fixed (byte* ptr = _encKey2)
                    {
                        return new Span<byte>(ptr, 0x10);
                    }
                }
            }
            public long StartTime => _startTime;
            public long EndTime => _endTime;

            public Span<byte> EcdsaSig
            {
                get
                {
                    fixed (byte* ptr = _ecdsaSig)
                    {
                        return new Span<byte>(ptr, 0x28);
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct Act
        {
            private short _actType;
            private short _versionFlag;
            private int _version;
            private fixed byte _accountId[8];
            private fixed byte _primKeyTable[0x800];
            private fixed byte _unk1[0x40];
            private fixed byte _openPsId[0x10];
            private fixed byte _unk2[0x10];
            private fixed byte _unk4[0x10];
            private fixed byte _secondTable[0x650];
            private fixed byte _rsaSig[0x100];
            private fixed byte _unkSig[0x40];
            private fixed byte _ecdsaSig[0x28];
            public short ActType => _actType;
            public short VersionFlag => _versionFlag;
            public int Version => _version;
            public Span<byte> AccountId
            {
                get
                {
                    fixed (byte* ptr = _accountId)
                    {
                        return new Span<byte>(ptr, 8);
                    }
                }
            }
            public Span<byte> PrimKeyTable
            {
                get
                {
                    fixed (byte* ptr = _primKeyTable)
                    {
                        return new Span<byte>(ptr, 0x800);
                    }
                }
            }
            public Span<byte> Unk1
            {
                get
                {
                    fixed (byte* ptr = _unk1)
                    {
                        return new Span<byte>(ptr, 0x40);
                    }
                }
            }
            public Span<byte> OpenPsId
            {
                get
                {
                    fixed (byte* ptr = _openPsId)
                    {
                        return new Span<byte>(ptr, 0x10);
                    }
                }
            }
            public Span<byte> Unk2
            {
                get
                {
                    fixed (byte* ptr = _unk2)
                    {
                        return new Span<byte>(ptr, 0x10);
                    }
                }
            }
            public Span<byte> Unk4
            {
                get
                {
                    fixed (byte* ptr = _unk4)
                    {
                        return new Span<byte>(ptr, 0x10);
                    }
                }
            }
            public Span<byte> SecondTable
            {
                get
                {
                    fixed (byte* ptr = _secondTable)
                    {
                        return new Span<byte>(ptr, 0x650);
                    }
                }
            }
            public Span<byte> RsaSig
            {
                get
                {
                    fixed (byte* ptr = _rsaSig)
                    {
                        return new Span<byte>(ptr, 0x100);
                    }
                }
            }
            public Span<byte> UnkSig
            {
                get
                {
                    fixed (byte* ptr = _unkSig)
                    {
                        return new Span<byte>(ptr, 0x40);
                    }
                }
            }
            public Span<byte> EcdsaSig
            {
                get
                {
                    fixed (byte* ptr = _ecdsaSig)
                    {
                        return new Span<byte>(ptr, 0x28);
                    }
                }
            }

        }

        public static int sceNpDrmGetIDps(Span<byte> idps)
        {
            if (_psId != null && idps != null)
            {
                _psId.CopyTo(idps);
                return 0;
            }
            return -0x7faaf6ff;
        }

        private static int GetActKey(Span<byte> key, ReadOnlySpan<byte> keyTable, int count)
        {
            Span<byte> idps = stackalloc byte[16];
            Span<byte> decKey = stackalloc byte[16];
            int ret = sceNpDrmGetIDps(idps);
            if (ret > -1)
            {
                ret = AesHelper.AesEncrypt(KeyVault.drmActdatKey, decKey, idps);
                if (ret == 16)
                {
                    for (int i = 0; i < count; i++)
                    {
                        ret = AesHelper.AesDecrypt(keyTable[(i * 0x10)..], key[(i * 0x10)..], decKey);
                        if (ret != 16)
                        {
                            ret = unchecked((int)0x80550902);
                            break;
                        }
                    }
                    ret = 0;
                }
            }
            idps.Clear();
            decKey.Clear();
            return ret;
        }

        private static int GetFreeVersionKey(Span<byte> versionKey, ReadOnlySpan<byte> rifBuf)
        {
            return -1;
        }

        private static int GetLicenseVersionKey(Span<byte> versionKey, ReadOnlySpan<byte> actBuf, ReadOnlySpan<byte> rifBuf)
        {
            Span<byte> buf1 = stackalloc byte[16];
            Span<byte> buf2 = stackalloc byte[16];
            int ret = sceNpDrmVerifyAct(actBuf);
            if (ret > -1)
            {
                ret = sceNpDrmVerifyRif(rifBuf);
                if (ret > -1)
                {
                    var act = MemoryMarshal.AsRef<Act>(actBuf);
                    var rif = MemoryMarshal.AsRef<Rif>(rifBuf);
                    if (!act.AccountId.SequenceEqual(rif.AccountId))
                    {
                        return -0x7faaf6fb;
                    }
                    if (((act.ActType & 0xff) << 8 | act.ActType >> 8) < ((rif.Version & 0xff) << 8 | rif.Version >> 9))
                    {
                        return -0x7faaf6ef;
                    }
                    AesHelper.AesDecrypt(rif.EncKey1, buf1, KeyVault.drmRifKey);
                    int keyIdx = MemoryMarshal.Read<int>(buf1[12..]);
                    keyIdx >>= 0x18;
                    if (keyIdx >= 0x80)
                    {
                        return -0x7faaf6fe;
                    }
                    ret = GetActKey(buf2, act.PrimKeyTable[(keyIdx * 0x10)..], 1);
                    if (ret > -1)
                    {
                        ret = AesHelper.AesDecrypt(rif.EncKey2, versionKey, buf2);
                        if (ret != 16)
                        {
                            ret = -0x7faaf6fe;
                        }
                    }
                }
            }
            buf1.Clear();
            buf2.Clear();
            return ret;
        }

        private static int CheckRifExpress(ReadOnlySpan<byte> rifBuf)
        {
            // TODO
            return 0;
        }

        private static int GenVersionKey(Span<byte> versionKey, int type)
        {
            type &= 0xffffff;
            int ret = 0;
            if (type != 0)
            {
                ret = unchecked((int)0x80550901);
                if (type < 4)
                {
                    ret = AesHelper.AesEncrypt(versionKey, versionKey, KeyVault.drmVersionKeyKey.AsSpan().Slice(type * 0x10, 0x10));
                    ret = ret == 16 ? 0 : unchecked((int)0x80550902);
                }
            }
            return ret;
        }

        public static int sceNpDrmGetVersionKey(Span<byte> versionKey, ReadOnlySpan<byte> actBuf, ReadOnlySpan<byte> rifBuf, int type)
        {
            int ret = -0x7faaf6ff;
            if (versionKey != null && rifBuf != null)
            {
                var rif = MemoryMarshal.AsRef<Rif>(rifBuf);
                if (rif.DrmType == 0x300000)
                {
                    ret = GetFreeVersionKey(versionKey, rifBuf);
                }
                else
                {
                    if (actBuf == null)
                    {
                        return ret;
                    }
                    ret = GetLicenseVersionKey(versionKey, actBuf, rifBuf);
                }
                if (ret > -1)
                {
                    ret = CheckRifExpress(rifBuf);
                    if (ret > -1)
                    {
                        ret = GenVersionKey(versionKey, type);
                    }
                }
            }
            return ret;
        }

        public static int sceNpDrmGetFixedKey(Span<byte> fixedKey, ReadOnlySpan<byte> contentId, int type)
        {
            Span<byte> buf = stackalloc byte[0x30];
            Span<byte> mkey = stackalloc byte[48];
            int ret = -0x7faaf6ff;
            if ((type & 0x1000000) != 0)
            {
                contentId[..0x30].CopyTo(buf);
                ret = AMCTRL.sceDrmBBMacInit(mkey, 1);
                if (ret == 0)
                {
                    ret = AMCTRL.sceDrmBBMacUpdate(mkey, buf, 0x30);
                    if (ret == 0)
                    {
                        ret = AMCTRL.sceDrmBBMacFinal(mkey, fixedKey, KeyVault.DrmFixedKey);
                        if (ret == 0)
                        {
                            ret = GenVersionKey(fixedKey, (int)(type & 0xfeffffff));
                        }
                        else
                        {
                            ret = -0x7faaf6fe;
                        }
                    }
                }
            }
            return ret;
        }

        public static int sceNpDrmGetContentKey(Span<byte> contentKey, ReadOnlySpan<byte> actBuf, ReadOnlySpan<byte> rifBuf) => sceNpDrmGetVersionKey(contentKey, actBuf, rifBuf, 0);

        public static int sceNpDrmVerifyAct(ReadOnlySpan<byte> actBuf)
        {
            int ret = -0x7faaf6ff;
            if (actBuf != null)
            {
                ret = -0x7faaf6fa;
                var act = MemoryMarshal.AsRef<Act>(actBuf);
                if (((act.VersionFlag & 0xFF) << 8 | act.VersionFlag >> 8) < 2)
                {
                    if (((act.ActType & 0xFF) << 8 | act.ActType >> 8) < 2)
                    {
                        ret = VerifySig(act.EcdsaSig, KeyVault.drmActRifSig, actBuf, 0x1010);
                    }
                }
            }
            return ret;
        }

        private static int VerifySig(ReadOnlySpan<byte> sig, ReadOnlySpan<byte> pubKey, ReadOnlySpan<byte> data, int dataSize)
        {
            int ret;
            Span<byte> digest = stackalloc byte[32];
            if (dataSize < 0x801)
            {
                ret = SceDdrdb.sceDdrdbHash(data, dataSize, digest);
            }
            else
            {
                ret = (SHA1.HashData(data[..dataSize], digest) > 0 ? 0 : -1);
            }
            if (ret > -1)
            {
                ret = SceDdrdb.sceDdrdbSigvry(pubKey, digest, sig);
            }
            return ret;
        }

        private static int FillFuseId(Span<uint> fuseId)
        {
            if (_fuseId != 0)
            {
                uint heigh = (uint)((_fuseId >> 32) & 0xffffffff);
                uint low = (uint)(_fuseId & 0xffffffff);
                fuseId[0] = BinaryPrimitives.ReverseEndianness(heigh);
                fuseId[1] = BinaryPrimitives.ReverseEndianness(low);
            }
            return -1;
        }

        private static int VerifyRif(ReadOnlySpan<byte> rifBuf)
        {
            int ret = -0x7faaf6ff;
            if (rifBuf != null)
            {
                ret = -0x7faaf6fa;
                var rif = MemoryMarshal.AsRef<Rif>(rifBuf);
                if (((rif.VersionFlag & 0xff) << 8 | rif.VersionFlag >> 8) < 3)
                {
                    if (((rif.Version & 0xff) << 8 | rif.Version >> 8) < 2)
                    {
                        int ret2;
                        if ((rif.DrmType & 0x10000) == 0)
                        {
                            ret = VerifySig(rif.EcdsaSig, KeyVault.drmActRifSig, rifBuf, 0x70);
                            ret2 = -0x7faaf6fc;
                        }
                        else
                        {
                            Span<byte> buffer = stackalloc byte[0x90];
                            rifBuf[..0x70].CopyTo(buffer);
                            if (_psId == null)
                            {
                                return ret;
                            }
                            ret = sceNpDrmGetIDps(buffer[0x70..]);
                            if (ret < 0)
                            {
                                return ret;
                            }
                            Span<uint> fuseId = MemoryMarshal.Cast<byte, uint>(buffer[0x80..]);
                            ret = FillFuseId(fuseId);
                            if (ret < 0)
                            {
                                return ret;
                            }
                            ret = VerifySig(rif.EcdsaSig, KeyVault.drmActRifSig, buffer, 0x90);
                            ret2 = -0x7faaf6e8;
                        }
                        if (ret != 0)
                        {
                            ret = ret2;
                        }
                    }
                }
            }
            return ret;
        }

        public static int sceNpDrmVerifyRif(ReadOnlySpan<byte> rifBuf)
        {
            int ret = VerifyRif(rifBuf);
            if (ret > -1)
            {
                var rif = MemoryMarshal.AsRef<Rif>(rifBuf);
                if (rif.DrmType == 0x3000000)
                {
                    ret = -1; // TODO
                }
            }
            return ret;
        }

        public static int KsceNpDrmEbootSigGenMultiDisc(string fileName, ReadOnlySpan<byte> sceDiscInfo,
            Span<byte> ebootSig, int swVer)
        {
            return SceNpDrmEbootSigGenMultiDisc(fileName, sceDiscInfo, ebootSig, swVer, _memory.Span, BLK_SIZE);
        }
        public static int KsceNpDrmEbootSigGenPs1(Stream file, Span<byte> ebootSig, int swVer)
        {
            return SceNpDrmEbootSigGen(file, 3, ebootSig, swVer, _memory.Span, BLK_SIZE);
        }

        public static int KsceNpDrmEbootSigGenPsp(Stream file, Span<byte> ebootSig, int swVer)
        {
            return SceNpDrmEbootSigGen(file, 2, ebootSig, swVer, _memory.Span, BLK_SIZE);
        }

        public static int KsceNpDrmPspEbootSigGen(Stream file, Span<byte> ebootSig)
        {
            return SceNpDrmPspEbootSigGen(file, ebootSig, _memory.Span, BLK_SIZE);
        }

        public static int KsceNpDrmEbootSigGenPs1(string fileName, Span<byte> ebootSig, int swVer)
        {
            return SceNpDrmEbootSigGen(fileName, 3, ebootSig, swVer, _memory.Span, BLK_SIZE);
        }

        public static int KsceNpDrmEbootSigGenPsp(string fileName, Span<byte> ebootSig, int swVer)
        {
            return SceNpDrmEbootSigGen(fileName, 2, ebootSig, swVer, _memory.Span, BLK_SIZE);
        }

        public static int KsceNpDrmPspEbootSigGen(string fileName, Span<byte> ebootSig)
        {
            return SceNpDrmPspEbootSigGen(fileName, ebootSig, _memory.Span, BLK_SIZE);
        }

        private static int SceNpDrmEbootSigGenMultiDisc(string fileName, ReadOnlySpan<byte> sceDiskInfo,
            Span<byte> ebootSig, int swVer, Span<byte> buffer, int blockSize)
        {
            Span<byte> pbpHdrDigest = stackalloc byte[32];
            Span<byte> discsDigest = stackalloc byte[32];
            Span<byte> ebootSigtmp = stackalloc byte[512];
            ref var sceEbootPbp = ref Utils.AsRef<SceEbootPbp>(ebootSigtmp);
            if ((blockSize & 0x3F) != 0)
            {
                blockSize &= unchecked((int)0xFFFFFFC0);
            }

            if (sceDiskInfo == null || string.IsNullOrWhiteSpace(fileName) || buffer == null || ebootSig == null ||
                blockSize < 0x400)
            {
                return unchecked((int)0x80870001);
            }

            Span<byte> secureTick = stackalloc byte[8] { 0xD4, 0x7A, 0x2C, 0x13, 0x64, 0x59, 0xE2, 0x00 };
            //RandomNumberGenerator.Fill(secureTick);

            ebootSig.Fill(0);
            sceEbootPbp.SwVer = swVer;
            sceEbootPbp.Aid = Aid;
            sceEbootPbp.SecureTick = Utils.AsRef<ulong>(secureTick);

            var sha224 = SHA224.Create();
            var hash = sha224.ComputeHash(sceDiskInfo[..200].ToArray());
            var ret = SceSblGcAuthMgrDrmBBForDriver_4B506BE7(hash, sceDiskInfo[200..], 1000);
            if (ret != 0)
            {
                return ret;
            }

            var discInfo = Utils.AsRef<sceDiscInfo>(sceDiskInfo);
            ret = unchecked((int)0x80870005);
            if (discInfo.DiscCount > 6)
            {
                return ret;
            }
            var fi = new FileInfo(fileName);
            if (!fi.Exists)
            {
                return -1;
            }

            if (fi.Length != discInfo.FileSize)
            {
                return ret;
            }

            ret = unchecked((int)0x80870001);
            if (discInfo.DiscCount < 7)
            {
                if (fi.Length < discInfo.Offsets[0])
                {
                    return ret;
                }
                if (discInfo.DiscCount > 1 && fi.Length < discInfo.Offsets[1])
                {
                    return ret;
                }
                if (discInfo.DiscCount > 2 && fi.Length < discInfo.Offsets[2])
                {
                    return ret;
                }
                if (discInfo.DiscCount > 3 && fi.Length < discInfo.Offsets[3])
                {
                    return ret;
                }
                if (discInfo.DiscCount > 4 && fi.Length < discInfo.Offsets[4])
                {
                    return ret;
                }
                if (discInfo.DiscCount > 5 && fi.Length < discInfo.Offsets[5])
                {
                    return ret;
                }

                using var stream = fi.OpenRead();
                ret = SceMultiDiscDigest(stream, fi.Length, discInfo.DiscCount, discInfo.Offsets, pbpHdrDigest, discsDigest, buffer, blockSize);
                if (discInfo.DiscCount >= 0)
                {
                    stream.Close();
                    ret = SceSblGcAuthMgrDrmBBForDriver_4B506BE7(discsDigest, discInfo.DiscsSig, 1000);
                    if (ret != 0)
                    {
                        return ret;
                    }

                    sceEbootPbp.PbpSize = fi.Length;
                    sceEbootPbp.Magic = 0x47495349544C554D;
                    sceEbootPbp.KeyType = 1;
                    sceEbootPbp.Type = 4;
                    discInfo.Id.CopyTo(sceEbootPbp.ContentId);
                    sceEbootPbp.DiscCount = discInfo.DiscCount;
                    discInfo.Offsets.CopyTo(sceEbootPbp.DiscOffsets);
                    ret = SceSblGcAuthMgrDrmBBForDriver_050DC6DF(pbpHdrDigest, sceEbootPbp.PbpHdrSig, 1);
                    if (ret != 0)
                    {
                        return ret;
                    }
                    discInfo.DiscsSig.CopyTo(sceEbootPbp.NpUmdImgSig);
                    var ebootsigDigst = sha224.ComputeHash(ebootSigtmp.Slice(0, 0x1C8).ToArray());
                    ret = SceSblGcAuthMgrDrmBBForDriver_050DC6DF(ebootsigDigst, sceEbootPbp.Sig, 1);
                    if (ret != 0)
                    {
                        return ret;
                    }
                    ebootSigtmp.CopyTo(ebootSig);
                    ret = 0;
                }
            }
            return ret;
        }

        private static int SceMultiDiscDigest(Stream stream, long fileSize, int diskCount,
            ReadOnlySpan<long> diskOffsets, Span<byte> pbpHdrDigest, Span<byte> dataDigest, Span<byte> buffer,
            int blockSize)
        {
            var sha224 = SHA224.Create();
            var ret = unchecked((int)0x80870005);
            stream.Seek(0, SeekOrigin.Begin);
            buffer = buffer.Slice(0, blockSize);
            var readSize = stream.Read(buffer);
            if (readSize < 0x28)
            {
                return ret;
            }
            var pbpHeader = Utils.AsRef<PbpHeader>(buffer);
            if (pbpHeader.Sig != 0x50425000)
            {
                return ret;
            }

            var tmp = readSize;
            if (pbpHeader.DataPsarOff < readSize)
            {
                tmp = pbpHeader.DataPsarOff;
            }

            var paramReadSize = pbpHeader.Icon0Off;
            if (paramReadSize > 0x400)
            {
                paramReadSize = 0x400;
            }

            if (tmp < paramReadSize)
            {
                return ret;
            }

            sha224.TransformFinalBlock(buffer.ToArray(), 0, paramReadSize);
            sha224.Hash.CopyTo(pbpHdrDigest);

            sha224.Initialize();
            sha224.TransformBlock(buffer.ToArray(), 0, paramReadSize, null, 0);

            long end = pbpHeader.DataPsarOff + 0xC0000;
            if (diskOffsets[0] >= pbpHeader.DataPsarOff && diskOffsets[0] < end)
            {
                end = diskOffsets[0];
            }

            long start = pbpHeader.DataPsarOff;
            stream.Seek(start, SeekOrigin.Begin);
            for (; start < end; start += readSize)
            {
                var toRead = (int)(end - start);
                if (toRead > blockSize)
                {
                    toRead = blockSize;
                }

                readSize = stream.Read(buffer.Slice(0, toRead));
                if (readSize == 0)
                {
                    return -1;
                }
                sha224.TransformBlock(buffer.ToArray(), 0, readSize, null, 0);
            }

            if (diskCount != 0)
            {
                var start1 = diskOffsets[0];
                end = diskOffsets[0] + 0xC0000;
                if (end >= 0)
                {
                    var discNo = 0;
                    var idx = 5;
                    while (true)
                    {
                        if (fileSize < end)
                        {
                            end = fileSize;
                        }

                        if (++discNo < diskCount)
                        {
                            start = diskOffsets[idx - 4];
                            if (start >= start1 && start < end)
                            {
                                end = start;
                            }
                        }

                        if (start1 < end)
                        {
                            stream.Seek(start1, SeekOrigin.Begin);
                            while (true)
                            {
                                var toRead = (int)(end - start1);
                                if (toRead > blockSize)
                                {
                                    toRead = blockSize;
                                }

                                readSize = stream.Read(buffer.Slice(0, toRead));
                                if (readSize == 0)
                                {
                                    return -1;
                                }
                                sha224.TransformBlock(buffer.ToArray(), 0, readSize, null, 0);
                                start1 += toRead;
                                if (start1 >= end)
                                {
                                    break;
                                }
                            }
                        }

                        if (discNo == diskCount)
                        {
                            break;
                        }
                        start1 = diskOffsets[idx - 4];
                        idx++;
                        end = start1 + 0xC0000;

                    }
                }
            }
            sha224.TransformFinalBlock(buffer.ToArray(), 0, 0);
            sha224.Hash.CopyTo(dataDigest);

            stream.Seek(pbpHeader.DataPsarOff, SeekOrigin.Begin);
            readSize = stream.Read(buffer.Slice(0, 0x100));
            if (readSize == 0x100)
            {
                ret = 0;
            }
            else
            {
                ret = unchecked((int)0x80870005);
            }

            return ret;
        }

        private static int SceNpDrmEbootSigGen(string fileName, int type, Span<byte> ebootSig, int swVer, Span<byte> buffer, int blockSize)
        {
            if(string.IsNullOrWhiteSpace(fileName)) return -0x7f78ffff;
            var fi = new FileInfo(fileName);
            if (!fi.Exists)
            {
                return -1;
            }
            using(FileStream fstream = fi.OpenRead()){
                return SceNpDrmEbootSigGen(fstream, type, ebootSig, swVer, buffer, blockSize);
            }

        }
        private static int SceNpDrmEbootSigGen(Stream ebootStream, int type, Span<byte> ebootSig, int swVer, Span<byte> buffer, int blockSize)
        {
            Span<byte> pbpHdrDigest = stackalloc byte[32];
            Span<byte> npUmdImgDigest = stackalloc byte[32];
            Span<byte> ebootSigtmp = stackalloc byte[512];
            ref var sceEbootPbp = ref Utils.AsRef<SceEbootPbp>(ebootSigtmp);
            if ((blockSize & 0x3F) != 0)
            {
                blockSize &= unchecked((int)0xFFFFFFC0);
            }

            if (ebootStream == null || buffer == null || ebootSig == null || blockSize < 0x400 || type > 3)
            {
                return -0x7f78ffff;
            }

            Span<byte> secureTick = stackalloc byte[8] { 0xD4, 0x7A, 0x2C, 0x13, 0x64, 0x59, 0xE2, 0x00 };
            //RandomNumberGenerator.Fill(secureTick);

            ebootSig.Fill(0);
            sceEbootPbp.SwVer = swVer;
            sceEbootPbp.Aid = Aid;
            sceEbootPbp.SecureTick = Utils.AsRef<ulong>(secureTick);

            long flen = ebootStream.Length;
            int ret = SceEbootPbpDigest(ebootStream, flen, pbpHdrDigest, npUmdImgDigest, buffer, blockSize);
            if (ret < 0)
            {
                return ret;
            }
            
            sceEbootPbp.KeyType = 1;
            sceEbootPbp.PbpSize = flen;
            sceEbootPbp.Type = type;
            var psarSig = Encoding.ASCII.GetString(buffer.Slice(0, 8));
            if (type == 3)
            {
                if (psarSig != "PSISOIMG")
                {
                    return -0x7f78fffb;
                }
                sceEbootPbp.Magic = 0x474953315350504E;
            }
            else
            {
                if (psarSig != "NPUMDIMG")
                {
                    return -0x7f78fffb;
                }

                sceEbootPbp.Magic = 0x474953444D55504E;
                buffer.Slice(0x10, 0x30).CopyTo(sceEbootPbp.ContentId);
            }

            ret = SceSblGcAuthMgrDrmBBForDriver_050DC6DF(pbpHdrDigest, sceEbootPbp.PbpHdrSig, 1);
            if (ret < 0)
            {
                return ret;
            }

            ret = SceSblGcAuthMgrDrmBBForDriver_050DC6DF(npUmdImgDigest, sceEbootPbp.NpUmdImgSig, 1);
            if (ret < 0)
            {
                return ret;
            }

            var sha224 = SHA224.Create();
            var ebootsigDigst = sha224.ComputeHash(ebootSigtmp.Slice(0, 0x1C8).ToArray());
            ret = SceSblGcAuthMgrDrmBBForDriver_050DC6DF(ebootsigDigst, sceEbootPbp.Sig, 1);
            if (ret < 0)
            {
                return ret;
            }
            ebootSigtmp.CopyTo(ebootSig);
            ret = 0;
            return ret;
        }

        private static int SceEbootPbpDigest(Stream stream, long fileSize, Span<byte> pbpHdrDigest,
            Span<byte> npUmdImgDigest, Span<byte> buffer, int blockSize)
        {
            var sha224 = SHA224.Create();
            var ret = unchecked((int)0x80870005);
            stream.Seek(0, SeekOrigin.Begin);
            buffer = buffer.Slice(0, blockSize);
            var readSize = stream.Read(buffer);
            if (readSize < 0x28)
            {
                return ret;
            }

            var pbpHeader = Utils.AsRef<PbpHeader>(buffer);
            if (fileSize < pbpHeader.DataPsarOff + 0xFF || pbpHeader.Sig != 0x50425000)
            {
                return ret;
            }

            var tmp = readSize;
            if (pbpHeader.DataPsarOff < readSize)
            {
                tmp = pbpHeader.DataPsarOff;
            }

            var paramReadSize = pbpHeader.Icon0Off;
            if (paramReadSize > 0x400)
            {
                paramReadSize = 0x400;
            }

            if (tmp < paramReadSize)
            {
                return ret;
            }

            sha224.TransformFinalBlock(buffer.ToArray(), 0, paramReadSize);
            sha224.Hash.CopyTo(pbpHdrDigest);

            sha224.Initialize();

            var alignedFileSize = (pbpHeader.DataPsarOff + 0x1C0000 + 64 - 1) & ~(64 - 1);
            if (alignedFileSize < fileSize)
            {
                fileSize = alignedFileSize;
            }

            var fixsize2 = fileSize;
            if (pbpHeader.DataPsarOff + 0x1C0000 < fileSize)
            {
                fixsize2 = pbpHeader.DataPsarOff + 0x1C0000;
            }

            if (pbpHeader.DataPsarOff < fixsize2)
            {
                var offset = pbpHeader.DataPsarOff;
                stream.Seek(offset, SeekOrigin.Begin);
                while (true)
                {
                    readSize = stream.Read(buffer);
                    var bsize = readSize;
                    if (readSize <= 0)
                    {
                        ret = unchecked((int)0x80870002);
                        return ret;
                    }

                    if (fixsize2 < offset + readSize)
                    {
                        bsize = (int)(fixsize2 - offset);
                    }
                    sha224.TransformBlock(buffer.ToArray(), 0, bsize, null, 0);

                    offset += readSize;
                    if (offset >= fixsize2)
                    {
                        break;
                    }
                }
                sha224.TransformFinalBlock(buffer.ToArray(), 0, 0);
                sha224.Hash.CopyTo(npUmdImgDigest);
                stream.Seek(pbpHeader.DataPsarOff, SeekOrigin.Begin);
                readSize = stream.Read(buffer.Slice(0, 0x100));
                if (readSize == 0x100)
                {
                    ret = 0;
                }
                else
                {
                    ret = unchecked((int)0x80870005);
                }
            }


            return ret;
        }

        private static int SceNpDrmPspEbootSigGen(string fileName, Span<byte> ebootSig, Span<byte> buffer, int blockSize)
        {
            if(string.IsNullOrWhiteSpace(fileName))
                return -0x7f78ffff;

            var fi = new FileInfo(fileName);
            if (!fi.Exists)
            {
                return -1;
            }
            using(FileStream fs = fi.OpenRead())
            {
                return SceNpDrmPspEbootSigGen(fs, ebootSig, buffer, blockSize);
            }
        }

        private static int SceNpDrmPspEbootSigGen(Stream ebootStream, Span<byte> ebootSig, Span<byte> buffer,int blockSize)
        {
            Span<byte> pbpHdrDigest = stackalloc byte[32];
            Span<byte> npUmdImgDigest = stackalloc byte[32];
            Span<byte> ebootSigtmp = stackalloc byte[0x100];
            ref var sceEbootPbp = ref Utils.AsRef<SceEbootPbp100>(ebootSigtmp);
            if ((blockSize & 0x3F) != 0)
            {
                blockSize &= unchecked((int)0xFFFFFFC0);
            }

            if (ebootStream == null || buffer == null || ebootSig == null || blockSize < 0x400)
            {
                return -0x7f78ffff;
            }

            long fileSize = ebootStream.Length;
            int ret = SceEbootPbpDigest(ebootStream, fileSize, pbpHdrDigest, npUmdImgDigest, buffer, blockSize);
            if (ret < 0)
            {
                return ret;
            }

            if (Encoding.ASCII.GetString(buffer.Slice(0, 8)) != "NPUMDIMG")
            {
                return -0x7f78fffb;
            }
            buffer.Slice(0, 0x40).CopyTo(ebootSigtmp);
            sceEbootPbp.Type = 0;
            sceEbootPbp.Magic = 0x474953444D55504E;

            ret = SceSblGcAuthMgrDrmBBForDriver_050DC6DF(pbpHdrDigest, sceEbootPbp.PbpHdrSig, 1);
            if (ret < 0)
            {
                return ret;
            }

            ret = SceSblGcAuthMgrDrmBBForDriver_050DC6DF(npUmdImgDigest, sceEbootPbp.NpUmdImgSig, 1);
            if (ret < 0)
            {
                return ret;
            }

            var sha224 = SHA224.Create();
            var ebootsigDigst = sha224.ComputeHash(ebootSigtmp.Slice(0, 0xC8).ToArray());
            ret = SceSblGcAuthMgrDrmBBForDriver_050DC6DF(ebootsigDigst, sceEbootPbp.Sig, 1);
            if (ret < 0)
            {
                return ret;
            }
            ebootSigtmp.CopyTo(ebootSig);
            ret = 0;

            return ret;
        }

        private static int SceSblGcAuthMgrDrmBBForDriver_050DC6DF(ReadOnlySpan<byte> digest, Span<byte> sig, int type)
        {
            var curve = ECDsaHelper.SetCurve(KeyVault.Eboot_p, KeyVault.Eboot_a, KeyVault.Eboot_b, KeyVault.Eboot_N, KeyVault.Eboot_Gx,
                KeyVault.Eboot_Gy);
            using var ecdsa = ECDsaHelper.Create(curve,
                KeyVault.Eboot_priv[type],
                KeyVault.Eboot_pubx[type],
                KeyVault.Eboot_puby[type], true);
            var signature = ecdsa.SignHash(digest.ToArray());
            signature.CopyTo(sig);
            return 0;
        }

        private static int SceSblGcAuthMgrDrmBBForDriver_4B506BE7(ReadOnlySpan<byte> digest, ReadOnlySpan<byte> sig, int keyType)
        {
            byte[] pubx;
            byte[] puby;
            switch (keyType)
            {
                case 1:
                    pubx = KeyVault.VitaKirk18PubKey1x;
                    puby = KeyVault.VitaKirk18PubKey1y;
                    break;
                case 0:
                    pubx = KeyVault.VitaKirk18PubKey0x;
                    puby = KeyVault.VitaKirk18PubKey0y;
                    break;
                case 1000:
                    pubx = KeyVault.VitaKirk18PubKey1000x;
                    puby = KeyVault.VitaKirk18PubKey1000y;
                    break;
                default:
                    return unchecked((int)0x808a040a);
            }
            var curve = ECDsaHelper.SetCurve(KeyVault.Eboot_p, KeyVault.Eboot_a, KeyVault.Eboot_b, KeyVault.Eboot_N, KeyVault.Eboot_Gx,
                KeyVault.Eboot_Gy);
            using var ecdsa = ECDsaHelper.Create(curve, pubx, puby);
            var verify = ecdsa.VerifyHash(digest.ToArray(), sig.ToArray());

            return verify ? 0 : -1;
        }
    }
}
