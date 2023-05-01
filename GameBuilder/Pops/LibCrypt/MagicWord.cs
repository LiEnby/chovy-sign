using GameBuilder.Cue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Pops.LibCrypt
{
    public class MagicWord
    {
        public static int ObfuscateMagicWord(int magicWord)
        {
            return magicWord ^ Constants.MAGIC_WORD_KEY;
        }
        public static int GenerateMagicWord(SbiEntry[] sbiEntries)
        {
            bool[] bits = new bool[16];

            HashSet<int> sbiSectors = new HashSet<int>();
            foreach(SbiEntry sbiEntry in sbiEntries)
                sbiSectors.Add(sbiEntry.Sector);
            
            for (int i = 0; i < bits.Length; i++)
                bits[i] = (sbiSectors.Contains(Constants.LIBCRYPT_PAIRS[i][0]) && sbiSectors.Contains(Constants.LIBCRYPT_PAIRS[i][1]));


            int magicWord = 0;

            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i]) magicWord |= 1;
                
                if(i+1 < bits.Length)
                    magicWord <<= 1;
            }

            return magicWord;
        }
    }
}
