using GameBuilder.Cue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static GameBuilder.Pops.LibCrypt.MagicWord;
using static GameBuilder.Pops.LibCrypt.SubChannel;

namespace GameBuilder.Pops.LibCrypt
{
    public class LibCryptInfo
    {
        public LibCryptMethod Method;
        private SbiReader? sbiReader;
        public int MagicWord
        {
            get
            {
                if (sbiReader is null) return 0;
                return GenerateMagicWord(sbiReader.Entries);
            }
        }

        public int ObfuscatedMagicWord
        {
            get
            {
                if (Method == LibCryptMethod.METHOD_SUB_CHANNEL) return 0;
                return ObfuscateMagicWord(this.MagicWord);
            }
        }

        public byte[] Subchannels
        {
            get
            {
                if (sbiReader is null) throw new Exception("Cannot create subchannels, if there is no SBI data.");
                return CreateSubchannelDat(MagicWord);
            }
        }
        public LibCryptInfo(SbiReader? sbi, LibCryptMethod method)
        {
            this.sbiReader = sbi;

            if (sbi is null) Method = LibCryptMethod.METHOD_MAGIC_WORD;
            else Method = method;
            
        }
    }
}
