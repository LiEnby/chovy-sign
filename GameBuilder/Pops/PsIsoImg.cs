using Li.Progress;
using GameBuilder.Atrac3;
using GameBuilder.Cue;
using GameBuilder.Psp;
using PspCrypto;
using System;
using System.Net;

namespace GameBuilder.Pops
{
    public class PsIsoImg : PopsImg, IDisposable
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

        internal void generatePsIsoHeader()
        {
            psarUtil.WriteStr("PSISOIMG0000");
            psarUtil.WriteInt64(0x00); // location of STARTDAT

            psarUtil.WritePadding(0x00, 0x3ec); // Skip forwards

            byte[] isoHdrPgd = compressor.GenerateIsoPgd();
            psarUtil.WriteBytes(isoHdrPgd);
            psarUtil.PadUntil(0x00, compressor.IsoOffset);
        }

        public override void CreatePsar()
        {
            // compress ISO and generate header.
            compressor.GenerateIsoHeaderAndCompress();

            // write STARTDAT location
            compressor.WriteSimpleDatLocation((compressor.IsoOffset + compressor.CompressedIso.Length) + StartDat.Length);
            
            // write general PSISO header
            generatePsIsoHeader();

            // Copy compressed ISO to PSAR stream..
            copyToProgress(compressor.CompressedIso, Psar, "Copy Compressed ISO to PSISOIMG");

            // write STARTDAT
            Int64 startDatLocation = Psar.Position;
            psarUtil.WriteBytes(StartDat);

            // write pgd
            psarUtil.WriteBytes(this.SimplePgd);

            // set STARTDAT location
            Psar.Seek(0xC, SeekOrigin.Begin);
            psarUtil.WriteInt64(startDatLocation);
        }

        public override void Dispose()
        {
            compressor.Dispose();
            base.Dispose();
        }
        private void onProgress(ProgressInfo inf)
        {
            this.updateProgress(inf.Done, inf.Remain, inf.CurrentProcess);
        }

        private DiscCompressor compressor;

    }
}