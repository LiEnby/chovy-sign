using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Cue
{
    public class CueTrack
    {

        public const int MODE2_SECTOR_SZ = 2352;
        public const int CDDA_SECTOR_SZ = 2352;

        public TrackType TrackType;
        public byte TrackNo;
        public DiscIndex[] TrackIndex;

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
            TrackIndex = new DiscIndex[2];
            for (int i = 0; i < TrackIndex.Length; i++)
                TrackIndex[i] = new DiscIndex(Convert.ToByte(i));

            binFileName = binFile;
            binFileSz = new FileInfo(binFileName).Length;

            TrackType = TrackType.TRACK_MODE2_2352;
            TrackNo = 0xFF;
        }

        public byte[] ToTocEntry()
        {
            byte[] tocEntry = new byte[10];

            tocEntry[0] = Convert.ToByte(this.TrackType);
            tocEntry[1] = 0;
            tocEntry[2] = CueReader.DecimalToBinaryDecimal(this.TrackNo);
            

            tocEntry[3] = this.TrackIndex[0].M;
            tocEntry[4] = this.TrackIndex[0].S;
            tocEntry[5] = this.TrackIndex[0].F;
            tocEntry[6] = 0;
            tocEntry[7] = this.TrackIndex[1].M;
            tocEntry[8] = this.TrackIndex[1].S;
            tocEntry[9] = this.TrackIndex[1].F;

            return tocEntry;
        }

    }
}
