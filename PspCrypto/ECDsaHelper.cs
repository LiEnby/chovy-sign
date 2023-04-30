using PspCrypto.Security.Cryptography;
using System;
using System.Security.Cryptography;

namespace PspCrypto
{
    public static class ECDsaHelper
    {
        public static ECCurve SetCurve(byte[] p, byte[] a, byte[] b, byte[] n, byte[] gx, byte[] gy)
        {
            return new ECCurve
            {
                A = a,
                B = b,
                Prime = p,
                Order = n,
                CurveType = ECCurve.ECCurveType.PrimeShortWeierstrass,
                Cofactor = new byte[] { 0x01 },
                G = new ECPoint { X = gx, Y = gy }
            };
        }

        public static (byte[], ECPoint) GenerateKey(byte[] p, byte[] a, byte[] b, byte[] n, byte[] gx, byte[] gy)
        {
            var curve = new ECCurve
            {
                A = a,
                B = b,
                Prime = p,
                Order = n,
                CurveType = ECCurve.ECCurveType.PrimeShortWeierstrass,
                Cofactor = new byte[] { 0x01 },
                G = { X = gx, Y = gy }
            };
            var ecdsa = new ECDsaManaged();
            ecdsa.GenerateKey(curve);
            var parameter = ecdsa.ExportExplicitParameters(true);
            return (parameter.D, parameter.Q);
        }

        public static ECDsa Create(ECCurve curve, byte[] privateKey)
        {
            return Create(curve, privateKey, new byte[privateKey.Length], new byte[privateKey.Length]);
        }


        public static ECDsa Create(ECCurve curve, byte[] pubx, byte[] puby)
        {
            return Create(curve, null, pubx, puby);
        }

        public static ECDsa Create(ECCurve curve, Span<byte> pubx, Span<byte> puby)
        {
            return Create(curve, null, pubx.ToArray(), puby.ToArray());
        }

        public static ECDsa Create(ECCurve curve, byte[] privateKey, byte[] pubx, byte[] puby, bool ebootPbp = false, int type = 1)
        {
            var par = new ECParameters
            {
                Curve = curve,
                D = privateKey,
                Q = { X = pubx, Y = puby }
            };
            return new ECDsaManaged(par, ebootPbp, type);
        }

        public static void SignNpImageHeader(Span<byte> npHdr)
        {
            var curve = SetCurve(KeyVault.ec_p, KeyVault.ec_a, KeyVault.ec_b2, KeyVault.ec_N2, KeyVault.Gx2,
                KeyVault.Gy2);
            using var ecdsa = Create(curve, KeyVault.ec_Priv2, KeyVault.Px2, KeyVault.Py2);
            var hash = ecdsa.SignData(npHdr[..0xD8].ToArray(), HashAlgorithmName.SHA1);
            hash.CopyTo(npHdr[0xD8..]);
        }

        public static bool VerifyEdat(Span<byte> edat)
        {
            var curve = SetCurve(KeyVault.ec_p, KeyVault.ec_a, KeyVault.ec_b2, KeyVault.ec_N2, KeyVault.Gx2,
                KeyVault.Gy2);
            using var ecdsa = Create(curve, KeyVault.EdatPx, KeyVault.EdatPy);
            return ecdsa.VerifyData(edat[..0x58], edat.Slice(0x58, 0x28), HashAlgorithmName.SHA1);
        }

        public static void SignEdat(Span<byte> edat)
        {
            var curve = SetCurve(KeyVault.ec_p, KeyVault.ec_a, KeyVault.ec_b2, KeyVault.ec_N2, KeyVault.Gx2,
                KeyVault.Gy2);
            using var ecdsa = Create(curve, KeyVault.EdatPirv, KeyVault.EdatPx, KeyVault.EdatPy);
            var sig = ecdsa.SignData(edat[..0x58].ToArray(), HashAlgorithmName.SHA1);
            sig.CopyTo(edat[0x58..]);
        }

        public static void SignParamSfo(ReadOnlySpan<byte> param, Span<byte> sig)
        {
            var curve = SetCurve(KeyVault.ec_p, KeyVault.ec_a, KeyVault.ec_b2, KeyVault.ec_N2, KeyVault.Gx2,
                KeyVault.Gy2);
            using var ecdsa = Create(curve, KeyVault.ec_Priv2, KeyVault.Px2, KeyVault.Py2);
            var sigTmp = ecdsa.SignData(param.ToArray(), HashAlgorithmName.SHA1);
            sigTmp.CopyTo(sig);
        }

        public static bool VerifyEbootPbp(Span<byte> data, Span<byte> sig)
        {
            var sha224 = SHA224.Create();
            var hash = sha224.ComputeHash(data.ToArray());
            var curve = SetCurve(KeyVault.Eboot_p, KeyVault.Eboot_a, KeyVault.Eboot_b, KeyVault.Eboot_N, KeyVault.Eboot_Gx,
                KeyVault.Eboot_Gy);
            using var ecdsa = Create(curve, KeyVault.Eboot_priv2, KeyVault.Eboot_pub2x, KeyVault.Eboot_pub2y, true);
            return ecdsa.VerifyHash(hash, sig);
        }
    }
}
