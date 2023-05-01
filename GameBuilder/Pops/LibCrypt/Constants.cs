using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Pops.LibCrypt
{
    public class Constants
    {
        public const int MAGIC_WORD_KEY = 0x72D0EE59;

        public static int[][] LIBCRYPT_PAIRS = new int[16][] {  new int[2] { 14105, 14110 },
                                                                new int[2] { 14231, 14236 },
                                                                new int[2] { 14485, 14490 },
                                                                new int[2] { 14579, 14584 },
                                                                new int[2] { 14649, 14654 },
                                                                new int[2] { 14899, 14904 },
                                                                new int[2] { 15056, 15061 },
                                                                new int[2] { 15130, 15135 },
                                                                new int[2] { 15242, 15247 },
                                                                new int[2] { 15312, 15317 },
                                                                new int[2] { 15378, 15383 },
                                                                new int[2] { 15628, 15633 },
                                                                new int[2] { 15919, 15924 },
                                                                new int[2] { 16031, 16036 },
                                                                new int[2] { 16101, 16106 },
                                                                new int[2] { 16167, 16172 }
        };
    }
}
