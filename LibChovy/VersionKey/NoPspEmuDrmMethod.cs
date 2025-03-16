using GameBuilder.Psp;
using PspCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LibChovy.VersionKey
{
    public class NoPspEmuDrmMethod
    {
        public static NpDrmInfo GetVersionKey(string contentId, int keyIndex)
        {
            using (MD5 md = MD5.Create())
            {
                byte[] versionKey = md.ComputeHash(Encoding.UTF8.GetBytes(contentId));
                SceNpDrm.sceNpDrmTransformVersionKey(versionKey, 0, keyIndex);

                return new NpDrmInfo(versionKey, contentId, keyIndex);
            }
        }
    }
}
