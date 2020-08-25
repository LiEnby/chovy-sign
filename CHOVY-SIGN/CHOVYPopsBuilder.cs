using Ionic.Zlib;
using Microsoft.Win32;
using Popstation;
using PSVIMGTOOLS;
using PSXPackager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace CHOVY_SIGN
{
    public partial class CHOVYPopsBuilder : Form
    {
        public CHOVYPopsBuilder()
        {
            InitializeComponent();
        }

        public bool MutedAudio = false;
        public string ReadSetting(string Setting)
        {
            string Value = "";
            try
            {

                RegistryKey key;
                key = Registry.CurrentUser.CreateSubKey(@"Software\CHOVYProject\Chovy-Pops-Sign");
                Value = key.GetValue(Setting).ToString();
                key.Close();
            }
            catch (Exception) { return ""; }
            return Value;
        }

        public void WriteSetting(string Setting, string Value)
        {
            try
            {
                RegistryKey key;
                key = Registry.CurrentUser.CreateSubKey(@"Software\CHOVYProject\Chovy-Pops-Sign");
                key.SetValue(Setting, Value);
                key.Close();
            }
            catch (Exception) { }
        }

        private void Disc1Path_TextChanged(object sender, EventArgs e)
        {
            if (Disc1Path.Text != "")
            {
                Disc2Path.Enabled = true;
                browseDisc2.Enabled = true;
            }
            else
            {
                Disc2Path.Text = "";
                Disc2Path.Enabled = false;
                browseDisc2.Enabled = false;
            }
        }

        private void Disc2Path_TextChanged(object sender, EventArgs e)
        {
            if (Disc2Path.Text != "")
            {
                Disc3Path.Enabled = true;
                browseDisc3.Enabled = true;
            }
            else
            {
                Disc3Path.Text = "";
                Disc3Path.Enabled = false;
                browseDisc3.Enabled = false;
            }
        }

        private void Disc3Path_TextChanged(object sender, EventArgs e)
        {
            if (Disc3Path.Text != "")
            {
                Disc4Path.Enabled = true;
                browseDisc4.Enabled = true;
            }
            else
            {
                Disc4Path.Text = "";
                Disc4Path.Enabled = false;
                browseDisc4.Enabled = false;
            }
        }

        private void Disc4Path_TextChanged(object sender, EventArgs e)
        {
            if (Disc4Path.Text != "")
            {
                Disc5Path.Enabled = true;
                browseDisc5.Enabled = true;
            }
            else
            {
                Disc5Path.Text = "";
                Disc5Path.Enabled = false;
                browseDisc5.Enabled = false;
            }
        }

        private void FindFromCMA_Click(object sender, EventArgs e)
        {
            this.Hide();
            string cmaDir = "";
            string accountId = "0000000000000000";

            try
            {
                //try qcma
                cmaDir = Registry.CurrentUser.OpenSubKey(@"Software\codestation\qcma").GetValue("appsPath").ToString();
                accountId = Registry.CurrentUser.OpenSubKey(@"Software\codestation\qcma").GetValue("lastAccountId").ToString();
            }
            catch (Exception)
            {
                try
                {
                    //try sony cma
                    cmaDir = Registry.CurrentUser.OpenSubKey(@"Software\Sony Corporation\Content Manager Assistant\Settings").GetValue("ApplicationHomePath").ToString();
                }
                catch (Exception)
                {
                    try
                    {
                        //try devkit cma
                        cmaDir = Registry.CurrentUser.OpenSubKey(@"Software\SCE\PSP2\Services\Content Manager Assistant for PlayStation(R)Vita DevKit\Settings").GetValue("ApplicationHomePath").ToString();
                    }
                    catch (Exception)
                    {
                        try
                        {
                            string DefaultDir = Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE"), Environment.GetEnvironmentVariable("HOMEPATH"), "Documents", "PS Vita");
                            if (Directory.Exists(DefaultDir))
                            {
                                cmaDir = DefaultDir;
                            }
                        }
                        catch (Exception)
                        {
                            //Do nothing
                        }
                    }
                }

            }


            CHOVYCmaSelector ccs = new CHOVYCmaSelector(cmaDir, accountId, true);
            ccs.FormClosing += Ccs_FormClosing;
            ccs.ShowDialog();
        }

        private void Ccs_FormClosing(object sender, FormClosingEventArgs e)
        {
            CHOVYCmaSelector ccs = (CHOVYCmaSelector)sender;
            string CmaDir = ccs.GetCmaDir();
            string CmaAid = ccs.GetCmaAid();
            string Backup = ccs.GetSelectedBackup();

            WriteSetting("CmaDir", CmaDir);

            if (Backup == "")
            {
                return;
            }

            string BackupPath = Path.Combine(CmaDir, "PSGAME", CmaAid, Backup, "game", "game.psvimg");
            if (!File.Exists(BackupPath))
            {
                MessageBox.Show("Could not find \n" + BackupPath + "\n Perhaps backup failed?", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ccs.Hide();
                this.Show();
                this.Focus();
                return;
            }

            byte[] AID = BitConverter.GetBytes(Convert.ToInt64(CmaAid, 16));
            Array.Reverse(AID);
            byte[] Key = CmaKeys.GenerateKey(AID);

            PSVIMGStream GamePsvimg = new PSVIMGStream(File.OpenRead(BackupPath), Key);

            BackupPath = Path.Combine(CmaDir, "PSGAME", CmaAid, Backup, "license", "license.psvimg");
            if (!File.Exists(BackupPath))
            {
                MessageBox.Show("Could not find \n" + BackupPath + "\n Perhaps backup failed?", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ccs.Hide();
                this.Show();
                this.Focus();
                return;
            }

            PSVIMGStream LicensePsvimg = new PSVIMGStream(File.OpenRead(BackupPath), Key);

            PSVIMGFileStream EbootPbp = new PSVIMGFileStream(GamePsvimg, "/EBOOT.PBP");
            byte[] VersionKey = Pbp.GetVersionKeyPs1(EbootPbp);
            string VerKey = BitConverter.ToString(VersionKey).Replace("-", "");
            WriteSetting("VersionKey", VerKey);

            string ContentID = Pbp.GetContentIdPS1(EbootPbp);
            PSVIMGFileStream LicenseRif = new PSVIMGFileStream(LicensePsvimg, "/" + ContentID + ".rif");
            byte[] LicenseRifBytes = new byte[LicenseRif.Length];
            LicenseRif.Read(LicenseRifBytes, 0x00, LicenseRifBytes.Length);

            LicenseRif.Close();
            LicensePsvimg.Close();
            EbootPbp.Close();
            GamePsvimg.Close();

            byte[] zRifBytes = ZlibStream.CompressBuffer(LicenseRifBytes);
            string Rif = Convert.ToBase64String(zRifBytes);
            WriteSetting("RifPath", Rif);

            Versionkey.Text = VerKey;
            RifPath.Text = Rif;

            ccs.Hide();
            this.Show();
            this.Focus();

            MessageBox.Show("KEYS HAVE BEEN EXTRACTED FROM CMA, YOU MAY NOW LIBERATE YOURSELF", "SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void browseDisc1_EnabledChanged(object sender, EventArgs e)
        {
            Color green = Color.Lime;
            Color black = Color.Black;
            bool enabled = this.browseDisc1.Enabled;
            this.browseDisc1.ForeColor = enabled ? green : black;
            this.browseDisc1.BackColor = enabled ? black : green;

        }

        private void browseDisc2_EnabledChanged(object sender, EventArgs e)
        {
            Color green = Color.Lime;
            Color black = Color.Black;
            bool enabled = this.browseDisc2.Enabled;
            this.browseDisc2.ForeColor = enabled ? green : black;
            this.browseDisc2.BackColor = enabled ? black : green;
        }

        private void browseDisc3_EnabledChanged(object sender, EventArgs e)
        {
            Color green = Color.Lime;
            Color black = Color.Black;
            bool enabled = this.browseDisc3.Enabled;
            this.browseDisc3.ForeColor = enabled ? green : black;
            this.browseDisc3.BackColor = enabled ? black : green;
        }

        private void browseDisc4_EnabledChanged(object sender, EventArgs e)
        {
            Color green = Color.Lime;
            Color black = Color.Black;
            bool enabled = this.browseDisc4.Enabled;
            this.browseDisc4.ForeColor = enabled ? green : black;
            this.browseDisc4.BackColor = enabled ? black : green;
        }

        private void browseDisc5_EnabledChanged(object sender, EventArgs e)
        {
            Color green = Color.Lime;
            Color black = Color.Black;
            bool enabled = this.browseDisc5.Enabled;
            this.browseDisc5.ForeColor = enabled ? green : black;
            this.browseDisc5.BackColor = enabled ? black : green;
        }

        private void FREEDOM_EnabledChanged(object sender, EventArgs e)
        {
            Color red = Color.FromArgb(192, 0, 0);
            Color black = Color.Black;
            bool enabled = this.FREEDOM.Enabled;
            this.FREEDOM.ForeColor = enabled ? red : black;
            this.FREEDOM.BackColor = enabled ? black : red;

        }

        private void CHOVYPopsBuilder_Load(object sender, EventArgs e)
        {
            if (ReadSetting("MuteAudio") == "1")
            {
                MutedAudio = true;
            }

            Versionkey.Text = ReadSetting("VersionKey");
            RifPath.Text = ReadSetting("RifPath");

            Disc1Path.Enabled = true;
            browseDisc1.Enabled = true;
            Disc2Path.Enabled = false;
            browseDisc2.Enabled = false;
            Disc3Path.Enabled = false;
            browseDisc3.Enabled = false;
            Disc4Path.Enabled = false;
            browseDisc4.Enabled = false;
            Disc5Path.Enabled = false;
            browseDisc5.Enabled = false;
        }

        private void CoolArtwork_Click(object sender, EventArgs e)
        {
            if (!MutedAudio)
            {
                MutedAudio = true;
                WriteSetting("MuteAudio", "1");
            }
            else
            {
                MutedAudio = false;
                WriteSetting("MuteAudio", "0");
            }
        }


        private void browseDisc1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select PS1 Disc image *.cue";
            ofd.Filter = "Cue Sheet Files (*.cue)|*.cue";
            ofd.ShowDialog();

            string fileName = ofd.FileName;
            Disc1Path.Text = fileName;

            string BinFile = CueReader.Read(fileName).First().FileName;
            string BinFilePath = Path.Combine(Path.GetDirectoryName(fileName), BinFile);

            GameEntry gmEntry = PackagePsx.FindGameInfo(BinFilePath);
            DiscId.Text = gmEntry.GameID;

            MobyGamesDB.GetGameInformation(gmEntry.GameID);
            GameTitle.Text = MobyGamesDB.Name;
            SaveDescription.Text = MobyGamesDB.Name + " - Save Data";

            if (MobyGamesDB.CoverImage != "(none)")
            {
                try
                {
                    BubbleIconPath.Text = MobyGamesDB.CoverImage;

                    WebClient wc = new WebClient();
                    byte[] ImageData = wc.DownloadData(MobyGamesDB.CoverImage);
                    MemoryStream ms = new MemoryStream(ImageData);
                    Bitmap bmp = new Bitmap(ms);
                    Bitmap resized = new Bitmap(bmp, new Size(80, 80));

                    bmp.Dispose();
                    BubbleIcon.BackgroundImage.Dispose();

                    BubbleIcon.BackgroundImage = resized;

                }
                catch (Exception) { };

            }

        }

        private void browseDisc2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select PS1 Disc image *.cue";
            ofd.Filter = "Cue Sheet Files (*.cue)|*.cue";
            ofd.ShowDialog();
            Disc2Path.Text = ofd.FileName;
        }

        private void browseDisc3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select PS1 Disc image *.cue";
            ofd.Filter = "Cue Sheet Files (*.cue)|*.cue";
            ofd.ShowDialog();
            Disc3Path.Text = ofd.FileName;
        }

        private void browseDisc4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select PS1 Disc image *.cue";
            ofd.Filter = "Cue Sheet Files (*.cue)|*.cue";
            ofd.ShowDialog();
            Disc4Path.Text = ofd.FileName;
        }

        private void browseDisc5_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select PS1 Disc image *.cue";
            ofd.Filter = "Cue Sheet Files (*.cue)|*.cue";
            ofd.ShowDialog();
            Disc5Path.Text = ofd.FileName;
        }

        private void browseIcon_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Portable Network Graphics *.png";
            ofd.Filter = "Portable Network Graphics Files (*.png)|*.png";
            ofd.ShowDialog();

            try
            {
                Bitmap bmp = new Bitmap(BubbleIconPath.Text);
                if (bmp.Width == 80 && bmp.Height == 80)
                {
                    BubbleIcon.Dispose();
                    BubbleIcon.Image = bmp;
                    BubbleIconPath.Text = ofd.FileName;
                }
                else
                {
                    MessageBox.Show("Invalid Icon file (not 80x80)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid PNG File.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void BubbleIconPath_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Bitmap bmp = new Bitmap(BubbleIconPath.Text);
                if (bmp.Width == 80 && bmp.Height == 80)
                {
                    BubbleIcon.Dispose();
                    BubbleIcon.Image = bmp;
                }
            }
            catch (Exception) { };
        }
        private void FREEDOM_Click(object sender, EventArgs e)
        {

        }

    }
}
