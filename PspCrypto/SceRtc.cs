using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PspCrypto
{
    public class SceRtc
    {
        public static ulong ksceRtcGetCurrentTick()
        {
            return ksceRtcGetCurrentSecureTick();
        }

        public static ulong ksceRtcGetCurrentNetworkTick()
        {
            return ksceRtcGetCurrentSecureTick();
        }
        public static ulong ksceRtcGetCurrentSecureTick()
        {
            DateTime epoch = new DateTime(1, 1, 1, 0, 0, 0);
            DateTime now = DateTime.UtcNow;
            TimeSpan ts = now.Subtract(epoch);
            return Convert.ToUInt64(Math.Floor(ts.TotalMilliseconds)) * 1000;
        }
    }
}
