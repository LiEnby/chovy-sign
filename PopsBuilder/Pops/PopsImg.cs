using Org.BouncyCastle.Crypto.Paddings;
using PopsBuilder.Psp;
using PspCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PopsBuilder.Pops
{
    public class PopsImg : NpDrmPsar
    {
        public PopsImg(byte[] versionKey, string contentId) : base(versionKey, contentId)
        {
            startDat = new MemoryStream();
            startDatUtil = new StreamUtil(startDat);

            simple = new MemoryStream();
            simpleUtil = new StreamUtil(simple);

            createStartDat();
            createSimpleDat();

            SimplePgd = generateSimplePgd();

        }
        internal MemoryStream startDat;
        internal StreamUtil startDatUtil;

        private MemoryStream simple;
        private StreamUtil simpleUtil;
        public byte[] SimplePgd;
        internal Random rng = new Random();
        private void createSimpleDat()
        {
            simpleUtil.WriteStr("SIMPLE  ");
            simpleUtil.WriteInt32(100);
            simpleUtil.WriteInt32(16);
            simpleUtil.WriteInt32(Resources.SIMPLE.Length);
            simpleUtil.WriteInt32(0);
            simpleUtil.WriteInt32(0);

            simpleUtil.WriteBytes(Resources.SIMPLE);
        }

        private void createStartDat()
        {
            startDatUtil.WriteStr("STARTDAT");
            startDatUtil.WriteInt32(0x1);
            startDatUtil.WriteInt32(0x1);
            startDatUtil.WriteInt32(0x50);
            startDatUtil.WriteInt32(Resources.STARTDAT.Length);
            startDatUtil.WriteInt32(0x0);
            startDatUtil.WriteInt32(0x0);

            startDatUtil.WritePadding(0, 0x30);

            startDatUtil.WriteBytes(Resources.STARTDAT);
        }

        private byte[] generateSimplePgd()
        {
            simple.Seek(0x0, SeekOrigin.Begin);
            byte[] simpleData = simple.ToArray();

            int simpleSz = DNASHelper.CalculateSize(simpleData.Length, 0x400);
            byte[] simpleEnc = new byte[simpleSz];

            // get pgd
            int sz = DNASHelper.Encrypt(simpleEnc, simpleData, VersionKey, simpleData.Length, 1, 1, blockSize: 0x400);
            byte[] pgd = simpleEnc.ToArray();
            Array.Resize(ref pgd, sz);

            return pgd;
        }


        public byte[] GenerateDataPsp()
        {
            Span<byte> loaderEnc = new byte[0x9B13];

            byte[] dataPspElf = Resources.DATAPSPSD;

            // calculate size low and high part ..
            uint szLow = Convert.ToUInt32(Psar.Length) >> 16;
            uint szHigh = Convert.ToUInt32(Psar.Length) & 0xFFFF;

            // convert to big endain bytes
            byte[] lowBits = BitConverter.GetBytes(Convert.ToUInt16(szLow)).ToArray();
            byte[] highBits = BitConverter.GetBytes(Convert.ToUInt16(szHigh)).ToArray();

            // overwrite data.psar size check ..
            Array.ConstrainedCopy(lowBits, 0, dataPspElf, 0x68C, 0x2);
            Array.ConstrainedCopy(highBits, 0, dataPspElf, 0x694, 0x2);

            SceMesgLed.Encrypt(loaderEnc, dataPspElf, 0x0DAA06F0, SceExecFileDecryptMode.DECRYPT_MODE_POPS_EXEC, VersionKey, ContentId, Resources.DATAPSPSDCFG);
            return loaderEnc.ToArray();
        }



    }
}
