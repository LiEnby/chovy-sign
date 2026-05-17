using GameBuilder.Pops;
using GameBuilder.Psp;
using Li.Progress;
using LibChovy;
using LibChovy.VersionKey;
using PspCrypto;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Vita.ContentManager;

namespace ChovySign_CLI
{
    internal class Program
    {
        private static ArgumentParsingMode mode = ArgumentParsingMode.ARG;
        private static List<string> parameters = new List<string>();
        private static string[] discs = new string[] { };
        private static bool pspCompress = false;
        private static string? popsDiscName;
        private static byte[]? popsIcon0File;
        private static byte[]? popsPic0File;
        private static PbpMode? pbpMode = null;
        private static NpDrmRif? rifFile = null;
        private static NpDrmInfo? drmInfo = null;

        // --simple-dat
        private static string? simpleDat = null;

        // --start-dat
        private static string? startDat = null;

        // cma
        private static bool packagePsvImg = true;
        private static string? outputFolder = null; 

        // --vkey-gen
        private static byte[]? actDat = null;
        private static byte[]? idps = null;
        private static string? rifFolder = null;

        // --pops-eboot-sign
        private static byte[]? ebootElf = null;
        private static byte[]? configBin = null;

        // --account-id
        private static UInt64? accountId = null;

        enum PbpMode
        {
            PSP = 0,
            POPS = 1,
            PCENGINE = 2,
            NEOGEO = 3
        }
        enum ArgumentParsingMode
        {
            ARG,
            POPS_DISC,
            PSP_UMD,
            
            VERSIONKEY,
            VERSIONKEY_EXTRACT,
            VERSIONKEY_GENERATOR,
            NOPSPEMUDRM_GEN,
            
            CMA_DEVKIT,
            CMA_OUTPUT_FOLDER,
            CMA_PACKAGE_PSVIMG,

            POPS_INFO,
            POPS_EBOOT,

            ACCOUNT_ID,

            KEYS_TXT_GEN,
            RIF,

            STARTDAT_FILEPATH,
            SIMPLEDAT_FILEPATH
        }
        public static int Error(string errorMsg, int ret)
        {
            Console.Error.WriteLine("ERROR: "+errorMsg);
            return ret;
        }
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
        }

        private static void onProgress(ProgressInfo info)
        {
            string msg = info.CurrentProcess + " " + info.ProgressInt.ToString() + "% (" + info.Done + "/" + info.Remain + ")";
            int spaceLen = (Console.WindowWidth - msg.Length) - 2;
            string emptySpace = " ";
            for (int i = 0; i < spaceLen; i++) 
                emptySpace += " ";
            Console.Write(msg + emptySpace + "\r");
        }

        private static void tryGenerateKeysTxt()
        {
            if (rifFolder is null || actDat is null || idps is null) return;
            UInt64 accountId = BitConverter.ToUInt64(actDat, 0x8);

            if (File.Exists("KEYS.TXT"))
                KeysTxtMethod.KeysTxt = File.ReadAllText("KEYS.TXT");
            else
                File.WriteAllText("KEYS.TXT", KeysTxtMethod.KeysTxt);

            HashSet<string> knownKeys = new HashSet<string>();
            foreach (string contentId in KeysTxtMethod.ContentIds)
                knownKeys.Add(contentId);

            StringBuilder addKeys = new StringBuilder();
            foreach (string rifFile in Directory.GetFiles(rifFolder, "*.rif"))
            {
                NpDrmRif rif = new NpDrmRif(File.ReadAllBytes(rifFile));
                if (knownKeys.Contains(rif.ContentId)) continue;

                if(rif.AccountId != accountId) { Error(rif.ContentId + " account id does not match: " + accountId.ToString("X") + " (was " + rif.AccountId.ToString("X") + ")", 10); continue; }
                string[] keys = new string[4];
                
                for (int i = 0; i < keys.Length; i++)
                    keys[i] = BitConverter.ToString(ActRifMethod.GetVersionKey(actDat, rif.Rif, idps, i).VersionKey).Replace("-", "");

                string[] keysTxtLine = new string[] { rif.ContentId, keys[0], keys[1], keys[2], keys[3] };
                string keysTxt = String.Join(' ', keysTxtLine);

                addKeys.AppendLine(keysTxt);
                //Console.WriteLine(keysTxt);
            }
            File.AppendAllText("KEYS.TXT", addKeys.ToString());
        }

