using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopsBuilder.Pops
{
    public class DiscInfo
    {
        private string cueFile;
        private string discName;
        private string discId;

        public string CueFile 
        { get
            {
                return cueFile;
            } 
        }

        public string DiscIdHdr
        {
            get
            {
                return "_" + DiscId.Substring(0, 4) + "_" + DiscId.Substring(4, 5);
            }
        }
        public string DiscName
        {
            get
            {
                return discName;
            }
        }

        public string DiscId
        {
            get
            {
                return discId.Replace("-", "").Replace("_", "").ToUpperInvariant();
            }
        }

        public DiscInfo(string cueFile, string discName, string discId)
        {
            this.cueFile = cueFile;
            this.discName = discName;
            this.discId = discId;
        }
    }
}
