﻿using Li.Progress;
using GameBuilder.Pops;
using GameBuilder.Psp;
using LibChovy;
using LibChovy.VersionKey;
using System.Text;
using Vita.ContentManager;
using PspCrypto;
using System.ComponentModel;

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

        // cma
        private static bool devKit = false;
        private static bool packagePsvImg = true;
        private static string? outputFolder = null; 

        // --vkey-gen
        private static byte[]? actDat = null;
        private static byte[]? idps = null;
        private static string? rifFolder = null;

        // --pops-eboot-sign
        private static byte[]? ebootElf = null;
        private static byte[]? configBin = null;

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
            
            CMA_DEVKIT,
            CMA_OUTPUT_FOLDER,
            CMA_PACKAGE_PSVIMG,

            POPS_INFO,
            POPS_EBOOT,

            KEYS_TXT_GEN,
            RIF
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

        private static void generateKeysTxt()
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
                    break;
                case ArgumentParsingMode.PSP_UMD:
                    if (parameters.Count < 1) return Error("--psp: a path to a disc image is required", 4);
                    if (parameters.Count > 2) return Error("--psp: no more than 2 arguments. ("+parameters.Count+" given)", 4);
                    discs = new string[1];
                    discs[0] = parameters[0];
                    
                    if (parameters.Count > 1)
                        pspCompress = parameters[1].ToLowerInvariant() == "true";
                    else
                        pspCompress = false;

                    break;
                case ArgumentParsingMode.VERSIONKEY:
                    if (parameters.Count != 3) return Error("--vkey: expect 3 arguments. ("+parameters.Count+" given)", 4);
                    drmInfo = new NpDrmInfo(StringToByteArray(parameters[0]), parameters[1], int.Parse(parameters[2]));
                    break;
                case ArgumentParsingMode.VERSIONKEY_EXTRACT:
                    if (parameters.Count != 2) return Error("--vkey-extract: expect 2 arguments. ("+parameters.Count+" given)", 4);
                    drmInfo = EbootPbpMethod.GetVersionKey(File.OpenRead(parameters[0]), int.Parse(parameters[1]));
                    break;
                case ArgumentParsingMode.VERSIONKEY_GENERATOR:
                    if(parameters.Count != 4) return Error("--vkey-gen: expect 4 arguments. ("+parameters.Count+" given)", 4);
                    drmInfo = ActRifMethod.GetVersionKey(File.ReadAllBytes(parameters[0]), File.ReadAllBytes(parameters[1]), StringToByteArray(parameters[2]), int.Parse(parameters[3]));
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
                    actDat = File.ReadAllBytes(parameters[0]);
                    idps = StringToByteArray(parameters[1]);
                    rifFolder = parameters[2];
                    break;
                case ArgumentParsingMode.POPS_EBOOT:
                    if (parameters.Count < 1) return Error("--pops-eboot-sign expects at most 1 arguments", 4);
                    if (!File.Exists(parameters[0])) return Error("--pops-eboot-sign: file not found", 4);
                    ebootElf = File.ReadAllBytes(parameters[0]);

                    if (parameters.Count >= 2 && File.Exists(parameters[1]))
                        configBin = File.ReadAllBytes(parameters[1]);
                    else
                        configBin = GameBuilder.Resources.DATAPSPSDCFG;
                    
                    break;
                case ArgumentParsingMode.CMA_OUTPUT_FOLDER:
                    if (parameters.Count < 1) return Error("--output-folder expects 1 output", 4);
                    if (!Directory.Exists(parameters[0])) return Error("--output-folder: directory not found", 4);
                    
                    SettingsReader.BackupsFolder = parameters[0];
                    break;
                case ArgumentParsingMode.RIF:
                    if (parameters.Count != 1) return Error("--rif expects only 1 argument,", 4);
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
                Console.WriteLine("--pops [disc1.cue] [disc2.cue] [disc3.cue] ... (up to 5)");
                Console.WriteLine("--pops-info [game title] [icon0.png] (optional) [pic1.png] (optional)");
                Console.WriteLine("--pops-eboot [eboot.elf] [config.bin] (optional)");

                Console.WriteLine("--psp [umd.iso] [compress; true/false] (optional)");
                
                Console.WriteLine("--rif [GAME.RIF]");
                
                Console.WriteLine("--devkit (Use 000000000000 account id)");
                Console.WriteLine("--no-psvimg (Disable creating a .psvimg file)");
                Console.WriteLine("--output-folder [output_folder]");

                Console.WriteLine("--vkey [versionkey] [contentid] [key_index]");
                Console.WriteLine("--vkey-extract [eboot.pbp] [key_index]");
                Console.WriteLine("--vkey-gen [act.dat] [license.rif] [console_id] [key_index]");
                
                Console.WriteLine("--keys-txt-gen [act.dat] [console_id] [psp_license_folder]");
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

                            case "--devkit":
                                devKit = true;
                                break;
                            default:
                                return Error("Unknown argument: " + arg, 1);
                        }
                        break;
                    case ArgumentParsingMode.VERSIONKEY:
                    case ArgumentParsingMode.VERSIONKEY_GENERATOR:
                    case ArgumentParsingMode.VERSIONKEY_EXTRACT:
                    case ArgumentParsingMode.PSP_UMD:
                    case ArgumentParsingMode.POPS_DISC:
                    case ArgumentParsingMode.POPS_EBOOT:
                    case ArgumentParsingMode.POPS_INFO:
                    case ArgumentParsingMode.RIF:
                    default:
                        parameters.Add(arg);
                        break;
                }
            }
            int res = complete();
            if(res != 0) return res;

            generateKeysTxt();

            if (drmInfo is null) return Error("no versionkey was found, exiting", 6);
            if (pbpMode is null) return Error("no pbp mode was set, exiting", 7);

            int targetKeyIndex = (pbpMode == PbpMode.PSP) ? 2 : 1;
            if (drmInfo.KeyIndex != targetKeyIndex)
            {
                SceNpDrm.sceNpDrmTransformVersionKey(drmInfo.VersionKey, drmInfo.KeyIndex, 2);
                drmInfo.KeyIndex = targetKeyIndex;
            }

            if (rifFile is null) return Error("Rif is not set, use --rif to specify base game RIF", 8);
            
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
                

                popsParameters.CreatePsvImg = packagePsvImg;
                popsParameters.Account.Devkit = devKit;

                // Allow for custom eboot.elf and configs
                popsParameters.ConfigBinOverride = configBin;
                popsParameters.EbootElfOverride = ebootElf;

                csign.Go(popsParameters);
                
            }
            else if(pbpMode == PbpMode.PSP)
            {
                PspParameters pspParameters = new PspParameters(drmInfo, rifFile);
                pspParameters.Account.Devkit = devKit;
                pspParameters.CreatePsvImg = packagePsvImg;

                pspParameters.Compress = pspCompress;
                pspParameters.Umd = new UmdInfo(discs.First());
                
                csign.Go(pspParameters);
            }
            return 0;
        }
    }
}