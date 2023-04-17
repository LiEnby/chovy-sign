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

        protected void updateProgress(int done, int remain, string what)
        {
            ProgressInfo inf = new ProgressInfo(done, remain, what);
            foreach (Action<ProgressInfo> cb in progressCallbacks)
                cb(inf);
        }
    }
}
