using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;

namespace PspCrypto.Security.Cryptography
{
    public abstract class SHA224 : HashAlgorithm
    {
        /// <summary>
        /// The hash size produced by the SHA224 algorithm, in bits.
        /// </summary>
        public const int HashSizeInBits = 224;

        /// <summary>
        /// The hash size produced by the SHA224 algorithm, in bytes.
        /// </summary>
        public const int HashSizeInBytes = HashSizeInBits / 8;

        public SHA224()
        {
            // SHA-224 hash length are 224 bits long
            HashSizeValue = HashSizeInBits;
        }

        public static new SHA224 Create() => new Implementation();

        public new static SHA224 Create(string hashName)
        {
            var o = (SHA224)CryptoConfig.CreateFromName(hashName);
            // in case machine.config isn't configured to use any SHA224 implementation
            if (o == null)
            {
                o = new Implementation();
            }
            return o;
        }

        /// <summary>
        /// Computes the hash of data using the SHA224 algorithm.
        /// </summary>
        /// <param name="source">The data to hash.</param>
        /// <returns>The hash of the data.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.
        /// </exception>
        public static byte[] HashData(byte[] source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return HashData(new ReadOnlySpan<byte>(source));
        }

        /// <summary>
        /// Computes the hash of data using the SHA224 algorithm.
        /// </summary>
        /// <param name="source">The data to hash.</param>
        /// <returns>The hash of the data.</returns>
        public static byte[] HashData(ReadOnlySpan<byte> source)
        {
            byte[] buffer = GC.AllocateUninitializedArray<byte>(HashSizeInBytes);

            int written = HashData(source, buffer.AsSpan());
            Debug.Assert(written == buffer.Length);

            return buffer;
        }

        /// <summary>
        /// Computes the hash of data using the SHA224 algorithm.
        /// </summary>
        /// <param name="source">The data to hash.</param>
        /// <param name="destination">The buffer to receive the hash value.</param>
        /// <returns>The total number of bytes written to <paramref name="destination" />.</returns>
        /// <exception cref="ArgumentException">
        /// The buffer in <paramref name="destination"/> is too small to hold the calculated hash
        /// size. The SHA224 algorithm always produces a 224-bit hash, or 28 bytes.
        /// </exception>
        public static int HashData(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            if (!TryHashData(source, destination, out int bytesWritten))
                throw new ArgumentException("Destination is too short.", nameof(destination));

            return bytesWritten;
        }


        /// <summary>
        /// Attempts to compute the hash of data using the SHA224 algorithm.
        /// </summary>
        /// <param name="source">The data to hash.</param>
        /// <param name="destination">The buffer to receive the hash value.</param>
        /// <param name="bytesWritten">
        /// When this method returns, the total number of bytes written into <paramref name="destination"/>.
        /// </param>
        /// <returns>
        /// <see langword="false"/> if <paramref name="destination"/> is too small to hold the
        /// calculated hash, <see langword="true"/> otherwise.
        /// </returns>
        public static bool TryHashData(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten)
        {
            if (destination.Length < HashSizeInBytes)
            {
                bytesWritten = 0;
                return false;
            }

            bytesWritten = HashProviderDispenser.OneShotHashProvider.HashData(HashAlgorithmNames.SHA224, source, destination);
            Debug.Assert(bytesWritten == HashSizeInBytes);

            return true;
        }

        private sealed class Implementation : SHA224
        {
            private readonly HashProvider _hashProvider;

            public Implementation()
            {
                _hashProvider = HashProviderDispenser.CreateHashProvider(HashAlgorithmNames.SHA224);
                HashSizeValue = _hashProvider.HashSizeInBytes * 8;
            }

            protected sealed override void HashCore(byte[] array, int ibStart, int cbSize) =>
                _hashProvider.AppendHashData(array, ibStart, cbSize);

            protected sealed override void HashCore(ReadOnlySpan<byte> source) =>
                _hashProvider.AppendHashData(source);

            protected sealed override byte[] HashFinal() =>
                _hashProvider.FinalizeHashAndReset();

            protected sealed override bool TryHashFinal(Span<byte> destination, out int bytesWritten) =>
                _hashProvider.TryFinalizeHashAndReset(destination, out bytesWritten);

            public sealed override void Initialize() => _hashProvider.Reset();

            protected sealed override void Dispose(bool disposing)
            {
                _hashProvider.Dispose(disposing);
                base.Dispose(disposing);
            }
        }
    }
}
