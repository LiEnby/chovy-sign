﻿using GameBuilder.Pops;
using PspCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Li.Utilities;

namespace GameBuilder.Psp
{
    public class NpUmdImg : NpDrmPsar
    {
        const int RATIO_LIMIT = 90;
        const int BLOCK_BASIS = 0x10;
        const int SECTOR_SZ = 2048;
        const int BLOCK_SZ = BLOCK_BASIS * SECTOR_SZ;
        public NpUmdImg(NpDrmInfo drmInfo, UmdInfo umdImage, bool compress) : base(drmInfo)
        {
            this.compress = compress;

            this.npHdr = new BuildStream();
            this.npHdrUtil = new StreamUtil(npHdr);

            this.npHdrBody = new BuildStream();
            this.npHdrBodyUtil = new StreamUtil(npHdrBody);

            this.npTbl = new BuildStream();
            this.npTblUtil = new StreamUtil(npTbl);
            
            this.isoData = new BuildStream();
            this.isoDataUtil = new StreamUtil(isoData);

            this.headerKey = Rng.RandomBytes(0x10);

            this.umdImage = umdImage;
            isoBlocks = Convert.ToInt64((umdImage.IsoStream.Length + BLOCK_SZ - 1) / BLOCK_SZ);

        }

        public void PatchSfo()
        {
            Sfo sfoKeys = Sfo.ReadSfo(umdImage.DataFiles["PARAM.SFO"]);
            //if ((sfoKeys["CATEGORY"] as String) == "UG") // "UMD Game"
            sfoKeys["CATEGORY"] = "EG"; // set it to "Eboot Game"
            umdImage.DataFiles["PARAM.SFO"] = sfoKeys.WriteSfo();
        }
        private void createNpHdr()
        {
            npHdrUtil.WriteStr("NPUMDIMG");
            npHdrUtil.WriteInt32(DrmInfo.KeyIndex);
            npHdrUtil.WriteInt32(BLOCK_BASIS);
            npHdrUtil.WriteCStrWithPadding(DrmInfo.ContentId, 0x00, 0x30);

            createNpUmdBody();
            byte[] npumdDec = npHdrBody.ToArray();
            byte[] npumdEnc = encryptHeader(npumdDec);

            npHdrUtil.WriteBytes(npumdEnc);
            npHdrUtil.WriteBytes(this.headerKey);
            npHdrUtil.WriteBytes(this.dataKey);

            byte[] npumdhdr = npHdr.ToArray();
            byte[] npumdheaderHash = hashBlock(npumdhdr);

            npHdrUtil.WriteBytes(npumdheaderHash);
            npHdrUtil.WriteBytes(Rng.RandomBytes(0x8)); // padding
            npHdrUtil.PadUntil(0x00, 0x100);
        }

        public override void CreatePsar()
        {
            PatchSfo();
            createNpUmdTbl();
            byte[] tbl = encryptTable();
            this.dataKey = hashBlock(tbl);
            createNpHdr();

            byte[] npHdrBuf = npHdr.ToArray();
            ECDsaHelper.SignNpImageHeader(npHdrBuf);
            psarUtil.WriteBytes(npHdrBuf);
            psarUtil.WriteBytes(tbl);

            copyToProgress(isoData, Psar, "Copy UMD Image to NPUMDIMG");
        }

        private byte[] signParamSfo(byte[] paramSfo)
        {
            int paramSfoLen = paramSfo.Length;
            byte[] contentIdBytes = Encoding.UTF8.GetBytes(DrmInfo.ContentId);
            Array.Resize(ref paramSfo, paramSfoLen + 0x30);
            Array.ConstrainedCopy(contentIdBytes, 0, paramSfo, paramSfoLen, contentIdBytes.Length);
            byte[] signature = new byte[0x30];

            ECDsaHelper.SignParamSfo(paramSfo, signature);
            return signature;
        }

