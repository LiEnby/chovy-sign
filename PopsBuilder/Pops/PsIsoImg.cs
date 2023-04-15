using Org.BouncyCastle.Crypto.Paddings;
using PopsBuilder.Atrac3;
using PopsBuilder.Cue;
using PopsBuilder.Psp;
using PspCrypto;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace PopsBuilder.Pops
{
    public class PsIsoImg : PopsImg
    {
        internal PsIsoImg(NpDrmInfo versionKey, DiscCompressor discCompressor) : base(versionKey)
        {
            this.compressor = discCompressor;
        }

        public PsIsoImg(NpDrmInfo versionKey, DiscInfo disc, IAtracEncoderBase encoder) : base(versionKey)
        {
            this.compressor = new DiscCompressor(this, disc, encoder);
        }

        public PsIsoImg(NpDrmInfo versionKey, DiscInfo disc) : base(versionKey)
        {
            this.compressor = new DiscCompressor(this, disc, new Atrac3ToolEncoder());
        }
        public void CreatePsar(bool isPartOfMultiDisc=false)
        {
            compressor.GenerateIsoHeaderAndCompress();
            if (!isPartOfMultiDisc) compressor.WriteSimpleDatLocation((compressor.IsoOffset + compressor.CompressedIso.Length) + StartDat.Length);

            psarUtil.WriteStr("PSISOIMG0000");
            psarUtil.WriteInt64(0x00); // location of STARTDAT

            psarUtil.WritePadding(0x00, 0x3ec); // Skip forwards

            byte[] isoHdrPgd = compressor.GenerateIsoPgd();
            psarUtil.WriteBytes(isoHdrPgd);
            psarUtil.PadUntil(0x00, compressor.IsoOffset);

            compressor.CompressedIso.Seek(0x00, SeekOrigin.Begin);
            compressor.CompressedIso.CopyTo(Psar);

            Psar.Seek(0x00, SeekOrigin.Begin);
            if (isPartOfMultiDisc) return;

            // write STARTDAT
            Int64 startDatLocation = Psar.Position;
            psarUtil.WriteBytes(StartDat);

            // write pgd
            psarUtil.WriteBytes(this.SimplePgd);

            // set STARTDAT location
            Psar.Seek(0xC, SeekOrigin.Begin);
            psarUtil.WriteInt64(startDatLocation);
            
            Psar.Seek(0x00, SeekOrigin.Begin);
        }
        private DiscCompressor compressor;

    }
}