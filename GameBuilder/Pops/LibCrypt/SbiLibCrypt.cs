using GameBuilder.Cue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Pops.LibCrypt
{
    public class SbiLibCrypt : LibCryptInfo
    {
        private SbiReader sbiReader;
        public override int MagicWord
        {
            get
            {
                return LibCrypt.MagicWord.GenerateMagicWord(sbiReader.Entries);
            }
        }

        public SbiLibCrypt(SbiReader sbi, LibCryptMethod method)
        {
            this.sbiReader = sbi;
            this.Method = method;
        }
    }
}
