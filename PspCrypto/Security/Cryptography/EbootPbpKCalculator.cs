using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;

namespace PspCrypto.Security.Cryptography
{
    internal class EbootPbpKCalculator : IDsaKCalculator
    {
        private int _type;

        private BigInteger _n;

        public EbootPbpKCalculator(int type)
        {
            _type = type;
        }

        public bool IsDeterministic => true;

        private readonly Memory<byte> _hash = new byte[0x40];

        public void Init(BigInteger n, SecureRandom random)
        {
            throw new NotImplementedException();
        }
        public void Init(BigInteger n, BigInteger d, byte[] message)
        {
            _n = n;
            Span<byte> hmacIn = stackalloc byte[0x38];
            message[..0x1C].CopyTo(hmacIn);
            KeyVault.Eboot_priv[_type].CopyTo(hmacIn[0x1C..]);

            var hmac = new HMac(new Sha256Digest());
            hmac.Init(new KeyParameter(KeyVault.Eboot_hmacKey));
            hmac.BlockUpdate(hmacIn);
            var hmac_hash_iv = new byte[hmac.GetMacSize()];
            hmac.DoFinal(hmac_hash_iv);

            int ret;
            do
            {
                ret = can_be_reversed_80C17A(message, 0x1c, hmac_hash_iv, _hash.Span);
                if (ret != 0 || (ret = can_be_reversed_80C17A(message, 0x1c, hmac_hash_iv, _hash.Span[0x20..])) != 0)
                {
                    throw new Exception();
                }

            } while (ret != 0);
        }

        public BigInteger NextK()
        {
            var bn = new BigInteger(1, _hash.Span[..0x3c]);
            var ret = bn.Mod(_n);
            return ret;
        }


        private static int can_be_reversed_80C17A(Span<byte> src, int some_size, Span<byte> iv,
            Span<byte> src_xored_digest)
        {
            Span<byte> src_xored = stackalloc byte[0x20];
            iv.CopyTo(src_xored);

            if (some_size > 0x20)
            {
                return 0x12;
            }

            for (int i = 0; i < some_size; i++)
            {
                src_xored[i] ^= src[i];
            }

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(src_xored.ToArray());
            hash.CopyTo(src_xored_digest);

            for (int i = 0; i < 0x20; i++)
            {
                iv[i] ^= src_xored_digest[i];
            }

            for (int i = 0; i < 0x20; i++)
            {
                if (iv[i] != 0xFF)
                {
                    iv[i] += 1;
                    break;
                }

                iv[i] = 0;
            }

            return 0;
        }

    }
}
