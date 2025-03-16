using PspCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Psp
{
    public static class Rng
    {
        public static byte[] RandomBytes(int length)
        {
            byte[] randomBytes = new byte[length];
            KIRKEngine.sceUtilsBufferCopyWithRange(randomBytes, randomBytes.Length, null, 0, KIRKEngine.KIRK_CMD_PRNG);
            return randomBytes;
        }
        public static uint RandomUInt()
        {
            byte[] uintBytes = RandomBytes(0x4);
            return BitConverter.ToUInt32(uintBytes);
        }
        public static int RandomInt()
        {
            byte[] intBytes = RandomBytes(0x4);
            return BitConverter.ToInt32(intBytes);
        }


        public static string RandomStr(int length)
        {
            string allowedChars = "abcdefghijklmnopqrstuvwxyzABDECFGHIJKLMNOPQRSTUVWXYZ1234567890 ";
            StringBuilder sb = new StringBuilder();
            do
            {
                sb.Append(allowedChars[Convert.ToInt32(RandomUInt() % allowedChars.Length-1)]);
            } while (sb.Length < length);

            return sb.ToString();
        }
    }
}
