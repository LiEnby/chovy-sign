using GameBuilder;
using GameBuilder.Psp;
using Vita.ContentManager;

namespace LibChovy
{
    public abstract class ChovySignParameters
    {

        public int FirmwareVersion = 0x3600000;
        public bool CreatePsvImg = true;
        public StreamType BuildStreamType = StreamType.TYPE_MEMORY_STREAM;
        public ChovyTypes Type = ChovyTypes.UNKNOWN;

        public NpDrmInfo DrmInfo;
        public NpDrmRif DrmRif;
        public Account Account;

        protected string? backupsFolder = null;
        internal byte[]? startPngOverride = null;


        public ChovySignParameters(NpDrmInfo drmInfo, NpDrmRif rif)
        {
            this.DrmInfo = drmInfo;
            this.DrmRif = rif;
            this.Account = new Account(rif.AccountId);
        }

        protected virtual string outputFolder
        {
            get
            {
                if (backupsFolder is null) backupsFolder = SettingsReader.BackupsFolder; // set to default backup folder
                if (!CreatePsvImg) return backupsFolder;
                return Path.Combine(SettingsReader.GetPspFolder(backupsFolder), Account.AccountIdStr);
            }
            set
            {
                backupsFolder = value;
            }
        }

        public virtual string StartPngFilepath 
        { 
            set
            {
                if(File.Exists(value))
                    this.startPngOverride = File.ReadAllBytes(value);
            } 
        }

        public abstract string OutputFolder { get; set; }
    }
}
