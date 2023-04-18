using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibChovy
{
    public class PopsParameters : ChovySignParameters
    {

        //public int SetSDKVersion
        public string[] DiscList;
        public string DiscName;
        public Bitmap? Icon0Override;
        public Bitmap? Pic0Override;
        public Bitmap? Pic1Override;

        public bool MultiDisc
        {
            get
            {
                return DiscList.Length > 1;
            }
        }
    }
}
