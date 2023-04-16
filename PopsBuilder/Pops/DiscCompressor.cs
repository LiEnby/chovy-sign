using GameBuilder.Atrac3;
using GameBuilder.Cue;
using GameBuilder.Psp;
using PspCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Pops
{
    public class DiscCompressor
    {
        const int COMPRESS_BLOCK_SZ = 0x9300;
        const int DEFAULT_ISO_OFFSET = 0x100000;
        public int IsoOffset;

        internal DiscCompressor(NpDrmPsar srcImg, DiscInfo disc, IAtracEncoderBase encoder, int offset = DEFAULT_ISO_OFFSET)
        {
            this.srcImg = srcImg;
            this.disc = disc;
            this.cue = new CueReader(disc.CueFile);

            this.IsoHeader = new MemoryStream();
            this.CompressedIso = new MemoryStream();

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
            isoHeaderUtil.WriteStrWithPadding(disc.DiscIdHdr, 0x00, 0x400);
        }


        public byte[] GenerateIsoPgd()
        {
            IsoHeader.Seek(0x0, SeekOrigin.Begin);
            byte[] isoHdr = IsoHeader.ToArray();

            int headerSize = DNASHelper.CalculateSize(isoHdr.Length, 0x400);
            byte[] headerEnc = new byte[headerSize];

            int sz = DNASHelper.Encrypt(headerEnc, isoHdr, srcImg.DrmInfo.VersionKey, isoHdr.Length, srcImg.DrmInfo.KeyType, 1, blockSize: 0x400);
            byte[] isoHdrPgd = headerEnc.ToArray();
            Array.Resize(ref isoHdrPgd, sz);

            return isoHdrPgd;
        }

        private void writeIsoLocation()
        {
            isoHeaderUtil.WriteInt32(0);
            isoHeaderUtil.WriteInt32(0);
            isoHeaderUtil.WriteInt32(0);

            isoHeaderUtil.WriteInt32(IsoOffset); // always 0x100000 on single disc game

            isoHeaderUtil.WritePadding(0x00, 0x620);
        }

        private void writeCompressedIso()
        {
            using (CueStream cueStr = cue.OpenTrack(cue.FirstDataTrackNo))
            {
                using (EccRemoverStream eccRem = new EccRemoverStream(cueStr))
                {
                    while (eccRem.Position < eccRem.Length)
                    {
                        Console.Write(Math.Floor(Convert.ToDouble(eccRem.Position) / Convert.ToDouble(eccRem.Length) * 100.0) + "%\r");
                        writeCompressedIsoBlock(eccRem);
                    }
                }
            }
        }
        public void GenerateIsoHeaderAndCompress()
        {
            writeHeader();
            writeTOC();
            writeIsoLocation();
            writeName();

            writeCompressedIso();

            isoHeaderUtil.PadUntil(0x0, 0xb3880);

            // now write CD-Audio data.
            writeCompressedCDATracks();
        }

        public void WriteSimpleDatLocation(Int64 location)
        {
            IsoHeader.Seek(0xE20, SeekOrigin.Begin);
            isoHeaderUtil.WriteInt64(location); 
        }
        private void writeName()
        {
            // copied from crash bandicoot warped

            isoHeaderUtil.WriteInt64(0x00); // SIMPLE.DAT location

            isoHeaderUtil.WriteInt32(2047); // unk
            isoHeaderUtil.WriteStrWithPadding(disc.DiscName, 0x00, 0x80);
            isoHeaderUtil.WriteInt32(3); // unk

            isoHeaderUtil.WriteInt32(0x72d0ee59); // appears to be constant?
            isoHeaderUtil.WriteInt32(0);
            isoHeaderUtil.WriteInt32(0);
            isoHeaderUtil.WriteInt32(0);

            isoHeaderUtil.WritePadding(0, 0x2D40);
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

                Console.WriteLine("Encoding track " + i + " to ATRAC3.");

                using (CueStream audioStream = cue.OpenTrack(i))
                {
                    uint key = Rng.RandomUInt();

                    Atrac3ToolEncoder enc = new Atrac3ToolEncoder();

                    byte[] pcmData = new byte[audioStream.Length];
                    audioStream.Read(pcmData, 0x00, pcmData.Length);

                    byte[] atracData = enc.EncodeToAtrac(pcmData);

                    writeCDAEntry(Convert.ToInt32(CompressedIso.Position), atracData.Length, key);

                    using (MemoryStream atracStream = new MemoryStream(atracData))
                    {
                        using (MemoryStream encryptedAtracStream = new MemoryStream())
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


        private DiscInfo disc;
        private CueReader cue;
        private NpDrmPsar srcImg;

        public MemoryStream IsoHeader;
        public MemoryStream CompressedIso;

        private StreamUtil isoHeaderUtil;
        private IAtracEncoderBase atrac3Encoder;
    }
}
