using Avalonia.Controls;
using GameBuilder.Pops.LibCrypt;
using GameBuilder;
using System.IO;
using Vita.ContentManager;
using System;
using System.Globalization;

namespace ChovySign_GUI.Settings
{
    public partial class SettingsTab : UserControl
    {
        public static SettingsTab? Settings;

        public StreamType BuildStreamType
        {
            get
            {
                return (StreamType) this.streamType.SelectedIndex;
            }
        }

        public LibCryptMethod LibcryptMode
        {
            get
            {
                if (this.libCryptMode.SelectedIndex == 0) return LibCryptMethod.METHOD_MAGIC_WORD;
                else return LibCryptMethod.METHOD_SUB_CHANNEL;
            }
        }

        public string CmaDirectory
        {
            get
            {
                return cmaDirectory.Value;
            }
            set
            {
                cmaDirectory.Value = value;
            }
        }

        public bool DownloadPs1Covers
        {
            get
            {
                return downloadCovers.IsToggled;
            }
            set
            {
                downloadCovers.IsToggled = value;
            }
        }
        public UInt64 AccountId
        {
            get
            {
                return cmaAccountId.Value;
            }
            set
            {
                cmaAccountId.Value = value;
            }
        }
        public bool PackagePsvimg
        {
            get
            {
                return packagePsvimg.IsToggled;
            }
        }


        public SettingsTab()
        {
            InitializeComponent();

            libCryptMode.Items = new string[2] { "Magic Word in ISO Header", "Sub Channel PGD" };
            streamType.Items = new string[2] { "MemoryStream - Create EBOOT in memory, faster, but high memory usage", "FileStream - Create EBOOT with temporary files, slower, but less memory usage" };

            cmaDirectory.Default = SettingsReader.BackupsFolder;
            cmaAccountId.Default = SettingsReader.AccountId;

            Settings = this;
        }
    }
}
