using Li.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Cue
{
    public class CueReader : IDisposable
    {
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

        public static byte BinaryDecimalConv(int i)
        {
            return Convert.ToByte(Convert.ToInt32((i % 10) + 16 * ((i / 10) % 10)));
        }
        public int IdxToSectorRel(CueIndex index)
        {
            int offset = (((index.Mrel * 60) + index.Srel) * 75 + index.Frel);
            return offset;
        }
        public int IdxToSector(CueIndex index)
        {
            int offset = (((index.m * 60) + index.s) * 75 + index.f);
            return offset;
        }

        public CueIndex SectorToIdx(int sector)
        {
            CueIndex idx = new CueIndex(1);

            int x = sector;
            int f = sector % 75;
            x = x - f;
            x = Convert.ToInt32(Math.Floor(Convert.ToDouble(x) / 75.0));
            int s = x % 60;
            int m = Convert.ToInt32(Math.Floor(Convert.ToDouble(x) / 60.0));

            //idx.Mrel = Convert.ToByte(Convert.ToInt32(((sector / 75) / 60)));
            //idx.Srel = Convert.ToByte(Convert.ToInt32(((sector / 75) % 60)));
            //idx.Frel = Convert.ToByte(Convert.ToInt32(((sector % 75))));
            //idx.Sdelta = 2; // why?

            idx.Mrel = Convert.ToInt16(m);
            idx.Srel = Convert.ToInt16(s);
            idx.Frel = Convert.ToInt16(f);

            return idx; 
        }

        private int getFirstDataTrackNo()
        {
            foreach (CueTrack track in tracks)
                if (track is not null)
                    if (track.TrackType == TrackType.TRACK_MODE2_2352) return track.TrackNo;

            // no non-data tracks? 
            return 1;
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
            int startSector = IdxToSector(track.TrackIndex[1]);
            int fileSectorSz = Convert.ToInt32(track.binFileSz / track.SectorSz);
            int endSector = Convert.ToInt32(startSector + fileSectorSz);

            // find first track to start after this one ..
            for (int i = 0; i < tracks.Length; i++)
            {
                CueTrack? cTrack = tracks[i];
                if (cTrack is not null)
                {
                    if (cTrack.TrackNo <= track.TrackNo) continue;
                    int sector = IdxToSector(cTrack.TrackIndex[0]);

                    if (sector < endSector) endSector = sector;
                }
            }

            int sectorsLength = (endSector - startSector);
            return sectorsLength;
        }

        public CueStream OpenTrack(int trackNo)
        {
            if (!openTracks.ContainsKey(trackNo))
            {
                CueTrack track = GetTrackNumber(trackNo);
                int sectorStart = IdxToSectorRel(track.TrackIndex[1]);
                int sectorLen = findTrackSz(trackNo);

                CueStream trackBin = new CueStream(File.OpenRead(track.binFileName), sectorStart * track.SectorSz, sectorLen * track.SectorSz);
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
            byte[] tocA1Entry = new byte[10] { 0x41, 0x00, 0xA1, 0x00, 0x00, 0x00, 0x00, BinaryDecimalConv(GetTotalTracks()), 0x00, 0x00 };

            // the A2 track is a bit more complicated ..
            int totalSectors = getTotalSectorSz();
            CueIndex idx = SectorToIdx(totalSectors);
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

        private void fixUpMsf()
        {

            Dictionary<string, int> positions = new Dictionary<string, int>();
            int totalPosition = 0;

            for (int i = 0; i < tracks.Length; i++)
            {
                if (tracks[i] is null) continue;

                if (!positions.ContainsKey(tracks[i].binFileName))
                {
                    positions[tracks[i].binFileName] = totalPosition;
                    double sz = Convert.ToDouble(tracks[i].binFileSz) / Convert.ToDouble(tracks[i].SectorSz);

                    totalPosition += Convert.ToInt32(sz);
                }

            }

            for (int i = 0; i < tracks.Length; i++)
            {
                if (tracks[i] is null) continue;
                int pos = positions[tracks[i].binFileName];

                CueIndex idx = this.SectorToIdx(pos);
                // pregap not included on first track

                if (tracks[i].TrackNo == 1) tracks[i].TrackIndex[0].Sdelta = 0;

                // add pregap
                idx.Mdelta = Convert.ToInt16(tracks[i].TrackIndex[1].Mrel - tracks[i].TrackIndex[0].Mrel);
                idx.Sdelta = Convert.ToInt16(tracks[i].TrackIndex[1].Srel - tracks[i].TrackIndex[0].Srel);
                idx.Fdelta = Convert.ToInt16(tracks[i].TrackIndex[1].Frel - tracks[i].TrackIndex[0].Frel);

                tracks[i].TrackIndex[0].Mdelta = idx.m;
                tracks[i].TrackIndex[0].Sdelta = idx.s;
                tracks[i].TrackIndex[0].Fdelta = idx.f;

                // index is always ofset by 2 thou
                idx.Sdelta = 2;
                tracks[i].TrackIndex[1].Mdelta = idx.m;
                tracks[i].TrackIndex[1].Sdelta = idx.s;
                tracks[i].TrackIndex[1].Fdelta = idx.f;
            }
        }

        public byte[] CreateToc()
        {
            using (MemoryStream toc = new MemoryStream())
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

        public CueReader(string cueFile)
        {
            openTracks = new Dictionary<int, CueStream>();
            for (int trackNo = 0; trackNo < tracks.Length; trackNo++) tracks[trackNo] = null;

            using (TextReader cueReader = File.OpenText(cueFile))
            {
                CueTrack? curTrack = null;

                for (string? cueData = cueReader.ReadLine();
                    cueData != null;
                    cueData = cueReader.ReadLine())
                {
                    string[] cueLn = cueData.Trim().Replace("\r", "").Replace("\n", "").Split(' ');

                    if (cueData.StartsWith("    ")) // index of track
                    {
                        if (cueLn[0] == "INDEX")
                        {
                            if (curTrack is null) throw new Exception("tried to create new index, when track was null");

                            int indexNumber = Convert.ToByte(int.Parse(cueLn[1]));
                            string[] msf = cueLn[2].Split(':');

                            curTrack.TrackIndex[indexNumber].Mrel = Convert.ToByte(Int32.Parse(msf[0]));
                            curTrack.TrackIndex[indexNumber].Srel = Convert.ToByte(Int32.Parse(msf[1]));
                            curTrack.TrackIndex[indexNumber].Frel = Convert.ToByte(Int32.Parse(msf[2]));

                            setTrackNumber(curTrack.TrackNo, ref curTrack);
                        }
                    }
                    else if (cueData.StartsWith("  ")) // start of new track
                    {
                        if (cueLn[0] == "TRACK")
                        {
                            if (curTrack is null) throw new Exception("tried to create new track, when track was null");

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
                    }
                    else // new file
                    {
                        if (cueLn[0] == "FILE")
                        {
                            if (curTrack != null) setTrackNumber(curTrack.TrackNo, ref curTrack);

                            // parse out filename..
                            string[] cueFnameParts = new string[cueLn.Length - 2];
                            Array.ConstrainedCopy(cueLn, 1, cueFnameParts, 0, cueFnameParts.Length);
                            string cueFname = String.Join(' ', cueFnameParts);

                            // open file ..
                            string binFileName = cueFname.Substring(1, cueFname.Length - 2);
                            string? folderContainingCue = Path.GetDirectoryName(cueFile);
                            
                            if (folderContainingCue != null)
                                binFileName = Path.Combine(folderContainingCue, binFileName);

                            curTrack = new CueTrack(binFileName);
                        }
                    }

                }
            }

            fixUpMsf();
        }
    }
}
