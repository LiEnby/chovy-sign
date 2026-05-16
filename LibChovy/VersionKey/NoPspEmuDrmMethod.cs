using GameBuilder.Psp;
using PspCrypto;
using System.Security.Cryptography;
using System.Text;

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
