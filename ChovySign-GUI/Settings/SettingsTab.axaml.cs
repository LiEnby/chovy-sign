using Avalonia.Controls;
using GameBuilder.Pops.LibCrypt;
using System.IO;
using Vita.ContentManager;

namespace ChovySign_GUI.Settings
{
    public partial class SettingsTab : UserControl
    {
        public static SettingsTab? Settings;

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

        public bool DevkitMode
        {
            get
            {
                return devkitAccount.IsToggled;
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
            
            if (!Directory.Exists(this.CmaDirectory))
                cmaDirectory.Value = SettingsReader.BackupsFolder;

            Settings = this;
        }
    }
}
