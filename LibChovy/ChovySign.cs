using GameBuilder;
using GameBuilder.Pops;
using GameBuilder.Psp;
using Li.Progress;
using PspCrypto;
using Vita.PsvImgTools;

namespace LibChovy
{
    public class ChovySign : ProgressTracker
    {

        private void onProgress(ProgressInfo inf)
        {
            updateProgress(inf.Done, inf.Remain, inf.CurrentProcess);
        }

        private void createPSP(PspParameters parameters)
        {
            using(NpUmdImg img = new NpUmdImg(parameters.DrmInfo, parameters.Umd, parameters.Compress))
            {
                img.PatchSfo();
                using (PbpBuilder pbpBuilder = new PbpBuilder(parameters.Umd.DataFiles["PARAM.SFO"], parameters.Umd.DataFiles["ICON0.PNG"], parameters.Umd.DataFiles["ICON1.PMF"], parameters.Umd.DataFiles["PIC0.PNG"], parameters.Umd.DataFiles["PIC1.PNG"], parameters.Umd.DataFiles["SND0.AT3"], img, 1))
                {
                    pbpBuilder.RegisterCallback(onProgress);
                    pbpBuilder.CreatePsarAndPbp();
                    pbpBuilder.PbpStream.Seek(0x00, SeekOrigin.Begin);

                    byte[] ebootsig = new byte[0x200];
                    SceNpDrm.KsceNpDrmEbootSigGenPsp(pbpBuilder.PbpStream, ebootsig, parameters.FirmwareVersion);

                    if (!parameters.CreatePsvImg)
                    {
                        File.WriteAllBytes(Path.Combine(parameters.OutputFolder, "__sce_ebootpbp"), ebootsig.ToArray());
                        pbpBuilder.WritePbpToFile(Path.Combine(parameters.OutputFolder, "EBOOT.PBP"));
                    }
                    else
                    {
                        createPsvImg(parameters.OutputFolder, parameters.Umd.DiscId, parameters.Umd.DataFiles["PARAM.SFO"], parameters.Umd.DataFiles["ICON0.PNG"], pbpBuilder, parameters.DrmRif, ebootsig, parameters.Account.CmaKey);
                    }
                }
            }
        }

        private void createPOPS(PopsParameters parameters)
        {

            Sfo psfo = new Sfo();
            psfo.AddKey("BOOTABLE", 1, 4);
            psfo.AddKey("CATEGORY", "ME", 4);
            psfo.AddKey("DISC_ID", parameters.FirstDisc.DiscId, 16);
            psfo.AddKey("DISC_VERSION", "1.00", 8);
            psfo.AddKey("LICENSE", "Chovy-Sign is licensed under the GPLv3, And was made possible by SquallATF and Li.", 512);
            psfo.AddKey("PARENTAL_LEVEL", 0, 4);
            psfo.AddKey("PSP_SYSTEM_VER", "6.61", 8);
            psfo.AddKey("REGION", 32768, 4);
            psfo.AddKey("TITLE", parameters.Name, 128);
            byte[] sfo = psfo.WriteSfo();

            PopsImg img;
            if (parameters.MultiDisc)
                img = new PsTitleImg(parameters.DrmInfo, parameters.Discs);
            else
                img = new PsIsoImg(parameters.DrmInfo, parameters.FirstDisc);

            // apply eboot elf overrides
            if(parameters.EbootElfOverride is not null)
            {
                img.EbootElf = parameters.EbootElfOverride;
                img.PatchEboot = false;
            }
            if (parameters.ConfigBinOverride is not null)
            {
                img.ConfigBin = parameters.ConfigBinOverride;
            }

            using (PbpBuilder pbpBuilder = new PbpBuilder(sfo, parameters.Icon0, null, parameters.Pic0, parameters.Pic1, null, img, 0))
            {
                pbpBuilder.RegisterCallback(onProgress);
                pbpBuilder.CreatePsarAndPbp();
                pbpBuilder.PbpStream.Seek(0x00, SeekOrigin.Begin);

                byte[] ebootsig = new byte[0x200];
                SceNpDrm.KsceNpDrmEbootSigGenPs1(pbpBuilder.PbpStream, ebootsig, parameters.FirmwareVersion);

                if (!parameters.CreatePsvImg)
                {
                    File.WriteAllBytes(Path.Combine(parameters.OutputFolder, "__sce_ebootpbp"), ebootsig.ToArray());
                    pbpBuilder.WritePbpToFile(Path.Combine(parameters.OutputFolder, "EBOOT.PBP"));
                }
                else
                {
                    createPsvImg(parameters.OutputFolder, parameters.FirstDisc.DiscId, sfo, parameters.Icon0, pbpBuilder, parameters.DrmRif, ebootsig, parameters.Account.CmaKey);
                }
            }
        }

