using GameBuilder.Psp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibChovy
{
    public class PspParameters : ChovySignParameters
    {
        public PspParameters(NpDrmInfo drmInfo, NpDrmRif rif) : base(drmInfo, rif)
        {
            Type = ChovyTypes.PSP;
        }
        public bool Compress;
        public UmdInfo Umd;
        public override string OutputFolder
        {
            get
            {
                return Path.Combine(outputFolder, Umd.DiscId);
            }
        }

    }
}
