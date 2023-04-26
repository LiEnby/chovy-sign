using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PspCrypto
{
    public class DNASStream : Stream
    {
        internal static readonly byte[] DnasKeyBase =
        {
            0x2A, 0x05, 0x54, 0x40, 0x62, 0xD9, 0x1F, 0xE3, 0xF2, 0xD0, 0x2B, 0xC6, 0x21, 0xFF, 0x20, 0x0E,
            0xB1, 0x44, 0x28, 0xDF, 0x0A, 0xCD, 0x14, 0x5B, 0xC8, 0x19, 0x36, 0x90, 0xD1, 0x42, 0x99, 0x2F
        };

        internal static readonly byte[] DnasKey1 =
        {
            0xED, 0xE2, 0x5D, 0x2D, 0xBB, 0xF8, 0x12, 0xE5, 0x3C, 0x5C, 0x59, 0x32, 0xFA, 0xE3, 0xE2, 0x43
        };

        internal static readonly byte[] DnasKey2 =
        {
            0x27, 0x74, 0xFB, 0xEB, 0xA4, 0xA0, 0x01, 0xD7, 0x02, 0x56, 0x9E, 0x33, 0x8C, 0x19, 0x57, 0x83
        };

        private static Memory<byte> _gMemory = new byte[0x640];
        private static Memory<byte> _gCipherMemory = new byte[0x200];

        private Stream _baseStream;
        private byte[] _versionKey;

        private long _position;
        private long _pgdOffset;
        private int _keyIndex;
        private int _openFlag;
        private long _dataOffset;
        private long _tableOffset;

        public int KeyIndex => _keyIndex;

        private PgdDesc _desc;

        internal unsafe struct PgdHeader
        {
            public uint Magic;
            public int KeyIndex;
            public int DrmType;
            public int Unk12;
            private fixed byte _descKey[0x10];
            public Span<byte> DescKey
            {
                get
                {
                    fixed (byte* ptr = _descKey)
                    {
                        return new Span<byte>(ptr, 0x10);
                    }
                }
            }
            private fixed byte _keyHash[0x10];
            public Span<byte> KeyHash
            {
                get
                {
                    fixed (byte* ptr = _keyHash)
                    {
                        return new Span<byte>(ptr, 0x10);
                    }
                }
            }
            private fixed byte _pgdDesc[0x30];
            public Span<byte> PgdDesc
            {
                get
                {
                    fixed (byte* ptr = _pgdDesc)
                    {
                        return new Span<byte>(ptr, 0x30);
                    }
                }
            }
            private fixed byte _macTableHash[0x10];
            public Span<byte> MacTableHash
            {
                get
                {
                    fixed (byte* ptr = _macTableHash)
                    {
                        return new Span<byte>(ptr, 0x10);
                    }
                }
            }
            private fixed byte _hash70[0x10];
            public Span<byte> Hash70
            {
                get
                {
                    fixed (byte* ptr = _hash70)
                    {
                        return new Span<byte>(ptr, 0x10);
                    }
                }
            }
            private fixed byte _hash80[0x10];
            public Span<byte> Hash80
            {
                get
                {
                    fixed (byte* ptr = _hash80)
                    {
                        return new Span<byte>(ptr, 0x10);
                    }
                }
            }
        }

        internal unsafe struct PgdDesc
        {
            private fixed byte _key[0x10];

            public Span<byte> Key
            {
                get
                {
                    fixed (byte* ptr = _key)
                    {
                        return new Span<byte>(ptr, 0x10);
                    }
                }
            }
            public int Version;
            public int DataSize;
            public int BlockSize;
            public int DataOffset;
            private fixed byte _unk20[0x10];

            public Span<byte> Unk20
            {
                get
                {
                    fixed (byte* ptr = _unk20)
                    {
                        return new Span<byte>(ptr, 0x10);
                    }
                }
            }

        }

        public int BlockSize => _desc.BlockSize;

        public DNASStream(Stream stream, long pgdOffset, ReadOnlySpan<byte> versionKey, int flag = 2)
            : this(stream, pgdOffset, versionKey.ToArray(), flag)
        {

        }

        public DNASStream(Stream stream, long pgdOffset, byte[] versionKey = null, int flag = 2)
        {
            _baseStream = stream;
            _pgdOffset = pgdOffset;
            var offset = stream.Seek(pgdOffset, SeekOrigin.Begin);
            if (offset != _pgdOffset)
            {
                throw new ArgumentOutOfRangeException();
            }
            Span<byte> hdr = _gMemory[..0x90].Span;
            var size = stream.Read(hdr);
            if (size != 0x90)
            {
                throw new ArgumentException("stream too small", nameof(stream));
            }
            var header = Utils.AsRef<PgdHeader>(hdr);
            _keyIndex = header.KeyIndex;
            if (_keyIndex == 1)
            {
                _versionKey = versionKey ?? new byte[16];
            }
            else
            {
                if (versionKey == null)
                {
                    throw new ArgumentNullException(nameof(versionKey));
                }
                Span<byte> mkey = stackalloc byte[Marshal.SizeOf<AMCTRL.MAC_KEY>()];
                AMCTRL.sceDrmBBMacInit(mkey, 1);
                AMCTRL.sceDrmBBMacUpdate(mkey, DnasKeyBase, (_keyIndex - 1) * 0x10);
                AMCTRL.sceDrmBBMacFinal(mkey, _versionKey, versionKey);
                return;
            }

            int macType;
            int cipherType;

            if (header.DrmType == 1)
            {
                flag |= 4;
                macType = 1;
                cipherType = 1;
                if (header.KeyIndex > 1)
                {
                    flag |= 0xc;
                    macType = 3;
                }
            }
            else if (header.DrmType == 0 && (flag & 4) == 0)
            {
                macType = 2;
                cipherType = 2;
            }
            else
            {
                throw new IOException();
            }

            byte[] dnasKey = null;

            if ((flag & 2) != 0)
            {
                dnasKey = DnasKey1;
            }
            else if ((flag & 1) != 0)
            {
                dnasKey = DnasKey2;
            }

            if (dnasKey == null)
            {
                throw new IOException();
            }

            var ret = CheckBBMac(hdr, 0x80, dnasKey, header.Hash80, macType);
            if (ret != 0)
            {
                throw new IOException("Wrong MAC 0x80");
            }

            if (!Utils.isEmpty(_versionKey, 0x10))
            {
                ret = CheckBBMac(hdr, 0x70, _versionKey, header.Hash70, macType);
            }
            else
            {
                ret = GetMacKey(hdr, 0x70, _versionKey, header.Hash70, macType);
            }

            if (ret != 0)
            {
                throw new IOException("Wrong MAC 0x70");
            }

            ret = DoBBCipher(header.PgdDesc, 0x30, 0, _versionKey, header.DescKey, cipherType);
            if (ret != 0)
            {
                throw new IOException($"Error 0x{ret:X8}");
            }
            var desc = Utils.AsRef<PgdDesc>(header.PgdDesc);
            if (desc.Version != 0)
            {
                throw new IOException($"Error 0x{8051020:X8}");
            }

            if (desc.BlockSize != 0x400)
            {
                throw new IOException($"Error 0x{80510204:X8}");
            }

            _openFlag = flag | 0x10;
            _desc = desc;
            _dataOffset = _desc.DataOffset + pgdOffset;
            var blockSize = desc.BlockSize;
            var alignSize = (desc.DataSize + 15) & ~15;
            var tableSize = ((alignSize + blockSize - 1) & ~(blockSize - 1)) / (blockSize / 16);
            _tableOffset = pgdOffset + 0x90 + alignSize;
            if (header.KeyIndex < 3 && 0x7ffff < tableSize)
            {

            }
            {
                Span<byte> mkey = stackalloc byte[Marshal.SizeOf<AMCTRL.MAC_KEY>()];
                ret = AMCTRL.sceDrmBBMacInit(mkey, macType);
                stream.Seek(_tableOffset, SeekOrigin.Begin);
                int read = 0;
                if (tableSize != 0)
                {
                    Span<byte> dataBuf = new byte[0x400];
                    do
                    {
                        var tmpSize = tableSize - read;
                        if (tmpSize > 0x400)
                        {
                            tmpSize = 0x400;
                        }

                        var data = dataBuf.Slice(0, tmpSize);
                        var readSize = stream.Read(data);
                        if (readSize != tmpSize)
                        {
                            throw new IOException();
                        }
                        ret = AMCTRL.sceDrmBBMacUpdate(mkey, data, tmpSize);
                        if (ret != 0)
                        {
                            throw new Exception();
                        }
                        read += 0x400;
                    } while (read < tableSize);
                }
                ret = AMCTRL.sceDrmBBMacFinal2(mkey, header.MacTableHash, _versionKey);
                if (ret != 0)
                {
                    throw new IOException($"Error 0x{80510204:X8}");
                }
            }

            _position = 0;

        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int ret;
            Span<byte> bufferSpan = buffer;
            if ((_openFlag & 0x10) == 0)
            {
                throw new IOException($"Error 0x{80510207:X8}");
            }

            var dataSize = _desc.DataSize;
            var seekOffset = _position;
            if (seekOffset < dataSize)
            {
                var macType = 2;
                var cipherType = 2;
                if ((_openFlag & 4) != 0)
                {
                    macType = 1;
                    if ((_openFlag & 8) != 0)
                    {
                        macType = 3;
                    }

                    cipherType = 1;
                }

                var endOffset = dataSize;
                if (seekOffset + count <= dataSize)
                {
                    endOffset = (int)(seekOffset + count);
                }

                var blockSize = _desc.BlockSize;
                var alignOffset = (int)seekOffset;
                var totalReadSize = 0;
                while (true)
                {
                    if (endOffset <= alignOffset)
                    {
                        return totalReadSize;
                    }

                    if ((_openFlag & 0x10) == 0)
                    {
                        break;
                    }

                    var align = alignOffset & blockSize - 1;

                    alignOffset -= align;
                    var alignBlockSize = endOffset - alignOffset;
                    int readBytes;
                    int uVar6;
                    Span<byte> readBuffer;
                    if (align == 0 && blockSize <= alignBlockSize)
                    {
                        readBytes = alignBlockSize & ~(blockSize - 1);
                        readBuffer = bufferSpan;
                        uVar6 = readBytes;
                    }
                    else
                    {
                        readBytes = blockSize;
                        readBuffer = _gMemory.Span;
                        if (dataSize < alignOffset + blockSize)
                        {
                            readBytes = (dataSize - alignOffset + 15) & ~15;
                        }

                        uVar6 = blockSize;
                        if (endOffset < alignOffset + blockSize)
                        {
                            uVar6 = alignBlockSize;
                        }
                    }

                    _baseStream.Seek(_dataOffset + alignOffset, SeekOrigin.Begin);
                    var realReadBytes = _baseStream.Read(readBuffer.Slice(0, readBytes));
                    if (realReadBytes < readBytes)
                    {
                        throw new IOException();
                    }

                    var tableOffset = (alignOffset / blockSize) * 0x10;
                    _baseStream.Seek(_tableOffset + tableOffset, SeekOrigin.Begin);
                    var blockNr = 0;
                    if (readBytes != 0)
                    {
                        var tableReadOffset = 0;
                        var cipherSpan = _gCipherMemory.Span;
                        do
                        {
                            alignBlockSize = readBytes - tableReadOffset;
                            if (blockSize < readBytes - tableReadOffset)
                            {
                                alignBlockSize = blockSize;
                            }

                            if ((blockNr & 0x1f) == 0)
                            {
                                if (blockSize == 0)
                                {
                                    throw new IOException();
                                }

                                var tableBlock = readBytes / blockSize - blockNr;
                                if (tableBlock == 0)
                                {
                                    tableBlock = 1;
                                }

                                if (0x20 < tableBlock)
                                {
                                    tableBlock = 0x20;
                                }
                                cipherSpan.Fill(0);
                                realReadBytes = _baseStream.Read(cipherSpan[..(tableBlock * 16)]);
                                if (realReadBytes < tableBlock * 16)
                                {
                                    throw new IOException();
                                }
                            }
                            if (_keyIndex < 3)
                            {
                                ret = CheckBBMac(readBuffer[tableReadOffset..], alignBlockSize, _versionKey,
                                    cipherSpan.Slice((blockNr & 0x1f) * 16), macType);
                            }
                            else
                            {
                                ret = CheckBBMac(readBuffer[tableReadOffset..], alignBlockSize, _versionKey,
                                    cipherSpan[((blockNr & 0x1f) * 16)..], macType, alignOffset);
                            }

                            if (ret != 0)
                            {
                                throw new IOException();
                            }

                            tableReadOffset += blockSize;
                            blockNr++;
                        } while (tableReadOffset < readBytes);
                    }
                    ret = DoBBCipher(readBuffer, readBytes, alignOffset + align >> 4, _versionKey, _desc.Key, cipherType);

                    if (ret != 0)
                    {
                        throw new IOException();
                    }
                    var iVar2 = uVar6 - align;
                    seekOffset += iVar2;
                    _position = seekOffset;
                    if (readBuffer != bufferSpan)
                    {
                        readBuffer.Slice(align, iVar2).CopyTo(bufferSpan);
                    }
                    bufferSpan = bufferSpan[iVar2..];
                    totalReadSize += iVar2;
                    alignOffset += uVar6;
                }
            }

            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if ((_openFlag & 0x10) == 0)
            {
                throw new IOException($"Error 0x{80510206:X8}");
            }

            var dataSize = _desc.DataSize;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    break;
                case SeekOrigin.Current:
                    offset += -_position;
                    break;
                case SeekOrigin.End:
                    offset += dataSize;
                    break;
            }
            if (offset > 0xffffffff)
            {
                offset = 0xffffffff;
            }

            if (offset > dataSize)
            {
                offset = dataSize;
            }

            _position = offset;

            return _position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        internal static int CheckBBMac(ReadOnlySpan<byte> data, int size, ReadOnlySpan<byte> key, ReadOnlySpan<byte> hash, int macType, int seed = 0)
        {
            Span<byte> mkey = stackalloc byte[Marshal.SizeOf<AMCTRL.MAC_KEY>()];
            Span<byte> tmpKey = stackalloc byte[0x10];
            int ret = unchecked((int)0x80510201);
            if (hash != null)
            {
                ret = AMCTRL.sceDrmBBMacInit(mkey, macType);
                if (ret != 0)
                {
                    return ret;
                }

                ret = AMCTRL.sceDrmBBMacUpdate(mkey, data, size);
                if (ret != 0)
                {
                    return ret;
                }
                key.CopyTo(tmpKey);
                if (seed != 0)
                {
                    var tmpXor = MemoryMarshal.Cast<byte, int>(tmpKey);
                    tmpXor[0] ^= seed;
                }

                ret = AMCTRL.sceDrmBBMacFinal2(mkey, hash, tmpKey);
                if (ret != 0)
                {
                    ret = unchecked((int)0x80510207);
                }

            }

            return ret;
        }

        internal static int GetMacKey(ReadOnlySpan<byte> data, int size, Span<byte> key, ReadOnlySpan<byte> hash, int macType, int seed = 0)
        {
            Span<byte> mkey = stackalloc byte[Marshal.SizeOf<AMCTRL.MAC_KEY>()];
            Span<byte> tmpKey = stackalloc byte[0x10];
            int ret = unchecked((int)0x80510201);
            if (hash != null)
            {
                ret = AMCTRL.sceDrmBBMacInit(mkey, macType);
                if (ret != 0)
                {
                    return ret;
                }

                ret = AMCTRL.sceDrmBBMacUpdate(mkey, data, size);
                if (ret != 0)
                {
                    return ret;
                }

                ret = AMCTRL.bbmac_getkey(mkey, hash, tmpKey);
                if (ret != 0)
                {
                    ret = unchecked((int)0x80510207);
                }
                if (seed != 0)
                {
                    var tmpXor = MemoryMarshal.Cast<byte, int>(tmpKey);
                    tmpXor[0] ^= seed;
                }
                tmpKey.CopyTo(key);

            }

            return ret;
        }

        internal static int DoBBCipher(Span<byte> data, int size, int seed, ReadOnlySpan<byte> versionKey,
            ReadOnlySpan<byte> headerKey, int cipherType)
        {
            int ret = AMCTRL.sceDrmBBCipherInit(out var ckey, cipherType, 2, headerKey, versionKey, seed);
            if (ret != 0)
            {
                return ret;
            }

            ret = AMCTRL.sceDrmBBCipherUpdate(ref ckey, data, size);
            if (ret != 0)
            {
                return ret;
            }

            ret = AMCTRL.sceDrmBBCipherFinal(ref ckey);
            return ret;
        }

        public byte[] VersionKey => _versionKey;

        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _desc.DataSize;
        public override long Position
        {
            get => _position;
            set => _position = value;
        }
    }
}
