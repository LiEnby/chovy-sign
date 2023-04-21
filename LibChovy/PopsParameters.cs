using GameBuilder.Pops;
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
            discList = new List<DiscInfo>();
        }
        private string? nameOverride;
        private List<DiscInfo> discList;

        private byte[]? pic0;
        private byte[]? pic1;
        private byte[]? icon0;

        public DiscInfo FirstDisc
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
        }

        public void AddCd(string cd)
        {
            DiscInfo disc = new DiscInfo(cd);
            if (nameOverride is not null) disc.DiscName = nameOverride;
            else discList.Add(disc);
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
        public DiscInfo[] Discs
        {
            get
            {
                return discList.ToArray();
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
        
        public bool MultiDisc
        {
            get
            {
                return Discs.Length > 1;
            }
        }
    }
}
