using Param_SFO;
using System;
using System.IO;
using System.Windows.Forms;

namespace CHOVY
{
    public partial class CHOVYCmaSelector : Form
    {
        public CHOVYCmaSelector(string CMA = "", string AID = "")
        {
            InitializeComponent();
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
                string BackupuPath = Path.Combine(CmaDir, "PGAME");
                foreach (string Dir in Directory.GetDirectories(BackupuPath))
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
                string BackupuPath = Path.Combine(CmaDir, "PGAME", Aid);
                foreach (string Dir in Directory.GetDirectories(BackupuPath))
                {
                    try
                    {
                        string SfoPath = Path.Combine(BackupuPath, Dir, "sce_sys", "param.sfo");
                        FileStream SfoStream = File.OpenRead(SfoPath);
                        PARAM_SFO sfo = new PARAM_SFO(SfoStream);
                        string Title = sfo.Title;
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