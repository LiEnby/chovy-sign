using DiscUtils.Iso9660Ps1;
using DiscUtils.Raw;
using DiscUtils.Streams;
using GameBuilder.Cue;
using Li.Utilities;
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

        public string? SbiFile
        {
            get
            {
                string sbiFileName = Path.ChangeExtension(CueFile, ".sbi");
                if(File.Exists(sbiFileName)) return sbiFileName;
                else return null;
            }
        }
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
                if (discName is null) return "";
                else return discName;
            }
            set
            {
                discName = value;
            }
        }

        public string DiscId
        {
            get
            {
                return discId.Replace("-", "").Replace("_", "").ToUpperInvariant().PadRight(9, '0').Substring(0, 9).ToUpperInvariant();                
            }
        }

        public DiscInfo(string cueFile)
        {
            this.cueFile = cueFile;

            using(CueReader cue = new CueReader(cueFile))
            {
                using (CueStream binStream = cue.OpenTrack(cue.FirstDataTrackNo))
                {
                    StreamUtil binUtil = new StreamUtil(binStream);

                    // Get disc id from SYSTEM.CNF
                    using (CDReader cdReader = new CDReader(binStream, false, true, cue.GetTrackNumber(cue.FirstDataTrackNo).SectorSz))
                    {
                        using (SparseStream systemCnfStream = cdReader.OpenFile("SYSTEM.CNF", FileMode.Open))
                        {
                            systemCnfStream.Seek(0x18, SeekOrigin.Begin);
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

                    binStream.Seek(0x9340, SeekOrigin.Begin);
                    discName = binUtil.ReadCDStr(0x20);
                }
            }
            if (discName == "") discName = Path.GetFileNameWithoutExtension(cueFile);
            if (discId is null) discId = "SLUS00001";

        }
    }
}
