using Org.BouncyCastle.Crypto.Paddings;
using PopsBuilder.Atrac3;
using PopsBuilder.Cue;
using PspCrypto;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace PopsBuilder.Pops
{
    public class PsIsoImg : PopsImg
    {
        internal PsIsoImg(byte[] versionkey, string contentId, DiscCompressor discCompressor) : base(versionkey, contentId)
        {
            this.compressor = discCompressor;
        }

        public PsIsoImg(byte[] versionkey, string contentId, DiscInfo disc, IAtracEncoderBase encoder) : base(versionkey, contentId)
        {
            this.compressor = new DiscCompressor(this, disc, encoder);
        }

        public PsIsoImg(byte[] versionkey, string contentId, DiscInfo disc) : base(versionkey, contentId)
        {
            this.compressor = new DiscCompressor(this, disc, new Atrac3ToolEncoder());
        }
        public void CreatePsar(bool isPartOfMultiDisc=false)
        {
            compressor.GenerateIsoHeaderAndCompress();
            if (!isPartOfMultiDisc) compressor.WriteSimpleDatLocation((compressor.IsoOffset + compressor.CompressedIso.Length) + startDat.Length);

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
            startDat.Seek(0x00, SeekOrigin.Begin);
            startDat.CopyTo(Psar);

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