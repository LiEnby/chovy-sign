using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PspCrypto.Security.Cryptography
{
    internal class HashProviderDispenser
    {
        public static HashProvider CreateHashProvider(string hashAlgorithmId)
        {
            switch (hashAlgorithmId)
            {
                case HashAlgorithmNames.SHA224:
                    return new SHAManagedHashProvider(hashAlgorithmId);
            }
            throw new CryptographicException(string.Format("'{0}' is not a known hash algorithm.", hashAlgorithmId));
        }

        public static class OneShotHashProvider
        {
            public static unsafe int MacData(
                string hashAlgorithmId,
                ReadOnlySpan<byte> key,
                ReadOnlySpan<byte> source,
                Span<byte> destination)
            {
                using HashProvider provider = CreateMacProvider(hashAlgorithmId, key);
                provider.AppendHashData(source);
                return provider.FinalizeHashAndReset(destination);
            }

            public static int HashData(string hashAlgorithmId, ReadOnlySpan<byte> source, Span<byte> destination)
            {
                HashProvider provider = CreateHashProvider(hashAlgorithmId);
                provider.AppendHashData(source);
                return provider.FinalizeHashAndReset(destination);
            }
        }

        public static unsafe HashProvider CreateMacProvider(string hashAlgorithmId, ReadOnlySpan<byte> key)
        {
            switch (hashAlgorithmId)
            {
                case HashAlgorithmNames.SHA224:
                    return new HMACManagedHashProvider(hashAlgorithmId, key);
            }
            throw new CryptographicException(string.Format("'{0}' is not a known hash algorithm.", hashAlgorithmId));
        }
    }
}
