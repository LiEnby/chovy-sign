using GameBuilder;
using GameBuilder.Psp;
using Vita.ContentManager;

namespace LibChovy
{
    public abstract class ChovySignParameters
    {
        public ChovySignParameters(NpDrmInfo drmInfo, NpDrmRif rif)
        {
            this.DrmInfo = drmInfo;
            this.DrmRif = rif;

            this.Account = new Account(rif.AccountId);
            this.CreatePsvImg = true;
            this.FirmwareVersion = 0x3600000;
            this.BuildStreamType = StreamType.TYPE_MEMORY_STREAM;
            this.OutputFolder = SettingsReader.BackupsFolder;
        }

        public int FirmwareVersion;
        public bool CreatePsvImg;
        public NpDrmInfo DrmInfo;
        public NpDrmRif DrmRif;
        public Account Account;
        public ChovyTypes Type;

        public StreamType BuildStreamType;
        protected string backupsFolder;

        protected virtual string outputFolder
        {
            get
            {
                return Path.Combine(SettingsReader.GetPspFolder(backupsFolder), Account.AccountIdStr);
            }
            set
            {
                backupsFolder = value;
            }
        }
        public abstract string OutputFolder { get; set; }
    }
}
