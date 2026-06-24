using GameBuilder.Atrac3;
using GameBuilder.Psp;
using Li.Utilities;
using PspCrypto;


namespace GameBuilder.Pops
{
    public abstract class PopsImg : NpDrmPsar
    {

        private byte[] simplePng = Resources.SIMPLE;

        public byte[] EbootElf;
        public byte[] ConfigBin;
        public bool PatchEboot;

        public IAtracEncoderBase AtracEncoder = OperatingSystem.IsMacOS() ? new AtracdencEncoder() : new Atrac3ToolEncoder();

        public byte[] SimplePgd
        {
            get
            {
                using (BuildStream simple = new BuildStream())
                {
                    StreamUtil simpleUtil = new StreamUtil(simple);

                    simpleUtil.WriteStr("SIMPLE  ");
                    simpleUtil.WriteInt32(100);
                    simpleUtil.WriteInt32(16);
                    simpleUtil.WriteInt32(simplePng.Length);
                    simpleUtil.WriteInt32(0);
                    simpleUtil.WriteInt32(0);

                    simpleUtil.WriteBytes(simplePng);

                    return CreatePgd(simple.ToArray(), DrmInfo.GetFixedKey());
                }
            }
            set
            {
                simplePng = value;
            }
        }
        public PopsImg(NpDrmInfo npdrmInfo) : base(npdrmInfo)
        {
            this.StartDat = Resources.STARTDATPOPS;

            this.EbootElf = Resources.DATAPSPSD;
            this.ConfigBin = Resources.DATAPSPSDCFG;
            this.PatchEboot = true;
        }

        public byte[] CreatePgd(byte[] buffer, byte[]? versionkey = null)
        {

            int bufferSz = DNASHelper.CalculateSize(buffer.Length, 0x400);
            byte[] bufferEnc = new byte[bufferSz];

            // encrypt pgd
            int sz = DNASHelper.Encrypt(bufferEnc, buffer, versionkey is null ? DrmInfo.VersionKey : versionkey, buffer.Length, DrmInfo.KeyIndex, 1, blockSize: 0x400);
            byte[] pgd = bufferEnc.ToArray();
            Array.Resize(ref pgd, sz);

            return pgd;
        }


        public override byte[] GenerateDataPsp()
        {
            byte[] loaderEnc = new byte[this.EbootElf.Length * 2];

            if (this.PatchEboot)
            {
                // calculate size low and high part ..
                uint szLow = Convert.ToUInt32(Psar.Length) >> 16;
                uint szHigh = Convert.ToUInt32(Psar.Length) & 0xFFFF;

                // convert to big endain bytes
                byte[] lowBits = BitConverter.GetBytes(Convert.ToUInt16(szLow)).ToArray();
                byte[] highBits = BitConverter.GetBytes(Convert.ToUInt16(szHigh)).ToArray();

                // overwrite data.psar size check ..
                Array.ConstrainedCopy(lowBits, 0, this.EbootElf, 0x68C, 0x2);
                Array.ConstrainedCopy(highBits, 0, this.EbootElf, 0x694, 0x2);
            }

            int sz = SceMesgLed.Encrypt(
                loaderEnc,
                this.EbootElf,
                0x0DAA06F0,
                SceExecFileDecryptMode.DECRYPT_MODE_POPS_EXEC,
                DrmInfo.VersionKey,
                DrmInfo.ContentId, 
                this.ConfigBin);

            if(sz > 0)
            {
                Array.Resize(ref loaderEnc, sz);
                return loaderEnc;
            }

            throw new Exception("Failed to encrypt simple.prx!, error code: " + sz);
        }


    }
}
