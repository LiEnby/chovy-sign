using Li.Utilities;

namespace GameBuilder.Cue
{
    public class CueReader : IDisposable
    {
        public const int INDEX_PREGAP = 0;
        public const int INDEX_TRACK_START = 1;

        public int FirstDataTrackNo
        {
            get
            {
                return getFirstDataTrackNo();
            }
        }


        private CueTrack[] tracks = new CueTrack[99];
        private Dictionary<int, CueStream> openTracks;

        public int GetTotalTracks()
        {
            int totalTracks = 0;
            for(int i = 0; i < tracks.Length; i++)
                if (tracks[i] is not null) totalTracks++;
            return totalTracks;
        }

        public static byte BinaryDecimalToDecimal(int i)
        {
            return Convert.ToByte(Convert.ToInt32(10 * (i - i % 16) / 16 + i % 16));
        }
        public static byte DecimalToBinaryDecimal(int i)
        {
            return Convert.ToByte(Convert.ToInt32((i % 10) + 16 * ((i / 10) % 10)));
        }
        public static int IdxToSectorRel(DiscIndex index)
        {
            int offset = (((index.mRelative * 60) + index.sRelative) * 75 + index.fRelative);
            return offset;
        }
        public static int IdxToSector(DiscIndex index)
        {
            int offset = (((index.m * 60) + index.s) * 75 + index.f);
            return offset;
        }

        public static DiscIndex SectorToIdx(int sector, byte index=1)
        {
            DiscIndex idx = new DiscIndex(index);

            int x = sector;
            int f = sector % 75;
            x = x - f;
            x = Convert.ToInt32(Math.Floor(Convert.ToDouble(x) / 75.0));
            int s = x % 60;
            int m = Convert.ToInt32(Math.Floor(Convert.ToDouble(x) / 60.0));

            idx.MRelative = Convert.ToInt16(m);
            idx.SRelative = Convert.ToInt16(s);
            idx.FRelative = Convert.ToInt16(f);

            return idx; 
        }

        private int getFirstDataTrackNo()
        {
            return tracks.Where(track => track is not null).First(track => track.TrackType == TrackType.TRACK_MODE2_2352).TrackNo;
        }
        private void setTrackNumber(int trackNo, ref CueTrack? track)
        {
            tracks[trackNo - 1] = track;
        }
        public CueTrack GetTrackNumber(int trackNo)
        {
            return tracks[trackNo - 1];
        }
        private int findTrackSz(int trackNo)
        {
            CueTrack track = GetTrackNumber(trackNo);

            // total iso (size / sector size)
            int startSector = IdxToSector(track.TrackIndex[INDEX_TRACK_START]);
            int pregapSectorSz = IdxToSectorRel(track.TrackIndex[INDEX_TRACK_START]);

            int fileSectorSz = Convert.ToInt32(track.binFileSz / track.SectorSz);
            int endSector = Convert.ToInt32(startSector + (fileSectorSz - pregapSectorSz));

            // find first track to start after this one ..
            for (int tid = 0; tid < tracks.Length; tid++)
            {
                if (tracks[tid] is null) continue;
                if (tracks[tid].TrackNo <= track.TrackNo) continue;

                for(int idx = 0; idx < tracks[tid].TrackIndex.Length; idx++)
                {
                    if (tracks[tid].TrackIndex[idx] is null) continue;

                    int gotSector = IdxToSector(tracks[tid].TrackIndex[idx]);
                    if (gotSector > startSector && gotSector < endSector) endSector = gotSector;
                }
                
            }
            
            int sectorsLength = Math.Max(endSector, startSector) - Math.Min(endSector, startSector);
            return sectorsLength;
        }

        public CueStream OpenTrack(int trackNo)
        {
            if (!openTracks.ContainsKey(trackNo))
            {
                CueTrack track = GetTrackNumber(trackNo);
                int sectorStart = IdxToSectorRel(track.TrackIndex[INDEX_TRACK_START]);
                int sectorLen = findTrackSz(trackNo);

                CueStream trackBin = new CueStream(File.OpenRead(track.binFileName),
                                                    sectorStart * track.SectorSz, 
                                                    sectorLen * track.SectorSz);
                openTracks[trackNo] = trackBin;
                return trackBin;
            }
            else
            {
                CueStream openTrack = openTracks[trackNo];
                if (!openTrack.IsClosed)
                {
                    openTracks.Remove(trackNo);
                    return OpenTrack(trackNo);
                }
                else
                {
                    openTracks[trackNo].Seek(0x00, SeekOrigin.Begin);
                    return openTracks[trackNo];
                }
            }
            
        }

