using GameBuilder;
using GameBuilder.Cue;
using GameBuilder.Psp;
using Li.Utilities;
using PspCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Pops
{
    public abstract class PopsImg : NpDrmPsar
    {
        
        public PopsImg(NpDrmInfo versionKey) : base(versionKey)
        {
            simple = new MemoryStream();
            simpleUtil = new StreamUtil(simple);

            this.StartDat = NpDrmPsar.CreateStartDat(Resources.STARTDATPOPS);
            this.createSimpleDat();
            this.SimplePgd = CreatePgd(simple.ToArray());

        }
        internal void createSimpleDat()
        {
            simpleUtil.WriteStr("SIMPLE  ");
            simpleUtil.WriteInt32(100);
            simpleUtil.WriteInt32(16);
            simpleUtil.WriteInt32(Resources.SIMPLE.Length);
            simpleUtil.WriteInt32(0);
            simpleUtil.WriteInt32(0);

            simpleUtil.WriteBytes(Resources.SIMPLE);
        }
        public byte[] CreatePgd(byte[] buffer)
        {

            int bufferSz = DNASHelper.CalculateSize(buffer.Length, 0x400);
            byte[] bufferEnc = new byte[bufferSz];

            // get pgd
            int sz = DNASHelper.Encrypt(bufferEnc, buffer, DrmInfo.VersionKey, buffer.Length, DrmInfo.KeyIndex, 1, blockSize: 0x400);
            byte[] pgd = bufferEnc.ToArray();
            Array.Resize(ref pgd, sz);

            return pgd;
        }


        public override byte[] GenerateDataPsp()
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

            SceMesgLed.Encrypt(loaderEnc, dataPspElf, 0x0DAA06F0, SceExecFileDecryptMode.DECRYPT_MODE_POPS_EXEC, DrmInfo.VersionKey, DrmInfo.ContentId, Resources.DATAPSPSDCFG);
            return loaderEnc.ToArray();
        }

        private MemoryStream simple;
        private StreamUtil simpleUtil;

        public byte[] StartDat;
        public byte[] SimplePgd;

    }
}
