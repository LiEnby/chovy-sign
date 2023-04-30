using System;
using System.Runtime.InteropServices;

namespace PspCrypto
{
    public static class Utils
    {
        public static bool IsEmpty(Span<byte> buf, int buf_size)
        {
            if (buf != null && buf.Length >= buf_size)
            {
                int i;
                for (i = 0; i < buf_size; i++)
                {
                    if (buf[i] != 0) return false;
                }
            }
            return true;
        }

        public static void BuildDrmBBMacFinal2(Span<byte> mac)
        {
            Span<byte> checksum = new byte[20 + 0x10];
            ref var aesHdr = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_AES128CBC_HEADER>(checksum);
            aesHdr.mode = KIRKEngine.KIRK_MODE_ENCRYPT_CBC;
            aesHdr.keyseed = 0x63;
            aesHdr.data_size = 0x10;
            mac.CopyTo(checksum.Slice(20));
            KIRKEngine.sceUtilsBufferCopyWithRange(checksum, 0x10, checksum, 0x10,
                KIRKEngine.KIRK_CMD_ENCRYPT_IV_0);
            checksum.Slice(20, 0x10).CopyTo(mac);
        }
    }
}
