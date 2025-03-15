using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Li.Utilities
{
    public static class MathUtil
    {
        public static int CalculateDifference(int val1, int val2)
        {
            int smaller = Convert.ToInt32(Math.Min(val1, val2));
            int larger = Convert.ToInt32(Math.Max(val1, val2));

            return larger - smaller;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
        }

        public static int CalculatePaddingAmount(int total, int alignTo)
        {
            int remainder = total % alignTo;
            int padAmt = alignTo - (remainder);
            if ((remainder) == 0) return 0;
            return padAmt;
        }
    }
}
