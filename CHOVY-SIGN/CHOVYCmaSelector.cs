using ParamSfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace CHOVY_SIGN
{
    public partial class CHOVYCmaSelector : Form
    {
        public bool PSGame = false;
        public CHOVYCmaSelector(string CMA = "", string AID = "", bool PS1=false)
        {
            InitializeComponent();
            PSGame = PS1;
            CMADir.Text = CMA;
            AIDSelector.Text = AID;
        }

        public string GetSelectedBackup()
        {
            try
            {
                return BackupList.Text.Substring(0, 9);
            }
            catch (Exception)
            {
                return "";
            }
        }
        public string GetCmaDir()
        {
            return CMADir.Text;
        }
        public string GetCmaAid()
        {
            return AIDSelector.Text;
        }

        private void UpdateAidList()
        {
            AIDSelector.Items.Clear();
            try
            {
                string CmaDir = CMADir.Text;
                string BackupPath;
                if (!PSGame)
                    BackupPath = Path.Combine(CmaDir, "PGAME");
                else
                    BackupPath = Path.Combine(CmaDir, "PSGAME");
                foreach (string Dir in Directory.GetDirectories(BackupPath))
                {
                    AIDSelector.Items.Add(Path.GetFileName(Dir));
                }
                AIDSelector.SelectedIndex = 0;
            }
            catch (Exception) { }

        }
        private void UpdateBackupList()
        {
            BackupList.Items.Clear();

            try
            {
                string CmaDir = CMADir.Text;
                string Aid = AIDSelector.Text;
                string BackupPath;
                if(!PSGame)
                    BackupPath = Path.Combine(CmaDir, "PGAME", Aid);
                else
                    BackupPath = Path.Combine(CmaDir, "PSGAME", Aid);
                foreach (string Dir in Directory.GetDirectories(BackupPath))
                {
                    try
                    {
                        string SfoPath = Path.Combine(BackupPath, Dir, "sce_sys", "param.sfo");
                        FileStream SfoStream = File.OpenRead(SfoPath);
                        Dictionary<string,object> SfoKeys = Sfo.ReadSfo(SfoStream);
                        string Title = (string)SfoKeys["TITLE"];
                        SfoStream.Close();
                        string BackupName = (Path.GetFileName(Dir) + " - " + Title);
                        BackupList.Items.Add(BackupName);
                    }
                    catch (Exception)
                    {
                        BackupList.Items.Add(Path.GetFileName(Dir));
                    }

                }
            }
            catch (Exception) { }

        }

        private void AIDSelector_TextChanged(object sender, EventArgs e)
        {
            UpdateBackupList();
        }

        private void CMADir_TextChanged(object sender, EventArgs e)
        {
            UpdateBackupList();
            UpdateAidList();
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "CMA Backups Directory";
            fbd.ShowDialog();

            CMADir.Text = fbd.SelectedPath;
        }

        private void GitRifAndVerKey_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}