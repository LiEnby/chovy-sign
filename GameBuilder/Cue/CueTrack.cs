using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Cue
{
    public class CueTrack : DiscTrack
    {

        public const int MODE2_SECTOR_SZ = 2352;
        public const int CDDA_SECTOR_SZ = 2352;

        public int TrackLength;
        public int SectorSz
        {
            get
            {
                if (TrackType == TrackType.TRACK_CDDA) return CDDA_SECTOR_SZ;
                else return MODE2_SECTOR_SZ;
            }
        }
        internal long binFileSz;
        internal string binFileName;

        internal CueTrack(string binFile)
        {
            binFileName = binFile;
            binFileSz = new FileInfo(binFileName).Length;

            this.TrackType = TrackType.TRACK_MODE2_2352;
            this.TrackNo = 0xFF;
        }

        

    }
}
