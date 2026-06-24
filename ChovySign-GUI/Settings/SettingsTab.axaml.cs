using Avalonia.Controls;
using GameBuilder.Pops.LibCrypt;
using GameBuilder;
using Vita.ContentManager;
using System;
using GameBuilder.Atrac3;

namespace ChovySign_GUI.Settings
{
    public partial class SettingsTab : UserControl
    {
        public static SettingsTab? Settings = null;

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

        public AtracEncoderEnum AtracEncoder {
            get
            {
                // skip because no at3tool for these platforms ...
                if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux()) return (AtracEncoderEnum)this.atrac3Encoder.SelectedIndex + 1;

                // use selected atrac encoder.
                return (AtracEncoderEnum)this.atrac3Encoder.SelectedIndex;
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

            // at3tool only ever released for linux and windows. 
            if (OperatingSystem.IsLinux() || OperatingSystem.IsWindows())
                atrac3Encoder.Items = new string[2] { "Sony at3tool", "atracdenc" };
            else
                atrac3Encoder.Items = new string[1] { "atracdenc" };
            

            streamType.Items = new string[2] { "MemoryStream - Create EBOOT in memory, faster, but high memory usage", "FileStream - Create EBOOT with temporary files, slower, but less memory usage" };

            cmaDirectory.Default = SettingsReader.BackupsFolder;
            cmaAccountId.Default = SettingsReader.AccountId;

            Settings = this;
        }
    }
}
