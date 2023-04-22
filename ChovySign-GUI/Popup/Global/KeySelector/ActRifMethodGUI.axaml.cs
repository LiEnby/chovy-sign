using Avalonia.Controls;
using Avalonia.Interactivity;
using ChovySign_GUI.Global;
using GameBuilder.Psp;
using GameBuilder.VersionKey;
using Li.Utilities;
using LibChovy.Config;
using System;
using System.IO;
using static ChovySign_GUI.Popup.Global.MessageBox;

namespace ChovySign_GUI.Popup.Global.KeySelector
{
    public partial class ActRifMethodGUI : Window
    {
        private const string keygenHideConsoleIdKey = "KEYGEN_HIDE_CONSOLEID";
        private const string keygenIdpsKey = "KEYGEN_CID";
        private const string keygenActKey = "KEYGEN_ACT";
        private const string keygenRifKey = "KEYGEN_RIF";

        private NpDrmRif? rif;

        public NpDrmRif? Rif
        {
            get
            {
                return rif;
            }
        }


        public ActRifMethodGUI()
        {
            bool? lastHideConsoleId = ChovyConfig.CurrentConfig.GetBool(keygenHideConsoleIdKey);
            byte[]? lastCid = ChovyConfig.CurrentConfig.GetBytes(keygenIdpsKey);
            string? lastAct = ChovyConfig.CurrentConfig.GetString(keygenActKey);
            string? lastRif = ChovyConfig.CurrentConfig.GetString(keygenRifKey);

            InitializeComponent();

            // reload previous settings
            if(lastHideConsoleId is not null)
            {
                hideConsoleId.IsChecked = lastHideConsoleId;
                this.idpsInput.Password = (bool)lastHideConsoleId;
            }

            if (lastAct is not null)
                actFile.FilePath = lastAct;

            if (lastCid is not null)
                idpsInput.Text = BitConverter.ToString(lastCid).Replace("-", "");
            if (lastAct is not null)
                actFile.FilePath = lastAct;
            if (lastRif is not null)
                rifFile.FilePath = lastRif;

            hideConsoleId.Checked += onChangeCidState;
            hideConsoleId.Unchecked += onChangeCidState;

            idpsInput.TextChanged += onIdpsChange;
            actFile.FileChanged += onActFileChange;
            rifFile.FileChanged += onRifFileChange;

            check();

        }

        private void onRifFileChange(object? sender, EventArgs e)
        {
            BrowseButton? filePth = sender as BrowseButton;
            if (filePth is null) return;
            if (!filePth.ContainsFile) return;
            ChovyConfig.CurrentConfig.SetString(keygenRifKey, filePth.FilePath);

            check();
        }

        private void onActFileChange(object? sender, EventArgs e)
        {
            BrowseButton? filePth = sender as BrowseButton;
            if (filePth is null) return;
            if (!filePth.ContainsFile) return;
            ChovyConfig.CurrentConfig.SetString(keygenActKey, filePth.FilePath);

            check();
        }

        private void onIdpsChange(object? sender, EventArgs e)
        {
            LabeledTextBox? labledTxtBox = sender as LabeledTextBox;
            
            if (labledTxtBox is null) return;
            if (labledTxtBox.Text.Length != 32) return;

            try
            {
                byte[] idps = MathUtil.StringToByteArray(labledTxtBox.Text);
                ChovyConfig.CurrentConfig.SetBytes(keygenIdpsKey, idps);
                check();
            }
            catch{ };
        }

        private void keyGenClick(object sender, RoutedEventArgs e)
        {
            byte[][] keys = new byte[0x5][];

            Window? currentWindow = this.VisualRoot as Window;
            if (currentWindow is not Window) throw new Exception("could not find current window");

            try
            {
                // read data
                byte[] idps = MathUtil.StringToByteArray(idpsInput.Text);
                byte[] act = File.ReadAllBytes(actFile.FilePath);
                byte[] rif = File.ReadAllBytes(rifFile.FilePath);

                // generate keys
                for (int i = 0; i < 0x5; i++)
                    keys[i] = ActRifMethod.GetVersionKey(act, rif, idps, i).VersionKey;

                this.rif = new NpDrmRif(rif);

                this.Close(keys);

            }
            catch { MessageBox.Show(currentWindow, "Failed to generate key...", "Failed", MessageBoxButtons.Ok); }

        }

        private void check()
        {
            bool s = true;
            
            if (idpsInput.Text.Length != 32) s = false;
            if (!actFile.ContainsFile) s = false;
            if (!rifFile.ContainsFile) s = false;

            keyGen.IsEnabled = s;
        }

        private void onChangeCidState(object? sender, RoutedEventArgs e)
        {
            CheckBox? checkBox = sender as CheckBox;
            if (checkBox is null) return;

            bool? hideConsoleId = checkBox.IsChecked;
            if (hideConsoleId is null) return;

            ChovyConfig.CurrentConfig.SetBool(keygenHideConsoleIdKey, (bool)hideConsoleId);
            this.idpsInput.Password = (bool)hideConsoleId;
        }
    }
}