        private void createPsvImg(string outputFolder, string discId,
                                  byte[] sfo, byte[] icon0, 
                                  PbpBuilder pbp, NpDrmRif license, byte[] sig,
                                  byte[] cmaKey)
        {
            // create GAME.PSVIMG
            pbp.PbpStream.Seek(0x00, SeekOrigin.Begin);

            string gameFolder = Path.Combine(outputFolder, "game");
            string licenseFolder = Path.Combine(outputFolder, "license");
            string sceSysFolder = Path.Combine(outputFolder, "sce_sys");

            if (!Directory.Exists(gameFolder)) Directory.CreateDirectory(gameFolder);
            if (!Directory.Exists(licenseFolder)) Directory.CreateDirectory(licenseFolder);
            if (!Directory.Exists(sceSysFolder)) Directory.CreateDirectory(sceSysFolder);


            using (FileStream gamePsvimg = File.Open(Path.Combine(gameFolder, "game.psvimg"), FileMode.Create, FileAccess.ReadWrite))
            {
                PSVIMGBuilder psvImg = new PSVIMGBuilder(gamePsvimg, cmaKey);
                psvImg.RegisterCallback(onProgress);
                psvImg.AddFile(pbp.PbpStream, "ux0:pspemu/temp/game/PSP/GAME/" + discId, "/EBOOT.PBP");
                psvImg.AddFile(sig, "ux0:pspemu/temp/game/PSP/GAME/" + discId, "/__sce_ebootpbp");
                long contentSize = psvImg.Finish();

                // create GAME.PSVMD
                using (FileStream gamePsvmd = File.Open(Path.Combine(gameFolder, "game.psvmd"), FileMode.Create, FileAccess.ReadWrite))
                    PSVMDBuilder.CreatePsvmd(gamePsvmd, gamePsvimg, contentSize, "game", cmaKey);
            }

            // create LICENSE.PSVIMG
            using (FileStream gamePsvimg = File.Open(Path.Combine(licenseFolder, "license.psvimg"), FileMode.Create, FileAccess.ReadWrite))
            {
                PSVIMGBuilder psvImg = new PSVIMGBuilder(gamePsvimg, cmaKey);
                psvImg.RegisterCallback(onProgress);
                psvImg.AddFile(license.Rif, "ux0:pspemu/temp/game/PSP/LICENSE", "/" + license.ContentId + ".rif");
                long contentSize = psvImg.Finish();

                // create LICENSE.PSVMD
                using (FileStream gamePsvmd = File.Open(Path.Combine(licenseFolder, "license.psvmd"), FileMode.Create, FileAccess.ReadWrite))
                    PSVMDBuilder.CreatePsvmd(gamePsvmd, gamePsvimg, contentSize, "license", cmaKey);

            }

            // write SCE_SYS
            File.WriteAllBytes(Path.Combine(sceSysFolder, "param.sfo"), sfo);
            File.WriteAllBytes(Path.Combine(sceSysFolder, "icon0.png"), icon0);

        }

        public void Go(ChovySignParameters parameters)
        {

            SceNpDrm.Aid = parameters.DrmRif.AccountId;

            BuildStream.BuildUsingStreamType = parameters.BuildStreamType;

            if (!Directory.Exists(parameters.OutputFolder))
                Directory.CreateDirectory(parameters.OutputFolder);

            if (parameters.Type == ChovyTypes.PSP)
                createPSP((PspParameters)parameters);
            else
                createPOPS((PopsParameters)parameters);
        }
    }
}
