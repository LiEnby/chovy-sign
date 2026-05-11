using PspCrypto;
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

        public byte[] GetFixedKey()
        {
            byte[] fixedKey = new byte[this.VersionKey.Length];
            byte[] cidBytes = Encoding.ASCII.GetBytes(this.ContentId);
            Array.Resize(ref cidBytes, 0x30);

            SceNpDrm.sceNpDrmGetFixedKey(fixedKey, cidBytes, this.KeyIndex | 0x1000000);

            return fixedKey;
        }
    }
}
