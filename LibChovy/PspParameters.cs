using GameBuilder.Psp;

namespace LibChovy
{
    public class PspParameters : ChovySignParameters
    {
        public bool Compress = false;
        public UmdInfo? Umd = null;
        public PspParameters(NpDrmInfo drmInfo, NpDrmRif rif) : base(drmInfo, rif)
        {
            Type = ChovyTypes.PSP;
        }
        public override string OutputFolder
        {
            get
            {
                return Path.Combine(outputFolder, Umd.DiscId);
            }
            set
            {
                outputFolder = value;
            }
        }

    }
}
