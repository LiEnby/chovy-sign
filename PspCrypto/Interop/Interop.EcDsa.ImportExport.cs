using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using ECDsaTest.SafeHandles;

internal static partial class Interop
{
    internal static partial class Crypto
    {
        [DllImport(Libraries.CryptoNative, EntryPoint = "CryptoNative_EcKeyCreateByExplicitParameters")]
        internal static extern SafeEcKeyHandle EcKeyCreateByExplicitParameters(
            ECCurve.ECCurveType curveType,
            byte[] qx, int qxLength,
            byte[] qy, int qyLength,
            byte[] d, int dLength,
            byte[] p, int pLength,
            byte[] a, int aLength,
            byte[] b, int bLength,
            byte[] gx, int gxLength,
            byte[] gy, int gyLength,
            byte[] order, int nLength,
            byte[] cofactor, int cofactorLength,
            byte[] seed, int seedLength);

        internal static SafeEcKeyHandle EcKeyCreateByExplicitCurve(ECCurve curve)
        {
            byte[] p;
            if (curve.IsPrime)
            {
                p = curve.Prime;
            }
            else if (curve.IsCharacteristic2)
            {
                p = curve.Polynomial;
            }
            else
            {
                throw new PlatformNotSupportedException(string.Format("The specified curve '{0}' or its parameters are not valid for this platform.", curve.CurveType.ToString()));
            }

            SafeEcKeyHandle key = Interop.Crypto.EcKeyCreateByExplicitParameters(
                curve.CurveType,
                null, 0,
                null, 0,
                null, 0,
                p, p.Length,
                curve.A, curve.A.Length,
                curve.B, curve.B.Length,
                curve.G.X, curve.G.X.Length,
                curve.G.Y, curve.G.Y.Length,
                curve.Order, curve.Order.Length,
                curve.Cofactor, curve.Cofactor.Length,
                curve.Seed, curve.Seed == null ? 0 : curve.Seed.Length);

            if (key == null || key.IsInvalid)
            {
                if (key != null)
                    key.Dispose();
                throw Interop.Crypto.CreateOpenSslCryptographicException();
            }

            // EcKeyCreateByExplicitParameters may have polluted the error queue, but key was good in the end.
            // Clean up the error queue.
            Interop.Crypto.ErrClearError();

            return key;
        }
    }
}
