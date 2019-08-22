using CHOVY.Properties;
using DiscUtils.Iso9660;
using Microsoft.Win32;
using Param_SFO;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Text;
using System.Windows.Forms;

namespace CHOVY
{


    public partial class CHOVY : Form
    {
        bool MutedAudio = false;
        public byte[] GetSfo(string ISOFile) 
        {
            FileStream ISO = File.OpenRead(ISOFile);
            
            CDReader cdr = new CDReader(ISO, false);
            Stream ParamSfo = cdr.OpenFile(@"PSP_GAME\PARAM.SFO", FileMode.Open,FileAccess.Read);

            byte[] Sfo = new byte[ParamSfo.Length];
            ParamSfo.Read(Sfo, 0x00, (int)ParamSfo.Length);
            ISO.Close();
            
            return Sfo;
            
        }
        public byte[] GetIcon(string ISOFile)
        {
            FileStream ISO = File.OpenRead(ISOFile);
            CDReader cdr = new CDReader(ISO, false);
            Stream ParamSfo = cdr.OpenFile(@"PSP_GAME\ICON0.PNG", FileMode.Open, FileAccess.Read);

            byte[] Icon0 = new byte[ParamSfo.Length];
            ParamSfo.Read(Icon0, 0x00, (int)ParamSfo.Length);
            ISO.Close();

            return Icon0;

        }
        public static string GetTitleID(string ISOFile)
        {
            FileStream ISO = File.OpenRead(ISOFile);
            CDReader cdr = new CDReader(ISO, false);
            Stream ParamSfo = cdr.OpenFile(@"PSP_GAME\PARAM.SFO", FileMode.Open, FileAccess.Read);

            PARAM_SFO sfo = new PARAM_SFO(ParamSfo);
            
            string TitleID = sfo.GetValue("DISC_ID");
            ISO.Close();
            return TitleID;
        }

        public static bool isMini(string ISOFile)
        {
            FileStream ISO = File.OpenRead(ISOFile);
            CDReader cdr = new CDReader(ISO, false);
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
                key = Registry.CurrentUser.CreateSubKey(@"Software\CHOVY");
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
                key = Registry.CurrentUser.CreateSubKey(@"Software\CHOVY");
                key.SetValue(Setting, Value);
                key.Close();
            }
            catch (Exception) { }
        }

        public CHOVY()
        {
            InitializeComponent();
        }
        private void CHOVY_Load(object sender, EventArgs e)
        {

            if(ReadSetting("MuteAudio") == "1")
            {
                MutedAudio = true;
            }

            Versionkey.Text = ReadSetting("VersionKey");
            RifPath.Text = ReadSetting("RifPath");
        }

