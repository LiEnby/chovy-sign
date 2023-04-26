using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace PspCrypto
{
    using PgdHeader = DNASStream.PgdHeader;
    using PgdDesc = DNASStream.PgdDesc;

    public static class DNASHelper
    {
        public static int CalculateSize(int dataSize, int blockSize)
        {
            int alignSize = (dataSize + 15) & ~15;
            int tableSize = ((alignSize + blockSize - 1) & ~(blockSize - 1)) / (blockSize / 16);
            int pgdSize = 0x90 + alignSize + tableSize;
            return pgdSize;
        }

        public static int Encrypt(Span<byte> pgdData, ReadOnlySpan<byte> data, ReadOnlySpan<byte> key, int dataSize, int keyIndex, int drmType, int flag = 2, int blockSize = 0x400)
        {
            // Additional size variables.
            var dataOffset = 0x90;
            var alignSize = (dataSize + 15) & ~15;
            var tableOffset = dataOffset + alignSize;
            var tableSize = ((alignSize + blockSize - 1) & ~(blockSize - 1)) / (blockSize / 16);
            var pgdSize = 0x90 + alignSize + tableSize;

            if (pgdData.Length < pgdSize)
            {
                return -1;
            }

            data[..dataSize].CopyTo(pgdData[dataOffset..]);

            ref var pgdHdr = ref Utils.AsRef<PgdHeader>(pgdData);
            pgdHdr.Magic = 0x44475000;
            pgdHdr.KeyIndex = keyIndex;
            pgdHdr.DrmType = drmType;

            // Select the hashing, crypto and open modes.
            int macType;
            int cipherType;
            var openFlag = flag;
            if (drmType == 1)
            {
                macType = 1;
                cipherType = 1;
                openFlag |= 4;
                if (keyIndex > 1)
                {
                    macType = 3;
                    openFlag |= 0xc;
                }
            }
            else
            {
                macType = 2;
                cipherType = 2;
            }

            // Select the fixed DNAS key.


            byte[] dnasKey = null;

            if ((openFlag & 2) != 0)
            {
                dnasKey = DNASStream.DnasKey1;
            }
            else if ((openFlag & 1) != 0)
            {
                dnasKey = DNASStream.DnasKey2;
            }

            if (dnasKey == null)
            {
                throw new Exception();
            }

            // Set the decryption parameters in the decrypted header.
            ref var pgdDesc = ref Utils.AsRef<PgdDesc>(pgdHdr.PgdDesc);
            pgdDesc.DataSize = dataSize;
            pgdDesc.BlockSize = blockSize;
            pgdDesc.DataOffset = dataOffset;

            // Generate random header and data keys.
            RandomNumberGenerator.Fill(pgdData.Slice(0x10, 0x30));

            // Encrypt the data.
            DNASStream.DoBBCipher(pgdData[dataOffset..], alignSize, 0, key, pgdDesc.Key, cipherType);

            // Build data MAC hash.
            var tableNum = tableSize / 16;
            for (int i = 0; i < tableNum; i++)
            {
                int rsize = alignSize - i * blockSize;
                if (rsize > blockSize)
                    rsize = blockSize;
                if (keyIndex < 3)
                {
                    BuildBBMac(pgdData[(dataOffset + i * blockSize)..], rsize, key,
                        pgdData[(tableOffset + i * 16)..],
                        macType);
                }
                else
                {
                    BuildBBMac(pgdData[(dataOffset + i * blockSize)..], rsize, key,
                        pgdData[(tableOffset + i * 16)..],
                        macType);
                }
            }

            // Build table MAC hash.
            BuildBBMac(pgdData.Slice(tableOffset), tableSize, key, pgdHdr.MacTableHash, macType);

            // Encrypt the PGD header block (0x30 bytes).
            DNASStream.DoBBCipher(pgdHdr.PgdDesc, 0x30, 0, key, pgdHdr.DescKey, cipherType);

            // Build MAC hash at 0x70 (key hash).
            BuildBBMac(pgdData, 0x70, key, pgdHdr.Hash70, macType);

            // Build MAC hash at 0x80 (DNAS hash).
            BuildBBMac(pgdData, 0x80, dnasKey, pgdHdr.Hash80, macType);

            return pgdSize;
        }

        static int BuildBBMac(ReadOnlySpan<byte> data, int size, ReadOnlySpan<byte> key, Span<byte> hash, int macType, int seed = 0)
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

                ret = AMCTRL.sceDrmBBMacFinal(mkey, hash, tmpKey);
                if (ret != 0)
                {
                    ret = unchecked((int)0x80510207);
                }

                if (macType == 3)
                {
                    Utils.BuildDrmBBMacFinal2(hash);
                }

            }

            return ret;
        }
    }
}
