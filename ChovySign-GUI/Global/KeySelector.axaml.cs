using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Remote.Protocol.Input;
using ChovySign_GUI.Popup.Global;
using ChovySign_GUI.Popup.Global.KeySelector;
using GameBuilder.Psp;
using Ionic.Zlib;
using Li.Utilities;
using LibChovy.Config;
using LibChovy.VersionKey;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Vita.ContentManager;
using Vita.PsvImgTools;
using static ChovySign_GUI.Popup.Global.MessageBox;
using static PspCrypto.SceNpDrm;

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

        private bool lastValid;
        private int keyIndex = 1;
        private NpDrmRif? npRif;
        private byte[]? versionKey;

        public bool IsValid
        {
            get
            {
                try
                {
                    if (vKey.Text is null) return false;
                    if (vKey.Text.Length != 32) return false;

                    if (zRif.Text is null) return false;
                    if (zRif.Text.Length <= 0) return false;

                    byte[] rif = new NpDrmRif(zRif.Text).Rif;
                    if (rif.Length <= 0) return false;
                    return (VersionKey is not null && Rif is not null);
                }
                catch { return false; };
            }
        }

        public byte[]? VersionKey
        {
            get
            {
                return versionKey;
            }
            set
            {
                if (value is null) return;

                versionKey = value;
                ChovyConfig.CurrentConfig.SetBytes(versionKeyConfigKey, versionKey);
                vKey.Text = BitConverter.ToString(versionKey).Replace("-", "");

                OnVersionKeyChanged(new EventArgs());
            }
        }
        public byte[]? Rif
        {
            get
            {
                if (npRif is null) return null;
                return npRif.Rif;
            }
            set
            {
                if (value is null) return;

                npRif = new NpDrmRif(value);
                zRif.Text = npRif.ZRif;
                ChovyConfig.CurrentConfig.SetBytes(licenseDataConfigKey, npRif.Rif);

                OnRifChanged(new EventArgs());
            }
        }
        private void reloadCfg()
        {
            byte[]? rifData = ChovyConfig.CurrentConfig.GetBytes(licenseDataConfigKey);
            byte[]? vkeyData = ChovyConfig.CurrentConfig.GetBytes(versionKeyConfigKey);

            if(vkeyData is not null)
            {
                vKey.Text = BitConverter.ToString(vkeyData).Replace("-", "");
                versionKey = vkeyData;
            }

            if (rifData is not null)
            {
                npRif = new NpDrmRif(rifData);
                zRif.Text = npRif.ZRif;
            }
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
            VersionKeyMethod? method = await keyObt.ShowDialog<VersionKeyMethod>(currentWindow);
            if (method is null) return;

            NpDrmInfo? key = null;
            NpDrmRif? rif = null;

            switch (method)
            {
                case VersionKeyMethod.ACT_RIF_METHOD:
                    ActRifMethodGUI actRifMethodGUI = new ActRifMethodGUI();
                    NpDrmInfo[]? keys = await actRifMethodGUI.ShowDialog<NpDrmInfo[]>(currentWindow);
                    if (keys is null) break;

                    key = keys[keyIndex];
                    rif = actRifMethodGUI.Rif;
                    
                    break;
                case VersionKeyMethod.EBOOT_PBP_METHOD:
                    CmaBackupPicker ebootBackupSelector = new CmaBackupPicker();
                    ebootBackupSelector.BackupType = ((keyIndex == 1) ? "PSGAME" : "PGAME");

                    string? gameBackupFolder = await ebootBackupSelector.ShowDialog<string>(currentWindow);
                    string accountId = ebootBackupSelector.AccountId;
                    if (gameBackupFolder is null) break;
                    if (accountId == "") break;

                    key = CMAVersionKeyHelper.GetKeyFromGamePsvimg(gameBackupFolder, accountId);
                    rif = CMAVersionKeyHelper.GetRifFromLicensePsvimg(gameBackupFolder, accountId);
                    break;
                case VersionKeyMethod.KEYS_TXT_METHOD:
                    CmaBackupPicker pspLicenseBackupSelector = new CmaBackupPicker();
                    pspLicenseBackupSelector.BackupType = "PGAME";
                    pspLicenseBackupSelector.Filter = KeysTxtMethod.TitleIds;

                    gameBackupFolder = await pspLicenseBackupSelector.ShowDialog<string>(currentWindow);
                    accountId = pspLicenseBackupSelector.AccountId;
                    if (gameBackupFolder is null) break;
                    if (accountId == "") break;

                    rif = CMAVersionKeyHelper.GetRifFromLicensePsvimg(gameBackupFolder, accountId);
                    if (rif is null) break;

                    key = KeysTxtMethod.GetVersionKey(rif.ContentId, this.KeyIndex);
                    break;

            }



            if (key is not null)
            {
                if (key.KeyIndex != this.keyIndex)
                {
                    await MessageBox.Show(currentWindow, "VersionKey obtained, but had keyindex: " + key.KeyIndex + " however keyindex " + this.keyIndex + " was required.", "KeyIndex mismatch!", MessageBoxButtons.Ok);
                    return;
                }

                VersionKey = key.VersionKey;
            }

            if (rif is not null)
                Rif = rif.Rif;

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

        public event EventHandler<EventArgs>? VersionKeyChanged;
        protected virtual void OnVersionKeyChanged(EventArgs e)
        {
            if (IsValid != lastValid) OnValidStateChanged(new EventArgs());


            if (VersionKeyChanged is not null)
                VersionKeyChanged(this, e);
        }
        public event EventHandler<EventArgs>? RifChanged;
        protected virtual void OnRifChanged(EventArgs e)
        {
            if (IsValid != lastValid) OnValidStateChanged(new EventArgs());

            if (RifChanged is not null)
                RifChanged(this, e);
        }

        public event EventHandler<EventArgs>? ValidStateChanged;
        protected virtual void OnValidStateChanged(EventArgs e)
        {
            lastValid = IsValid;
            if (ValidStateChanged is not null)
                ValidStateChanged(this, e);
        }
        public KeySelector()
        {
            InitializeComponent();
            reloadCfg();
            lastValid = IsValid;

            zRif.TextChanged += onZrifChanged;
            vKey.TextChanged += onVkeyChanged;
        }

        private void onVkeyChanged(object? sender, EventArgs e)
        {
            FilteredTextBox? txt = sender as FilteredTextBox;
            if (txt is null) return;

            if (lastValid != IsValid)
                OnValidStateChanged(new EventArgs());
            
            try
            {
                if (txt.Text is null) return;
                if (txt.Text.Length != 32) return;

                this.VersionKey = MathUtil.StringToByteArray(txt.Text);
            }
            catch { };
        }

        private void onZrifChanged(object? sender, EventArgs e)
        {
            FilteredTextBox? txt = sender as FilteredTextBox;
            if (txt is null) return;

            if (lastValid != IsValid)
                OnValidStateChanged(new EventArgs());

            try
            {
                byte[] rifBytes = new NpDrmRif(txt.Text).Rif;
                if (rifBytes.Length != 0) this.Rif = rifBytes;
            }
            catch { };

        }
    }
}
