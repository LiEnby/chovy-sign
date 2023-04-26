using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Formats.Asn1;

namespace PspCrypto.Security.Cryptography
{
    //
    // Common infrastructure for AsymmetricAlgorithm-derived classes that layer on OpenSSL.
    //
    internal static partial class AsymmetricAlgorithmHelpers
    {

        // private static readonly Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>[]> ReaderAsn;

        // static AsymmetricAlgorithmHelpers()
        // {
        //     var assembly = typeof(Aes).Assembly;
        //     var r = assembly.GetType("System.Security.Cryptography.Asn1.AsnReader");
        //     var par1Type = typeof(ReadOnlyMemory<byte>);
        //     var par2Type = assembly.GetType("System.Security.Cryptography.Asn1.AsnEncodingRules");
        //     var asn1TagType = assembly.GetType("System.Security.Cryptography.Asn1.Asn1Tag");
        //     var constructor = r.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis,
        //         new[] { par1Type, par2Type }, new ParameterModifier[0]);
        //     if (constructor == null)
        //     {
        //         throw new NotImplementedException();
        //     }
        //     var readSequence = r.GetMethod("ReadSequence");
        //     if (readSequence == null)
        //     {
        //         throw new NotImplementedException();
        //     }
        //     var throwIfNotEmpty = r.GetMethod("ThrowIfNotEmpty");
        //     if (throwIfNotEmpty == null)
        //     {
        //         throw new NotImplementedException();
        //     }
        //     var readIntegerBytes = r.GetMethod("ReadIntegerBytes", BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, new Type[0], new ParameterModifier[0]);
        //     if (readIntegerBytes == null)
        //     {
        //         throw new NotImplementedException();
        //     }
        //     var par1 = Expression.Parameter(par1Type, "data");
        //     var derField = par2Type.GetField("DER");
        //     var readerVar = Expression.Variable(r, "reader");
        //     var sequenceReaderVar = Expression.Variable(r, "sequenceReader");
        //     var rDerVar = Expression.Variable(typeof(ReadOnlyMemory<byte>), "rDer");
        //     var sDerVar = Expression.Variable(typeof(ReadOnlyMemory<byte>), "sDer");
        //     var sequenceField = asn1TagType.GetField("Sequence");
        //     var expBlock = Expression.Block(new[] { readerVar, sequenceReaderVar, rDerVar, sDerVar },
        //         Expression.Assign(readerVar, Expression.New(constructor, par1, Expression.Field(null, derField))),
        //         Expression.Assign(sequenceReaderVar, Expression.Call(readerVar, readSequence, Expression.Field(null, sequenceField))),
        //         Expression.Call(readerVar, throwIfNotEmpty),
        //         Expression.Assign(rDerVar, Expression.Call(sequenceReaderVar, readIntegerBytes)),
        //         Expression.Assign(sDerVar, Expression.Call(sequenceReaderVar, readIntegerBytes)),
        //         Expression.Call(sequenceReaderVar, throwIfNotEmpty),
        //         Expression.NewArrayInit(typeof(ReadOnlyMemory<byte>), rDerVar, sDerVar));
        //     ReaderAsn = Expression.Lambda<Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>[]>>(expBlock, par1).Compile();

        // }

        /// <summary>
        /// Convert Der format of (r, s) to Ieee1363 format
        /// </summary>
        public static byte[] ConvertDerToIeee1363(ReadOnlySpan<byte> input, int fieldSizeBits)
        {
            int fieldSizeBytes = BitsToBytes(fieldSizeBits);
            int encodedSize = 2 * fieldSizeBytes;
            byte[] response = new byte[encodedSize];

            ConvertDerToIeee1363(input, fieldSizeBits, response);
            return response;
        }

        internal static int ConvertDerToIeee1363(ReadOnlySpan<byte> input, int fieldSizeBits, Span<byte> destination)
        {
            int fieldSizeBytes = BitsToBytes(fieldSizeBits);
            int encodedSize = 2 * fieldSizeBytes;

            Debug.Assert(destination.Length >= encodedSize);

            try
            {
                AsnValueReader reader = new AsnValueReader(input, AsnEncodingRules.DER);
                AsnValueReader sequenceReader = reader.ReadSequence();
                reader.ThrowIfNotEmpty();
                ReadOnlySpan<byte> rDer = sequenceReader.ReadIntegerBytes();
                ReadOnlySpan<byte> sDer = sequenceReader.ReadIntegerBytes();
                sequenceReader.ThrowIfNotEmpty();

                CopySignatureField(rDer, destination.Slice(0, fieldSizeBytes));
                CopySignatureField(sDer, destination.Slice(fieldSizeBytes, fieldSizeBytes));
                return encodedSize;
            }
            catch (AsnContentException e)
            {
                throw new CryptographicException("ASN1 corrupted data.", e);
            }
        }

        public static int BitsToBytes(int bitLength)
        {
            int byteLength = (bitLength + 7) / 8;
            return byteLength;
        }

        private static void CopySignatureField(ReadOnlySpan<byte> signatureField, Span<byte> response)
        {
            if (signatureField.Length > response.Length)
            {
                if (signatureField.Length != response.Length + 1 ||
                    signatureField[0] != 0 ||
                    signatureField[1] <= 0x7F)
                {
                    // The only way this should be true is if the value required a zero-byte-pad.
                    Debug.Fail($"A signature field was longer ({signatureField.Length}) than expected ({response.Length})");
                    throw new CryptographicException();
                }

                signatureField = signatureField.Slice(1);
            }

            // If the field is too short then it needs to be prepended
            // with zeroes in the response.  Since the array was already
            // zeroed out, just figure out where we need to start copying.
            int writeOffset = response.Length - signatureField.Length;
            response.Slice(0, writeOffset).Clear();
            signatureField.CopyTo(response.Slice(writeOffset));
        }
    }
}
