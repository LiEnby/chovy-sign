using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace CHOVY
{
    class psvimgtools
    {
        public static Process PSVIMG_EXTRACT(string AID, string PSVIMG, string OUTPUT)
        {
            PSVIMG = PSVIMG.Replace("/", "\\");
            OUTPUT = OUTPUT.Replace("/", "\\");
            string CmaKey = CmaKeys.GenerateKeyStr(AID);

            Process psvimgtools = new Process();
            psvimgtools.StartInfo.FileName = Path.Combine(Application.StartupPath, "tools", "psvimg-extract.exe");
            psvimgtools.StartInfo.Arguments = "-K " + CmaKey + " \"" + PSVIMG + "\" \"" + OUTPUT + "\"";
            psvimgtools.StartInfo.CreateNoWindow = true;
            psvimgtools.StartInfo.UseShellExecute = false;
            psvimgtools.StartInfo.RedirectStandardInput = true; // NO IDEA WHY BUT IT DOESNT WORK WITHOUT THIS
            psvimgtools.StartInfo.RedirectStandardOutput = true;
            psvimgtools.StartInfo.RedirectStandardError = true;
            psvimgtools.Start();
            return psvimgtools;
        }

        public static Process PSVIMG_CREATE(string AID, string TYPE,string INPUT, string OUTPUT)
        {
            INPUT = INPUT.Replace("/", "\\");
            OUTPUT = OUTPUT.Replace("/", "\\");

            string CmaKey = CmaKeys.GenerateKeyStr(AID);

            Process psvimgtools = new Process();
            psvimgtools.StartInfo.FileName = Path.Combine(Application.StartupPath, "tools", "psvimg-create.exe");
            psvimgtools.StartInfo.Arguments = "-n "+TYPE+" -K " + CmaKey + " \"" + INPUT + "\" \"" + OUTPUT + "\"";
            psvimgtools.StartInfo.CreateNoWindow = true;
            psvimgtools.StartInfo.UseShellExecute = false;
            psvimgtools.StartInfo.RedirectStandardInput = true; // NO IDEA WHY BUT IT DOESNT WORK WITHOUT THIS
            psvimgtools.StartInfo.RedirectStandardOutput = true;
            psvimgtools.StartInfo.RedirectStandardError = true;
            psvimgtools.Start();
            return psvimgtools;
        }
    }
}