        public override byte[] GenerateDataPsp()
        {
            byte[] startDat = CreateStartDat(umdImage.Minis ? Resources.STARTDATMINIS : Resources.STARTDATPSP);
            using (BuildStream dataPsp = new BuildStream())
            {
                StreamUtil dataPspUtil = new StreamUtil(dataPsp);
                byte[] signature = signParamSfo(umdImage.DataFiles["PARAM.SFO"]);
                dataPspUtil.WriteBytes(signature);
                dataPspUtil.WritePadding(0x00, 0x530);
                dataPspUtil.WriteCStrWithPadding(DrmInfo.ContentId, 0x00, 0x30);
                dataPspUtil.WriteInt32BE(DrmInfo.KeyIndex);
                dataPspUtil.WriteInt32(0);
                dataPspUtil.WriteInt32(0);
                dataPspUtil.WriteInt32(0);
                dataPspUtil.WriteBytes(startDat);
                return dataPsp.ToArray();
            }
        }

        private int compressAndWriteBlock(Int64 isoOffset)
        {
            int wsize = 0;
            byte[] isoBuf = new byte[BLOCK_SZ];
            wsize = umdImage.IsoStream.Read(isoBuf, 0x00, BLOCK_SZ);

            byte[] wbuf = isoBuf;

            if (this.compress) // Compress data.
            {
                byte[] lzRcBuf = Lz.compress(isoBuf, true);
                //memset(lzrc_buf + lzrc_size, 0, 16);

                int ratio = (lzRcBuf.Length * 100) / BLOCK_SZ;

                if (ratio < RATIO_LIMIT)
                {
                    wbuf = lzRcBuf;

                    wsize = lzRcBuf.Length;
                    wsize += MathUtil.CalculatePaddingAmount(wsize, 16);
                    Array.Resize(ref lzRcBuf, wsize);
                }
            }

            int unpaddedSz = wsize;
            wsize += MathUtil.CalculatePaddingAmount(wsize, 16);
            Array.Resize(ref wbuf, wsize);
            encryptBlock(wbuf, Convert.ToInt32(isoOffset));
            byte[] hash = hashBlock(wbuf);

            npTblUtil.WriteBytes(hash);
            npTblUtil.WriteUInt32(Convert.ToUInt32(isoOffset));
            npTblUtil.WriteUInt32(Convert.ToUInt32(unpaddedSz));
            npTblUtil.WriteInt32(0);
            npTblUtil.WriteInt32(0);

            isoData.Write(wbuf, 0, wsize);

            return wsize;

        }

        private void createNpUmdTbl()
        {
            Int64 tableSz = isoBlocks * 0x20;
            Int64 isoSz = umdImage.IsoStream.Length;
            Int64 isoOffset = 0x100 + tableSz;

            for (int i = 0; i < isoBlocks; i++)
            {
                isoOffset += compressAndWriteBlock(isoOffset);
                updateProgress(Convert.ToInt32(umdImage.IsoStream.Position), Convert.ToInt32(umdImage.IsoStream.Length), "Compress & Encrypt UMD Image");
            }

        }

        private byte[] encryptTable()
        {
            byte[] table = npTbl.ToArray();

            // Encrypt Table
            var tp = MemoryMarshal.Cast<byte, uint>(table);

            for (int i = 0; i < (table.Length / 0x20); i++)
                XorTable(tp[(i * 8)..]);
            
            var tpbyt = MemoryMarshal.Cast<uint, byte>(tp);

            return tpbyt.ToArray();
        }
        private static void XorTable(Span<uint> tp)
        {
            tp[4] ^= tp[3] ^ tp[2];
            tp[5] ^= tp[2] ^ tp[1];
            tp[6] ^= tp[0] ^ tp[3];
            tp[7] ^= tp[1] ^ tp[0];
        }


        private byte[] encryptBlock(byte[] blockData, int offset)
        {
            AMCTRL.CIPHER_KEY ckey = new AMCTRL.CIPHER_KEY();
            AMCTRL.sceDrmBBCipherInit(out ckey, 1, DrmInfo.KeyIndex, headerKey, DrmInfo.VersionKey, offset >> 4);
            AMCTRL.sceDrmBBCipherUpdate(ref ckey, blockData, blockData.Length);
            AMCTRL.sceDrmBBCipherFinal(ref ckey);
            return blockData;
        }

