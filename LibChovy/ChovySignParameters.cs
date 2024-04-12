using GameBuilder;
using GameBuilder.Psp;
using LibChovy.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vita.ContentManager;

namespace LibChovy
{
    public abstract class ChovySignParameters
    {
        public ChovySignParameters(NpDrmInfo drmInfo, NpDrmRif rif)
        {
            this.DrmInfo = drmInfo;
            this.DrmRif = rif;

            this.Account = new Account(DrmRif.AccountId);
            this.CreatePsvImg = true;
            this.FirmwareVersion = 0x3600000;
            this.BuildStreamType = StreamType.TYPE_MEMORY_STREAM;
        }

        public int FirmwareVersion;
        public bool CreatePsvImg;
        public NpDrmInfo DrmInfo;
        public NpDrmRif DrmRif;
        public Account Account;
        public ChovyTypes Type;

        public StreamType BuildStreamType;

        protected string? outputFolderOverride;

        protected virtual string outputFolder
        {
            get
            {
                if (outputFolderOverride is null) return Path.Combine(SettingsReader.PspFolder, Account.AccountIdStr);
                return outputFolderOverride;
            }
            set
            {
                outputFolderOverride = value;
            }
        }
        public abstract string OutputFolder { get; }
        

    }
}
