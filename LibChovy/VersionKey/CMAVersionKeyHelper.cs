using GameBuilder.Psp;
using Li.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vita.ContentManager;
using Vita.PsvImgTools;

namespace LibChovy.VersionKey
{
    public class CMAVersionKeyHelper
    {

        public static PSVIMGFileStream? GetFileFromPsvImg(string psvImgFile, string fileToOpen, byte[] cmaKey)
        {
            try
            {
                FileStream psvImgFileStream = File.OpenRead(psvImgFile);
                PSVIMGStream psvImgStream = new PSVIMGStream(psvImgFileStream, cmaKey);
                PSVIMGFileStream fileStream = new PSVIMGFileStream(psvImgStream, fileToOpen);
                return fileStream;
            }
            catch { return null; }
        }

        public static NpDrmInfo? GetKeyFromGamePsvimg(string gameBackupFolder, string accountId, int keyIndex)
        {
            string gamePsvimgFile = Path.Combine(gameBackupFolder, "game", "game.psvimg");
            if (!File.Exists(gamePsvimgFile)) return null;

            byte[] accountIdBin = MathUtil.StringToByteArray(accountId);
            byte[] cmaKey = KeyGenerator.GenerateKey(accountIdBin);
            using (PSVIMGFileStream? pbp = GetFileFromPsvImg(gamePsvimgFile, "/EBOOT.PBP", cmaKey))
            {
                if (pbp is null) return null;
                return EbootPbpMethod.GetVersionKey(pbp, keyIndex);
            }
        }

        public static NpDrmRif? GetRifFromLicensePsvimg(string gameBackupFolder, string accountId)
        {
            string licensePsvImgFile = Path.Combine(gameBackupFolder, "license", "license.psvimg");
            if (!File.Exists(licensePsvImgFile)) return null;
            byte[] accountIdBin = MathUtil.StringToByteArray(accountId);
            byte[] cmaKey = KeyGenerator.GenerateKey(accountIdBin);

            using (PSVIMGFileStream? rif = GetFileFromPsvImg(licensePsvImgFile, ".RIF", cmaKey))
            {
                if (rif is null) return null;

                byte[] rifData = new byte[rif.Length];
                rif.Read(rifData, 0x00, rifData.Length);

                return new NpDrmRif(rifData);
            }
        }
    }
}
