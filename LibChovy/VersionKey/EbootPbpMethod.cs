using GameBuilder.Psp;
using Li.Utilities;
using PspCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LibChovy.VersionKey
{
    public class EbootPbpMethod
    {
        private static byte[] getKey(byte[] bbmac, byte[] headerBody)
        {
            byte[] versionKey = new byte[0x10];
            Span<byte> mkey = stackalloc byte[Marshal.SizeOf<PspCrypto.AMCTRL.MAC_KEY>()];
            AMCTRL.sceDrmBBMacInit(mkey, 3);
            AMCTRL.sceDrmBBMacUpdate(mkey, headerBody, headerBody.Length);
            AMCTRL.bbmac_getkey(mkey, bbmac, versionKey);
            return versionKey;
        }
        public static NpDrmInfo GetVersionKey(Stream ebootStream, int keyIndex)
        {
            using (ebootStream)
            {
                StreamUtil ebootUtil = new StreamUtil(ebootStream);
                ebootStream.Seek(0x1, SeekOrigin.Begin);
                string pbpMagic = ebootUtil.ReadStrLen(0x3);
                if (pbpMagic == "PBP")
                {
                    int dataPspLocation = ebootUtil.ReadInt32At(0x20);
                    int dataPsarLocation = ebootUtil.ReadInt32At(0x24);
                    ebootStream.Seek(dataPsarLocation, SeekOrigin.Begin);

                    string psarMagic = ebootUtil.ReadStrLen(8);

                    switch (psarMagic)
                    {
                        case "NPUMDIMG":
                            int orginalKeyIndex = ebootUtil.ReadInt32();
                            string contentId = ebootUtil.ReadStringAt(dataPsarLocation + 0x10);

                            byte[] npUmdHdr = ebootUtil.ReadBytesAt(dataPsarLocation, 0xC0);
                            byte[] npUmdHeaderHash = ebootUtil.ReadBytesAt(dataPsarLocation + 0xC0, 0x10);
                            
                            byte[] versionkey = getKey(npUmdHeaderHash, npUmdHdr);
                            
                            SceNpDrm.sceNpDrmTransformVersionKey(versionkey, orginalKeyIndex, keyIndex);
                            return new NpDrmInfo(versionkey, contentId, keyIndex);
                        case "PSISOIMG":
                            using (DNASStream dnas = new DNASStream(ebootStream, dataPsarLocation + 0x400))
                            {
                                contentId = ebootUtil.ReadStringAt(dataPspLocation + 0x560);
                                orginalKeyIndex = dnas.KeyIndex;
                                versionkey = dnas.VersionKey;

                                SceNpDrm.sceNpDrmTransformVersionKey(versionkey, orginalKeyIndex, keyIndex);
                                return new NpDrmInfo(versionkey, contentId, keyIndex);
                            }
                        case "PSTITLEI":
                            using (DNASStream dnas = new DNASStream(ebootStream, dataPsarLocation + 0x200))
                            {
                                contentId = ebootUtil.ReadStringAt(dataPspLocation + 0x560);
                                orginalKeyIndex = dnas.KeyIndex;
                                versionkey = dnas.VersionKey;

                                SceNpDrm.sceNpDrmTransformVersionKey(versionkey, orginalKeyIndex, keyIndex);
                                return new NpDrmInfo(versionkey, contentId, keyIndex);
                            }
                        default:
                            throw new Exception("Cannot obtain versionkey from this EBOOT.PBP (magic:" + psarMagic + ")");
                    }

                }
                else
                {
                    throw new Exception("Invalid PBP (got \"" + pbpMagic + "\", expected \"PBP\")");
                }
            }
        }
    }
}
