using Avalonia.Controls;
using Avalonia.Interactivity;
using ChovySign_GUI.Global;
using ChovySign_GUI.Popup.Global;
using GameBuilder.Psp;
using LibChovy;
using LibChovy.Config;
using System;
using System.Linq;
using Vita.ContentManager;
using static ChovySign_GUI.Popup.Global.MessageBox;

namespace ChovySign_GUI.Ps1
{
    public partial class Ps1Tab : UserControl
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
        public Ps1Tab()
        {
            InitializeComponent();
            devkitAccount.IsChecked = ChovyConfig.CurrentConfig.GetBool(useDevkitModeConfigKey);
            devkitAccount.Unchecked += onDevkitModeUnchecked;
            devkitAccount.Checked += onDevkitModeChecked;

            discSelector.DiscsSelected += onDiscSelected;
            keySelector.ValidStateChanged += onKeyValidityChanged;
            gameInfo.TitleChanged += onTitleChanged;

            progressStatus.BeforeStart += onProcessStarting;
            progressStatus.Finished += onProcessFinished;
            check();
        }

        

        private async void onProcessFinished(object? sender, EventArgs e)
        {
            keySelector.IsEnabled = true;
            discSelector.IsEnabled = true;
            gameInfo.IsEnabled = true;

            Window? currentWindow = this.VisualRoot as Window;
            if (currentWindow is not Window) throw new Exception("could not find current window");

            await MessageBox.Show(currentWindow, "Finished creating PS1 Game!\nCan now go restore it to your PSVita using Content Manager.", "Done!", MessageBoxButtons.Ok);

        }

        private void onProcessStarting(object? sender, EventArgs e)
        {
            keySelector.IsEnabled = false;
            discSelector.IsEnabled = false;
            gameInfo.IsEnabled = false;

            if (keySelector.Rif is null) return;
            if (keySelector.VersionKey is null) return;

            NpDrmRif rifInfo = new NpDrmRif(keySelector.Rif);
            NpDrmInfo drmInfo = new NpDrmInfo(keySelector.VersionKey, rifInfo.ContentId, keySelector.KeyIndex);
            PspParameters pspParameters = new PspParameters(drmInfo, rifInfo);

            PopsParameters popsParameters = new PopsParameters(drmInfo, rifInfo);

            foreach (string disc in discSelector.Discs)
                popsParameters.AddCd(disc);

            popsParameters.Name = gameInfo.Title;
            popsParameters.Icon0 = gameInfo.Icon0;
            popsParameters.Pic0 = gameInfo.Pic0;
            popsParameters.Pic1 = gameInfo.Pic1;

            if (devkitAccount.IsChecked == true)
                popsParameters.Account = new Account(0);

            progressStatus.Parameters = popsParameters;


        }

        private void onTitleChanged(object? sender, EventArgs e)
        {
            check();
        }

        private void check()
        {
            this.progressStatus.IsEnabled = (discSelector.AnyDiscsSelected && keySelector.IsValid && gameInfo.Title != "");      
        }
        private void onKeyValidityChanged(object? sender, EventArgs e)
        {
            KeySelector? keySelector = sender as KeySelector;
            if (keySelector is null) return;

            check();
        }

        private async void onDiscSelected(object? sender, EventArgs e)
        {
            CueSelector? cueSelector = sender as CueSelector;
            if (cueSelector is null) return;

            if (cueSelector.AnyDiscsSelected)
                await this.gameInfo.GetGameInfo(cueSelector.Discs.First());

            check();
        }
    }
}
