using Li.Progress;
using Li.Utilities;
using GameBuilder.Atrac3;
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
    public class PsTitleImg : PopsImg
    {
        const int MAX_DISCS = 5;
        const int PSISO_ALIGN = 0x8000;

        private int discNumber = 0;

        private void onProgress(ProgressInfo inf)
        {
            this.updateProgress(inf.Done, inf.Remain, inf.CurrentProcess + " (disc " + discNumber + ")");
        }

        public PsTitleImg(NpDrmInfo drmInfo, DiscInfo[] discs) : base(drmInfo)
        {
            if (discs.Length > MAX_DISCS) throw new Exception("Sorry, multi disc games only support up to 5 discs... (i dont make the rules)");
            this.compressors = new DiscCompressor[MAX_DISCS];
            this.discs = discs;

            // ensure disc name is the same for all discs..
            string discName = discs.First().DiscName;
            for (int i = 0; i < discs.Length; i++)
                discs[i].DiscName = discName;

            // create compressor objects.
            for (int i = 0; i < compressors.Length; i++)
            {
                if (i > (discs.Length - 1))
                {
                    compressors[i] = null;
                }
                else
                {
                    compressors[i] = new DiscCompressor(this, discs[i], new Atrac3ToolEncoder());
                    compressors[i].RegisterCallback(onProgress);
                }
            }


            isoMap = new MemoryStream();
            isoMapUtil = new StreamUtil(isoMap);

            isoPart = new MemoryStream();
            isoPartUtil = new StreamUtil(isoPart);


        }

        public override void CreatePsar()
        {
            createIsoMap();

            psarUtil.WriteStr("PSTITLEIMG000000");
            psarUtil.WriteInt64(PSISO_ALIGN + isoPart.Length); // location of STARTDAT
            
            psarUtil.WriteBytes(Rng.RandomBytes(0x10)); // dunno what this is
            psarUtil.WritePadding(0x00, 0x1D8);

            byte[] isoMap = generateIsoMapPgd();
            psarUtil.WriteBytes(isoMap);
            psarUtil.PadUntil(0x00, PSISO_ALIGN);

            copyToProgress(isoPart, Psar, "Copy ISOs to PSTITLEIMG");

            psarUtil.WriteBytes(StartDat);
            psarUtil.WriteBytes(SimplePgd);
        }

        private byte[] calculateChecksumForIsoImgTitle(byte[] header)
        {
            byte[] checksum = new byte[0x10];
            Span<byte> mkey = stackalloc byte[Marshal.SizeOf<PspCrypto.AMCTRL.MAC_KEY>()];

            PspCrypto.AMCTRL.sceDrmBBMacInit(mkey, 3);
            PspCrypto.AMCTRL.sceDrmBBMacUpdate(mkey, header, header.Length /*0xb3c80*/);
            Span<byte> newKey = new byte[20 + 0x10];
            PspCrypto.AMCTRL.sceDrmBBMacFinal(mkey, newKey[20..], DrmInfo.VersionKey);
            ref var aesHdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_AES128CBC_HEADER>(newKey);
            aesHdr.mode = KIRKEngine.KIRK_MODE_ENCRYPT_CBC;
            aesHdr.keyseed = 0x63;
            aesHdr.data_size = 0x10;
            PspCrypto.KIRKEngine.sceUtilsBufferCopyWithRange(newKey, 0x10, newKey, 0x10, KIRKEngine.KIRK_CMD_ENCRYPT_IV_0);

            newKey.Slice(20, 0x10).CopyTo(checksum);

            return checksum;
        }

        private byte[] generateIsoMapPgd()
        {
            isoMap.Seek(0, SeekOrigin.Begin);
            byte[] isoMapBuf = isoMap.ToArray();

            int encryptedSz = DNASHelper.CalculateSize(isoMapBuf.Length, 1024);
            var isoMapEnc = new byte[encryptedSz];

            DNASHelper.Encrypt(isoMapEnc, isoMapBuf, DrmInfo.VersionKey, isoMapBuf.Length, DrmInfo.KeyIndex, 1);

            return isoMapEnc;
        }

        private void createIsoMap()
        {
            byte[] checksums = new byte[0x10 * MAX_DISCS];
            for(int i = 0; i < MAX_DISCS; i++)
            {
                discNumber++;
                if (compressors[i] is null) { isoMapUtil.WriteInt32(0); continue; };

                int padLen = MathUtil.CalculatePaddingAmount(Convert.ToInt32(isoPart.Position), PSISO_ALIGN);
                isoPartUtil.WritePadding(0x00, padLen);

                using (PsIsoImg psIsoImg = new PsIsoImg(this.DrmInfo, compressors[i]))
                {
                    // write location of iso
                    isoMapUtil.WriteUInt32(Convert.ToUInt32(PSISO_ALIGN + isoPart.Position));

                    // compress current iso and generate headers
                    compressors[i].GenerateIsoHeaderAndCompress();
                    
                    // generate PSISOIMG header
                    psIsoImg.generatePsIsoHeader();

                    // Copy compressed ISO to PSISOIMG
                    copyToProgress(compressors[i].CompressedIso, psIsoImg.Psar, "Copy Compressed ISO (" + i + ") to PSISOIMG");

                    // read 0x400 bytes from PSAR copy iso header after that,.
                    psIsoImg.Psar.Seek(0x0, SeekOrigin.Begin);
                    compressors[i].IsoHeader.Seek(0x00, SeekOrigin.Begin);
                    byte[] isoHdr = new byte[compressors[i].IsoHeader.Length + 0x400];
                    psIsoImg.Psar.Read(isoHdr, 0x00, 0x400);
                    compressors[i].IsoHeader.Read(isoHdr, 0x400, Convert.ToInt32(compressors[i].IsoHeader.Length));

                    // Calculate checksum 
                    byte[] checksum = calculateChecksumForIsoImgTitle(isoHdr);
                    Array.ConstrainedCopy(checksum, 0, checksums, i * 0x10, 0x10);

                    // copy psiso to TITLE ..
                    copyToProgress(psIsoImg.Psar, isoPart, "Copy PSISOIMG (" + i + ") to PSTITLEIMG");

                }

            }
            isoMapUtil.WriteBytes(checksums);
            isoMapUtil.WriteStrWithPadding(discs.First().DiscIdHdr, 0x00, 0x20);

            isoMapUtil.WriteInt64(Convert.ToInt64(PSISO_ALIGN + isoPart.Length + StartDat.Length));
            isoMapUtil.WriteBytes(Rng.RandomBytes(0x80));
            isoMapUtil.WriteStrWithPadding(discs.First().DiscName, 0x00, 0x80);
            isoMapUtil.WriteInt32(MAX_DISCS);
            isoMapUtil.WritePadding(0x00, 0x70);
        }

        public override void Dispose()
        {
            isoPart.Dispose();
            isoMap.Dispose();
            base.Dispose();
        }

        private DiscInfo[] discs;
        private DiscCompressor[] compressors;

        private MemoryStream isoPart;
        private StreamUtil isoPartUtil;

        private MemoryStream isoMap;
        private StreamUtil isoMapUtil;
    }
}