        private int getLastTrackNo()
        {
            int trackNo = 0;
            for (int i = 0; i < tracks.Length; i++)
            {
                if (tracks[i] is null) continue;
                trackNo = tracks[i].TrackNo;
            }
            return trackNo;
        }
        private int getTotalSectorSz()
        {
            int sectors = 0;
            HashSet<string> countedBins = new HashSet<string>();

            for(int i = 0; i < tracks.Length; i++)
            {
                if (tracks[i] is null) continue;
                if (!countedBins.Contains(tracks[i].binFileName))
                {
                    countedBins.Add(tracks[i].binFileName);
                    sectors += Convert.ToInt32(tracks[i].binFileSz / CueTrack.MODE2_SECTOR_SZ);
                }
            }
            return sectors;
        }

        private bool haveAudioTracks
        {
            get
            {
                for (int i = 0; i < tracks.Length; i++)
                {
                    if (tracks[i] is null) continue;
                    if (tracks[i].TrackType == TrackType.TRACK_CDDA) return true;
                }
                return false;
            }
        }

        private byte[] createDummyTracks()
        {
            // every psn ps1 game have "A0" track that points to sector 6000 (MSF 01 20 00)
            byte[] tocA0Entry = new byte[10] { 0x41, 0x00, 0xA0, 0x00, 0x00, 0x00, 0x00, 0x01, 0x20, 0x00 };

            // And an A1 track (determines how many tracks there are)
            byte[] tocA1Entry = new byte[10] { 0x41, 0x00, 0xA1, 0x00, 0x00, 0x00, 0x00, DecimalToBinaryDecimal(GetTotalTracks()), 0x00, 0x00 };

            // the A2 track is a bit more complicated ..
            int totalSectors = getTotalSectorSz();
            DiscIndex idx = SectorToIdx(totalSectors);
            idx.Sdelta = 2;

            byte[] tocA2Entry = new byte[10] { 0x41, 0x00, 0xA2, 0x00, 0x00, 0x00, 0x00, idx.M, idx.S, idx.F };

            if (GetTrackNumber(getLastTrackNo()).TrackType == TrackType.TRACK_CDDA)
            {
                tocA2Entry[0x00] = 0x01;
                tocA1Entry[0x00] = 0x01;
            }

            byte[] tocDummy = new byte[10 * 3];
            Array.ConstrainedCopy(tocA0Entry, 0, tocDummy, 0, 10);
            Array.ConstrainedCopy(tocA1Entry, 0, tocDummy, 10, 10);
            Array.ConstrainedCopy(tocA2Entry, 0, tocDummy, 20, 10);

            return tocDummy;
        }

        private int getTrackSectorOnDisc(int trackNo)
        {
            int absolutePosition = 0;
            for(int i = 0; i < tracks.Length; i++)
            {
                if (tracks[i] is null) continue;
                if (tracks[i].TrackNo == trackNo) break;

                absolutePosition += findTrackSz(tracks[i].TrackNo);
            }
            return absolutePosition;
        }

        private int calcDifference(int val1, int val2)
        {
            return Math.Min(val1, val2) - Math.Max(val1, val2);
        }
        private void fixUpMsf()
        {

            Dictionary<string, int> binPositions = new Dictionary<string, int>();
            int totalPosition = 0;

            // get the absolute position every binary file would be on the actual disc.
            for (int i = 0; i < tracks.Length; i++)
            {
                if (tracks[i] is null) continue;

                if (!binPositions.ContainsKey(tracks[i].binFileName)) 
                {
                    binPositions[tracks[i].binFileName] = totalPosition;
                    int sz = Convert.ToInt32(Convert.ToDouble(tracks[i].binFileSz) / Convert.ToDouble(tracks[i].SectorSz));

                    totalPosition += Convert.ToInt32(sz);
                }

            }

            // update to absolute disc positions
            for (int tid = 0; tid < tracks.Length; tid++)
            {
                if (tracks[tid] is null) continue;
                DiscIndex discIdx = SectorToIdx(binPositions[tracks[tid].binFileName]);

                for (int idx = 0; idx < tracks[tid].TrackIndex.Length; idx++)
                {
                    if (tracks[tid].TrackIndex[idx] is null) continue;

                    tracks[tid].TrackIndex[idx].Mdelta = discIdx.m;
                    tracks[tid].TrackIndex[idx].Sdelta = discIdx.s;
                    tracks[tid].TrackIndex[idx].Fdelta = discIdx.f;
                }
            }

        }

        public byte[] CreateToc()
        {
            using (BuildStream toc = new BuildStream())
            {
                StreamUtil tocUtil = new StreamUtil(toc);
                tocUtil.WriteBytes(createDummyTracks());

                for (int trackNo = 0; trackNo < tracks.Length; trackNo++)
                {
                    if (tracks[trackNo] is not null)
                    {
                        tocUtil.WriteBytes(tracks[trackNo].ToTocEntry());
                    }
                }

                int remain = Convert.ToInt32(0x3F0 - toc.Length);
                tocUtil.WritePadding(0x00, remain);

                toc.Seek(0x00, SeekOrigin.Begin);
                return toc.ToArray();
            }
        }

