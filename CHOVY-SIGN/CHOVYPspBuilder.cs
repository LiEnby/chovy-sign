using CHOVY_SIGN.Properties;
using Ionic.Zlib;
using Microsoft.Win32;
using ParamSfo;
using PSVIMGTOOLS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CHOVY_SIGN
{
    public partial class CHOVYPspBuilder : Form
    {
        bool MutedAudio = false;
        public byte[] GetSfo(string ISOFile) 
        {
            FileStream ISO = File.OpenRead(ISOFile);

            DiscUtils.Iso9660.CDReader cdr = new DiscUtils.Iso9660.CDReader(ISO, false);
            Stream ParamSfo = cdr.OpenFile(@"PSP_GAME\PARAM.SFO", FileMode.Open,FileAccess.Read);

            byte[] Sfo = new byte[ParamSfo.Length];
            ParamSfo.Read(Sfo, 0x00, (int)ParamSfo.Length);
            ISO.Close();
            
            return Sfo;
            
        }
        public byte[] GetIcon(string ISOFile)
        {
            FileStream ISO = File.OpenRead(ISOFile);
            DiscUtils.Iso9660.CDReader cdr = new DiscUtils.Iso9660.CDReader(ISO, false);
            Stream ParamSfo = cdr.OpenFile(@"PSP_GAME\ICON0.PNG", FileMode.Open, FileAccess.Read);

            byte[] Icon0 = new byte[ParamSfo.Length];
            ParamSfo.Read(Icon0, 0x00, (int)ParamSfo.Length);
            ISO.Close();

            return Icon0;

        }
        public static string GetTitleID(string ISOFile)
        {
            FileStream ISO = File.OpenRead(ISOFile);
            DiscUtils.Iso9660.CDReader cdr = new DiscUtils.Iso9660.CDReader(ISO, false);
            Stream ParamSfo = cdr.OpenFile(@"PSP_GAME\PARAM.SFO", FileMode.Open, FileAccess.Read);

            Dictionary<string,object> SfoKeys = Sfo.ReadSfo(ParamSfo);
            
            string TitleID = (string)SfoKeys["DISC_ID"];
            ISO.Close();
            return TitleID;
        }

        public static bool isMini(string ISOFile)
        {
            FileStream ISO = File.OpenRead(ISOFile);
            DiscUtils.Iso9660.CDReader cdr = new DiscUtils.Iso9660.CDReader(ISO, false);
            Stream Icon0 = cdr.OpenFile(@"PSP_GAME\ICON0.PNG", FileMode.Open, FileAccess.Read);

            Bitmap bmp = new Bitmap(Icon0);
            bool isMini = (bmp.Width == 80 && bmp.Height == 80);
            bmp.Dispose();
            ISO.Close();

            return isMini;
        }
        public string ReadSetting(string Setting)
        {
            string Value = "";
            try
            {

                RegistryKey key;
                key = Registry.CurrentUser.CreateSubKey(@"Software\CHOVYProject\Chovy-Sign");
                Value = key.GetValue(Setting).ToString();
                key.Close();
            }
            catch (Exception) { return ""; }
            return Value;
        }

        public void WriteSetting(string Setting,string Value)
        {
            try
            {
                RegistryKey key;
                key = Registry.CurrentUser.CreateSubKey(@"Software\CHOVYProject\Chovy-Sign");
                key.SetValue(Setting, Value);
                key.Close();
            }
            catch (Exception) { }
        }

        public bool IsDexAidSet()
        {
            string isDex = ReadSetting("DexAid");
            if (isDex == "0")
            {
                return false;
            }
            else if(isDex == "1")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public CHOVYPspBuilder()
        {
            InitializeComponent();
        }
        private void CHOVY_Load(object sender, EventArgs e)
        {
            if (ReadSetting("MuteAudio") == "1")
            {
                MutedAudio = true;
            }

            Versionkey.Text = ReadSetting("VersionKey");
            RifPath.Text = ReadSetting("RifPath");
        }

        private void FREEDOM_EnabledChanged(object sender, EventArgs e)
        {
            Color red = Color.FromArgb(192, 0, 0);
            Color black = Color.Black;
            bool enabled = this.FREEDOM.Enabled;
            this.FREEDOM.ForeColor = enabled ? red : black;
            this.FREEDOM.BackColor = enabled ? black : red;
        }

        private void FREEDOM_Click(object sender, EventArgs e)
        {
            Action enable = () => {
                this.FREEDOM.Enabled = true;
            };

            bool isZrif = false;
            if(RifPath.Text == "" || !File.Exists(RifPath.Text))
            {
                // Check if valid 'zRIF'
                try
                {
                    ZlibStream.UncompressBuffer(Convert.FromBase64String(RifPath.Text));
                    isZrif = true;
                }
                catch(Exception)
                {
                    MessageBox.Show("INVALID RIF!\nPlease try \"Find from CMA\"", "RIF ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
            }
            if(Versionkey.Text.Length != 32)
            {
                MessageBox.Show("INVALID VERSION KEY!\nPlease try \"Find from CMA\"", "VERKEY ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if(ISOPath.Text == "" || !File.Exists(ISOPath.Text))
            {
                MessageBox.Show("INVALID ISO PATH!", "ISO ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.FREEDOM.Enabled = false;
            string CmaDir = ReadSetting("CmaDir");
            if(CmaDir == "")
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Select CMA Backups Directory";
                fbd.ShowDialog();
                CmaDir = fbd.SelectedPath;
           }
            string TitleID = GetTitleID(ISOPath.Text);

            string TmpDir = Path.Combine(Application.StartupPath, "_tmp");
            string GameWorkDir = Path.Combine(TmpDir, TitleID);
            string EbootFile = Path.Combine(GameWorkDir, "EBOOT.PBP");
            string EbootSignature = Path.Combine(GameWorkDir, "__sce_ebootpbp");

            Directory.CreateDirectory(TmpDir);
            Directory.CreateDirectory(GameWorkDir);
           

            //Read RIF data
            byte[] ContentId = new byte[0x24];
            byte[] RifAid = new byte[0x08];
            Stream rif = null; 
            if (!isZrif)
                rif = File.OpenRead(RifPath.Text);
            else
                rif = new MemoryStream(ZlibStream.UncompressBuffer(Convert.FromBase64String(RifPath.Text)));

            rif.Seek(0x10, SeekOrigin.Begin);
            rif.Read(ContentId, 0x00, 0x24);
            rif.Seek(0x08, SeekOrigin.Begin);
            rif.Read(RifAid, 0x00, 0x08);


            string ContentID = Encoding.UTF8.GetString(ContentId);
            string AidStr = BitConverter.ToString(RifAid).Replace("-", "").ToLower();
            if(IsDexAidSet())
            {
                AidStr = "0000000000000000";
            }
            string BackupWorkDir = Path.Combine(CmaDir, "PGAME", AidStr, TitleID);

            TotalProgress.Maximum = 100;
            Status.Text = "Overthrowing The PSPEMU Monarchy 0%";

            string BootupImage = "";
            if (isMini(ISOPath.Text))
            {
                BootupImage = Path.Combine(Application.StartupPath, "_tmp", "minis.png");
                Resources.MINIS.Save(BootupImage);
            }
            else
            {
                BootupImage = Path.Combine(Application.StartupPath, "_tmp", "chovy.png");
                Resources.ChovyLogo.Save(BootupImage);
            }

            Process signnp = pbp.GenPbpFromIso(ISOPath.Text, EbootFile, ContentID, Versionkey.Text, CompressPBP.Checked, BootupImage);
            while (!signnp.HasExited)
            {
                string Progress = signnp.StandardOutput.ReadLine();
                if(Progress.StartsWith("Writing ISO blocks: "))
                {
                    Progress = Progress.Remove(0,19);
                    int ProgressInt = int.Parse(Progress.Substring(0,3));
                    TotalProgress.Value = ProgressInt;
                    Status.Text = "Overthrowing The PSPEMU Monarchy " + ProgressInt.ToString() + "%";
                }
                Application.DoEvents();
            }
            TotalProgress.Value = 0;

            Status.Text = "Signing the Declaration of Independance 0%";
            UInt64 IntAid = BitConverter.ToUInt64(RifAid,0x00);
            Thread thrd = new Thread(() =>
            {

                int ChovyGenRes = pbp.gen__sce_ebootpbp(EbootFile, IntAid, EbootSignature);
                if (!File.Exists(EbootSignature) || ChovyGenRes != 0)
                {
                    MessageBox.Show("CHOVY-GEN Failed! Please check CHOVY.DLL exists\nand that the Microsoft Visual C++ 2015 Redistributable Update 3 RC is installed");
                    enable();
                    return;
                }
            });
            thrd.Start();
            while(thrd.IsAlive)
            {
                Application.DoEvents();
            }

            /*
             *  BUILD PSVIMG FILE
             */

            // Pacakge GAME
            byte[] CmaKey;
            if (IsDexAidSet())
            {
                CmaKey = CmaKeys.GenerateKey(new byte[0x8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }); 
            }
            else
            {
                CmaKey = CmaKeys.GenerateKey(RifAid);
            }

            

            string[] entrys = Directory.GetFileSystemEntries(GameWorkDir, "*", SearchOption.AllDirectories);
            long noEntrys = entrys.LongLength;
            string parentPath = "ux0:pspemu/temp/game/PSP/GAME/" + TitleID;
            int noBlocks = 0;
            foreach (string fileName in Directory.GetFiles(GameWorkDir,"*",SearchOption.AllDirectories))
            {
                noBlocks += Convert.ToInt32(new FileInfo(fileName).Length / PSVIMGConstants.PSVIMG_BLOCK_SIZE);
            }
            TotalProgress.Maximum = noBlocks;


            string BackupDir = Path.Combine(BackupWorkDir, "game");
            Directory.CreateDirectory(BackupDir);
            string psvimgFilepath = Path.Combine(BackupDir, "game.psvimg");
            FileStream gamePsvimg = File.OpenWrite(psvimgFilepath);
            gamePsvimg.SetLength(0);
            PSVIMGBuilder builder = new PSVIMGBuilder(gamePsvimg, CmaKey);

            foreach (string entry in entrys)
            {
                string relativePath = entry.Remove(0, GameWorkDir.Length);
                relativePath = relativePath.Replace('\\', '/');

                bool isDir = File.GetAttributes(entry).HasFlag(FileAttributes.Directory);

                if (isDir)
                {
                    builder.AddDir(entry, parentPath, relativePath);
                }
                else
                {
                    builder.AddFileAsync(entry, parentPath, relativePath);
                    while(!builder.HasFinished)
                    {
                        try
                        {
                            int tBlocks = builder.BlocksWritten;
                            TotalProgress.Value = tBlocks;
                            decimal progress = Math.Floor(((decimal)tBlocks / (decimal)noBlocks) * 100);
                            Status.Text = "Signing the Declaration of Independance " + progress.ToString() + "%";
                        }
                        catch (Exception) { }
 
                        Application.DoEvents();
                    }
                }
            }

            long ContentSize = builder.Finish();
            gamePsvimg = File.OpenRead(psvimgFilepath);
            FileStream gamePsvmd = File.OpenWrite(Path.Combine(BackupDir, "game.psvmd"));
            PSVMDBuilder.CreatePsvmd(gamePsvmd, gamePsvimg, ContentSize, "game", CmaKey);
            gamePsvmd.Close();
            gamePsvimg.Close();

            // Package LICENSE

            BackupDir = Path.Combine(BackupWorkDir, "license");
            psvimgFilepath = Path.Combine(BackupDir, "license.psvimg");

            Directory.CreateDirectory(BackupDir);
            FileStream licensePsvimg = File.OpenWrite(psvimgFilepath);
            licensePsvimg.SetLength(0);

            builder = new PSVIMGBuilder(licensePsvimg, CmaKey);
            if (!isZrif)
            {
                builder.AddFile(RifPath.Text, "ux0:pspemu/temp/game/PSP/LICENSE", "/" + ContentID + ".rif");
            }
            else
            { 
                rif.Seek(0x00, SeekOrigin.Begin);
                builder.AddFileFromStream(rif, "ux0:pspemu/temp/game/PSP/LICENSE", "/" + ContentID + ".rif");
            }

            rif.Close();
            ContentSize = builder.Finish();

            licensePsvimg = File.OpenRead(psvimgFilepath);
            FileStream licensePsvmd = File.OpenWrite(Path.Combine(BackupDir, "license.psvmd"));
            PSVMDBuilder.CreatePsvmd(licensePsvmd, licensePsvimg, ContentSize, "license", CmaKey);
            licensePsvmd.Close();
            licensePsvimg.Close();

            // Write PARAM.SFO & ICON0.PNG
            string SceSysWorkDir = Path.Combine(BackupWorkDir, "sce_sys");
            Directory.CreateDirectory(SceSysWorkDir);

            byte[] ParamSfo = GetSfo(ISOPath.Text);
            byte[] Icon0 = GetIcon(ISOPath.Text);
            File.WriteAllBytes(Path.Combine(SceSysWorkDir, "param.sfo"), ParamSfo);
            File.WriteAllBytes(Path.Combine(SceSysWorkDir, "icon0.png"), Icon0);

            Status.Text = "YOU HAVE MADE A SOCIAL CONTRACT WITH FREEDOM!";
            TotalProgress.Value = 0;

            try
            {
                Directory.Delete(TmpDir, true);
            }
            catch (Exception) { };

            if (!MutedAudio)
            {
                Stream str = Resources.Murica;
                SoundPlayer snd = new SoundPlayer(str);
                snd.Play();
            }
            
            enable();
            MessageBox.Show("YOU HAVE MADE A SOCIAL CONTRACT WITH FREEDOM!", "FREEDOM!", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            

            CHOVYCmaSelector ccs = new CHOVYCmaSelector(cmaDir, accountId);
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
            
            string BackupPath = Path.Combine(CmaDir, "PGAME", CmaAid, Backup, "game", "game.psvimg");
            if(!File.Exists(BackupPath))
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

            BackupPath = Path.Combine(CmaDir, "PGAME", CmaAid, Backup, "license", "license.psvimg");
            if (!File.Exists(BackupPath))
            {
                MessageBox.Show("Could not find \n"+BackupPath+"\n Perhaps backup failed?","License Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                ccs.Hide();
                this.Show();
                this.Focus();
                return;
            }

            PSVIMGStream LicensePsvimg = new PSVIMGStream(File.OpenRead(BackupPath), Key);

            PSVIMGFileStream EbootPbp = new PSVIMGFileStream(GamePsvimg, "/EBOOT.PBP");
            byte[] VersionKey = pbp.GetVersionKey(EbootPbp);
            string VerKey = BitConverter.ToString(VersionKey).Replace("-", "");
            WriteSetting("VersionKey", VerKey);

            string ContentID = pbp.GetContentId(EbootPbp);
            PSVIMGFileStream LicenseRif = new PSVIMGFileStream(LicensePsvimg, "/"+ ContentID+ ".rif");
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

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select PSP UMD image *.iso";
            ofd.Filter = "ISO9660 Image Files (*.iso)|*.iso";
            ofd.ShowDialog();
            ISOPath.Text = ofd.FileName;
        }

        private void CHOVY_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void PsmChan_Click(object sender, EventArgs e)
        {
            if(!MutedAudio)
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

        private void DexAidEnabler_Click(object sender, EventArgs e)
        {
            if (!IsDexAidSet())
            {
                WriteSetting("DexAid", "1");
                MessageBox.Show("Enabled DEX Aid\n(0x0000000000000000) will be used for CMA Backups.", "Dex Aid", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                WriteSetting("DexAid", "0");
                MessageBox.Show("Enabled Retail Aid,\nAid From RIF Will be used for CMA Backups.", "Dex Aid", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Ps1Menu_Click(object sender, EventArgs e)
        {

            if (!MutedAudio)
            {
                Stream str = Resources.POPS;
                SoundPlayer snd = new  SoundPlayer(str);
                snd.Play();
            }
            CHOVYPopsBuilder pops = new CHOVYPopsBuilder();
            this.Hide();
            pops.Show();
            pops.FormClosing += Pops_FormClosing;
            
        }

        private void Pops_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Show();
        }
    }
}
