using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PspCrypto.Security.Cryptography
{
    public class HMACSHA224 : HMAC
    {
        /// <summary>
        /// The hash size produced by the HMAC SHA224 algorithm, in bits.
        /// </summary>
        public const int HashSizeInBits = 224;

        /// <summary>
        /// The hash size produced by the HMAC SHA24 algorithm, in bytes.
        /// </summary>
        public const int HashSizeInBytes = HashSizeInBits / 8;

        public HMACSHA224()
            : this(RandomNumberGenerator.GetBytes(BlockSize))
        { }

        public HMACSHA224(byte[] key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            HashName = HashAlgorithmNames.SHA224;
            _hMacCommon = new HMACCommon(HashAlgorithmNames.SHA224, key, BlockSize);
            base.Key = _hMacCommon.ActualKey!;
            // this not really needed as it'll initialize BlockSizeValue with same value it has which is 64.
            // we just want to be explicit in all HMAC extended classes
            BlockSizeValue = BlockSize;
            HashSizeValue = _hMacCommon.HashSizeInBits;
            Debug.Assert(HashSizeValue == HashSizeInBits);
        }

        public override byte[] Key
        {
            get
            {
                return base.Key;
            }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                _hMacCommon.ChangeKey(value);
                base.Key = _hMacCommon.ActualKey!;
            }
        }

        protected override void HashCore(byte[] rgb, int ib, int cb) =>
            _hMacCommon.AppendHashData(rgb, ib, cb);

        protected override void HashCore(ReadOnlySpan<byte> source) =>
            _hMacCommon.AppendHashData(source);

        protected override byte[] HashFinal() =>
            _hMacCommon.FinalizeHashAndReset();

        protected override bool TryHashFinal(Span<byte> destination, out int bytesWritten) =>
            _hMacCommon.TryFinalizeHashAndReset(destination, out bytesWritten);

        public override void Initialize() => _hMacCommon.Reset();

        /// <summary>
        /// Computes the HMAC of data using the SHA224 algorithm.
        /// </summary>
        /// <param name="key">The HMAC key.</param>
        /// <param name="source">The data to HMAC.</param>
        /// <returns>The HMAC of the data.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key" /> or <paramref name="source" /> is <see langword="null" />.
        /// </exception>
        public static byte[] HashData(byte[] key, byte[] source)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(source);

            return HashData(new ReadOnlySpan<byte>(key), new ReadOnlySpan<byte>(source));
        }

        /// <summary>
        /// Computes the HMAC of data using the SHA224 algorithm.
        /// </summary>
        /// <param name="key">The HMAC key.</param>
        /// <param name="source">The data to HMAC.</param>
        /// <returns>The HMAC of the data.</returns>
        public static byte[] HashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source)
        {
            byte[] buffer = new byte[HashSizeInBytes];

            int written = HashData(key, source, buffer.AsSpan());
            Debug.Assert(written == buffer.Length);

            return buffer;
        }

        /// <summary>
        /// Computes the HMAC of data using the SHA224 algorithm.
        /// </summary>
        /// <param name="key">The HMAC key.</param>
        /// <param name="source">The data to HMAC.</param>
        /// <param name="destination">The buffer to receive the HMAC value.</param>
        /// <returns>The total number of bytes written to <paramref name="destination" />.</returns>
        /// <exception cref="ArgumentException">
        /// The buffer in <paramref name="destination"/> is too small to hold the calculated hash
        /// size. The SHA224 algorithm always produces a 224-bit HMAC, or 28 bytes.
        /// </exception>
        public static int HashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source, Span<byte> destination)
        {
            if (!TryHashData(key, source, destination, out int bytesWritten))
            {
                throw new ArgumentException("Destination is too short.", nameof(destination));
            }

            return bytesWritten;
        }

        /// <summary>
        /// Attempts to compute the HMAC of data using the SHA224 algorithm.
        /// </summary>
        /// <param name="key">The HMAC key.</param>
        /// <param name="source">The data to HMAC.</param>
        /// <param name="destination">The buffer to receive the HMAC value.</param>
        /// <param name="bytesWritten">
        /// When this method returns, the total number of bytes written into <paramref name="destination"/>.
        /// </param>
        /// <returns>
        /// <see langword="false"/> if <paramref name="destination"/> is too small to hold the
        /// calculated hash, <see langword="true"/> otherwise.
        /// </returns>
        public static bool TryHashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten)
        {
            if (destination.Length < HashSizeInBytes)
            {
                bytesWritten = 0;
                return false;
            }

            bytesWritten = HashProviderDispenser.OneShotHashProvider.MacData(HashAlgorithmNames.SHA224, key, source, destination);
            Debug.Assert(bytesWritten == HashSizeInBytes);

            return true;
        }

        private HMACCommon _hMacCommon;
        private const int BlockSize = 64;
    }
}