        public void Dispose()
        {
            foreach(CueStream openTrack in openTracks.Values)
            {
                if(openTrack.IsClosed)
                   openTrack.Close();
            }
            openTracks.Clear();
        }

        private string getFilename(string str)
        {
            if (!str.Contains(' ')) throw new FileNotFoundException("No binary file specified in the cue sheet");
            if (!str.Contains('"')) return str.Split(' ')[1];

            int start = str.IndexOf('"');
            str = str.Substring(start + 1);

            
            int end = str.IndexOf('"');
            str = str.Substring(0, end);
            return str;
        }
        public CueReader(string cueFile)
        {
            openTracks = new Dictionary<int, CueStream>();
            for (int trackNo = 0; trackNo < tracks.Length; trackNo++) tracks[trackNo] = null;

            // for parsing "PREGAP" sections, have to loop over all once done;
            Dictionary<int, DiscIndex> pregaps = new Dictionary<int, DiscIndex>();

            using (TextReader cueReader = File.OpenText(cueFile))
            {
                CueTrack? curTrack = null;

                for (string? cueData = cueReader.ReadLine();
                    cueData is not null;
                    cueData = cueReader.ReadLine())
                {
                    cueData = cueData.Trim().Replace("\r", "").Replace("\n", "");
                    string[] cueLn = cueData.Split(' ');

                    if (cueLn[0] == "INDEX")
                    {
                        if (curTrack is null) throw new NullReferenceException("Tried to specify index information, without a track selected");

                        int indexNumber = Convert.ToByte(int.Parse(cueLn[1]));
                        string[] msf = cueLn[2].Split(':');

                        curTrack.TrackIndex[indexNumber].MRelative = Convert.ToByte(Int32.Parse(msf[0]));
                        curTrack.TrackIndex[indexNumber].SRelative = Convert.ToByte(Int32.Parse(msf[1]));
                        curTrack.TrackIndex[indexNumber].FRelative = Convert.ToByte(Int32.Parse(msf[2]));

                        setTrackNumber(curTrack.TrackNo, ref curTrack);
                    }
                    else if (cueLn[0] == "PREGAP")
                    {
                        if (curTrack is null) throw new NullReferenceException("Tried to specify pregap information, without a track selected");
                        string[] msf = cueLn[1].Split(':');

                        // set pregap for this track, 
                        // will be handled by fixUpMsf at the end of this;

                        DiscIndex idx = new DiscIndex(0);
                        idx.MRelative = Convert.ToByte(Int32.Parse(msf[0]));
                        idx.SRelative = Convert.ToByte(Int32.Parse(msf[1]));
                        idx.FRelative = Convert.ToByte(Int32.Parse(msf[2]));
                        pregaps[curTrack.TrackNo] = idx;

                    }
                    else if (cueLn[0] == "TRACK")
                    {
                        if (curTrack is null) throw new NullReferenceException("Tried to specify track information, without a track selected");

                        if (curTrack.TrackNo != 0xFF)
                        {
                            setTrackNumber(curTrack.TrackNo, ref curTrack);
                            curTrack = new CueTrack(curTrack.binFileName);
                        }

                        curTrack.TrackNo = Convert.ToByte(int.Parse(cueLn[1]));
                        if (cueLn[2] == "MODE2/2352")
                            curTrack.TrackType = TrackType.TRACK_MODE2_2352;
                        else if (cueLn[2] == "AUDIO")
                            curTrack.TrackType = TrackType.TRACK_CDDA;

                        setTrackNumber(curTrack.TrackNo, ref curTrack);
                    }
                    else if (cueLn[0] == "FILE")
                    {
                        if (curTrack != null) setTrackNumber(curTrack.TrackNo, ref curTrack);

                        // parse out filename..
                        string binFileName = getFilename(cueData);
                        string? folderContainingCue = Path.GetDirectoryName(cueFile);

                        if (folderContainingCue != null)
                            binFileName = Path.Combine(folderContainingCue, binFileName);

                        if (!File.Exists(binFileName)) throw new FileNotFoundException("The referenced binary file \""+binFileName+"\" was not found");

                        curTrack = new CueTrack(binFileName);
                    }
                }
            }

            // apply pregaps
            foreach (KeyValuePair<int, DiscIndex> pregap in pregaps)
            {
                CueTrack track = GetTrackNumber(pregap.Key);

                track.TrackIndex[INDEX_PREGAP].MRelative = Convert.ToInt16(track.TrackIndex[INDEX_TRACK_START].MRelative - pregap.Value.MRelative);
                track.TrackIndex[INDEX_PREGAP].SRelative = Convert.ToInt16(track.TrackIndex[INDEX_TRACK_START].SRelative - pregap.Value.MRelative);
                track.TrackIndex[INDEX_PREGAP].FRelative = Convert.ToInt16(track.TrackIndex[INDEX_TRACK_START].FRelative - pregap.Value.MRelative);

                setTrackNumber(track.TrackNo, ref track);
            }

            fixUpMsf();
        }
    }
}
