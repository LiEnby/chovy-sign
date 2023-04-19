using Li.Progress;
using GameBuilder.Pops;
using GameBuilder.Psp;
using GameBuilder.VersionKey;
using PspCrypto;
using LibChovy;

namespace ChovySign_CLI
{
    internal class Program
    {
        private static ArgumentParsingMode mode = ArgumentParsingMode.ARG;
        private static List<string> parameters = new List<string>();
        private static string[] discs;
        private static bool pspCompress = false;
        private static bool devKit = false;
        private static string? popsDiscName;
        private static string? popsIcon0File;
        private static string? popsPic0File;
        private static PbpMode? pbpMode = null;
        private static NpDrmRif? rifFile = null;
        private static NpDrmInfo? drmInfo = null;
        enum PbpMode
        {
            PSP = 0,
            POPS = 1,
            PCENGINE = 2,
            NEOGEO = 3
        }
        enum ArgumentParsingMode
        {
            ARG = 0,
            POPS_DISC = 1,
            PSP_UMD = 2,
            VERSIONKEY = 3,
            VERSIONKEY_EXTRACT = 4,
            VERSIONKEY_GENERATOR = 5,
            POPS_INFO = 6,
            RIF = 7
        }
        public static int Error(string errorMsg, int ret)
        {
            ConsoleColor prevColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine("ERROR: "+errorMsg);
            Console.ForegroundColor = prevColor;

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

        private static int complete()
        {
            switch (mode)
            {
                case ArgumentParsingMode.POPS_DISC:
                    if (parameters.Count > 5) return Error("--pops: no more than 5 disc images allowed in a single game (sony's rules, not mine)", 5);
                    if (parameters.Count < 1) return Error("--pops: at least 1 disc image file is required.", 5);
                    discs = parameters.ToArray();
                    break;
                case ArgumentParsingMode.PSP_UMD:
                    if (parameters.Count < 1) return Error("--psp: a path to a disc image is required", 5);
                    if (parameters.Count > 2) return Error("--psp: no more than 2 arguments. ("+parameters.Count+" given)", 5);
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
                    if (parameters.Count != 1) return Error("--vkey-extract: expect 1 arguments. ("+parameters.Count+" given)", 4);
                    drmInfo = EbootPbpMethod.GetVersionKey(File.OpenRead(parameters[0]));
                    break;
                case ArgumentParsingMode.VERSIONKEY_GENERATOR:
                    if(parameters.Count != 4) return Error("--vkey-gen: expect 4 arguments. ("+parameters.Count+" given)", 4);
                    drmInfo = ActRifMethod.GetVersionKey(File.ReadAllBytes(parameters[0]), File.ReadAllBytes(parameters[1]), StringToByteArray(parameters[2]), int.Parse(parameters[3]));
                    break;
                case ArgumentParsingMode.POPS_INFO:
                    if (parameters.Count < 2) return Error("--pops-info takes at least 1 arguments ("+parameters.Count+" given)", 4);
                    if (parameters.Count > 3) return Error("--pops-info takes no more than 3 arguments("+parameters.Count+" given)", 4);
                    popsDiscName = parameters[0];
                    if (parameters.Count > 1)
                        popsIcon0File = parameters[1];
                    if (parameters.Count > 2) 
                        popsPic0File = parameters[2];
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
        public static int Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("Chovy-Sign v2 (CLI)");
                Console.WriteLine("--pops [disc1.cue] [disc2.cue] [disc3.cue] ... (up to 5)");
                Console.WriteLine("--pops-info [game title] [icon0.png] (optional) [pic1.png] (optional)");
                Console.WriteLine("--psp [umd.iso] [compress; true/false] (optional)");
                Console.WriteLine("--rif [GAME.RIF]");
                Console.WriteLine("--devkit (Use 000000000000 account id)");
                Console.WriteLine("--vkey [versionkey] [contentid] [key_index]");
                Console.WriteLine("--vkey-extract [eboot.pbp]");
                Console.WriteLine("--vkey-gen [act.dat] [license.rif] [console_id] [key_index]");
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
                            case "--rif":
                                mode = ArgumentParsingMode.RIF;

                                if (rifFile is not null)
                                    return Error("rif is already set", 3);

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
                    case ArgumentParsingMode.POPS_INFO:
                    case ArgumentParsingMode.RIF:
                    default:
                        parameters.Add(arg);
                        break;
                }
            }
            int res = complete();
            if(res != 0) return res;

            if (drmInfo is null) return Error("no versionkey was found, exiting", 6);

            Console.WriteLine("Version Key: " + BitConverter.ToString(drmInfo.VersionKey).Replace("-", "") + ", " + drmInfo.KeyIndex);

            if (pbpMode is null) return Error("no pbp mode was set, exiting", 7);
            
            if (pbpMode == PbpMode.PSP && drmInfo.KeyIndex != 2)
                return Error("KeyType is "+drmInfo.KeyIndex+", but PBP mode is PSP, you cant do that .. please use a type 1 versionkey.", 8);

            if (pbpMode == PbpMode.POPS && drmInfo.KeyIndex != 1)
                return Error("KeyType is " + drmInfo.KeyIndex + ", but PBP mode is POPS, you cant do that .. please use a type 1 versionkey.", 8);

            if (rifFile is null)
                return Error("Rif is not set, use --rif to specify base game RIF", 8);
            //if (pbpMode == PbpMode.POPS && (popsDiscName is null || popsIcon0File is null)) return Error("pbp mode is POPS, but you have not specified a disc title or icon file using --pops-info.", 9);
            ChovySign csign = new ChovySign();
            csign.RegisterCallback(onProgress);
            if (pbpMode == PbpMode.POPS)
            {
                PopsParameters popsParameters = new PopsParameters(drmInfo, rifFile);
                
                foreach (string disc in discs)
                    popsParameters.AddCd(disc);
    
                if(popsDiscName is not null)
                    popsParameters.Name = popsDiscName;
                
                if(File.Exists(popsIcon0File))
                    popsParameters.Icon0 = File.ReadAllBytes(popsIcon0File);

                popsParameters.Account.Devkit = devKit;

                csign.Go(popsParameters);
                
            }
            else if(pbpMode == PbpMode.PSP)
            {
                PspParameters pspParameters = new PspParameters(drmInfo, rifFile);
                pspParameters.Account.Devkit = devKit;
                pspParameters.Compress = pspCompress;
                pspParameters.Umd = new UmdInfo(discs.First());
                
                csign.Go(pspParameters);
            }
            return 0;
        }
    }
}