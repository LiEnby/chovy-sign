using GameBuilder.Atrac3;
using Li.Progress;
using GameBuilder.Cue;
using GameBuilder.Psp;
using PspCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Li.Utilities;
using GameBuilder.Pops.LibCrypt;

namespace GameBuilder.Pops
{
    public class DiscCompressor : ProgressTracker, IDisposable
    {
        const int COMPRESS_BLOCK_SZ = 0x9300;
        const int DEFAULT_ISO_OFFSET = 0x100000;
        public int IsoOffset;

        internal DiscCompressor(PopsImg srcImg, PSInfo disc, IAtracEncoderBase encoder, int offset = DEFAULT_ISO_OFFSET)
        {
            this.srcImg = srcImg;
            this.disc = disc;
            this.cue = new CueReader(disc.CueFile);

            this.IsoHeader = new BuildStream();
            this.CompressedIso = new BuildStream();

            this.isoHeaderUtil = new StreamUtil(IsoHeader);
            this.atrac3Encoder = encoder;
            this.IsoOffset = offset;
        }


        private void writeCompressedIsoBlock(Stream s)
        {
            byte[] isoBlock = new byte[COMPRESS_BLOCK_SZ];
            int read = s.Read(isoBlock, 0, isoBlock.Length);

            byte[] compressed = Lz.compress(isoBlock);

            ushort sz = Convert.ToUInt16(compressed.Length);
            int ptr = Convert.ToInt32(CompressedIso.Position);
            writeIsoTblEntry(ptr, sz, compressed);

            CompressedIso.Write(compressed, 0, compressed.Length);

        }
        

        private void writeIsoTblEntry(int ptr, ushort sz, byte[] data)
        {
            isoHeaderUtil.WriteInt32(ptr);
            isoHeaderUtil.WriteUInt16(sz);

            isoHeaderUtil.WriteInt16(1); // mark that this is part of the image.

            isoHeaderUtil.WriteBytes(calculatePs1CompressedIsoSegmentChecksum(data));

            isoHeaderUtil.WritePadding(0x00, 0x8);
        }


        private void writeHeader()
        {
            isoHeaderUtil.WriteCStrWithPadding(disc.DiscIdHdr, 0x00, 0x400);
        }
        private void writeIsoLocation()
        {
            isoHeaderUtil.WriteInt32(0);
            isoHeaderUtil.WriteInt32(0);
            isoHeaderUtil.WriteInt32(0);

            isoHeaderUtil.WriteInt32(IsoOffset); // always 0x100000 on single disc game

            isoHeaderUtil.WritePadding(0x00, 0x628);
        }

        private void writeCompressedIso()
        {
            using (CueStream cueStr = cue.OpenTrack(cue.FirstDataTrackNo))
            {
                using (EccRemoverStream eccRem = new EccRemoverStream(cueStr))
                {
                    while (eccRem.Position < eccRem.Length)
                    {                        
                        writeCompressedIsoBlock(eccRem);
                        updateProgress(Convert.ToInt32(eccRem.Position), Convert.ToInt32(eccRem.Length), "Compress & Encrypt Disc");
                    }
                }
            }
        }

        private void writeSubChannelPgd()
        {
            if(disc.LibCrypt.Method == LibCryptMethod.METHOD_SUB_CHANNEL)
            {
                byte[] subChannelsData = disc.LibCrypt.Subchannels;
                
                int sz = subChannelsData.Length / 0xC;
                uint location = Convert.ToUInt32(IsoOffset + CompressedIso.Position);
                writeSubchannelDatLocation(location, sz);

                byte[] pgdData = srcImg.CreatePgd(subChannelsData);

                CompressedIso.Write(pgdData, 0, pgdData.Length);
            }
        }

        public void GenerateIsoHeaderAndCompress()
        {
            writeHeader();
            writeTOC();
            writeIsoLocation();
            writeDiscInfo();
            writeLibCryptData();

            writeCompressedIso();

            isoHeaderUtil.PadUntil(0x0, 0xb3880);

            // write CD Audio data.
            writeCompressedCDATracks();

            // write subchannels
            writeSubChannelPgd();
        }

        private void writeSubchannelDatLocation(uint location, int totalSubchannels)
        {
            isoHeaderUtil.WriteUInt32At(location, 0xED4);
            isoHeaderUtil.WriteInt32At(totalSubchannels, 0xED8);
        }

