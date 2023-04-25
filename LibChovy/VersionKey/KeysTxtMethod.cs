using GameBuilder.Psp;
using Li.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibChovy.VersionKey
{
    public class KeysTxtMethod
    {
        public static string KeysTxt = Resources.KEYSTXT;
        public static string[] TitleIds
        {
            get
            {
                List<string> titleIds = new List<string>();
                string[] contentIds = ContentIds;
                foreach (string contentId in contentIds)
                    titleIds.Add(contentId.Substring(7, 9));
 
                return titleIds.ToArray();
            }
        }
        public static string[] ContentIds
        {
            get
            {
                List<string> contentIds = new List<string>();

                using (TextReader txt = new StringReader(KeysTxt))
                {
                    for (string? line = txt.ReadLine();
                        line is not null;
                        line = txt.ReadLine())
                    {
                        line = line.ReplaceLineEndings("");
                        string[] data = line.Split(' ');
                        if (data.Length != 5) continue;

                        contentIds.Add(data[0]);
                    }
                }

                return contentIds.ToArray();
            }
        }

        public static NpDrmInfo GetVersionKey(string contentId, int keyIndex)
        {
            using (TextReader txt = new StringReader(KeysTxt))
            {
                for(string? line = txt.ReadLine();
                    line is not null;
                    line = txt.ReadLine())
                {
                    line = line.ReplaceLineEndings("");
                    string[] data = line.Split(' ');
                    if (data.Length != 5) continue;

                    if (data[0].Equals(contentId, StringComparison.InvariantCultureIgnoreCase))
                        return new NpDrmInfo(MathUtil.StringToByteArray(data[1 + keyIndex]), contentId, keyIndex);
                }
            }
            throw new Exception("content id is not in keys.txt");
        }
    }
}
