using GameBuilder.Psp;
using Li.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.VersionKey
{
    public class KeysTxtMethod
    {
        public static NpDrmInfo GetVersionKey(string contentId, int keyIndex)
        {
            using (TextReader txt = new StringReader(Resources.KEYSTXT))
            {
                for(string? line = txt.ReadLine();
                    line is not null;
                    line = txt.ReadLine())
                {
                    line = line.ReplaceLineEndings("");
                    string[] data = line.Split(' ');

                    if (data[0].Equals(contentId, StringComparison.InvariantCultureIgnoreCase))
                        return new NpDrmInfo(MathUtil.StringToByteArray(data[1 + keyIndex]), contentId, keyIndex);
                }
            }
            throw new Exception("content id is not in keys.txt");
        }
    }
}
