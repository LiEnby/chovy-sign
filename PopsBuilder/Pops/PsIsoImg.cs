using GameBuilder.Atrac3;
using GameBuilder.Cue;
using GameBuilder.Psp;
using PspCrypto;
using System;
using System.Net;
using GameBuilder.Progress;

namespace GameBuilder.Pops
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
            this.compressor.RegisterCallback(onProgress);
        }

        public PsIsoImg(NpDrmInfo versionKey, DiscInfo disc) : base(versionKey)
        {
            this.compressor = new DiscCompressor(this, disc, new Atrac3ToolEncoder());
            this.compressor.RegisterCallback(onProgress);
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

            if (isPartOfMultiDisc) return;

            // write STARTDAT
            Int64 startDatLocation = Psar.Position;
            psarUtil.WriteBytes(StartDat);

            // write pgd
            psarUtil.WriteBytes(this.SimplePgd);

            // set STARTDAT location
            Psar.Seek(0xC, SeekOrigin.Begin);
            psarUtil.WriteInt64(startDatLocation);
        }


        private void onProgress(ProgressInfo inf)
        {
            this.UpdateProgress(inf.Done, inf.Remain, inf.CurrentProcess);
        }

        private DiscCompressor compressor;

    }
}