        public void WriteSimpleDatLocation(uint location)
        {
            isoHeaderUtil.WriteUInt32At(location, 0xE20); 
        }

        private void writeLibCryptData()
        {
            isoHeaderUtil.WriteInt32(disc.LibCrypt.ObfuscatedMagicWord);
            isoHeaderUtil.WriteInt32(0);
            isoHeaderUtil.WriteInt32(0);
            isoHeaderUtil.WriteInt32(0);

            isoHeaderUtil.WritePadding(0, 0x2D40);

        }
        private void writeDiscInfo()
        {
            isoHeaderUtil.WriteUInt32(Convert.ToUInt32(disc.LibCrypt.Method)); // libcrypt method
            isoHeaderUtil.WriteCStrWithPadding(disc.DiscName, 0x00, 0x80);  // disc title
            isoHeaderUtil.WriteInt32(3); // PARENTAL_LEVEL ?
        }

        private void writeCDAEntry(int position, int length, uint key)
        {
            isoHeaderUtil.WriteInt32(position);
            isoHeaderUtil.WriteInt32(length);
            isoHeaderUtil.WriteInt32(0);
            isoHeaderUtil.WriteUInt32(key);
        }

        private void writeCompressedCDATracks()
        {

            IsoHeader.Seek(0x800, SeekOrigin.Begin); // CDA Entries

            int totalTracks = cue.GetTotalTracks();
            for (int i = 1; i <= totalTracks; i++)
            {
                if (cue.GetTrackNumber(i).TrackType != TrackType.TRACK_CDDA) continue;
                updateProgress(i, totalTracks, "Convert CD Audio tracks to ATRAC3");

                using (CueStream audioStream = cue.OpenTrack(i))
                {
                    uint key = Rng.RandomUInt();

                    Atrac3ToolEncoder enc = new Atrac3ToolEncoder();

                    byte[] pcmData = new byte[audioStream.Length];
                    audioStream.Read(pcmData, 0x00, pcmData.Length);

                    byte[] atracData = enc.EncodeToAtrac(pcmData);

                    writeCDAEntry(Convert.ToInt32(CompressedIso.Position), atracData.Length, key);

                    using (BuildStream atracStream = new BuildStream(atracData))
                    {
                        using (BuildStream encryptedAtracStream = new BuildStream())
                        {
                            AtracCrypto.ScrambleAtracData(atracStream, encryptedAtracStream, key);
                            encryptedAtracStream.Seek(0x00, SeekOrigin.Begin);
                            encryptedAtracStream.CopyTo(CompressedIso);
                        }
                    }

                }
            }
        }

        private byte[] calculatePs1CompressedIsoSegmentChecksum(byte[] data)
        {
            byte[] outChecksum = new byte[0x10];

            Span<byte> mkey = stackalloc byte[Marshal.SizeOf<AMCTRL.MAC_KEY>()];

            AMCTRL.sceDrmBBMacInit(mkey, 3);
            AMCTRL.sceDrmBBMacUpdate(mkey, data, data.Length);
            Span<byte> checksum = new byte[20 + 0x10];
            AMCTRL.sceDrmBBMacFinal(mkey, checksum[20..], srcImg.DrmInfo.VersionKey);

            ref var aesHdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_AES128CBC_HEADER>(checksum);
            aesHdr.mode = KIRKEngine.KIRK_MODE_ENCRYPT_CBC;
            aesHdr.keyseed = 0x63;
            aesHdr.data_size = 0x10;
            KIRKEngine.sceUtilsBufferCopyWithRange(checksum, 0x10, checksum, 0x10, KIRKEngine.KIRK_CMD_ENCRYPT_IV_0);

            checksum.Slice(20, 0x10).CopyTo(outChecksum);

            return outChecksum;
        }

        private void writeTOC()
        {
            isoHeaderUtil.WriteBytes(cue.CreateToc());
        }

        public void Dispose()
        {
            IsoHeader.Dispose();
            CompressedIso.Dispose();
            cue.Dispose();
        }

        private PSInfo disc;
        private CueReader cue;
        private PopsImg srcImg;

        public BuildStream IsoHeader;
        public BuildStream CompressedIso;

        private StreamUtil isoHeaderUtil;
        private IAtracEncoderBase atrac3Encoder;
    }
}
