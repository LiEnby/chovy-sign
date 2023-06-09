using GameBuilder.Pops;
using GameBuilder.Pops.LibCrypt;
using GameBuilder.Psp;
using LibChovy.Art;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vita.ContentManager;

namespace LibChovy
{
    public class PopsParameters : ChovySignParameters
    {
        public PopsParameters(NpDrmInfo drmInfo, NpDrmRif rif) : base(drmInfo, rif)
        {
            Type = ChovyTypes.POPS;
            discList = new List<PSInfo>();

            EbootElfOverride = null;
            ConfigBinOverride = null;

            discIdOverride = null;
            nameOverride = null;
            libCryptMethod = LibCryptMethod.METHOD_MAGIC_WORD;
        }
        private string? discIdOverride;
        private string? nameOverride;
        private List<PSInfo> discList;
        private LibCryptMethod libCryptMethod;

        private byte[]? pic0;
        private byte[]? pic1;
        private byte[]? icon0;


        public byte[]? EbootElfOverride;
        public byte[]? ConfigBinOverride;

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
                if (outputFolderOverride is null) return Path.Combine(SettingsReader.Ps1Folder, Account.AccountIdStr);
                return outputFolderOverride;
            }
            set
            {
                outputFolderOverride = value;
            }
        }

        public override string OutputFolder
        {
            get
            {
                return Path.Combine(outputFolder, FirstDisc.DiscId);
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
                    if (FirstDisc.DiscName == "") return FirstDisc.DiscId;
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
    }
}
