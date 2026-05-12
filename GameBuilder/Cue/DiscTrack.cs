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
            TrackIndex = new DiscIndex[99];
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

            track.TrackIndex[CueReader.INDEX_PREGAP].MRelative = Convert.ToInt16(CueReader.BinaryDecimalToDecimal(tocEntry[3]) - track.TrackIndex[CueReader.INDEX_PREGAP].Mdelta);
            track.TrackIndex[CueReader.INDEX_PREGAP].SRelative = Convert.ToInt16(CueReader.BinaryDecimalToDecimal(tocEntry[4]) - track.TrackIndex[CueReader.INDEX_PREGAP].Sdelta); 
            track.TrackIndex[CueReader.INDEX_PREGAP].FRelative = Convert.ToInt16(CueReader.BinaryDecimalToDecimal(tocEntry[5]) - track.TrackIndex[CueReader.INDEX_PREGAP].Fdelta); 
            track.unk6 = tocEntry[6];
            track.TrackIndex[CueReader.INDEX_TRACK_START].MRelative = Convert.ToInt16(CueReader.BinaryDecimalToDecimal(tocEntry[7]) - track.TrackIndex[CueReader.INDEX_TRACK_START].Mdelta);
            track.TrackIndex[CueReader.INDEX_TRACK_START].SRelative = Convert.ToInt16(CueReader.BinaryDecimalToDecimal(tocEntry[8]) - track.TrackIndex[CueReader.INDEX_TRACK_START].Sdelta); 
            track.TrackIndex[CueReader.INDEX_TRACK_START].FRelative = Convert.ToInt16(CueReader.BinaryDecimalToDecimal(tocEntry[9]) - track.TrackIndex[CueReader.INDEX_TRACK_START].Fdelta); 

            return track;
        }

        public byte[] ToTocEntry()
        {
            byte[] tocEntry = new byte[0xA];

            tocEntry[0] = Convert.ToByte(this.TrackType);
            tocEntry[1] = unk1;
            tocEntry[2] = CueReader.DecimalToBinaryDecimal(this.TrackNo);


            tocEntry[3] = this.TrackIndex[CueReader.INDEX_PREGAP].M;
            tocEntry[4] = this.TrackIndex[CueReader.INDEX_PREGAP].S;
            tocEntry[5] = this.TrackIndex[CueReader.INDEX_PREGAP].F;
            tocEntry[6] = unk6;
            tocEntry[7] = this.TrackIndex[CueReader.INDEX_TRACK_START].M;
            tocEntry[8] = this.TrackIndex[CueReader.INDEX_TRACK_START].S;
            tocEntry[9] = this.TrackIndex[CueReader.INDEX_TRACK_START].F;

            return tocEntry;
        }
    }
}
