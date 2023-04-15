using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopsBuilder.Psp
{
    public class NpDrmInfo
    {
        public string ContentId;
        public byte[] VersionKey;
        public int KeyType;

        public NpDrmInfo(byte[] versionKey, string contentId, int keyType)
        {
            this.VersionKey = versionKey;
            this.KeyType = keyType;
            this.ContentId = contentId;
        }
    }
}
