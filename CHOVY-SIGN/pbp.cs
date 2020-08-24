using BasicDataTypes;
using ParamSfo;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CHOVY_SIGN
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

        const int KIRK_CMD_DECRYPT_PRIVATE = 1;
        const int KIRK_CMD_2 = 2;
        const int KIRK_CMD_3 = 3;
        const int KIRK_CMD_ENCRYPT_IV_0 = 4;
        const int KIRK_CMD_ENCRYPT_IV_FUSE = 5;
        const int KIRK_CMD_ENCRYPT_IV_USER = 6;
        const int KIRK_CMD_DECRYPT_IV_0 = 7;
        const int KIRK_CMD_DECRYPT_IV_FUSE = 8;
        const int KIRK_CMD_DECRYPT_IV_USER = 9;
        const int KIRK_CMD_PRIV_SIGN_CHECK = 10;
        const int KIRK_CMD_SHA1_HASH = 11;
        const int KIRK_CMD_ECDSA_GEN_KEYS = 12;
        const int KIRK_CMD_ECDSA_MULTIPLY_POINT = 13;
        const int KIRK_CMD_PRNG = 14;
        const int KIRK_CMD_15 = 15;
        const int KIRK_CMD_ECDSA_SIGN = 16;
        const int KIRK_CMD_ECDSA_VERIFY = 17;

        static byte[] npumdimg_private_key = new byte[0x14] { 0x14, 0xB0, 0x22, 0xE8, 0x92, 0xCF, 0x86, 0x14, 0xA4, 0x45, 0x57, 0xDB, 0x09, 0x5C, 0x92, 0x8D, 0xE9, 0xB8, 0x99, 0x70 };
        static byte[] npumdimg_public_key = new byte[0x28] {0x01, 0x21, 0xEA, 0x6E, 0xCD, 0xB2, 0x3A, 0x3E,0x23, 0x75, 0x67, 0x1C, 0x53, 0x62, 0xE8, 0xE2,0x8B, 0x1E, 0x78, 0x3B, 0x1A, 0x27, 0x32, 0x15,0x8B, 0x8C, 0xED, 0x98, 0x46, 0x6C, 0x18, 0xA3, 0xAC, 0x3B, 0x11, 0x06, 0xAF, 0xB4, 0xEC, 0x3B};

        // PSP
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int kirk_init();
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceDrmBBMacInit(MAC_KEY *mkey, int type);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceDrmBBMacUpdate(MAC_KEY *mkey, byte[] buf, int size);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceDrmBBMacFinal2(MAC_KEY* mkey, byte[] outp, byte[] vkey);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int bbmac_getkey(MAC_KEY *mkey, byte[] bbmac, byte[] vkey);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceUtilsBufferCopyWithRange(byte[] outbuff, int outsize, byte[] inbuff, int insize, int cmd);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern void encrypt_kirk16_private(byte[] dA_out, byte[] dA_dec);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern void decrypt_kirk16_private(byte[] dA_out, byte[] dA_dec);

        // PS1
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int process_pgd(byte[] pgd_buf, int pgd_size, int pgd_flag);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int process_pgd_file(string pgd_file);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern byte[] pdg_open(byte[] pgd_buf, int pgd_size, int pgd_flag);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int encrypt_pgd(byte[] data, int data_size, int block_size, int key_index, int drm_type, int flag, byte[] key, byte[] pgd_data);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int decrypt_pgd(byte[] pgd_data, int pgd_size, int flag, byte[] key);



        // PSVita
        [DllImport("CHOVY-GEN.DLL", CallingConvention = CallingConvention.Cdecl)]
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

        public static byte[] HashSfo(byte[] DataPspBytes, byte[] SfoBytes)
        {
            int SfoSize = SfoBytes.Length + 0x30;
            byte[] SfoData = new byte[SfoSize];
            Array.ConstrainedCopy(SfoBytes, 0, SfoData, 0, SfoBytes.Length);
            Array.ConstrainedCopy(DataPspBytes, 0, SfoData, SfoBytes.Length, DataPspBytes.Length);

            byte[] SfoHashIn = new byte[SfoSize + 0x4];
            byte[] SfoHashOut = new byte[0x14];

            byte[] SizeBytes = BitConverter.GetBytes(SfoSize);

            Array.ConstrainedCopy(SizeBytes, 0, SfoHashIn, 0, SizeBytes.Length);
            Array.ConstrainedCopy(SfoData, 0, SfoHashIn, 0x4, SfoSize);

            if (sceUtilsBufferCopyWithRange(SfoHashOut, SfoHashOut.Length, SfoHashIn, SfoHashIn.Length, KIRK_CMD_SHA1_HASH) != 0)
            {
                throw new Exception("Failed to generate SHA1 hash for DATA.PSP!");
            }

            return SfoHashOut;
        }
        public static byte[] SignSfo(bool PS1,byte[] DataToSign)
        {
            byte[] DataPspSignBufIn = new byte[0x34];
            byte[] DataPspSignBufOut = new byte[0x28];

            byte[] DataPspKeyPair = new byte[npumdimg_private_key.Length + npumdimg_public_key.Length];

            Array.ConstrainedCopy(npumdimg_private_key, 0, DataPspKeyPair, 0, npumdimg_private_key.Length);
            Array.ConstrainedCopy(npumdimg_public_key, 0, DataPspKeyPair, npumdimg_private_key.Length, npumdimg_public_key.Length);

            byte[] DataPspPrivateKeyEnc = new byte[0x20];
            encrypt_kirk16_private(DataPspPrivateKeyEnc, DataPspKeyPair);

            // Generate ECDSA signature.
            Array.ConstrainedCopy(DataPspPrivateKeyEnc, 0, DataPspSignBufIn, 0, DataPspPrivateKeyEnc.Length);
            Array.ConstrainedCopy(DataToSign, 0, DataPspSignBufIn, DataPspPrivateKeyEnc.Length, DataToSign.Length);

            if (sceUtilsBufferCopyWithRange(DataPspSignBufOut, DataPspSignBufOut.Length, DataPspSignBufIn, DataPspSignBufIn.Length, KIRK_CMD_ECDSA_SIGN) != 0)
            {
                throw new Exception("Failed to generate ECDSA signature for DATA.PSP!\n");
            }
            return DataPspSignBufOut;
        }

        public static void VerifySignature(bool PS1,byte[] Hash, byte[] Signature)
        {
            byte[] TestData = new byte[0x64];
            Array.ConstrainedCopy(npumdimg_public_key, 0, TestData, 0, npumdimg_public_key.Length);
            Array.ConstrainedCopy(Hash, 0, TestData, npumdimg_public_key.Length, Hash.Length);
            Array.ConstrainedCopy(Signature, 0, TestData, npumdimg_public_key.Length + Hash.Length, Signature.Length);
            if (sceUtilsBufferCopyWithRange(null, 0, TestData, 0x64, KIRK_CMD_ECDSA_VERIFY) != 0)
            {
                throw new Exception("ECDSA signature for DATA.PSP is invalid!\n");
            }
        }

        public static byte[] BuildDataPsp(byte[] Startdat, string ContentId, byte[] SfoBytes)
        {
            int NP_FLAGS = 0x2; 
            // Normally this is dependant on if a version key is used (and thus the app is npdrm_free)
            // Howevver vita does not accept npdrm_free PSP apps
            int DataPspSize = 0x594;
            if (Startdat.Length != 0)
                DataPspSize += Convert.ToInt32(Startdat.Length + 0xC);

            byte[] DataPspFile = new byte[DataPspSize];

            DataUtils.CopyString(DataPspFile, ContentId, 0x560);
            DataUtils.CopyInt32BE(DataPspFile, NP_FLAGS, 0x590);

            byte[] PspData = new byte[0x30];
            Array.ConstrainedCopy(DataPspFile, 0x560, PspData, 0, PspData.Length);

            byte[] SfoHash = HashSfo(PspData, SfoBytes);
            byte[] SfoSignature = SignSfo(false, SfoHash);
            VerifySignature(false, SfoHash, SfoSignature);

            Array.ConstrainedCopy(SfoSignature, 0, DataPspFile, 0, SfoSignature.Length);

            if(Startdat.Length != 0)
            {
                Array.ConstrainedCopy(Startdat, 0, DataPspFile, 0x5A0, Startdat.Length);
            }

            return DataPspFile;
        }

        public static byte[] BuildStartData(Bitmap bmp)
        {
            MemoryStream ImageData = new MemoryStream();
            bmp.Save(ImageData, ImageFormat.Png);
            byte[] PngBytes = ImageData.ToArray();
            ImageData.Dispose();

            int HeaderSize = 0x50;
            int StartDatSize = Convert.ToInt32(PngBytes.Length + HeaderSize);

            byte[] StartDat = new byte[StartDatSize];
            DataUtils.CopyString(StartDat, "STARTDAT",0x0);
            DataUtils.CopyInt32(StartDat, 0x1, 0x8);
            DataUtils.CopyInt32(StartDat, 0x1, 0xC);
            DataUtils.CopyInt32(StartDat, HeaderSize, 0x10);
            DataUtils.CopyInt32(StartDat, PngBytes.Length, 0x14);

            Array.ConstrainedCopy(PngBytes, 0, StartDat, 0x50, PngBytes.Length);
            return StartDat;
        }

        public static byte[] PatchSfo(byte[] SfoBytes, string ContentId)
        {
            MemoryStream SfoStream = new MemoryStream(SfoBytes);
            String TitleId = ContentId.Substring(0x7, 0x9);
            Sfo.WriteSfoString(SfoStream, "DISC_ID", TitleId);
            SfoStream.Seek(0x00, SeekOrigin.Begin);
            Sfo.WriteSfoString(SfoStream, "CATEGORY", "EG");
            SfoStream.Seek(0x00, SeekOrigin.Begin);
            byte[] OutputBytes = SfoStream.ToArray();
            SfoStream.Dispose();
            return OutputBytes;
        }
        public static void BuildPbp(Stream str, Bitmap start_image, string content_id, byte[] param_sfo,byte[] icon0, byte[] icon1pmf, byte[] pic0, byte[] pic1, byte[] sndat3)
        {
            kirk_init();
            byte[] NewSfo = PatchSfo(param_sfo, content_id);
            byte[] StartData = BuildStartData(start_image);
            byte[] DataPsp = BuildDataPsp(StartData, content_id, NewSfo);
            
        }
        public static int gen__sce_ebootpbp(string EbootFile, UInt64 AID, string OutSceebootpbpFile)
        {
            return chovy_gen(EbootFile, AID, OutSceebootpbpFile);
        }

        public static string GetContentIdPS1(Stream pbp)
        {
            pbp.Seek(0x1C, SeekOrigin.Begin);
            Int64 PSPFileOffset = Convert.ToInt64(readUInt32(pbp));
            pbp.Seek(PSPFileOffset + 0x560, SeekOrigin.Begin);
            byte[] ContentId = new byte[0x24];
            pbp.Read(ContentId, 0x00, 0x24);
            pbp.Close();
            return Encoding.UTF8.GetString(ContentId);
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
