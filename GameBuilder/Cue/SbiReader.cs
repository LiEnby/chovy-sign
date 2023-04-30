using Li.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Cue
{
    public class SbiReader
    {
        private StreamUtil sbiUtil;
        private List<SbiEntry> sbiEntries;
        public SbiEntry[] Entries 
        {
            get
            {
                return sbiEntries.ToArray();
            }
        }

        private void init(Stream sbiFile)
        {
            sbiEntries = new List<SbiEntry>();
            sbiUtil = new StreamUtil(sbiFile);
            string magic = sbiUtil.ReadStrLen(3);
            if (magic != "SBI")
                throw new Exception("Invalid SBI Sub Channel file.");
            sbiUtil.ReadByte();

            do
            {
                byte m = CueReader.BinaryDecimalToDecimal(sbiUtil.ReadByte());
                byte s = CueReader.BinaryDecimalToDecimal(sbiUtil.ReadByte());
                byte f = CueReader.BinaryDecimalToDecimal(sbiUtil.ReadByte());
                byte i = CueReader.BinaryDecimalToDecimal(sbiUtil.ReadByte());
                byte[] toc = sbiUtil.ReadBytes(0xA);

                DiscIndex idx = new DiscIndex(i);
                idx.Mrel = m;
                idx.Srel = s;
                idx.Frel = f;
                sbiEntries.Add(new SbiEntry(idx, toc));
            } while (sbiFile.Position < sbiFile.Length);
        }
        public SbiReader(string sbiFileName)
        {
            using (FileStream fsbi = File.OpenRead(sbiFileName))
            {
                init(fsbi);
            }
        }
        public SbiReader(Stream sbiFile)
        {
            init(sbiFile);
        }
    }
}
