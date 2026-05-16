using Avalonia.Controls;
using ChovySign_GUI.Popup.Global;
using ChovySign_GUI.Settings;
using GameBuilder.Psp;
using LibChovy;
using System;
using System.IO;
using Vita.ContentManager;
using static ChovySign_GUI.Popup.Global.MessageBox;

namespace ChovySign_GUI.Psp
{
    public partial class PspTab : UserControl
    {

        private void check()
        {
            this.progressStatus.IsEnabled = (
                this.keySelector.IsValid && 
                this.isoSelector.HasUmd &&
                SettingsTab.Settings is not null &&
                Directory.Exists(SettingsTab.Settings.CmaDirectory)
            );
        }
        public PspTab()
        {
            InitializeComponent();

            keySelector.ValidStateChanged += onValidStateChange;
            isoSelector.UmdChanged += onUmdChanged;

            progressStatus.BeforeStart += onProcessStarting;
            progressStatus.Finished += onProcessFinished;
            check();
        }


        private async void onProcessFinished(object? sender, EventArgs e)
        {
            keySelector.IsEnabled = true;
            isoSelector.IsEnabled = true;
            if (SettingsTab.Settings is null) return;
            SettingsTab.Settings.IsEnabled = true;

            Window? currentWindow = TopLevel.GetTopLevel(this) as Window;
            if (currentWindow is not Window) throw new Exception("could not find current window");

            _ = App.PlayFinishSound();
            await MessageBox.Show(currentWindow, "Finished creating PSP Game!\nCan now go restore it to your PSVita using Content Manager.", "Done!", MessageBoxButtons.Ok);
        }

        private void onProcessStarting(object? sender, EventArgs e)
        {
            if (SettingsTab.Settings is null) throw new NullReferenceException("SettingsTab.Settings is null");
            if (keySelector.Rif is null) throw new NullReferenceException("keySelector.Rif is null");
            if (keySelector.VersionKey is null) throw new NullReferenceException("keySelector.VersionKey is null");
            if (!Directory.Exists(SettingsTab.Settings.CmaDirectory)) throw new FileNotFoundException("SettingsTab.Settings.CmaDirectory not found!");

            NpDrmRif rifInfo = new NpDrmRif(keySelector.Rif);
            NpDrmInfo drmInfo = new NpDrmInfo(keySelector.VersionKey, rifInfo.ContentId, keySelector.KeyIndex);
            PspParameters pspParameters = new PspParameters(drmInfo, rifInfo);

            UmdInfo? umd = isoSelector.Umd;
            if (umd is null) throw new NullReferenceException("UmdInfo is null");

            pspParameters.Umd = umd;
            pspParameters.Compress = isoSelector.Compress;

            // read settings from settings tab.
            pspParameters.Account = new Account(SettingsTab.Settings.AccountId);
            pspParameters.OutputFolder = SettingsTab.Settings.CmaDirectory;
            pspParameters.CreatePsvImg = SettingsTab.Settings.PackagePsvimg;

            progressStatus.Parameters = pspParameters;


            keySelector.IsEnabled = false;
            isoSelector.IsEnabled = false;
            SettingsTab.Settings.IsEnabled = false;
        }

        private void onUmdChanged(object? sender, EventArgs e)
        {
            check();
        }

        private void onValidStateChange(object? sender, EventArgs e)
        {
            check();
        }
    }
}
