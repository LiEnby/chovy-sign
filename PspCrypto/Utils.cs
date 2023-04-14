using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace PspCrypto
{
    public static class Utils
    {
        public static bool isEmpty(Span<byte> buf, int buf_size)
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

        /// <summary>
        /// Re-interprets a span of bytes as a reference to structure of type T.
        /// The type may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// </summary>
        /// <remarks>
        /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(Span<byte> span)
            where T : struct
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                throw new ArgumentException(
                    $"Cannot use type '{typeof(T)}'. Only value types without pointers or references are supported.");
            }

            if (Unsafe.SizeOf<T>() > (uint)span.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            return ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));
        }

        /// <summary>
        /// Re-interprets a span of bytes as a reference to structure of type T.
        /// The type may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// </summary>
        /// <remarks>
        /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(ReadOnlySpan<byte> span)
            where T : struct
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                throw new ArgumentException(
                    $"Cannot use type '{typeof(T)}'. Only value types without pointers or references are supported.");
            }

            if (Unsafe.SizeOf<T>() > (uint)span.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            return ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));
        }

        public static void BuildDrmBBMacFinal2(Span<byte> mac)
        {
            Span<byte> checksum = new byte[20 + 0x10];
            ref var aesHdr = ref AsRef<KIRKEngine.KIRK_AES128CBC_HEADER>(checksum);
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