        private static int complete()
        {
            switch (mode)
            {
                case ArgumentParsingMode.POPS_DISC:
                    if (parameters.Count > 5) return Error("--pops: no more than 5 disc images allowed in a single game (sony's rules, not mine)", 4);
                    if (parameters.Count < 1) return Error("--pops: at least 1 disc image file is required.", 4);
                    discs = parameters.ToArray();
                    
                    foreach(string disc in discs) 
                        if(!Path.Exists(disc)) 
                            return Error("--pops: file not found", 4);

                    break;
                case ArgumentParsingMode.PSP_UMD:
                    if (parameters.Count < 1) return Error("--psp: a path to a disc image is required", 4);
                    if (parameters.Count > 2) return Error("--psp: no more than 2 arguments. ("+parameters.Count+" given)", 4);
                    if (!File.Exists(parameters[0])) return Error("--psp: file not found", 4);

                    discs = new string[1];
                    discs[0] = parameters[0];
                    
                    if (parameters.Count > 1)
                        pspCompress = parameters[1].ToLowerInvariant() == "true";
                    else
                        pspCompress = false;

                    break;
                case ArgumentParsingMode.VERSIONKEY:
                    if (parameters.Count > 3) return Error("--vkey: takes no more than 3 arguments. (" + parameters.Count+" given)", 4);
                    if (parameters.Count < 2) return Error("--vkey: takes atleast 2 arguments. (" + parameters.Count + " given)", 4);

                    int keyIndex = 0;
                    if (parameters.Count >= 3)
                        keyIndex = int.Parse(parameters[2]);

                    drmInfo = new NpDrmInfo(StringToByteArray(parameters[0]), parameters[1], keyIndex);
                    break;
                case ArgumentParsingMode.VERSIONKEY_EXTRACT:
                    if (parameters.Count > 2) return Error("--vkey-extract: takes no more than 2 arguments. (" + parameters.Count+" given)", 4);
                    if (parameters.Count < 1) return Error("--vkey-extract: takes at least 1 argument. (" + parameters.Count + " given)", 4);
                    if (!File.Exists(parameters[0])) return Error("--vkey-extract: file not found", 4);

                    keyIndex = 0;
                    if (parameters.Count >= 2)
                        keyIndex = int.Parse(parameters[1]);

                    drmInfo = EbootPbpMethod.GetVersionKey(File.OpenRead(parameters[0]), keyIndex);
                    break;
                case ArgumentParsingMode.VERSIONKEY_GENERATOR:
                    if(parameters.Count > 4) return Error("--vkey-gen: takes no more than 4 arguments. (" + parameters.Count+" given)", 4);
                    if (parameters.Count < 3) return Error("--vkey-gen: takes at least 3 arguments. (" + parameters.Count + " given)", 4);
                    if (!File.Exists(parameters[0])) return Error("--vkey-gen: act file not found", 4);
                    if (!File.Exists(parameters[1])) return Error("--vkey-gen: rif file not found", 4);

                    keyIndex = 0;
                    if(parameters.Count >= 4)
                        keyIndex = int.Parse(parameters[3]);

                    drmInfo = ActRifMethod.GetVersionKey(File.ReadAllBytes(parameters[0]), 
                                                         File.ReadAllBytes(parameters[1]), 
                                                         StringToByteArray(parameters[2]), 
                                                         keyIndex);
                    break;
                case ArgumentParsingMode.NOPSPEMUDRM_GEN:
                    if (parameters.Count > 1) return Error("--nopspemudrm: takes no more than 1 arguments. (" + parameters.Count + " given)", 4);
                    string contentId = "EP0099-ULUS09999_00-CHOVYSIGN0000000";

                    if(parameters.Count >= 1) contentId = parameters[0];
                    if (Regex.Matches(contentId, "^[A-Z]{2}[0-9]{4}-[A-Z]{4}[0-9]{5}_[0-9]{2}-[A-Z0-9]{16}$").Count <= 0) return Error("Content ID does not match the format XXYYYY-XXXXYYYYY_YY-ZZZZZZZZZZZZZZZZ\nWhere, X = A-Z, Y = 0-9, and Z = A-Z or 0-9.", 5);
                    
                    drmInfo = NoPspEmuDrmMethod.GetVersionKey(contentId, 0);
                    rifFile = NpDrmRif.CreateNoPspEmuDrmRif(contentId);

                    break;
                case ArgumentParsingMode.POPS_INFO:
                    if (parameters.Count < 2) return Error("--pops-info takes at least 1 arguments ("+parameters.Count+" given)", 4);
                    if (parameters.Count > 3) return Error("--pops-info takes no more than 3 arguments("+parameters.Count+" given)", 4);
                    popsDiscName = parameters[0];

                    if (parameters.Count > 1 && File.Exists(parameters[1]))
                        popsIcon0File = File.ReadAllBytes(parameters[1]);
                    if (parameters.Count > 2 && File.Exists(parameters[2])) 
                        popsPic0File = File.ReadAllBytes(parameters[2]);
                    break;
                case ArgumentParsingMode.KEYS_TXT_GEN:
                    if (parameters.Count != 3) return Error("--keys-txt-gen takes 3 arguments, (" + parameters.Count + " given)", 4);
                    if (!File.Exists(parameters[0])) return Error("--keys-txt-gen: act file not found", 4);
                    if (!Directory.Exists(parameters[2])) return Error("--keys-txt-gen: rif folder not found", 4);

                    actDat = File.ReadAllBytes(parameters[0]);
                    idps = StringToByteArray(parameters[1]);
                    rifFolder = parameters[2];
                    break;
                case ArgumentParsingMode.STARTDAT_FILEPATH:
                    if (parameters.Count != 1) return Error("--start-dat takes 1 arguments, (" + parameters.Count + " given)", 4);
                    if (!File.Exists(parameters[0])) return Error("--start-dat: file not found", 4);

                    startDat = parameters[0];
                    break;
                case ArgumentParsingMode.SIMPLEDAT_FILEPATH:
                    if (parameters.Count != 1) return Error("--simple-dat takes 1 arguments, (" + parameters.Count + " given)", 4);
                    if (!File.Exists(parameters[0])) return Error("--simple-dat: file not found", 4);

                    simpleDat = parameters[0];
                    break;
                case ArgumentParsingMode.POPS_EBOOT:
                    if (parameters.Count < 1) return Error("--pops-eboot takes at most 1 arguments, (" + parameters.Count + " given)", 4);
                    if (!File.Exists(parameters[0])) return Error("--pops-eboot: file not found", 4);
                    ebootElf = File.ReadAllBytes(parameters[0]);

                    if (parameters.Count >= 2 && File.Exists(parameters[1]))
                        configBin = File.ReadAllBytes(parameters[1]);
                    
                    break;
                case ArgumentParsingMode.CMA_OUTPUT_FOLDER:
                    if (parameters.Count < 1) return Error("--output-folder takes 1 arguments, (" + parameters.Count + " given)", 4);
                    if (!Directory.Exists(parameters[0])) return Error("--output-folder: directory not found", 4);

                    outputFolder = parameters[0];
                    break;
                case ArgumentParsingMode.ACCOUNT_ID:
                    if (parameters.Count < 1) return Error("--account-id takes onlnt 1 output, (" + parameters.Count + " given)", 4);

                    accountId = UInt64.Parse(parameters[0], NumberStyles.HexNumber);
                    break;
                case ArgumentParsingMode.RIF:
                    if (parameters.Count != 1) return Error("--rif takes only 1 argument, (" + parameters.Count + " given)", 4);
                    rifFile = new NpDrmRif(File.ReadAllBytes(parameters[0]));
                    break;
            }

            mode = ArgumentParsingMode.ARG;
            parameters.Clear();
            return 0;
        }
        /*
        public static void generateRif(byte[] idps, byte[] actBuf, byte[] versionKey, int versionKeyType, ulong accountId, string contentId)
        {
            byte[] vkey2 = new byte[versionKey.Length];
            Array.Copy(versionKey, vkey2, versionKey.Length);

            byte[] rkey = Rng.RandomBytes(0x10);
            int keyId = 0x10; // (Int32)(Rng.RandomUInt() % 0x80);
            Array.ConstrainedCopy(BitConverter.GetBytes(keyId).Reverse().ToArray(), 0, rkey, 0xC, 0x4);

            byte[] encKey1 = new byte[0x10];
            AesHelper.AesEncrypt(rkey, encKey1, KeyVault.drmRifKey);

            // get the act key
            byte[] actKey = new byte[0x10];
            
            SceNpDrm.SetPSID(idps);
            SceNpDrm.Aid = accountId;

            Act act = MemoryMarshal.AsRef<Act>(actBuf);
            GetActKey(actKey, act.PrimKeyTable[(keyId * 0x10)..], 1);

            // reverse version key back to main version key
            sceNpDrmTransformVersionKey(vkey2, versionKeyType, 0);

            byte[] encKey2 = new byte[0x10];
            AesHelper.AesEncrypt(vkey2, encKey2, actKey);

            using (MemoryStream rifStream = new MemoryStream())
            {
                StreamUtil rifUtil = new StreamUtil(rifStream);

                rifUtil.WriteInt16(0x0);
                rifUtil.WriteInt16(0x1);

                rifUtil.WriteInt32(0x2);
                rifUtil.WriteUInt64(accountId);

                rifUtil.WriteStrWithPadding(contentId, 0x00, 0x30);

                rifUtil.WriteBytes(encKey1); // enckey1
                rifUtil.WriteBytes(encKey2); // enckey2

                rifUtil.WriteUInt64(SceRtc.ksceRtcGetCurrentSecureTick());
                rifUtil.WriteUInt64(0x00); // expiry

                rifUtil.WritePadding(0xFF, 0x28);
            }
        }
        */
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Chovy-Sign v2 (CLI)");

