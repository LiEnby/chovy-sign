using Li.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vita.PsvImgTools
{
    class PSVIMGStreamUtil : StreamUtil
    {
        public PSVIMGStreamUtil(Stream s) : base(s)
        {
        }

        public void WriteSceDateTime(SceDateTime time)
        {
            this.WriteUInt16(time.Year);
            this.WriteUInt16(time.Month);
            this.WriteUInt16(time.Day);

            this.WriteUInt16(time.Hour);
            this.WriteUInt16(time.Minute);
            this.WriteUInt16(time.Second);
            this.WriteUInt32(time.Microsecond);
        }

        public void WriteSceIoStat(SceIoStat stats)
        {
            this.WriteUInt32(Convert.ToUInt32(stats.Mode));
            this.WriteUInt32(Convert.ToUInt32(stats.Attributes));
            this.WriteUInt64(stats.Size);
            this.WriteSceDateTime(stats.CreationTime);
            this.WriteSceDateTime(stats.AccessTime);
            this.WriteSceDateTime(stats.ModificaionTime);
            foreach (UInt32 i in stats.Private)
            {
                this.WriteUInt32(i);
            }
        }
    }
}
