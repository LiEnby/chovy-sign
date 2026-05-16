using Li.Progress;
using Li.Utilities;

namespace GameBuilder.Psp
{
    public abstract class NpDrmPsar : ProgressTracker, IDisposable
    {
        private byte[]? startPng = null;
        public NpDrmInfo DrmInfo;

        public BuildStream Psar;
        internal StreamUtil psarUtil;

        public NpDrmPsar(NpDrmInfo npDrmInfo)
        {
            DrmInfo = npDrmInfo;
            Psar = new BuildStream(); 
            psarUtil = new StreamUtil(Psar);
        }

        public byte[] StartDat
        {
            get
            {
                if (startPng is null) throw new NullReferenceException("no startdat png found");

                using (BuildStream startDatStream = new BuildStream())
                {
                    StreamUtil startDatUtil = new StreamUtil(startDatStream);

                    startDatUtil.WriteStr("STARTDAT");
                    startDatUtil.WriteInt32(0x1);
                    startDatUtil.WriteInt32(0x1);
                    startDatUtil.WriteInt32(0x50);
                    startDatUtil.WriteInt32(startPng.Length);
                    startDatUtil.WriteInt32(0x0);
                    startDatUtil.WriteInt32(0x0);

                    startDatUtil.WritePadding(0, 0x30);

                    startDatUtil.WriteBytes(startPng);

                    startDatStream.Seek(0x00, SeekOrigin.Begin);
                    return startDatStream.ToArray();
                }
            }
            set
            {
                startPng = value;
            }
        }

        public abstract void CreatePsar();
        public abstract byte[] GenerateDataPsp();

        public virtual void Dispose()
        {
            Psar.Dispose();
        }
    }
}