                Console.WriteLine("PS1 Options: ");
                Console.WriteLine("\t--pops [disc1.cue] [disc2.cue] [disc3.cue] ... (up to 5) - Specifiy PS1 Disc Images");
                Console.WriteLine("\t--pops-info [game title] [icon0.png] (optional) [pic1.png] (optional) - Specify PS1 Game information");
                Console.WriteLine("\t--pops-eboot [eboot.elf] [config.bin] (optional) - Override PS1 simple.prx and config.bin data");

                Console.WriteLine("PSP Options: ");
                Console.WriteLine("\t--psp [umd.iso] [compress; true/false] (optional) - Specify PSP Disc Image");

                Console.WriteLine("Vita CMA Options: ");
                Console.WriteLine("\t--account-id [account_id] - Override CMA account id");
                Console.WriteLine("\t--no-psvimg - Don't create a .psvimg CMA backup");
                Console.WriteLine("\t--output-folder [output_folder] - Override CMA Backups Directory");

                Console.WriteLine("NpDrm Options: ");
                Console.WriteLine("\t--rif [license.rif] - Specify Base Game RIF");
                Console.WriteLine("\t--vkey [versionkey] [content_id] [key_index] (optional) - Manually specify versionkey, contentid, and keyindex");
                Console.WriteLine("\t--vkey-extract [eboot.pbp] [key_index] (optional) - Extract versionkey from an eboot.pbp.");
                Console.WriteLine("\t--vkey-gen [act.dat] [license.rif] [console_id] [key_index] (optional) - Generate versionkey from act.dat, rif and consoleid.");
                Console.WriteLine("\t--nopspemudrm [content_id] (optional) - Dont use a base game, generate a nopspemudrm rif from a specified Content ID.");

