using Avalonia.Controls;
using Avalonia.Interactivity;
using Li.Utilities;
using ChovySign_GUI.Global;
using LibChovy.Config;
using LibChovy.VersionKey;
using GameBuilder.Psp;
using System;
using System.IO;
using System.Text.RegularExpressions;
using static ChovySign_GUI.Popup.Global.MessageBox;
namespace ChovySign_GUI.Popup.Global.KeySelector
{
    public partial class NoPspEmuDrmMethodGUI : Window
    {
        private const string nopspemuContentId = "NOPSPEMUDRM_CONTENTID";

        private NpDrmRif? rif;

        public NpDrmRif? Rif
        {
            get
            {
                return rif;
            }
        }


        public NoPspEmuDrmMethodGUI()
        {
            string? lastNoPspEmuDrmContentId = ChovyConfig.CurrentConfig.GetString(nopspemuContentId);

            InitializeComponent();

            // reload previous settings
            if(lastNoPspEmuDrmContentId is not null)
                this.contentIdInput.Text = (string)lastNoPspEmuDrmContentId;
            else
                this.contentIdInput.Text = "EP0099-ULUS09999_00-CHOVYSIGN0000000";

            contentIdInput.TextChanged += onContentIdChange;

            check();

        }

        private void onContentIdChange(object? sender, EventArgs e)
        {
            LabeledTextBox? labledTxtBox = sender as LabeledTextBox;

            check();

            if (labledTxtBox is null) return;
            if (Regex.Matches(labledTxtBox.Text, "^[A-Z]{2}[0-9]{4}-[A-Z]{4}[0-9]{5}_[0-9]{2}-[A-Z0-9]{16}$").Count <= 0) return;

            try
            {
                ChovyConfig.CurrentConfig.SetString(nopspemuContentId, labledTxtBox.Text);
            }
            catch{ };
        }

        private void keyGenClick(object sender, RoutedEventArgs e)
        {
            NpDrmInfo[] keys = new NpDrmInfo[0x5];

            Window? currentWindow = this.VisualRoot as Window;
            if (currentWindow is not Window) throw new Exception("could not find current window");

            try
            {
                // read data
                string contentId = contentIdInput.Text;

                // generate keys
                for (int i = 0; i < 0x5; i++)
                    keys[i] = NoPspEmuDrmMethod.GetVersionKey(contentId, i);

                this.rif = NpDrmRif.CreateNoPspEmuDrmRif(contentId);

                this.Close(keys);

            }
            catch { MessageBox.Show(currentWindow, "Failed to generate key...", "Failed", MessageBoxButtons.Ok); }

        }

        private void check()
        {
            bool s = true;

            if (contentIdInput.Text.Length != 36) s = false;
            if (Regex.Matches(contentIdInput.Text, "^[A-Z]{2}[0-9]{4}-[A-Z]{4}[0-9]{5}_[0-9]{2}-[A-Z0-9]{16}$").Count <= 0) s = false;

            keyGen.IsEnabled = s;
        }

    }
}
