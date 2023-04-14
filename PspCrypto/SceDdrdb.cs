using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace PspCrypto
{
    public static class SceDdrdb
    {
        public static int sceDdrdbHash(ReadOnlySpan<byte> src, int size, Span<byte> digest)
        {
            SHA1.HashData(src[..size], digest);
            return 0;
        }

        public static int sceDdrdbSigvry(ReadOnlySpan<byte> pubKey, ReadOnlySpan<byte> hash, ReadOnlySpan<byte> sig)
        {
            Span<byte> buff = stackalloc byte[Marshal.SizeOf<KIRKEngine.KIRK_CMD17_BUFFER>()];
            ref KIRKEngine.KIRK_CMD17_BUFFER buffer = ref MemoryMarshal.AsRef<KIRKEngine.KIRK_CMD17_BUFFER>(buff);
            pubKey[..0x28].CopyTo(buffer.public_key.point);
            hash[..20].CopyTo(buffer.message_hash);
            sig[..0x28].CopyTo(buffer.signature.sig);
            return KIRKEngine.sceUtilsBufferCopyWithRange(null, 0, buff, 100, KIRKEngine.KIRK_CMD_ECDSA_VERIFY);
        }
    }
}
