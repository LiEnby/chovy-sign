using Li.Progress;
using Li.Utilities;
using PspCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Psp
{
    public abstract class NpDrmPsar : ProgressTracker, IDisposable
    {
        public NpDrmPsar(NpDrmInfo npDrmInfo)
        {
            DrmInfo = npDrmInfo;
            Psar = new BuildStream();

            psarUtil = new StreamUtil(Psar);

        }

        public NpDrmInfo DrmInfo;
        public BuildStream Psar;
        internal StreamUtil psarUtil;
        public abstract void CreatePsar();
        public abstract byte[] GenerateDataPsp();
        public static byte[] CreateStartDat(byte[] image)
        {
            using(BuildStream startDatStream = new BuildStream())
            {
                StreamUtil startDatUtil = new StreamUtil(startDatStream);

                startDatUtil.WriteStr("STARTDAT");
                startDatUtil.WriteInt32(0x1);
                startDatUtil.WriteInt32(0x1);
                startDatUtil.WriteInt32(0x50);
                startDatUtil.WriteInt32(image.Length);
                startDatUtil.WriteInt32(0x0);
                startDatUtil.WriteInt32(0x0);

                startDatUtil.WritePadding(0, 0x30);

                startDatUtil.WriteBytes(image);

                startDatStream.Seek(0x00, SeekOrigin.Begin);
                return startDatStream.ToArray();
            }
        }

        public virtual void Dispose()
        {
            Psar.Dispose();
        }
    }
}
