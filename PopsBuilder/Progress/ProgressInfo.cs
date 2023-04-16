using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Progress
{
    public class ProgressInfo
    {
        private int totalDone;
        private int totalRemain;
        private string currentlyDoing;

        public int Done
        {
            get
            {
                return totalDone;
            }
        }

        public int Remain
        {
            get
            {
                return totalRemain;
            }
        }

        public string CurrentProcess
        {
            get
            {
                return currentlyDoing;
            }
        }

        public double Progress
        {
            get
            {
                return Convert.ToDouble(totalDone) / Convert.ToDouble(totalRemain) * 100.0;
            }
        }

        public int ProgressInt
        {
            get
            {
                return Convert.ToInt32(Math.Floor(Progress));
            }
        }

        internal ProgressInfo(int done, int remain, string currentProcess)
        {
            totalDone = done;
            totalRemain = remain;
            currentlyDoing = currentProcess;
        }
    }
}
