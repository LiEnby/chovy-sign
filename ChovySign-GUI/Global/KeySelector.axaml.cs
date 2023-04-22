using Avalonia.Controls;
using Avalonia.Interactivity;
using ChovySign_GUI.Popup.Global.KeySelector;
using GameBuilder.Psp;
using GameBuilder.VersionKey;
using Ionic.Zlib;
using LibChovy.Config;
using System;
using System.Net.Sockets;
using System.Text;

namespace ChovySign_GUI.Global
{
    public partial class KeySelector : UserControl
    {
        private string licenseDataConfigKey
        {
            get
            {
                return "KEY_INDEX_" + keyIndex + "_LICENSE_DATA";
            }
        }
        private string versionKeyConfigKey
        {
            get
            {
                return "KEY_INDEX_" + keyIndex + "_VERSION_KEY";
            }
        }

        private int keyIndex = 1;

        private void reloadCfg()
        {
            byte[]? licenseData = ChovyConfig.CurrentConfig.GetBytes(licenseDataConfigKey);
            byte[]? vKeyData = ChovyConfig.CurrentConfig.GetBytes(versionKeyConfigKey);


            if (licenseData is not null)
                zRif.Text = new NpDrmRif(licenseData).ZRif;

            if (vKeyData is not null)
                vKey.Text = BitConverter.ToString(vKeyData).Replace("-", "");

        }

        private async void getKeysClick(object sender, RoutedEventArgs e)
        {

            Button? btn = sender as Button;
            if (btn is null) return;

            btn.IsEnabled = false;

            Window? currentWindow = this.VisualRoot as Window;
            if (currentWindow is not Window) throw new Exception("could not find current window");

            KeyObtainMethods keyObt = new KeyObtainMethods();
            keyObt.KeyIndex = keyIndex;
            VersionKeyMethod method = await keyObt.ShowDialog<VersionKeyMethod>(currentWindow);

            byte[]? key = null;
            NpDrmRif? rif = null;

            switch (method)
            {
                case VersionKeyMethod.ACT_RIF_METHOD:
                    ActRifMethodGUI actRifMethodGUI = new ActRifMethodGUI();
                    byte[][]? keys = await actRifMethodGUI.ShowDialog<byte[][]>(currentWindow);
                    if (keys is null) break;
                    key = keys[keyIndex];
                    rif = actRifMethodGUI.Rif;
                    break;
            }

            if (key is not null)
            {
                ChovyConfig.CurrentConfig.SetBytes(versionKeyConfigKey, key);
                vKey.Text = BitConverter.ToString(key).Replace("-", "");
            }
            if (rif is not null)
            {
                ChovyConfig.CurrentConfig.SetBytes(licenseDataConfigKey, rif.Rif);
                zRif.Text = rif.ZRif;
            }

            btn.IsEnabled = true;
        }

        public int KeyIndex
        {
            get
            {
                return keyIndex;
            }
            set
            {
                keyIndex = value;
                reloadCfg();
            }
        }
        public KeySelector()
        {
            InitializeComponent();
            reloadCfg();
        }
    }
}
