using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder
{
    public static class MathUtil
    {
        public static int CalculateDifference(int val1, int val2)
        {
            int smaller = Convert.ToInt32(Math.Min(val1, val2));
            int larger = Convert.ToInt32(Math.Max(val1, val2));

            return larger - smaller;
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
