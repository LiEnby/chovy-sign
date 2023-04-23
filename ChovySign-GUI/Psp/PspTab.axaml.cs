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
        private const string useDevkitModeConfigKey = "USE_DEVKIT_ACCOUNT_ID";
        private async void onDevkitModeChecked(object? sender, RoutedEventArgs e)
        {
            CheckBox? checkBox = sender as CheckBox;
            if (checkBox is null) return;

            bool? devMode = checkBox.IsChecked;
            if (devMode is null) devMode = false;

            Window? currentWindow = this.VisualRoot as Window;
            if (currentWindow is not Window) throw new Exception("could not find current window");

            MessageBoxResult res = await MessageBox.Show(currentWindow, "This option will force the CMA Account ID to be all 0's\nWhich is how it is on Devkit, Testkit and IDU Firmware\nEnabling this if you have a retail firmware will result in the games just *not* showing up\n\nIf you DON'T know what this means, DON'T enable this.\ndo you want to continue?", "Are you sure?", MessageBoxButtons.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                ChovyConfig.CurrentConfig.SetBool(useDevkitModeConfigKey, (bool)devMode);
            }
            else
            {
                e.Handled = true;
                checkBox.IsChecked = false;
            }
        }

        private void onDevkitModeUnchecked(object? sender, RoutedEventArgs e)
        {
            CheckBox? checkBox = sender as CheckBox;
            if (checkBox is null) return;

            bool? devMode = checkBox.IsChecked;
            if (devMode is null) devMode = false;
            ChovyConfig.CurrentConfig.SetBool(useDevkitModeConfigKey, (bool)devMode);
        }

        private void check()
        {
            this.progressStatus.IsEnabled = (this.keySelector.IsValid && this.isoSelector.HasUmd);
        }
        public PspTab()
        {
            InitializeComponent();
            devkitAccount.IsChecked = ChovyConfig.CurrentConfig.GetBool(useDevkitModeConfigKey);
            devkitAccount.Unchecked += onDevkitModeUnchecked;
            devkitAccount.Checked += onDevkitModeChecked;

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

            Window? currentWindow = this.VisualRoot as Window;
            if (currentWindow is not Window) throw new Exception("could not find current window");

            await MessageBox.Show(currentWindow, "Finished creating PSP Game!\nCan now go restore it to your PSVita using Content Manager.", "Done!", MessageBoxButtons.Ok);
        }

        private void onProcessStarting(object? sender, EventArgs e)
        {
            keySelector.IsEnabled = false;
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
