using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Psp
{
    public class NpDrmInfo
    {
        public string ContentId;
        public byte[] VersionKey;
        public int KeyIndex;

        public NpDrmInfo(byte[] versionKey, string contentId, int keyType)
        {
            this.VersionKey = versionKey;
            this.KeyIndex = keyType;
            this.ContentId = contentId;
        }
    }
}
