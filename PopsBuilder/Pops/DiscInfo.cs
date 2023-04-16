using DiscUtils.Iso9660;
using DiscUtils.Streams;
using GameBuilder.Cue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Pops
{
    public class DiscInfo
    {
        private string cueFile;
        private string discName;
        private string discId;

        public string CueFile 
        { 
            get
            {
                return cueFile;
            } 
        }

        public string DiscIdHdr
        {
            get
            {
                return "_" + DiscId.Substring(0, 4) + "_" + DiscId.Substring(4, 5);
            }
        }
        public string DiscName
        {
            get
            {
                return discName;
            }
        }

        public string DiscId
        {
            get
            {
                return discId.Replace("-", "").Replace("_", "").ToUpperInvariant();
            }
        }

        public DiscInfo(string cueFile, string discName)
        {
            this.cueFile = cueFile;
            this.discName = discName;

            using(CueReader cue = new CueReader(cueFile))
            {
                using (CueStream cueStream = cue.OpenTrack(cue.FirstDataTrackNo))
                {
                    using (CDReader cdReader = new CDReader(cueStream, false, true, cue.GetTrackNumber(cue.FirstDataTrackNo).SectorSz))
                    {
                        using (SparseStream systemCnfStream = cdReader.OpenFile("SYSTEM.CNF", FileMode.Open))
                        {
                            using (StreamReader systemCnfReader = new StreamReader(systemCnfStream))
                            {
                                for (string? line = systemCnfReader.ReadLine(); line is not null; line = systemCnfReader.ReadLine())
                                {
                                    line = line.Trim().ReplaceLineEndings("").ToUpperInvariant();

                                    if (line.StartsWith("BOOT"))
                                    {
                                        // wew thats a big one liner xD
                                        this.discId = line.Split('=').Last().Trim().Split(';').First().Replace('\\', '/').Split('/').Last().Replace(".", "").Replace("_", "");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (discId is null) discId = "SLUS00001";

        }
    }
}
