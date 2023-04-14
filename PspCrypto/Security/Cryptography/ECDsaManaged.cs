using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using System;

namespace PspCrypto.Security.Cryptography
{
    internal class ECDsaManaged : System.Security.Cryptography.ECDsa
    {
        private ECKeyParameters _ecKeyParameters;
        private readonly bool _ebootPbp;
        private readonly int _type;

        public ECDsaManaged()
        {

        }

        public ECDsaManaged(System.Security.Cryptography.ECParameters parameters, bool ebootPbp, int type)
        {
            _ebootPbp = ebootPbp;
            _type = type;
            var gx = new BigInteger(1, parameters.Curve.G.X);
            var gy = new BigInteger(1, parameters.Curve.G.Y);
            var curve = ConvertECCurve(parameters.Curve);
            var g = curve.CreatePoint(gx, gy);
            var domainParameters = new ECDomainParameters(curve, g, curve.Order);
            if (parameters.D != null)
            {
                var privateKey = new BigInteger(1, parameters.D);
                _ecKeyParameters = new ECPrivateKeyParameters(privateKey, domainParameters);
            }
            else if (parameters.Q.X != null && parameters.Q.Y != null)
            {
                var publicKey = curve.CreatePoint(new BigInteger(1, parameters.Q.X), new BigInteger(1, parameters.Q.Y));
                _ecKeyParameters = new ECPublicKeyParameters(publicKey, domainParameters);
            }
            else
            {
                throw new ArgumentException("invalid parameters", nameof(parameters));
            }
        }

        public override byte[] SignHash(byte[] hash)
        {
            if (_ecKeyParameters is not ECPrivateKeyParameters)
            {
                throw new ArgumentException("key is not private Key");
            }
            var signer = CreateSigner();
            signer.Init(true, _ecKeyParameters);
            signer.BlockUpdate(hash);
            return signer.GenerateSignature();
        }

        public override bool VerifyHash(byte[] hash, byte[] signature)
        {
            var signer = CreateSigner();
            if (_ecKeyParameters is ECPrivateKeyParameters ecPrivateKeyParameters)
            {
                var publicKey = new ECPublicKeyParameters(
                    ecPrivateKeyParameters.Parameters.G.Multiply(ecPrivateKeyParameters.D),
                    ecPrivateKeyParameters.Parameters);
                signer.Init(false, publicKey);
            }
            else
            {
                signer.Init(false, _ecKeyParameters);
            }
            signer.BlockUpdate(hash);
            return signer.VerifySignature(signature);
        }

        protected override byte[] HashData(byte[] data, int offset, int count, System.Security.Cryptography.HashAlgorithmName hashAlgorithm)
        {
            var dataSpan = data.AsSpan().Slice(offset, count);
            if (hashAlgorithm == System.Security.Cryptography.HashAlgorithmName.SHA256)
            {
                return System.Security.Cryptography.SHA256.HashData(dataSpan);
            }
            else if (hashAlgorithm == System.Security.Cryptography.HashAlgorithmName.SHA1)
            {
                return System.Security.Cryptography.SHA1.HashData(dataSpan);
            }
            else
            {
                throw new NotSupportedException($"{hashAlgorithm} not supported");
            }
        }

        private ISigner CreateSigner()
        {
            IDigest digest = DigestUtilities.GetDigest("NONE");
            IDsa dsa = _ebootPbp ? new ECDsaSigner(new EbootPbpKCalculator(_type)) : new ECDsaSigner();
            var signer = new DsaDigestSigner(dsa, digest, PlainDsaEncoding.Instance);
            return signer;
        }

        private FpCurve _fpCurve;

        public override void GenerateKey(System.Security.Cryptography.ECCurve curve)
        {
            _fpCurve = ConvertECCurve(curve);
            var gx = new BigInteger(1, curve.G.X);
            var gy = new BigInteger(1, curve.G.Y);
            var g = _fpCurve.CreatePoint(gx, gy);
            var domainParameters = new ECDomainParameters(_fpCurve, g, _fpCurve.Order);
            var gen = new ECKeyPairGenerator();
            gen.Init(new ECKeyGenerationParameters(domainParameters, new SecureRandom()));
            var keyPair = gen.GenerateKeyPair();
            _ecKeyParameters = (ECKeyParameters)keyPair.Private;
        }

        public override System.Security.Cryptography.ECParameters ExportExplicitParameters(bool includePrivateParameters)
        {
            var normalG = _ecKeyParameters.Parameters.G;
            var curve = new System.Security.Cryptography.ECCurve
            {
                A = _fpCurve.A.ToBigInteger().ToByteArrayUnsigned(),
                B = _fpCurve.B.ToBigInteger().ToByteArrayUnsigned(),
                Prime = _fpCurve.Q.ToByteArrayUnsigned(),
                Order = _fpCurve.Order.ToByteArrayUnsigned(),
                Cofactor = _fpCurve.Cofactor.ToByteArrayUnsigned(),
                G = new System.Security.Cryptography.ECPoint
                {
                    X = normalG.XCoord.ToBigInteger().ToByteArrayUnsigned(),
                    Y = normalG.YCoord.ToBigInteger().ToByteArrayUnsigned()
                }
            };
            var parameters = new System.Security.Cryptography.ECParameters
            {
                Curve = curve
            };
            if (includePrivateParameters && _ecKeyParameters is ECPrivateKeyParameters privateKeyParameters)
            {
                parameters.D = privateKeyParameters.D.ToByteArrayUnsigned();
                Console.WriteLine(privateKeyParameters.D.ToString(16).ToUpper());
                var publicKey = privateKeyParameters.Parameters.G.Multiply(privateKeyParameters.D).Normalize();
                parameters.Q = new System.Security.Cryptography.ECPoint
                {
                    X = publicKey.XCoord.ToBigInteger().ToByteArrayUnsigned(),
                    Y = publicKey.YCoord.ToBigInteger().ToByteArrayUnsigned()
                };
            }
            else if (_ecKeyParameters is ECPublicKeyParameters publicKeyParameters)
            {
                var publicKey = publicKeyParameters.Q;
                parameters.Q = new System.Security.Cryptography.ECPoint
                {
                    X = publicKey.XCoord.ToBigInteger().ToByteArrayUnsigned(),
                    Y = publicKey.YCoord.ToBigInteger().ToByteArrayUnsigned()
                };
            }
            return parameters;
        }

        private static FpCurve ConvertECCurve(System.Security.Cryptography.ECCurve curve)
        {
            var p = new BigInteger(1, curve.Prime);
            var a = new BigInteger(1, curve.A);
            var b = new BigInteger(1, curve.B);
            var n = new BigInteger(1, curve.Order);
            var fpCurve = new FpCurve(p, a, b, n, BigInteger.One);
            return fpCurve;
        }
    }
}
