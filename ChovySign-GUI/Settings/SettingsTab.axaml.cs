using Avalonia.Controls;
using Avalonia.Interactivity;
using ChovySign_GUI.Popup.Global;
using GameBuilder;
using GameBuilder.Atrac3;
using GameBuilder.Pops.LibCrypt;
using LibChovy.Config;
using System;
using Vita.ContentManager;

namespace ChovySign_GUI.Settings
{
    public partial class SettingsTab : UserControl
    {
        public static SettingsTab? Settings = null;

        private async void deleteCfgClick(object sender, RoutedEventArgs e)
        {
            Window? currentWindow = TopLevel.GetTopLevel(this) as Window;
            if (currentWindow is not Window) throw new Exception("could not find current window");

            if(await MessageBox.Show(currentWindow, "Do you want to delete all Chovy-Sign2 data?\nThis option should be used if you want to fully un-install Chovy-Sign2 from your PC.", "Are you sure?", MessageBox.MessageBoxButtons.YesNo) == MessageBox.MessageBoxResult.Yes)
                ChovyConfig.CurrentConfig.Delete();
        }
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
        public string OutputDirectory
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
                atrac3Encoder.Items = new string[2] { "at3tool", "atracdenc" };
            else
                atrac3Encoder.Items = new string[1] { "atracdenc" };
            

            streamType.Items = new string[2] { "MemoryStream - Create EBOOT in memory, faster, but high memory usage", "FileStream - Create EBOOT with temporary files, slower, but less memory usage" };

            cmaDirectory.Default = SettingsReader.BackupsFolder;
            cmaAccountId.Default = SettingsReader.AccountId;

            Settings = this;
        }
    }
}
