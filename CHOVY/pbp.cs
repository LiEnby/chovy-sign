using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Paddings;
using PSVIMGTOOLS;
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

        // PSP
        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int kirk_init();
        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceDrmBBMacInit(MAC_KEY *mkey, int type);
        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceDrmBBMacUpdate(MAC_KEY *mkey, byte[] buf, int size);
        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceDrmBBMacFinal2(MAC_KEY* mkey, byte[] outp, byte[] vkey);
        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int bbmac_getkey(MAC_KEY *mkey, byte[] bbmac, byte[] vkey);
        // PS1
        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int process_pgd(byte[] pgd_buf, int pgd_size, int pgd_flag);
        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int process_pgd_file(string pgd_file);
        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern byte[] pdg_open(byte[] pgd_buf, int pgd_size, int pgd_flag);
        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int encrypt_pgd(byte[] data, int data_size, int block_size, int key_index, int drm_type, int flag, byte[] key, byte[] pgd_data);
        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int decrypt_pgd(byte[] pgd_data, int pgd_size, int flag, byte[] key);



        // PSVita
        [DllImport("CHOVY.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int chovy_gen(string ebootpbp, UInt64 AID, string outscefile);
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


        private static UInt32 readUInt32(Stream src)
        {
            byte[] intBuf = new byte[0x4];
            src.Read(intBuf, 0x00, 0x04);
            return BitConverter.ToUInt32(intBuf, 0);
        }

        private static Int32 readInt32(Stream src)
        {
            byte[] intBuf = new byte[0x4];
            src.Read(intBuf, 0x00, 0x04);
            return BitConverter.ToInt32(intBuf, 0);
        }

        public static byte[] DecryptPSISOHeader(Stream pbp)
        {
            pbp.Seek(0x24, SeekOrigin.Begin);
            Int64 PSISOOffset = Convert.ToInt64(readUInt32(pbp));
            pbp.Seek(PSISOOffset, SeekOrigin.Begin);

            byte[] pgd = new byte[0x100000];
            
            pbp.Seek(0x400, SeekOrigin.Current);
            pbp.Read(pgd, 0, pgd.Length);
            int fz = process_pgd(pgd, pgd.Length, 2);
            if(fz < 0)
            {
                throw new Exception("Failed to decrypt!");
            }
            else
            {
                return pgd;
            }
        }

        public unsafe static byte[] GetVersionKeyPs1(Stream pbp)
        {

            kirk_init();

            pbp.Seek(0x24, SeekOrigin.Begin);
            Int64 PSISOOffset = Convert.ToInt64(readUInt32(pbp));
            pbp.Seek(PSISOOffset, SeekOrigin.Begin);
            pbp.Seek(0x400, SeekOrigin.Current);
            pbp.Seek(0x4, SeekOrigin.Current);
            int key_index, drm_type;

            key_index = readInt32(pbp);
            drm_type = readInt32(pbp);

            pbp.Seek(PSISOOffset + 0x400, SeekOrigin.Begin);
            byte[] pgd_buf = new byte[0x70];
            pbp.Read(pgd_buf, 0x00, pgd_buf.Length);

            byte[] VER_KEY_ENC = new byte[0x10];
            pbp.Read(VER_KEY_ENC, 0x00, VER_KEY_ENC.Length);

            MAC_KEY mkey;
            byte[] VERSION_KEY = new byte[0x10];
            int mac_type;
            if (drm_type == 1)
            {
                mac_type = 1;
                if (key_index > 1)
                {
                    mac_type = 3;
                }
            }
            else
            {
                mac_type = 2;
            }

            sceDrmBBMacInit(&mkey, mac_type);
            sceDrmBBMacUpdate(&mkey, pgd_buf, 0x70);
            bbmac_getkey(&mkey, VER_KEY_ENC, VERSION_KEY);

            return VERSION_KEY;
        }
        public unsafe static byte[] GetVersionKey(Stream pbp)
        {
            MAC_KEY mkey;

            kirk_init();

            pbp.Seek(0x24,SeekOrigin.Begin);
            Int64 NPUMDIMGOffest = Convert.ToInt64(readUInt32(pbp));

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

        public static int gen__sce_ebootpbp(string EbootFile, UInt64 AID, string OutSceebootpbpFile)
        {
            return chovy_gen(EbootFile, AID, OutSceebootpbpFile);
        }

        public static string GetContentId(Stream pbp)
        {
            pbp.Seek(0x24, SeekOrigin.Begin);
            Int64 NPUMDIMGOffest = Convert.ToInt64(readUInt32(pbp));
            pbp.Seek(NPUMDIMGOffest+0x10, SeekOrigin.Begin);
            byte[] ContentId = new byte[0x24];
            pbp.Read(ContentId, 0x00, 0x24);
            pbp.Close();
            return Encoding.UTF8.GetString(ContentId);
        }
    }
}
