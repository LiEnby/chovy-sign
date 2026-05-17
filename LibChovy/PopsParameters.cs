using GameBuilder.Atrac3;
using GameBuilder.Pops;
using GameBuilder.Pops.LibCrypt;
using GameBuilder.Psp;
using LibChovy.Art;
using Vita.ContentManager;

namespace LibChovy
{
    public class PopsParameters : ChovySignParameters
    {
        private string? discIdOverride = null;
        private string? nameOverride = null;
        private List<PSInfo> discList = new List<PSInfo>();
        private LibCryptMethod libCryptMethod = LibCryptMethod.METHOD_MAGIC_WORD;

        private byte[]? pic0 = null;
        private byte[]? pic1 = null;
        private byte[]? icon0 = null;
        
        internal byte[]? simplePngOverride = null;

        public byte[]? EbootElfOverride = null;
        public byte[]? ConfigBinOverride = null;
        public IAtracEncoderBase? Atrac3EncoderOverride;
        
        public PopsParameters(NpDrmInfo drmInfo, NpDrmRif rif) : base(drmInfo, rif)
        {
            Type = ChovyTypes.POPS;
        }

        public PSInfo FirstDisc
        {
            get
            {
                return Discs.First();
            }
        }

        protected override string outputFolder
        {
            get
            {
                if (backupsFolder is null) backupsFolder = SettingsReader.BackupsFolder; // set to default backup folder
                return Path.Combine(SettingsReader.GetPs1Folder(backupsFolder), Account.AccountIdStr);
            }
            set
            {
                backupsFolder = value;
            }
        }

        public override string OutputFolder
        {
            get
            {
                return Path.Combine(outputFolder, FirstDisc.DiscId);
            }
            set
            {
                outputFolder = value;
            }
        }
        public byte[] Pic0
        {
            get
            {
                if (pic0 is null) return Resources.PIC0;
                else return pic0;
            }
            set
            {
                pic0 = value;
            }
        }
        public byte[] Icon0
        {
            get
            {
                if (icon0 is null)
                {
                    byte[] coverImg = Downloader.DownloadCover(FirstDisc).Result;
                    icon0 = coverImg;
                    return coverImg;
                }
                else
                {
                    return icon0;
                }
            }
            set
            {
                icon0 = value;
            }
        }

        public byte[] Pic1
        {
            get
            {
                if (pic1 is null) return Resources.PIC1;
                else return pic1;
            }
            set
            {
                pic1 = value;
            }
        }

        public void AddCd(string cd)
        {
            PSInfo disc = new PSInfo(cd);
            
            if (nameOverride is not null) disc.DiscName = this.nameOverride;
            if (discIdOverride is not null) disc.DiscId = this.discIdOverride;
            if (disc.SbiFile is not null) disc.LibCrypt.Method = this.CrackMethod;

            discList.Add(disc);
        }
        public void RemoveCd(string cd)
        {
            for (int i = 0; i < discList.Count; i++)
            {
                if (discList[i].CueFile == cd)
                {
                    discList.RemoveAt(i);
                    break;
                }
            }
        }
        public PSInfo[] Discs
        {
            get
            {
                return discList.ToArray();
            }
        }
        public string DiscId
        {
            get
            {
                if (discIdOverride is not null && discIdOverride.Length == 9) return discIdOverride;
                return FirstDisc.DiscId;
            }
            set
            {
                if (value.Equals(FirstDisc.DiscId, StringComparison.InvariantCultureIgnoreCase)) { discIdOverride = null; return; };
                if (value.Length != 9) { discIdOverride = null; return; };

                for (int i = 0; i < discList.Count; i++)
                    discList[i].DiscId = value;

                discIdOverride = value;
            }
        }
        public string Name
        {
            get
            {
                if (nameOverride is null)
                {
                    if (FirstDisc.DiscName == String.Empty) return FirstDisc.DiscId;
                    else return FirstDisc.DiscName;
                }
                else return nameOverride;
            }
            set
            {
                for (int i = 0; i < discList.Count; i++)
                    discList[i].DiscName = value;

                nameOverride = value;
            }
        }
        
        public LibCryptMethod CrackMethod
        {
            get
            {
                return libCryptMethod;
            }
            set
            {
                libCryptMethod = value;

                for (int i = 0; i < discList.Count; i++)
                    discList[i].LibCrypt.Method = value;
            }
        }
        public bool MultiDisc
        {
            get
            {
                return Discs.Length > 1;
            }
        }

        public virtual string SimplePngFilepath
        {
            set
            {
                if (File.Exists(value))
                    this.simplePngOverride = File.ReadAllBytes(value);
            }
        }

    }
}
