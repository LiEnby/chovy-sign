using GameBuilder.Pops;
using GameBuilder.Progress;
using GameBuilder.Psp;
using GameBuilder.VersionKey;

namespace ChovySign_CLI
{
    internal class Program
    {
        private static ArgumentParsingMode mode = ArgumentParsingMode.ARG;
        private static List<string> parameters = new List<string>();
        private static string[] discs;
        private static bool pspCompress = false;
        private static string? popsDiscName;
        private static string? popsIcon0File;
        private static string? popsPic0File;
        private static PbpMode? pbpMode = null;
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
            POPS_INFO = 6
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
            Console.Write(info.CurrentProcess + " " + info.ProgressInt.ToString() + "% (" + info.Done + "/" + info.Remain + ") \r");
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
                    if (parameters.Count < 2) return Error("--pops-info takes at least 2 arguments ("+parameters.Count+" given)", 4);
                    if (parameters.Count > 3) return Error("--pops-info takes no more than 3 arguments("+parameters.Count+" given)", 4);
                    popsDiscName = parameters[0];
                    popsIcon0File = parameters[1];
                    if (parameters.Count > 2) 
                        popsPic0File = parameters[2];
                    break;
            }

            mode = ArgumentParsingMode.ARG;
            parameters.Clear();
            return 0;
        }
        public static int Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("Chovy-Sign v2 (CLI)");
                Console.WriteLine("--pops [disc1.cue] [disc2.cue] [disc3.cue] ... (up to 5)");
                Console.WriteLine("--pops-info [game title] [icon0.png] [pic1.png] (optional)");
                Console.WriteLine("--psp [umd.iso] [compress; true/false] (optional)");
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
                    default:
                        parameters.Add(arg);
                        break;
                }
            }
            int res = complete();
            if(res != 0) return res;

            if (drmInfo is null) return Error("no versionkey was found, exiting", 6);
            if (pbpMode is null) return Error("no pbp mode was set, exiting", 7);

            if (pbpMode == PbpMode.PSP && drmInfo.KeyIndex != 2)
                return Error("KeyType is "+drmInfo.KeyIndex+", but PBP mode is PSP, you cant do that .. please use a type 1 versionkey.", 8);

            if (pbpMode == PbpMode.POPS && drmInfo.KeyIndex != 1)
                return Error("KeyType is " + drmInfo.KeyIndex + ", but PBP mode is POPS, you cant do that .. please use a type 1 versionkey.", 8);

            if (pbpMode == PbpMode.POPS && (popsDiscName is null || popsIcon0File is null)) return Error("pbp mode is POPS, but you have not specified a disc title or icon file using --pops-info.", 9);

            if (pbpMode == PbpMode.POPS)
            {

                DiscInfo[] discInfs = new DiscInfo[discs.Length];
                for (int i = 0; i < discInfs.Length; i++)
                    discInfs[i] = new DiscInfo(discs[i], popsDiscName);

                Sfo psfo = new Sfo();
                psfo.AddKey("BOOTABLE", 1, 4);
                psfo.AddKey("CATEGORY", "ME", 4);
                psfo.AddKey("DISC_ID", discInfs.First().DiscId, 16);
                psfo.AddKey("DISC_VERSION", "1.00", 8);
                psfo.AddKey("LICENSE", "Chovy-Sign is licensed under the GPLv3, POPS stuff was done by Li and SquallATF.", 512);
                psfo.AddKey("PARENTAL_LEVEL", 0, 4);
                psfo.AddKey("PSP_SYSTEM_VER", "6.60", 8);
                psfo.AddKey("REGION", 32768, 4);
                psfo.AddKey("TITLE", popsDiscName, 128);


                if (discs.Length == 1)
                {
                    using (PsIsoImg psIsoImg = new PsIsoImg(drmInfo, discInfs.First()))
                    {
                        psIsoImg.RegisterCallback(onProgress);
                        psIsoImg.CreatePsar();

                        PbpBuilder.CreatePbp(psfo.WriteSfo(),
                            File.ReadAllBytes(popsIcon0File),
                            null, 
                            (popsPic0File is not null) ? File.ReadAllBytes(popsPic0File) : null, 
                            Resources.PIC1,
                            null,
                            psIsoImg, 
                            "EBOOT.PBP", 
                            0);
                    }
                }
                else
                {
                    using (PsTitleImg psIsoImg = new PsTitleImg(drmInfo, discInfs))
                    {
                        psIsoImg.RegisterCallback(onProgress);
                        psIsoImg.CreatePsar();

                        PbpBuilder.CreatePbp(psfo.WriteSfo(),
                            File.ReadAllBytes(popsIcon0File),
                            null,
                            (popsPic0File is not null) ? File.ReadAllBytes(popsPic0File) : null,
                            Resources.PIC1,
                            null,
                            psIsoImg,
                            "EBOOT.PBP",
                            0);
                    }
                }
            }
            else if(pbpMode == PbpMode.PSP)
            {
                using (UmdInfo umd = new UmdInfo(discs.First()))
                {
                    using (NpUmdImg npUmd = new NpUmdImg(drmInfo, umd, pspCompress))
                    {
                        npUmd.RegisterCallback(onProgress);
                        npUmd.CreatePsar();

                        PbpBuilder.CreatePbp(umd.DataFiles["PARAM.SFO"], 
                            umd.DataFiles["ICON0.PNG"],
                            umd.DataFiles["ICON1.PMF"], 
                            umd.DataFiles["PIC0.PNG"], 
                            umd.DataFiles["PIC1.PNG"], 
                            umd.DataFiles["SND0.AT3"], 
                            npUmd, 
                            "EBOOT.PBP", 
                            1);

                    }
                }
            }
            return 0;
        }
    }
}