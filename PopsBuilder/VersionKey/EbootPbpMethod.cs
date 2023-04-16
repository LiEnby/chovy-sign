using GameBuilder.Psp;
using PspCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.VersionKey
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
        public static NpDrmInfo GetVersionKey(Stream ebootStream)
        {
            using (ebootStream)
            {
                StreamUtil ebootUtil = new StreamUtil(ebootStream);
                ebootStream.Seek(0x1, SeekOrigin.Begin);

                if (ebootUtil.ReadCStr() != "PBP")
                {
                    int dataPspLocation = ebootUtil.ReadInt32At(0x20);
                    int dataPsarLocation = ebootUtil.ReadInt32At(0x24);
                    ebootStream.Seek(dataPsarLocation, SeekOrigin.Begin);

                    string magic = ebootUtil.ReadCStr();

                    switch (magic)
                    {
                        case "NPUMDIMG":
                            int keyType = ebootUtil.ReadInt32();
                            string contentId = ebootUtil.ReadStringAt(dataPsarLocation + 0x10);

                            byte[] npUmdHdr = ebootUtil.ReadBytesAt(dataPsarLocation, 0x100);
                            byte[] npUmdBody = ebootUtil.ReadBytesAt(dataPsarLocation + 0xC0, 0x10);
                            
                            byte[] versionkey = getKey(npUmdHdr, npUmdBody);

                            return new NpDrmInfo(versionkey, contentId, keyType);
                        case "PSISOIMG0000":
                            using (DNASStream dnas = new DNASStream(ebootStream, dataPsarLocation + 0x400))
                            {
                                contentId = ebootUtil.ReadStringAt(dataPspLocation + 0x560);
                                keyType = dnas.KeyIndex;
                                versionkey = dnas.VersionKey;

                                return new NpDrmInfo(versionkey, contentId, keyType);
                            }
                        case "PSTITLEIMG000000":
                            using (DNASStream dnas = new DNASStream(ebootStream, dataPsarLocation + 0x200))
                            {
                                contentId = ebootUtil.ReadStringAt(dataPspLocation + 0x560);
                                keyType = dnas.KeyIndex;
                                versionkey = dnas.VersionKey;

                                return new NpDrmInfo(versionkey, contentId, keyType);
                            }
                        default:
                            throw new Exception("Cannot obtain versionkey from this EBOOT.PBP");
                    }

                }
                else
                {
                    throw new Exception("Invalid PBP");
                }
            }
        }
    }
}
