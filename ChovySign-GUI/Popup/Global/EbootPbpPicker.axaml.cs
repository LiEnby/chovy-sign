using Avalonia.Controls;
using Avalonia.Interactivity;
using ChovySign_GUI.Global;
using ChovySign_GUI.Settings;
using GameBuilder.Psp;
using LibChovy.Config;
using LibChovy.VersionKey;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vita.ContentManager;

namespace ChovySign_GUI.Popup.Global
{
    public partial class EbootPbpPicker : Window
    {

        private string[]? gameDirectories;        
        private string[]? filter;

        public string[] Filter
        {
            set
            {
                this.filter = value;
                reloadEbootsList();
            }
        }

        public string Ms0Dir
        {
            get
            {
                if (SettingsTab.Settings is null) throw new NullReferenceException("SettingsTab is null");
                return SettingsTab.Settings.OutputDirectory;
            }
            set
            {
                if (Directory.Exists(value))
                {
                    if (SettingsTab.Settings is null) throw new NullReferenceException("SettingsTab is null");
                    SettingsTab.Settings.OutputDirectory = value; 
                }
            }
        }


        private void selectBtnClick(object sender, RoutedEventArgs e)
        {
            if (gameDirectories is null) { this.Close(); return; }
            if (this.ebootList.SelectedIndex == -1) { this.Close(); return; }
            
            string selectedFile = Path.Combine(gameDirectories[this.ebootList.SelectedIndex], "EBOOT.PBP");
            if (File.Exists(selectedFile))
                this.Close(selectedFile);
            else
                this.Close();
        }

        private void reloadEbootsList()
        {
            this.selectBtn.IsEnabled = false;
            this.ebootList.ItemsSource = new string[0];
            try
            {
                string[] ebootDirectories = Directory.GetDirectories(Path.Combine(Ms0Dir, "PSP", "GAME"));

                List<string> filteredGameDirectories = new List<string>();
                List<string> gameList = new List<string>();
                foreach (string gameDirectory in ebootDirectories)
                {
                    string ebootFile = Path.Combine(gameDirectory, "EBOOT.PBP");
                    if (!File.Exists(ebootFile)) continue;

                    using(PbpReader pbpReader = new PbpReader(ebootFile))
                    {
                        try
                        {
                            Sfo psfo = Sfo.ReadSfo(pbpReader.ParamSfo);
                            string? discId = psfo["DISC_ID"] as string;
                            string? title = psfo["TITLE"] as string;
                            string contentId = EbootPbpMethod.GetVersionKey(pbpReader.BaseStream, 0).ContentId;

                            if (discId is null) continue;
                            if (title is null) continue;
                            if (!File.Exists(Path.Combine(Ms0Dir, "PSP", "LICENSE", contentId + ".rif"))) continue;

                            if (filter is not null)
                                if (!filter.Any(discId.Contains)) continue;

                            gameList.Add(discId + " - " + title);
                            filteredGameDirectories.Add(gameDirectory);
                        }
                        catch { continue; }

                    }
                }

                this.gameDirectories = filteredGameDirectories.ToArray();
                this.ebootList.ItemsSource = gameList;
            }
            catch { }
        }

        public EbootPbpPicker()
        {
            InitializeComponent();
            this.ms0Dir.FilePath = Ms0Dir;
            this.ms0Dir.FileChanged += onMs0DirChanged;
            this.ebootList.SelectionChanged += onSelectedBackupChanged;
            reloadEbootsList();
        }

        private void onSelectedBackupChanged(object? sender, SelectionChangedEventArgs e)
        {
            ListBox? lstBox = sender as ListBox;
            if (lstBox is null) return;
            if (lstBox.SelectedIndex == -1) selectBtn.IsEnabled = false;
            else selectBtn.IsEnabled = true;
        }

        private void onAccountSelectionChanged(object? sender, System.EventArgs e)
        {
            LabeledComboBox? cbox = sender as LabeledComboBox;
            if (cbox is null) return;
            reloadEbootsList();
        }

        private void onMs0DirChanged(object? sender, System.EventArgs e)
        {
            BrowseButton? button = sender as BrowseButton;
            if (button is null) return;
            if (button.ContainsFile)
            {
                Ms0Dir = button.FilePath;
                reloadEbootsList();
            }
        }
    }
}
