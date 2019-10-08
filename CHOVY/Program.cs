using PSVIMGTOOLS;
using System;
using System.IO;
using System.Windows.Forms;

namespace CHOVY
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*
            
            FileStream fs = File.OpenWrite(@"C:\Users\earsy\Documents\Visual Studio 2017\Projects\CSharpImgTools\CSharpImgTools\bin\Debug\test.psvimg");
            PSVIMGBuilder psvBuild = new PSVIMGBuilder(fs, CmaKeys.GenerateKey(new byte[] { 0xcf, 0x61, 0x85, 0xd5, 0xa7, 0x87, 0x0c, 0x4a }));
            psvBuild.AddFile(@"C:\Users\earsy\Documents\Visual Studio 2017\Projects\CSharpImgTools\CSharpImgTools\bin\Debug\EBOOT.PBP", @"ux0:temp/game/PCSG90096/app/PCSG90096", @"/EBOOT.PBP");
            psvBuild.AddDir(@"C:\Users\earsy\Documents\Visual Studio 2017\Projects\CSharpImgTools\CSharpImgTools\bin\Debug", @"ux0:temp/game/PCSG90096/app/PCSG90096", @"/sce_sys");
            psvBuild.AddFile(@"C:\Users\earsy\Documents\Visual Studio 2017\Projects\CSharpImgTools\CSharpImgTools\bin\Debug\param.sfo", @"ux0:temp/game/PCSG90096/app/PCSG90096", @"/sce_sys/param.sfo");
            psvBuild.Finish();

            MemoryStream ms = new MemoryStream();
            FileStream fs2 = File.OpenRead(@"C:\Users\earsy\Documents\Visual Studio 2017\Projects\CSharpImgTools\CSharpImgTools\bin\Debug\test.psvimg");
            fs2.CopyTo(ms);
            fs2.Close();
            ms.Seek(0x00, SeekOrigin.Begin);
            FileStream fs3 = File.OpenWrite(@"C:\Users\earsy\Documents\Visual Studio 2017\Projects\CSharpImgTools\CSharpImgTools\bin\Debug\test.psvimgd");
            PSVIMGStream psv = new PSVIMGStream(ms, CmaKeys.GenerateKey(new byte[] { 0xcf, 0x61, 0x85, 0xd5, 0xa7, 0x87, 0x0c, 0x4a }));
            psv.CopyTo(fs3);
            fs3.Close();
            psv.Close();
            */


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CHOVY());
        }
    }
}
