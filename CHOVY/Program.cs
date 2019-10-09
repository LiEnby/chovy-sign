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
            FileStream psvmd = File.OpenRead(@"C:\Users\earsy\Documents\PS Vita\PGAME\cf6185d5a7870c4a\UCES00304\game\game.psvmd");
            //File.WriteAllBytes(@"C:\Users\earsy\Documents\PS Vita\PGAME\cf6185d5a7870c4a\UCES00304\game\game.psvmdi", PSVMDBuilder.DecryptPsvmd(psvmd, CmaKeys.GenerateKey(new byte[] { 0xcf, 0x61, 0x85, 0xd5, 0xa7, 0x87, 0x0c, 0x4a })));
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CHOVY());
        }
    }
}
