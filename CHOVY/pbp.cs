using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CHOVY
{
    class pbp
    {

        unsafe struct MAC_KEY
        {
            int type;
            fixed byte key[0xF];
            fixed byte pad[0xF];
            int pad_size;
        }


        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int kirk_init();
        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceDrmBBMacInit(MAC_KEY *mkey, int type);
        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceDrmBBMacUpdate(MAC_KEY *mkey, byte[] buf, int size);
        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int bbmac_getkey(MAC_KEY *mkey, byte[] bbmac, byte[] vkey);

        public static Process GenPbpFromIso(string ISOPath, string OutputPBP, string ContentId, string VersionKey, bool compress, string bootup="")
        {
            string SignNpArgs = "-pbp ";
            if(compress)
            {
                SignNpArgs += "-c ";
            }
            SignNpArgs += "\"" + ISOPath + "\" ";
            SignNpArgs += "\"" + OutputPBP + "\" ";
            SignNpArgs += ContentId+" ";
            SignNpArgs += VersionKey;
            if(bootup != "")
            {
                SignNpArgs += " \"" + bootup + "\" ";
            }

            Process signnp = new Process();
            signnp.StartInfo.FileName = Path.Combine(Application.StartupPath, "tools", "sign_np.exe");
            signnp.StartInfo.Arguments = SignNpArgs;
            signnp.StartInfo.CreateNoWindow = true;
            signnp.StartInfo.UseShellExecute = false;
            signnp.StartInfo.RedirectStandardOutput = true;
            signnp.StartInfo.RedirectStandardError = true;
            signnp.Start();
            return signnp;
        }


        public unsafe static byte[] GetVersionKey(string pbpfile)
        {
            MAC_KEY mkey;

            kirk_init();

            FileStream pbp = File.OpenRead(pbpfile);
            BinaryReader binPBP = new BinaryReader(pbp);
            pbp.Seek(0x24,SeekOrigin.Begin);
            int NPUMDIMGOffest = binPBP.ReadInt32();

            pbp.Seek(NPUMDIMGOffest, SeekOrigin.Begin);

            byte[] NP_HEADER = new byte[0x0100];
            pbp.Read(NP_HEADER, 0x00, 0x0100);

            byte[] VER_KEY_ENC = new byte[0x40];
            pbp.Seek(NPUMDIMGOffest+0xC0, SeekOrigin.Begin);
            pbp.Read(VER_KEY_ENC, 0x00, 0x40);

            byte[] VERSION_KEY = new byte[16];

            sceDrmBBMacInit(&mkey, 3);
            sceDrmBBMacUpdate(&mkey, NP_HEADER, 0xc0);
            bbmac_getkey(&mkey, VER_KEY_ENC, VERSION_KEY);
            pbp.Close();
            return VERSION_KEY;
        }

        public static Process gen__sce_ebootpbp(string EbootFile,string AID)
        {
            Process chovygen = new Process();
            chovygen.StartInfo.FileName = Path.Combine(Application.StartupPath, "tools", "chovy-gen.exe");
            chovygen.StartInfo.WorkingDirectory = Path.GetDirectoryName(EbootFile);
            chovygen.StartInfo.Arguments = AID + " \""+EbootFile+"\"";
            chovygen.StartInfo.CreateNoWindow = true;
            chovygen.StartInfo.UseShellExecute = false;
            chovygen.StartInfo.RedirectStandardOutput = true;
            chovygen.StartInfo.RedirectStandardError = true;
            chovygen.Start();
            return chovygen;
        }
        public static string GetContentId(string pbpfile)
        {
            FileStream pbp = File.OpenRead(pbpfile);
            BinaryReader binPBP = new BinaryReader(pbp);
            pbp.Seek(0x24, SeekOrigin.Begin);
            int NPUMDIMGOffest = binPBP.ReadInt32();
            pbp.Seek(NPUMDIMGOffest+0x10, SeekOrigin.Begin);
            byte[] ContentId = new byte[0x24];
            pbp.Read(ContentId, 0x00, 0x24);
            pbp.Close();
            return Encoding.UTF8.GetString(ContentId);
        }
    }
}