        private void FREEDOM_Click(object sender, EventArgs e)
        {
            if(RifPath.Text == "" || !File.Exists(RifPath.Text))
            {
                MessageBox.Show("INVALID RIF PATH!\nPlease try \"Find from CMA\"", "RIF ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
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
             string GameWorkDir = Path.Combine(TmpDir, "ux0_pspemu_temp_game_PSP_GAME_"+ TitleID);
           // string GameWorkDir = Path.Combine(TmpDir, TitleID);
            string LicenseWorkDir = Path.Combine(TmpDir, "ux0_pspemu_temp_game_PSP_LICENSE");
            string EbootFile = Path.Combine(GameWorkDir, "EBOOT.PBP");
            string EbootSignature = Path.Combine(GameWorkDir, "__sce_ebootpbp");
            string GamePathFile = Path.Combine(GameWorkDir, "VITA_PATH.TXT");
            string LicensePathFile = Path.Combine(LicenseWorkDir, "VITA_PATH.TXT");


            try
            {
                Directory.Delete(TmpDir, true);
            }
            catch (Exception) { };
            Directory.CreateDirectory(TmpDir);
            Directory.CreateDirectory(GameWorkDir);
           

            //Read RIF data
            byte[] ContentId = new byte[0x24];
            byte[] RifAid = new byte[0x08];
            FileStream rif = File.OpenRead(RifPath.Text);
            rif.Seek(0x10, SeekOrigin.Begin);
            rif.Read(ContentId, 0x00, 0x24);
            rif.Seek(0x08, SeekOrigin.Begin);
            rif.Read(RifAid, 0x00, 0x08);
            rif.Close();

            string ContentID = Encoding.UTF8.GetString(ContentId);
            string Aid = BitConverter.ToString(RifAid).Replace("-", "").ToLower();
            string BackupWorkDir = Path.Combine(CmaDir, "PGAME", Aid, TitleID);

            TotalProgress.Style = ProgressBarStyle.Continuous;
            Status.Text = "Overthrowing The PSPEMU Monarchy 00%";

            string BootupImage = "";
            if (isMini(ISOPath.Text))
            {
                BootupImage = Path.Combine(Application.StartupPath, "_tmp", "minis.png");
                Resources.MINIS.Save(BootupImage);
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
                    Status.Text = "Overthrowing The PSPEMU Monarchy " + Progress;
                }
                Application.DoEvents();
            }
            TotalProgress.Value = 100;

            if (isMini(ISOPath.Text))
            {
                try
                {
                    File.Delete(BootupImage);
                }
                catch (Exception) { };
                
            }

            File.WriteAllText(GamePathFile, "ux0:pspemu/temp/game/PSP/GAME/" + TitleID + "\x00");

            TotalProgress.Style = ProgressBarStyle.Marquee;
            Status.Text = "Signing the Declaration of Independance";
            Process ChovyGen = pbp.gen__sce_ebootpbp(EbootFile, Aid);
            while(!ChovyGen.HasExited)
            {
                Application.DoEvents();
            }
            if (!File.Exists(EbootSignature) || ChovyGen.ExitCode != 0)
            {
                MessageBox.Show("CHOVY-GEN.EXE FAILED!\nArguments:\n" + ChovyGen.StartInfo.Arguments + "\nStdOut:\n" + ChovyGen.StandardOutput.ReadToEnd() + "\nStdErr:\n" + ChovyGen.StandardError.ReadToEnd());
                return;
            }

            /*
             *  BUILD PSVIMG FILE
             */

            // Pacakge GAME


            string BackupGameDir = Path.Combine(BackupWorkDir, "game");
            Directory.CreateDirectory(BackupGameDir);
            Process psvimg_create = psvimgtools.PSVIMG_CREATE(Aid, "game" ,TmpDir, Path.Combine(BackupWorkDir,"game"));
            while(!psvimg_create.HasExited)
            {
                Application.DoEvents();
            }
            if (psvimg_create.ExitCode != 0)
            {
                MessageBox.Show("PSVIMG-CREATE.EXE FAILED!\nArguments:\n" + psvimg_create.StartInfo.Arguments + "\nStdOut:\n" + psvimg_create.StandardOutput.ReadToEnd() + "\nStdErr:\n" + psvimg_create.StandardError.ReadToEnd());
                return;
            }

            // Package LICENSE
            try
            {
                Directory.Delete(TmpDir, true);
            }
            catch (Exception) { };
            Directory.CreateDirectory(TmpDir);
            Directory.CreateDirectory(LicenseWorkDir);
            File.Copy(RifPath.Text, Path.Combine(LicenseWorkDir, ContentID + ".rif"), true);
            File.WriteAllText(LicensePathFile, "ux0:pspemu/temp/game/PSP/LICENSE\x00");
            Directory.CreateDirectory(BackupGameDir);
            psvimg_create = psvimgtools.PSVIMG_CREATE(Aid, "license", TmpDir, Path.Combine(BackupWorkDir, "license"));
            while (!psvimg_create.HasExited)
            {
                Application.DoEvents();
            }
            if(psvimg_create.ExitCode != 0)
            {
                MessageBox.Show("PSVIMG-CREATE.EXE FAILED!\nArguments:\n" + psvimg_create.StartInfo.Arguments + "\nStdOut:\n" + psvimg_create.StandardOutput.ReadToEnd() + "\nStdErr:\n" + psvimg_create.StandardError.ReadToEnd());
                return;
            }

            try
            {
                Directory.Delete(TmpDir, true);
            }
            catch (Exception) { };

            // Write PARAM.SFO & ICON0.PNG
            string SceSysWorkDir = Path.Combine(BackupWorkDir, "sce_sys");
            Directory.CreateDirectory(SceSysWorkDir);

            byte[] ParamSfo = GetSfo(ISOPath.Text);
            byte[] Icon0 = GetIcon(ISOPath.Text);
            File.WriteAllBytes(Path.Combine(SceSysWorkDir, "param.sfo"), ParamSfo);
            File.WriteAllBytes(Path.Combine(SceSysWorkDir, "icon0.png"), Icon0);

            Status.Text = "YOU HAVE MADE A SOCIAL CONTRACT WITH FREEDOM!";
            TotalProgress.Value = 0;
            TotalProgress.Style = ProgressBarStyle.Continuous;
            
            if(!MutedAudio)
            {
                Stream str = Resources.Murica;
                SoundPlayer snd = new SoundPlayer(str);
                snd.Play();
            }
            

            MessageBox.Show("YOU HAVE MADE A SOCIAL CONTRACT WITH FREEDOM!", "FREEDOM!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void FindFromCMA_Click(object sender, EventArgs e)
        {
           /* Ccs_FormClosing(null, null);
            return;*/

            this.Hide();
            string cmaDir = "";
            string accountId = "0000000000000000";
            try
            {
                cmaDir = ReadSetting("CmaDir");
            }
            catch(Exception)
            {
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

            ccs.Hide();
            this.Show();
            this.Focus();

            if (Backup == "")
            {
                return;
            }

            string output = Path.Combine(Application.StartupPath, "_tmp");
            Directory.CreateDirectory(output);

            string OutputPbp = Path.Combine(output, "ux0_pspemu_temp_game_PSP_GAME_" + Backup, "EBOOT.PBP");
            string GameFolder = Path.Combine(output, "ux0_pspemu_temp_game_PSP_GAME_" + Backup);
            string LicenseFolder = Path.Combine(output, "ux0_pspemu_temp_game_PSP_LICENSE");

            Status.Text = "Declaring Independance from the GAME.PSVIMG";
            string BackupPath = Path.Combine(CmaDir, "PGAME", CmaAid, Backup, "game", "game.psvimg");
            if(!File.Exists(BackupPath))
            {
                return;
            }
            TotalProgress.Style = ProgressBarStyle.Marquee;
            Process psvimg = psvimgtools.PSVIMG_EXTRACT(CmaAid, BackupPath, output);
            while (!psvimg.HasExited)
            {
                Application.DoEvents();
            }
            if(!Directory.Exists(GameFolder) || psvimg.ExitCode != 0)
            {
                MessageBox.Show("PSVIMG-EXTRACT.EXE FAILED!\nArguments:\n" + psvimg.StartInfo.Arguments + "\nStdOut:\n" + psvimg.StandardOutput.ReadToEnd() + "\nStdErr:\n" + psvimg.StandardError.ReadToEnd());
                return;
            }

            Status.Text = "Declaring Independance from the LICENSE.PSVIMG";
            BackupPath = Path.Combine(CmaDir, "PGAME", CmaAid, Backup, "license", "license.psvimg");
            psvimg = psvimgtools.PSVIMG_EXTRACT(CmaAid, BackupPath, output);
            while(!psvimg.HasExited)
            {
                Application.DoEvents();
            }
            if (!Directory.Exists(LicenseFolder) || psvimg.ExitCode != 0)
            {
                MessageBox.Show("PSVIMG-EXTRACT.EXE FAILED!\nArguments:\n" + psvimg.StartInfo.Arguments + "\nStdOut:\n" + psvimg.StandardOutput.ReadToEnd() + "\nStdErr:\n" + psvimg.StandardError.ReadToEnd());
                return;
            }

            /* -- This was some debug stuff
            OpenFileDialog Openpbp = new OpenFileDialog();
            Openpbp.ShowDialog();
            string OutputPbp = Openpbp.FileName;

            OpenFileDialog OpenRif = new OpenFileDialog();
            OpenRif.ShowDialog();
            string Rif = OpenRif.FileName;
            */

            byte[] VersionKey = pbp.GetVersionKey(OutputPbp);
            string VerKey = BitConverter.ToString(VersionKey).Replace("-", "");
            WriteSetting("VersionKey", VerKey);

            string ContentID = pbp.GetContentId(OutputPbp);
            string Rif = Path.Combine(Application.StartupPath, "GAME.RIF");
            string LicensePath = Path.Combine(output, "ux0_pspemu_temp_game_PSP_LICENSE", ContentID + ".rif");
            if(!File.Exists(LicensePath))
            {
                MessageBox.Show("Cannot find RIF", "RIF ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            File.Copy(LicensePath, Rif, true);
            WriteSetting("RifPath", Rif);
            WriteSetting("CmaDir", CmaDir);

            Versionkey.Text = VerKey;
            RifPath.Text = Rif;

           if (Directory.Exists(output))
            {
                try
                {
                    Directory.Delete(output, true);
                }
                catch (Exception) { }

            } 

            Status.Text = "Progress %";
            TotalProgress.Style = ProgressBarStyle.Continuous;
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
    }
}
