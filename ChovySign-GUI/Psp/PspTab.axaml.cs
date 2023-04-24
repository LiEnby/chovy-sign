using Avalonia.Controls;
using Avalonia.Interactivity;
using ChovySign_GUI.Popup.Global;
using GameBuilder.Psp;
using LibChovy;
using LibChovy.Config;
using System;
using Vita.ContentManager;
using static ChovySign_GUI.Popup.Global.MessageBox;

namespace ChovySign_GUI.Psp
{
    public partial class PspTab : UserControl
    {

        private void check()
        {
            this.progressStatus.IsEnabled = (this.keySelector.IsValid && this.isoSelector.HasUmd);
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
            devkitAccount.IsEnabled = true;

            Window? currentWindow = this.VisualRoot as Window;
            if (currentWindow is not Window) throw new Exception("could not find current window");

            await MessageBox.Show(currentWindow, "Finished creating PSP Game!\nCan now go restore it to your PSVita using Content Manager.", "Done!", MessageBoxButtons.Ok);
        }

        private void onProcessStarting(object? sender, EventArgs e)
        {
            keySelector.IsEnabled = false;
            devkitAccount.IsEnabled = false;
            isoSelector.IsEnabled = false;
            if (keySelector.Rif is null) return;
            if (keySelector.VersionKey is null) return;

            NpDrmRif rifInfo = new NpDrmRif(keySelector.Rif);
            NpDrmInfo drmInfo = new NpDrmInfo(keySelector.VersionKey, rifInfo.ContentId, keySelector.KeyIndex);
            PspParameters pspParameters = new PspParameters(drmInfo, rifInfo);

            UmdInfo? umd = isoSelector.Umd;
            if (umd is null) return;

            pspParameters.Umd = umd;
            pspParameters.Compress = isoSelector.Compress;

            if (devkitAccount.IsDevkitMode)
                pspParameters.Account = new Account(0);

            progressStatus.Parameters = pspParameters;
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
