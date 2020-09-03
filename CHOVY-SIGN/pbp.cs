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
    class Pbp
    {

        unsafe struct MAC_KEY
        {
            int type;
            fixed byte key[0xF];
            fixed byte pad[0xF];
            int pad_size;
        }
        unsafe struct CIPHER_KEY
        {
            UInt32 type;
            UInt32 seed;
            fixed byte key[16];
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
        private unsafe static extern int lzrc_compress(byte[] outData, int out_len, byte[] inData, int in_len);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int kirk_init();
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern void encrypt_kirk16_private(byte[] dA_out, byte[] dA_dec);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern void decrypt_kirk16_private(byte[] dA_out, byte[] dA_dec);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int bbmac_build_final2(int type, byte[] mac);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int bbmac_getkey(MAC_KEY* mkey, byte[] bbmac, byte[] vkey);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceDrmBBMacInit(MAC_KEY *mkey, int type);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceDrmBBMacUpdate(MAC_KEY *mkey, byte[] buf, int size);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceDrmBBMacFinal(MAC_KEY* mkey, byte[] outp, byte[] vkey);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceDrmBBMacFinal2(MAC_KEY* mkey, byte[] outp, byte[] vkey);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceUtilsBufferCopyWithRange(byte[] outbuff, int outsize, byte[] inbuff, int insize, int cmd);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceDrmBBCipherInit(CIPHER_KEY* ckey, int type, int mode, byte[] header_key, byte[] version_key, UInt32 seed);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceDrmBBCipherUpdate(CIPHER_KEY* ckey, byte[] data, int size);
        [DllImport("CHOVY-KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int sceDrmBBCipherFinal(CIPHER_KEY* ckey);

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

        public static int NumberOfSectors = 0;
        public static int SectorsDone = 0;
        public static bool DoEvents = true;
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
                throw new Exception("Failed to decrypt IS1ISO Header.");
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

            byte[] NP_HEADER_ENC = new byte[0x0100];
            pbp.Read(NP_HEADER_ENC, 0x00, 0x0100);

            byte[] NP_HEADER_DEC = new byte[0x40];
            pbp.Seek(NPUMDIMGOffest+0xC0, SeekOrigin.Begin);
            pbp.Read(NP_HEADER_DEC, 0x00, 0x40);

            byte[] VERSION_KEY = new byte[16];

            sceDrmBBMacInit(&mkey, 3);
            sceDrmBBMacUpdate(&mkey, NP_HEADER_ENC, 0xc0);
            bbmac_getkey(&mkey, NP_HEADER_DEC, VERSION_KEY);
            pbp.Close();
            
            return VERSION_KEY;
        }

        // Normally this is dependant on if a version key is used (and thus the app is npdrm_free)
        // Howevver vita does not accept npdrm_free PSP apps
        const int NP_FLAGS = 0x2;
        const int RATIO_LIMIT = 90;
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
                throw new Exception("Failed to generate ECDSA signature");
            }
            return DataPspSignBufOut;
        }

        public static void VerifySignature(bool PS1,byte[] Hash, byte[] Signature)
        {
            DoEvents = false; // Chovy crashes if Application.DoEvents is called while verifying a signature.. for some reason
            byte[] TestData = new byte[0x64];
            Array.ConstrainedCopy(npumdimg_public_key, 0, TestData, 0, npumdimg_public_key.Length);
            Array.ConstrainedCopy(Hash, 0, TestData, npumdimg_public_key.Length, Hash.Length);
            Array.ConstrainedCopy(Signature, 0, TestData, npumdimg_public_key.Length + Hash.Length, Signature.Length);
            if (sceUtilsBufferCopyWithRange(null, 0, TestData, TestData.Length, KIRK_CMD_ECDSA_VERIFY) != 0)
            {
                throw new Exception("ECDSA signature is invalid!");
            }
            DoEvents = true;
        }

        public static byte[] BuildDataPsp(byte[] Startdat, string ContentId, byte[] SfoBytes)
        {
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

        public static byte[] BuildDataPsar()
        {
            int DataPsarSize = 0x100;
            byte[] DataPsar = new byte[DataPsarSize];
            return DataPsar;
        }

        unsafe public static byte[] BuildNpumdimgHeader(int iso_size, int iso_blocks, int block_basis, string content_id, int np_flags, byte[] version_key, byte[] header_key, byte[] data_key)
        {
            byte[] NpumdimgHeader = new byte[0x100]; // Header Size

            DataUtils.CopyString(NpumdimgHeader, "NPUMDIMG", 0);

            DataUtils.CopyInt32(NpumdimgHeader, np_flags, 0x8);
            DataUtils.CopyInt32(NpumdimgHeader, block_basis, 0xC);

            DataUtils.CopyString(NpumdimgHeader, content_id, 0x10);

            // NpuimgBody
            DataUtils.CopyInt16(NpumdimgHeader, 0x800, 0x40); // Sector Size

            if (iso_size > 0x40000000)
                DataUtils.CopyUInt16(NpumdimgHeader, 0xE001, 0x42);
            else
                DataUtils.CopyUInt16(NpumdimgHeader, 0xE000, 0x42);
            
            DataUtils.CopyUInt32(NpumdimgHeader, 0, 0x44);
            DataUtils.CopyUInt32(NpumdimgHeader, 0x1010, 0x48);
            DataUtils.CopyUInt32(NpumdimgHeader, 0x0, 0x4C);
            DataUtils.CopyUInt32(NpumdimgHeader, 0x0, 0x50);
            DataUtils.CopyUInt32(NpumdimgHeader, 0x0, 0x54); // LBA Start
            DataUtils.CopyUInt32(NpumdimgHeader, 0x0, 0x58);

            if (((iso_blocks * block_basis) - 1) > 0x6C0BF)
                DataUtils.CopyInt32(NpumdimgHeader, 0x6C0BF, 0x5C); // Number of Sectors
            else
                DataUtils.CopyInt32(NpumdimgHeader, (iso_blocks * block_basis) - 1, 0x5C); // Number of Sectors
            
            DataUtils.CopyInt32(NpumdimgHeader, 0x00, 0x60);
            DataUtils.CopyInt32(NpumdimgHeader, (iso_blocks * block_basis) - 1, 0x64); // LBA End
            DataUtils.CopyInt32(NpumdimgHeader, 0x01003FFE, 0x68);
            DataUtils.CopyInt32(NpumdimgHeader, 0x100, 0x6C); // Block Entry Offset

            String DiscId = content_id.Substring(7, 4) + "-" + content_id.Substring(11, 5);
            DataUtils.CopyString(NpumdimgHeader, DiscId, 0x70); // Disc Id

            DataUtils.CopyInt32(NpumdimgHeader, 0x00, 0x80); // Header Start Offset
            DataUtils.CopyInt32(NpumdimgHeader, 0x00, 0x84);
            NpumdimgHeader[0x88] = 0;
            NpumdimgHeader[0x89] = 0; // bbmac param
            NpumdimgHeader[0x8A] = 0;
            NpumdimgHeader[0x8C] = 0;
            DataUtils.CopyInt32(NpumdimgHeader, 0x00, 0x8C);
            DataUtils.CopyInt32(NpumdimgHeader, 0x00, 0x90);
            DataUtils.CopyInt32(NpumdimgHeader, 0x00, 0x94);
            DataUtils.CopyInt32(NpumdimgHeader, 0x00, 0x98);
            DataUtils.CopyInt32(NpumdimgHeader, 0x00, 0x9C);

            // NpuimgBody

            Array.ConstrainedCopy(header_key, 0, NpumdimgHeader, 0xA0, header_key.Length);
            Array.ConstrainedCopy(data_key, 0, NpumdimgHeader, 0xB0, data_key.Length);

            // Generate Padding
            byte[] PaddingBytes = new byte[0x8];
            sceUtilsBufferCopyWithRange(PaddingBytes, PaddingBytes.Length, null, 0, KIRK_CMD_PRNG);
            Array.ConstrainedCopy(PaddingBytes, 0, NpumdimgHeader, 0xD0, PaddingBytes.Length); // Padding Area

            // Prepare buffers to encrypt the NPUMDIMG body.
            MAC_KEY mck;
            CIPHER_KEY bck;

            // Encrypt NPUMDIMG body.
            
            byte[] ToEncrypt = new byte[0x60];
            Array.ConstrainedCopy(NpumdimgHeader, 0x40, ToEncrypt, 0, ToEncrypt.Length);
            sceDrmBBCipherInit(&bck, 1, 2, header_key, version_key, 0);
            sceDrmBBCipherUpdate(&bck, ToEncrypt, 0x60);
            sceDrmBBCipherFinal(&bck);
            Array.ConstrainedCopy(ToEncrypt, 0x00, NpumdimgHeader, 0x40, ToEncrypt.Length);
          
            // Generate header hash.
            byte[] header_hash = new byte[0x10];

            sceDrmBBMacInit(&mck, 3);
            sceDrmBBMacUpdate(&mck, NpumdimgHeader, 0xC0);
            sceDrmBBMacFinal(&mck, header_hash, version_key);
            bbmac_build_final2(3, header_hash);

            Array.ConstrainedCopy(header_hash, 0, NpumdimgHeader, 0xC0, header_hash.Length);

            // Prepare the signature hash input buffer.
            byte[] NpumdimgSha1InBuf = new byte[0xD8 + 0x4];
            byte[] NpuimgHash = new byte[0x14];

            // Setup Hash
            DataUtils.CopyInt32(NpumdimgSha1InBuf, 0xD8, 0);
            Array.ConstrainedCopy(NpumdimgHeader, 0x00, NpumdimgSha1InBuf, 0x04, NpumdimgSha1InBuf.Length - 0x4);

            // Hash the input buffer.
            if (sceUtilsBufferCopyWithRange(NpuimgHash, NpuimgHash.Length, NpumdimgSha1InBuf, NpumdimgSha1InBuf.Length, KIRK_CMD_SHA1_HASH) != 0)
            {
                throw new Exception("Failed to generate SHA1 hash for NPUMDIMG header!");
            }


            byte[] NpumdimgSignInBuf = new byte[0x34];
            byte[] NpumdimgSignature = new byte[0x28];

            // Create ECDSA key pair.
            byte[] NpumdimgKeypair = new byte[0x3C];

            Array.ConstrainedCopy(npumdimg_private_key, 0, NpumdimgKeypair, 0, npumdimg_private_key.Length);
            Array.ConstrainedCopy(npumdimg_public_key, 0, NpumdimgKeypair, npumdimg_private_key.Length, npumdimg_public_key.Length);
           
            // Encrypt NPUMDIMG private key.
            byte[] NpumdimgPrivateKeyEnc = new byte[0x20];
            encrypt_kirk16_private(NpumdimgPrivateKeyEnc, NpumdimgKeypair);

            // Generate ECDSA signature.
            Array.ConstrainedCopy(NpumdimgPrivateKeyEnc, 0x00, NpumdimgSignInBuf, 0, NpumdimgPrivateKeyEnc.Length);
            Array.ConstrainedCopy(NpuimgHash, 0x00, NpumdimgSignInBuf, NpumdimgPrivateKeyEnc.Length, NpuimgHash.Length);
            if (sceUtilsBufferCopyWithRange(NpumdimgSignature, NpumdimgSignature.Length, NpumdimgSignInBuf, NpumdimgSignInBuf.Length, KIRK_CMD_ECDSA_SIGN) != 0)
            {
                throw new Exception("ERROR: Failed to generate ECDSA signature for NPUMDIMG header!");
            }

            // Verify the generated ECDSA signature.
            VerifySignature(false, NpuimgHash, NpumdimgSignature);

            // Finally put ECDSA signature into header.
            Array.ConstrainedCopy(NpumdimgSignature, 0, NpumdimgHeader, 0xD8, NpumdimgSignature.Length);

            return NpumdimgHeader;
        }
        unsafe public static void SignIso(int HeaderOffset, Stream BaseStr, Stream Iso, string ContentId, byte[] VersionKey, bool Compress)
        {
            MAC_KEY MKey;
            CIPHER_KEY CKey;

            Int64 IsoSize = Iso.Length;
            Int64 TableOffset = Convert.ToInt64(HeaderOffset);
            int BlockBasis = 0x10;
            int BlockSize = BlockBasis * 2048;
            
            Int64 IsoBlocks = (IsoSize + BlockSize - 1) / BlockSize;
            NumberOfSectors = Convert.ToInt32(IsoBlocks);

            Int64 TableSize = IsoBlocks * 0x20;
            Int64 NpOffset = TableOffset - 0x100;
            int NpSize = 0x100;

            // Generate Random Header Key
            byte[] HeaderKey = new byte[0x10];
            //sceUtilsBufferCopyWithRange(HeaderKey, HeaderKey.Length, null, 0, KIRK_CMD_PRNG);

            byte[] TableBuffer = new byte[TableSize];
            DataUtils.WriteBytes(BaseStr, TableBuffer, TableSize);

            // Write ISO Blocks
            byte[] IsoBuffer = new byte[BlockSize * 2];
            byte[] LZRCBuffer = new byte[BlockSize * 2];
            byte[] Tb = new byte[0x20];
            Int64 IsoOffset = 0x100 + TableSize;
            int LZRCSize, Ratio;
            int TbOffset = 0;
            int WSize = 0;
            for (int i = 0; i < IsoBlocks; i++)
            {
                SectorsDone = i;
                Array.Clear(IsoBuffer, 0, IsoBuffer.Length);
                Array.Clear(LZRCBuffer, 0, LZRCBuffer.Length);
                Array.Clear(Tb, 0, Tb.Length);

                TbOffset = i * 0x20;
                Array.ConstrainedCopy(TableBuffer, TbOffset, Tb, 0, Tb.Length);

                if ((Iso.Position + BlockSize) > IsoSize)
                {
                    Int64 Remaining = IsoSize - Iso.Position;
                    Iso.Read(IsoBuffer, 0x00, Convert.ToInt32(Remaining));
                    WSize = Convert.ToInt32(Remaining);
                }
                else
                {
                    Iso.Read(IsoBuffer, 0x00, BlockSize);
                    WSize = BlockSize;
                }


                if (Compress)
                {
                    LZRCSize = lzrc_compress(LZRCBuffer, BlockSize * 2, IsoBuffer, BlockSize);
                    Ratio = (LZRCSize * 100) / BlockSize;

                    if (Ratio < RATIO_LIMIT)
                    {
                        WSize = (LZRCSize + 15) & ~15;
                    }
                }

                // Set table entry.
                DataUtils.CopyInt32(Tb, Convert.ToInt32(IsoOffset), 0x10);
                DataUtils.CopyInt32(Tb, WSize, 0x14);
                DataUtils.CopyInt32(Tb, 0, 0x18);
                DataUtils.CopyInt32(Tb, 0, 0x1C);

                // Encrypt Block
                
                sceDrmBBCipherInit(&CKey, 1, 2, HeaderKey, VersionKey, Convert.ToUInt32((IsoOffset >> 4)));
                if(!Compress)
                    sceDrmBBCipherUpdate(&CKey, IsoBuffer, WSize);
                else
                    sceDrmBBCipherUpdate(&CKey, LZRCBuffer, WSize);
                sceDrmBBCipherFinal(&CKey);
                

                // Build MAC.
                sceDrmBBMacInit(&MKey, 3);
                if (!Compress)
                    sceDrmBBMacUpdate(&MKey, IsoBuffer, WSize);
                else
                    sceDrmBBMacUpdate(&MKey, LZRCBuffer, WSize);
                sceDrmBBMacFinal(&MKey, Tb, VersionKey);

                bbmac_build_final2(3, Tb);

                // Encrypt table.
                byte[] EncTb = EncryptTable(Tb);

                // Write ISO data.
                WSize = (WSize + 15) & ~15;
                if (!Compress)
                    BaseStr.Write(IsoBuffer, 0x00, WSize);
                else
                    BaseStr.Write(LZRCBuffer, 0x00, WSize);

                // Update offset.
                IsoOffset += WSize;

                // Copy TB Back
                Array.ConstrainedCopy(EncTb, 0, TableBuffer, TbOffset, EncTb.Length);
                //Array.ConstrainedCopy(Tb, 0, TableBuffer, TbOffset, Tb.Length);

            }
            // Generate data key.
            byte[] DataKey = new byte[0x10];
            sceDrmBBMacInit(&MKey, 3);
            sceDrmBBMacUpdate(&MKey, TableBuffer, Convert.ToInt32(TableSize));
            sceDrmBBMacFinal(&MKey, DataKey, VersionKey);
            bbmac_build_final2(3, DataKey);

            byte[] NpumdimgHeader = BuildNpumdimgHeader(Convert.ToInt32(IsoSize), Convert.ToInt32(IsoBlocks), BlockBasis, ContentId, NP_FLAGS, VersionKey, HeaderKey, DataKey);
            BaseStr.Seek(NpOffset, SeekOrigin.Begin);
            BaseStr.Write(NpumdimgHeader, 0x00, NpSize);
            BaseStr.Seek(TableOffset, SeekOrigin.Begin);
            DataUtils.WriteBytes(BaseStr, TableBuffer, TableSize);
        }

        public static UInt32[] ByteArrayToUint32Array(byte[] ByteArray)
        {
            UInt32[] decode = new UInt32[ByteArray.Length / 4];
            System.Buffer.BlockCopy(ByteArray, 0, decode, 0, ByteArray.Length);
            return decode;
        }
        public static byte[] UInt32ArrayToByteArray(UInt32[] Uint32Array)
        {
            byte[] decode = new byte[Uint32Array.Length * 4];
            System.Buffer.BlockCopy(Uint32Array, 0, decode, 0, Uint32Array.Length * 4);
            return decode;
        }
        public static byte[] EncryptTable(byte[] table)
        {

            UInt32[] p = ByteArrayToUint32Array(table);
            UInt32 k0, k1, k2, k3;

            k0 = p[0] ^ p[1];
            k1 = p[1] ^ p[2];
            k2 = p[0] ^ p[3];
            k3 = p[2] ^ p[3];

            p[4] ^= k3;
            p[5] ^= k1;
            p[6] ^= k2;
            p[7] ^= k0;

            return UInt32ArrayToByteArray(p);
        }
        public static void BuildPbp(Stream Str, Stream Iso, bool Compress, byte[] VersionKey, Bitmap start_image, string content_id, byte[] param_sfo,byte[] icon0, byte[] icon1pmf, byte[] pic0, byte[] pic1, byte[] sndat3)
        {
            DoEvents = true;
            kirk_init();

            byte[] NewSfo = PatchSfo(param_sfo, content_id);
            byte[] StartData = BuildStartData(start_image);
            byte[] DataPsp = BuildDataPsp(StartData, content_id, NewSfo);
            byte[] DataPsar = BuildDataPsar();

            // Build Header
            int PbpHeaderSize = icon0.Length + icon1pmf.Length + pic0.Length + pic1.Length + sndat3.Length + param_sfo.Length + DataPsp.Length;
            Str.Seek(0x00, SeekOrigin.Begin);
            Str.SetLength(PbpHeaderSize + 4096);
            
            DataUtils.WriteInt32(Str, 0x50425000);
            DataUtils.WriteInt32(Str, 0x00010001);

            int OffsetToWrite = 0x28;
            // Write Sfo
            Str.Seek(0x08, SeekOrigin.Begin); // SFO Offset
            DataUtils.WriteInt32(Str, OffsetToWrite);
            Str.Seek(OffsetToWrite, SeekOrigin.Begin);
            Str.Write(NewSfo, 0x00, NewSfo.Length);
            OffsetToWrite += NewSfo.Length;

            //Write Icon0
            Str.Seek(0x0C, SeekOrigin.Begin); // Icon0 Offset
            DataUtils.WriteInt32(Str, OffsetToWrite);
            Str.Seek(OffsetToWrite, SeekOrigin.Begin);
            Str.Write(icon0, 0x00, icon0.Length);
            OffsetToWrite += icon0.Length;


            //Write Icon1
            Str.Seek(0x10, SeekOrigin.Begin); // Icon1 Offset
            DataUtils.WriteInt32(Str, OffsetToWrite);
            Str.Seek(OffsetToWrite, SeekOrigin.Begin);
            Str.Write(icon1pmf, 0x00, icon1pmf.Length);
            OffsetToWrite += icon1pmf.Length;

            //Write Pic0
            Str.Seek(0x14, SeekOrigin.Begin); // Pic0 Offset
            DataUtils.WriteInt32(Str, OffsetToWrite);
            Str.Seek(OffsetToWrite, SeekOrigin.Begin);
            Str.Write(pic0, 0x00, pic0.Length);
            OffsetToWrite += pic0.Length;
            
            //Write Pic1
            Str.Seek(0x18, SeekOrigin.Begin); // Pic0 Offset
            DataUtils.WriteInt32(Str, OffsetToWrite);
            Str.Seek(OffsetToWrite, SeekOrigin.Begin);
            Str.Write(pic1, 0x00, pic1.Length);
            OffsetToWrite += pic1.Length;

            //Write SND0
            Str.Seek(0x1C, SeekOrigin.Begin); // SND0 Offset
            DataUtils.WriteInt32(Str, OffsetToWrite);
            Str.Seek(OffsetToWrite, SeekOrigin.Begin);
            Str.Write(sndat3, 0x00, sndat3.Length);
            OffsetToWrite += sndat3.Length;

            //Write DATAPSP
            Str.Seek(0x20, SeekOrigin.Begin); // DATAPSP Offset
            DataUtils.WriteInt32(Str, OffsetToWrite);
            Str.Seek(OffsetToWrite, SeekOrigin.Begin);
            Str.Write(DataPsp, 0x00, DataPsp.Length);
            OffsetToWrite += DataPsp.Length;

            // DATA.PSAR is 0x100 aligned.
            OffsetToWrite = (OffsetToWrite + 15) & ~15;
            while ((OffsetToWrite % 0x100) != 0)
                OffsetToWrite += 0x10;

            // Write DaTAPSAR
            Str.Seek(0x24, SeekOrigin.Begin); // DATAPSAR Offset
            DataUtils.WriteInt32(Str, OffsetToWrite);
            Str.Seek(OffsetToWrite, SeekOrigin.Begin);
            Str.Write(DataPsar, 0x00, DataPsar.Length);
            OffsetToWrite += DataPsar.Length;


            // Sign ISO Contents.
            SignIso(OffsetToWrite, Str, Iso, content_id, VersionKey, Compress);

            NumberOfSectors = 0;
            SectorsDone = 0;
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