                Console.WriteLine("Packaging: ");
                Console.WriteLine("\t--simple-dat [simple.png] - Override simple.dat (Copyright image), this option does nothing in --psp mode");
                Console.WriteLine("\t--start-dat [start.png] - Override start.dat (startup image)");


                //Console.WriteLine("Debug: ");
                //Console.WriteLine("\t--keys-txt-gen [act.dat] [console_id] [psp_license_folder] - Generate keys.txt from a folder of RIFs");

            }


            foreach (string arg in args)
            {
                if (arg.StartsWith("--")) { int ret = complete(); if (ret != 0) return ret; } 

                switch (mode)
                {
                    case ArgumentParsingMode.ARG:
                        switch (arg)
                        {
                            case "--pops":
                                mode = ArgumentParsingMode.POPS_DISC;

                                if (pbpMode is not null)
                                    return Error("pbpMode is already set to: " + pbpMode.ToString() + " cannot do that *and* POPS", 2);
                                
                                pbpMode = PbpMode.POPS;
                                break;
                            case "--pops-info":
                                mode = ArgumentParsingMode.POPS_INFO;
                                break;
                            case "--psp":
                                mode = ArgumentParsingMode.PSP_UMD;

                                if (pbpMode is not null)
                                    return Error("pbpMode is already set to: " + pbpMode.ToString() + " cannot do that *and* PSP", 2);

                                pbpMode = PbpMode.PSP;
                                break;

                            case "--vkey":
                                mode = ArgumentParsingMode.VERSIONKEY;

                                if (drmInfo is not null)
                                    return Error("versionkey is already set", 3);

                                break;
                            case "--vkey-extract":
                                mode = ArgumentParsingMode.VERSIONKEY_EXTRACT;

                                if (drmInfo is not null)
                                    return Error("versionkey is already set", 3);

                                break;
                            case "--vkey-gen":
                                mode = ArgumentParsingMode.VERSIONKEY_GENERATOR;

                                if (drmInfo is not null)
                                    return Error("versionkey is already set", 3);

                                break;
                            case "--nopspemudrm":
                                mode = ArgumentParsingMode.NOPSPEMUDRM_GEN;
                                
                                if (drmInfo is not null && rifFile is not null)
                                    return Error("versionkey or rif is already set", 3);

                                break;
                            case "--start-dat":
                                mode = ArgumentParsingMode.STARTDAT_FILEPATH;
                                break;
                            case "--simple-dat":
                                mode = ArgumentParsingMode.SIMPLEDAT_FILEPATH;
                                break;
                            case "--keys-txt-gen":
                                mode = ArgumentParsingMode.KEYS_TXT_GEN;

                                if (rifFolder is not null)
                                    return Error("rif folder already set", 3);
                                break;
                            case "--rif":
                                mode = ArgumentParsingMode.RIF;

                                if (rifFile is not null)
                                    return Error("rif is already set", 3);

                                break;
                            case "--pops-eboot":
                                mode = ArgumentParsingMode.POPS_EBOOT;
                                break;

                            case "--output-folder":
                                mode = ArgumentParsingMode.CMA_OUTPUT_FOLDER;
                                break;

                            case "--no-psvimg":
                                packagePsvImg = false;
                                break;

                            case "--account-id":
                                mode = ArgumentParsingMode.ACCOUNT_ID;
                                break;
                            default:
                                return Error("Unknown argument: " + arg, 1);
                        }
                        break;
                    case ArgumentParsingMode.ACCOUNT_ID:
                    case ArgumentParsingMode.VERSIONKEY:
                    case ArgumentParsingMode.VERSIONKEY_GENERATOR:
                    case ArgumentParsingMode.VERSIONKEY_EXTRACT:
                    case ArgumentParsingMode.PSP_UMD:
                    case ArgumentParsingMode.POPS_DISC:
                    case ArgumentParsingMode.POPS_EBOOT:
                    case ArgumentParsingMode.POPS_INFO:
                    case ArgumentParsingMode.NOPSPEMUDRM_GEN:
                    case ArgumentParsingMode.RIF:
                    case ArgumentParsingMode.SIMPLEDAT_FILEPATH:
                    case ArgumentParsingMode.STARTDAT_FILEPATH:
                    default:
                        parameters.Add(arg);
                        break;
                }
            }
            int res = complete();
            if(res != 0) return res;

            // debug feature, was used for generating keys.txt initally;
            // keys.txt method generally not needed anymore
            tryGenerateKeysTxt();

            if (pbpMode is null) return Error("no pbp mode was set, use either --psp or --pops", 7);
            if (drmInfo is null) return Error("no versionkey was found, exiting, use (one of) --vkey options; (or --nopspemudrm)", 6);
            if (rifFile is null) return Error("no rif was found, use --rif to specify base game RIF (or --nopspemudrm) ", 8);
            
            int targetKeyIndex = (pbpMode == PbpMode.PSP) ? 2 : 1;
            if (drmInfo.KeyIndex != targetKeyIndex)
            {
                SceNpDrm.sceNpDrmTransformVersionKey(drmInfo.VersionKey, drmInfo.KeyIndex, 2);
                drmInfo.KeyIndex = targetKeyIndex;
            }

            Console.WriteLine("Mode: " + pbpMode.ToString());
            Console.WriteLine("Using ContentID: " + drmInfo.ContentId);
            Console.WriteLine("Using KeyIndex: " + drmInfo.KeyIndex);
            Console.WriteLine("Using VersionKey: " + BitConverter.ToString(drmInfo.VersionKey).Replace("-", ""));
            
            ChovySign csign = new ChovySign();
            csign.RegisterCallback(onProgress);
            if (pbpMode == PbpMode.POPS)
            {
                PopsParameters popsParameters = new PopsParameters(drmInfo, rifFile);
                
                foreach (string disc in discs)
                    popsParameters.AddCd(disc);
    
                if(popsDiscName is not null)
                    popsParameters.Name = popsDiscName;
                
                if(popsIcon0File is not null)
                    popsParameters.Icon0 = popsIcon0File;

                if (popsPic0File is not null)
                    popsParameters.Pic0 = popsPic0File;

                if (accountId is not null)
                    popsParameters.Account = new Account(Convert.ToUInt64(accountId));

                if (outputFolder is not null)
                    popsParameters.OutputFolder = outputFolder;

                if (startDat is not null)
                    popsParameters.StartPngFilepath = startDat;

                if (simpleDat is not null)
                    popsParameters.SimplePngFilepath = simpleDat;

                popsParameters.CreatePsvImg = packagePsvImg;


                // Allow for custom eboot.elf and configs
                popsParameters.ConfigBinOverride = configBin;
                popsParameters.EbootElfOverride = ebootElf;

                csign.Go(popsParameters);
                
            }
            else if(pbpMode == PbpMode.PSP)
            {
                PspParameters pspParameters = new PspParameters(drmInfo, rifFile);
                
                if (accountId is not null)
                    pspParameters.Account = new Account(Convert.ToUInt64(accountId));

                if (outputFolder is not null)
                    pspParameters.OutputFolder = outputFolder;

                if (startDat is not null)
                    pspParameters.StartPngFilepath = startDat;

                pspParameters.CreatePsvImg = packagePsvImg;

                pspParameters.Compress = pspCompress;
                pspParameters.Umd = new UmdInfo(discs.First());
                
                csign.Go(pspParameters);
            }
            return 0;
        }
    }
}