using System;
using System.Security.Cryptography;

namespace PspCrypto
{
    public class RijndaelMod : Aes
    {
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return CreateTransform(rgbKey, encrypting: false);
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return CreateTransform(rgbKey, encrypting: true);
        }

        public override void GenerateIV()
        {
        }

        public override void GenerateKey()
        {
            byte[] key = new byte[KeySize / BitsPerByte];
            RandomNumberGenerator.Fill(key);
            Key = key;
        }

        private ICryptoTransform CreateTransform(byte[] rgbKey, bool encrypting)
        {
            if (rgbKey == null)
                throw new ArgumentNullException(nameof(rgbKey));
            return new RijndaelModTransform(rgbKey, BlockSizeValue, encrypting);
        }

        private const int BitsPerByte = 8;
    }
}
