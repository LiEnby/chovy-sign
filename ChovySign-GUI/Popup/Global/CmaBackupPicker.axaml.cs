using Avalonia.Controls;
using Avalonia.Interactivity;
using ChovySign_GUI.Global;
using GameBuilder.Psp;
using LibChovy.Config;
using Org.BouncyCastle.Utilities.Bzip2;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vita.ContentManager;

namespace ChovySign_GUI.Popup.Global
{
    public partial class CmaBackupPicker : Window
    {

        private const string lookingInLabelText = "Looking in: ";
        private string[]? gameDirectories;
        private string backupSubFolder = "";
        
        private string[]? filter;

        private string? cmaAccountId = null;
        private string? cmaBackupDir = null;

        private const string backupPickerCmaDirectoryConfigKey = "BACKUP_PICKER_CMA_DIRECTORY";

        public string[] Filter
        {
            set
            {
                this.filter = value;
                reloadBackupsList();
            }
        }
        public string AccountId
        {
            get
            {
                if (cmaAccountId is null) return "";
                else return cmaAccountId;
            }
            set
            {
                cmaAccountId = value;
            }
        }
        public string BackupDir
        {
            get
            {
                if(cmaBackupDir is null)
                {
                    string? savedBackupFolder = ChovyConfig.CurrentConfig.GetString(backupPickerCmaDirectoryConfigKey);
                    if (savedBackupFolder is null) savedBackupFolder = SettingsReader.BackupsFolder;

                    cmaBackupDir = savedBackupFolder;
                }

                return cmaBackupDir;
            }
            set
            {
                if (Directory.Exists(value))
                {
                    cmaBackupDir = value;
                    ChovyConfig.CurrentConfig.SetString(backupPickerCmaDirectoryConfigKey, cmaBackupDir);
                }
            }
        }

        private string accountIdSearchFolder
        {
            get
            {
                string searchIn = Path.Combine(BackupDir, this.BackupType);
                return searchIn;
            }
        }
        private string backupSearchFolder
        {
            get
            {
                if (AccountId == "") return accountIdSearchFolder;
                return Path.Combine(accountIdSearchFolder, AccountId);
            }
        }
        public string BackupType
        {
            get
            {
                return backupSubFolder;
            }
            set
            {
                backupSubFolder = value;
                lookingInLbl.Content = lookingInLabelText + backupSubFolder;
                
                reloadAccountIdsList();
                reloadBackupsList();
            }
        }

        private void reloadAccountIdsList()
        {
            try
            {
                string[] usedAccountIds = Directory.GetDirectories(accountIdSearchFolder);
                List<string> accountIdLst = new List<string>();
                foreach (string accountId in usedAccountIds)
                {
                    string aid = Path.GetFileName(accountId);
                    if (aid.Length != 16) continue;
                    accountIdLst.Add(aid);
                    
                }

                this.accId.Items = accountIdLst.ToArray();

                if (accountIdLst.Count > 0)
                    this.accId.SelectedIndex = 0;
            }
            catch { };
            this.selectBtn.IsEnabled = false;
        }

        private void selectBtnClick(object sender, RoutedEventArgs e)
        {
            if (gameDirectories is null) { this.Close(); return; }
            if (this.backupList.SelectedIndex == -1) { this.Close(); return; }
            
            string selectedDir = gameDirectories[this.backupList.SelectedIndex];
            if (Directory.Exists(selectedDir))
                this.Close(selectedDir);
            else
                this.Close();
        }

        private void reloadBackupsList()
        {
            this.selectBtn.IsEnabled = false;
            this.backupList.Items = new string[0];
            try
            {
                if(!Directory.Exists(backupSearchFolder)) { return; }
                string[] gameBackupDirectories = Directory.GetDirectories(backupSearchFolder);
                List<string> filteredGameDirectories = new List<string>();
                List<string> gameList = new List<string>();
                foreach (string gameDirectory in gameBackupDirectories)
                {
                    string paramFile = Path.Combine(gameDirectory, "sce_sys", "param.sfo");
                    if (File.Exists(paramFile))
                    {
                        try
                        {
                            Sfo psfo = Sfo.ReadSfo(File.ReadAllBytes(paramFile));
                            string? discId = psfo["DISC_ID"] as string;
                            string? title = psfo["TITLE"] as string;
                            if (discId is null) continue;
                            if (title is null) continue;

                            // filter games set in "Filter" property.
                            if (filter is not null)
                                if (!filter.Any(discId.Contains)) continue;

                            gameList.Add(discId + " - " + title);
                            filteredGameDirectories.Add(gameDirectory);
                        }
                        catch { continue; };
                    }
                }

                this.gameDirectories = filteredGameDirectories.ToArray();
                this.backupList.Items = gameList;
            }
            catch { }
        }

        public CmaBackupPicker()
        {
            InitializeComponent();
            this.cmaDir.FilePath = BackupDir;
            this.backupSubFolder = "APP";
            this.accId.SelectionChanged += onAccountSelectionChanged;
            this.cmaDir.FileChanged += onCmaDirChanged;
            this.backupList.SelectionChanged += onSelectedBackupChanged;
            reloadAccountIdsList();
            reloadBackupsList();
        }

        private void onSelectedBackupChanged(object? sender, SelectionChangedEventArgs e)
        {
            ListBox? lstBox = sender as ListBox;
            if (lstBox.SelectedIndex == -1) selectBtn.IsEnabled = false;
            else selectBtn.IsEnabled = true;
        }

        private void onAccountSelectionChanged(object? sender, System.EventArgs e)
        {
            LabeledComboBox? cbox = sender as LabeledComboBox;
            if (cbox is null) return;
            AccountId = cbox.SelectedItem;
            reloadBackupsList();
        }

        private void onCmaDirChanged(object? sender, System.EventArgs e)
        {
            BrowseButton? button = sender as BrowseButton;
            if (button is null) return;
            if (button.ContainsFile)
            {
                BackupDir = button.FilePath;
                reloadAccountIdsList();
                reloadBackupsList();
            }
        }
    }
}
