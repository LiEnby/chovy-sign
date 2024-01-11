using Avalonia.Controls;
using ChovySign_GUI.Global;
using ChovySign_GUI.Popup.Global;
using ChovySign_GUI.Settings;
using GameBuilder.Psp;
using LibChovy;
using System;
using System.Linq;
using Vita.ContentManager;
using static ChovySign_GUI.Popup.Global.MessageBox;

namespace ChovySign_GUI.Ps1
{
    public partial class Ps1Tab : UserControl
    {
        public Ps1Tab()
        {
            InitializeComponent();

            discSelector.DiscsSelected += onDiscSelected;
            keySelector.ValidStateChanged += onKeyValidityChanged;
            gameInfo.TitleChanged += onTitleChanged;
            gameInfo.DiscIdChanged += onDiscIdChanged;

            progressStatus.BeforeStart += onProcessStarting;
            progressStatus.Finished += onProcessFinished;
            check();
        }

        
        private async void onProcessFinished(object? sender, EventArgs e)
        {
            keySelector.IsEnabled = true;
            discSelector.IsEnabled = true;
            gameInfo.IsEnabled = true;
            SettingsTab.Settings.IsEnabled = true;

            Window? currentWindow = this.VisualRoot as Window;
            if (currentWindow is not Window) throw new Exception("could not find current window");

            _ = App.PlayFinishSound();
            await MessageBox.Show(currentWindow, "Finished creating PS1 Game!\nCan now go restore it to your PSVita using Content Manager.", "Done!", MessageBoxButtons.Ok);
        }

        private void onProcessStarting(object? sender, EventArgs e)
        {
            keySelector.IsEnabled = false;
            discSelector.IsEnabled = false;
            gameInfo.IsEnabled = false;
            SettingsTab.Settings.IsEnabled = false;

            if (keySelector.Rif is null) return;
            if (keySelector.VersionKey is null) return;

            NpDrmRif rifInfo = new NpDrmRif(keySelector.Rif);
            NpDrmInfo drmInfo = new NpDrmInfo(keySelector.VersionKey, rifInfo.ContentId, keySelector.KeyIndex);

            PopsParameters popsParameters = new PopsParameters(drmInfo, rifInfo);

            foreach (string disc in discSelector.Discs)
                popsParameters.AddCd(disc);

            popsParameters.Name    =   gameInfo.Title;
            popsParameters.DiscId  =   gameInfo.DiscId;
            popsParameters.Icon0   =   gameInfo.Icon0;
            popsParameters.Pic0    =   gameInfo.Pic0;
            popsParameters.Pic1    =   gameInfo.Pic1;

            // read settings from settings tab.
            if (SettingsTab.Settings.DevkitMode) popsParameters.Account = new Account(0);
            
            popsParameters.CrackMethod = SettingsTab.Settings.LibcryptMode;
            SettingsReader.BackupsFolder = SettingsTab.Settings.CmaDirectory;
            popsParameters.CreatePsvImg = SettingsTab.Settings.PackagePsvimg;

            progressStatus.Parameters = popsParameters;


        }

        private void onDiscIdChanged(object? sender, EventArgs e)
        {
            check();
        }

        private void onTitleChanged(object? sender, EventArgs e)
        {
            check();
        }

        private void check()
        {
            this.progressStatus.IsEnabled = (discSelector.AnyDiscsSelected && keySelector.IsValid && gameInfo.Title != "" && gameInfo.DiscId.Length == 9);      
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
