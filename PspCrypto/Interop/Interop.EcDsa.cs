using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ECDsaTest;
using ECDsaTest.SafeHandles;

internal static partial class Interop
{
    internal static partial class Crypto
    {
        internal static bool EcDsaSignEx(ReadOnlySpan<byte> dgst, Span<byte> sig, [In, Out] ref int siglen,
            ReadOnlySpan<byte> kinv, ReadOnlySpan<byte> rp, SafeEcKeyHandle ecKey) => EcDsaSignEx(
            ref MemoryMarshal.GetReference(dgst), dgst.Length, ref MemoryMarshal.GetReference(sig), ref siglen,
            ref MemoryMarshal.GetReference(kinv), kinv.Length, ref MemoryMarshal.GetReference(rp), rp.Length, ecKey);

        [DllImport(Libraries.CryptoNative, EntryPoint = "CryptoNative_EcDsaSignEx")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EcDsaSignEx(ref byte dgst, int dlen, ref byte sig, [In, Out] ref int siglen,
            ref byte kinv, int kinvlen, ref byte rp, int rplen, SafeEcKeyHandle ecKey);

        // returns the maximum length of a DER encoded ECDSA signature created with this key.
        [DllImport(Libraries.CryptoNative, EntryPoint = "CryptoNative_EcDsaSize")]
        private static extern int CryptoNative_EcDsaSize(SafeEcKeyHandle ecKey);

        internal static int EcDsaSize(SafeEcKeyHandle ecKey)
        {
            int ret = CryptoNative_EcDsaSize(ecKey);

            if (ret == 0)
            {
                throw CreateOpenSslCryptographicException();
            }

            return ret;
        }


        internal static PspParameter EcPspParameter(SafeEcKeyHandle key, ReadOnlySpan<byte> sha256, int len)
        {
            SafeBignumHandle kinv_bn, rp_bn;
            int kinvlen, rplen;

            bool refAdded = false;
            try
            {
                key.DangerousAddRef(ref refAdded);
                var ret = EcPspParameter(key, ref MemoryMarshal.GetReference(sha256), len, out kinv_bn, out kinvlen,
                    out rp_bn, out rplen);
                if (!ret)
                {
                    throw Interop.Crypto.CreateOpenSslCryptographicException();
                }

                using (kinv_bn)
                using (rp_bn)
                {
                    var par = new PspParameter
                    {
                        Kinv = Crypto.ExtractBignum(kinv_bn, kinvlen),
                        Rp = Crypto.ExtractBignum(rp_bn, rplen)
                    };
                    return par;
                }
            }
            finally
            {
                if (refAdded)
                    key.DangerousRelease();
            }
        }

        [DllImport(Libraries.CryptoNative, EntryPoint = "CryptoNative_EcPspParameter")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EcPspParameter(SafeEcKeyHandle ecKey, ref byte sha256, int len, out SafeBignumHandle kinv,
            out int kinvlen, out SafeBignumHandle rp, out int rplen);
    }
}