        private byte[] hashBlock(byte[] blockData)
        {
            byte[] hash = new byte[0x10];
            Span<byte> mkey = stackalloc byte[Marshal.SizeOf<PspCrypto.AMCTRL.MAC_KEY>()];
            AMCTRL.sceDrmBBMacInit(mkey, 3);
            AMCTRL.sceDrmBBMacUpdate(mkey, blockData, blockData.Length);
            AMCTRL.sceDrmBBMacFinal(mkey, hash, DrmInfo.VersionKey);
            Utils.BuildDrmBBMacFinal2(hash);
            return hash;
        }


        private void createNpUmdBody()
        {
            npHdrBodyUtil.WriteUInt16(SECTOR_SZ); // sector_sz

            if (umdImage.IsoStream.Length > 0x40000000)
                npHdrBodyUtil.WriteUInt16(0xE001); // unk_2
            else
                npHdrBodyUtil.WriteUInt16(0xE000); //unk_2

            npHdrBodyUtil.WriteUInt32(0); // unk_4
            npHdrBodyUtil.WriteUInt32(0x1010); // unk_8
            npHdrBodyUtil.WriteUInt32(0); // unk_12
            npHdrBodyUtil.WriteUInt32(0); // unk_16

            npHdrBodyUtil.WriteUInt32(0x00); // LBA START
            npHdrBodyUtil.WriteUInt32(0); // unk_24


            npHdrBodyUtil.WriteUInt32(Math.Min(Convert.ToUInt32((isoBlocks * BLOCK_BASIS) - 1), 0x6C0BF)); // nsectors
            npHdrBodyUtil.WriteUInt32(0); // unk_32

            npHdrBodyUtil.WriteUInt32(Convert.ToUInt32((isoBlocks * BLOCK_BASIS) - 1));
            npHdrBodyUtil.WriteUInt32(0x01003FFE); // unk_40
            npHdrBodyUtil.WriteUInt32(0x100); // block_entry_offset 

            npHdrBodyUtil.WriteCStrWithPadding(umdImage.DiscIdSeperated, 0x00, 0x10);

            npHdrBodyUtil.WriteInt32(0); // header_start_offset 
            npHdrBodyUtil.WriteInt32(0); // unk_68

            npHdrBodyUtil.WriteByte(0x00); // unk_72
            npHdrBodyUtil.WriteByte(0x00); // bbmac param
            npHdrBodyUtil.WriteByte(0x00); // unk_74 
            npHdrBodyUtil.WriteByte(0x00); // unk_75 

            npHdrBodyUtil.WriteInt32(0); // unk_76 
            npHdrBodyUtil.WriteInt32(0); // unk_80  
            npHdrBodyUtil.WriteInt32(0); // unk_84 
            npHdrBodyUtil.WriteInt32(0); // unk_88 
            npHdrBodyUtil.WriteInt32(0); // unk_92 

        }

        private byte[] encryptHeader(byte[] headerBytes)
        {
            AMCTRL.CIPHER_KEY ckey = new AMCTRL.CIPHER_KEY();
            AMCTRL.sceDrmBBCipherInit(out ckey, 1, DrmInfo.KeyIndex, headerKey, DrmInfo.VersionKey, 0);
            AMCTRL.sceDrmBBCipherUpdate(ref ckey, headerBytes, headerBytes.Length);
            AMCTRL.sceDrmBBCipherFinal(ref ckey);
            return headerBytes;
        }

        public override void Dispose()
        {
            npHdr.Dispose();
            npHdrBody.Dispose();
            isoData.Dispose();
            npTbl.Dispose();
            base.Dispose();
        }

        private Int64 isoBlocks;
        private bool compress;

        UmdInfo umdImage;

        private byte[] headerKey;
        private byte[] dataKey;


        private BuildStream npHdr;
        private StreamUtil npHdrUtil;

        private BuildStream npHdrBody;
        private StreamUtil npHdrBodyUtil;

        private BuildStream isoData;
        private StreamUtil isoDataUtil;

        private BuildStream npTbl;
        private StreamUtil npTblUtil;
    }
}
