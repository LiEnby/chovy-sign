using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Li.Progress
{
    public class ProgressTracker
    {
        private List<Action<ProgressInfo>> progressCallbacks = new List<Action<ProgressInfo>>();

        public void RegisterCallback(Action<ProgressInfo> cb)
        {
            progressCallbacks.Add(cb);
        }

        protected void copyToProgress(Stream src, Stream dst, string msg)
        {
            src.Seek(0, SeekOrigin.Begin);
            byte[] readBuffer = new byte[0x30000];
            while (src.Position < src.Length)
            {
                int readAmt = src.Read(readBuffer, 0x00, readBuffer.Length);
                dst.Write(readBuffer, 0x00, readAmt);

                updateProgress(Convert.ToInt32(src.Position), Convert.ToInt32(src.Length), msg);
            }
        }

        protected void updateProgress(int done, int remain, string what)
        {
            ProgressInfo inf = new ProgressInfo(done, remain, what);
            foreach (Action<ProgressInfo> cb in progressCallbacks)
                cb(inf);
        }
    }
}
