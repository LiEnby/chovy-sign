using PspCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PopsBuilder.Psp
{
    public class NpDrmPsar : IDisposable
    {
        public NpDrmPsar(byte[] versionKey, string contentId)
        {
            VersionKey = versionKey;
            ContentId = contentId;

            Psar = new MemoryStream();
            psarUtil = new StreamUtil(Psar);
        }

        public byte[] VersionKey;
        public string ContentId;

        public MemoryStream Psar;
        internal StreamUtil psarUtil;

        public virtual void Dispose()
        {
            Psar.Dispose();
        }
    }
}
