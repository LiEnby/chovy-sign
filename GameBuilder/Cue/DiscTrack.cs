using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Cue
{
    public class DiscTrack
    {
        private byte unk1;
        private byte unk6;
        public TrackType TrackType;
        public byte TrackNo;
        public DiscIndex[] TrackIndex;

        public DiscTrack()
        {
            TrackIndex = new DiscIndex[2];
            for (int i = 0; i < TrackIndex.Length; i++)
                TrackIndex[i] = new DiscIndex(Convert.ToByte(i));

            unk1 = 0;
            unk6 = 0;

        }

        public static DiscTrack FromTocEntry(byte[] tocEntry)
        {
            if (tocEntry.Length != 0xA) throw new Exception("Invalid TOC Entry.");

            DiscTrack track = new DiscTrack();
            track.TrackType = (TrackType)tocEntry[0];
            track.unk1 = tocEntry[1];
            track.TrackNo = CueReader.BinaryDecimalToDecimal(tocEntry[2]);

            track.TrackIndex[0].Mrel = Convert.ToInt16(CueReader.BinaryDecimalToDecimal(tocEntry[3]) - track.TrackIndex[0].Mdelta);
            track.TrackIndex[0].Srel = Convert.ToInt16(CueReader.BinaryDecimalToDecimal(tocEntry[4]) - track.TrackIndex[0].Sdelta); 
            track.TrackIndex[0].Frel = Convert.ToInt16(CueReader.BinaryDecimalToDecimal(tocEntry[5]) - track.TrackIndex[0].Fdelta); 
            track.unk6 = tocEntry[6];
            track.TrackIndex[1].Mrel = Convert.ToInt16(CueReader.BinaryDecimalToDecimal(tocEntry[7]) - track.TrackIndex[1].Mdelta);
            track.TrackIndex[1].Srel = Convert.ToInt16(CueReader.BinaryDecimalToDecimal(tocEntry[8]) - track.TrackIndex[1].Sdelta); 
            track.TrackIndex[1].Frel = Convert.ToInt16(CueReader.BinaryDecimalToDecimal(tocEntry[9]) - track.TrackIndex[1].Fdelta); 

            return track;
        }

        public byte[] ToTocEntry()
        {
            byte[] tocEntry = new byte[0xA];

            tocEntry[0] = Convert.ToByte(this.TrackType);
            tocEntry[1] = unk1;
            tocEntry[2] = CueReader.DecimalToBinaryDecimal(this.TrackNo);


            tocEntry[3] = this.TrackIndex[0].M;
            tocEntry[4] = this.TrackIndex[0].S;
            tocEntry[5] = this.TrackIndex[0].F;
            tocEntry[6] = unk6;
            tocEntry[7] = this.TrackIndex[1].M;
            tocEntry[8] = this.TrackIndex[1].S;
            tocEntry[9] = this.TrackIndex[1].F;

            return tocEntry;
        }
    }
}
