using PopsBuilder.Cue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopsBuilder.Pops
{
    public class EccRemoverStream : Stream
    {
        private byte[] currentSector;

        private long position;
        private Stream baseStream;
        public EccRemoverStream(Stream s)
        {
            baseStream = s;
            currentSector = new byte[CueTrack.MODE2_SECTOR_SZ];

            invalidateSectorCache();
        }
        private int positionInSector
        {
            get
            {
                return Convert.ToInt32(position % CueTrack.MODE2_SECTOR_SZ);
            }
        }
        private int remainInSector
        {
            get
            {
                return CueTrack.MODE2_SECTOR_SZ - positionInSector;
            }
        }
        private int positionSector
        {
            get
            {
                return findSector(position);
            }
        }
        public Stream BaseStream
        {
            get
            {
                return baseStream;
            }
        }
        public override bool CanRead
        {
            get
            {
                return baseStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return baseStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return baseStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return position;
            }
            set
            {
                long newPos = value;

                if (newPos < 0) newPos = 0;
                if (newPos > Length) newPos = Length;


                int oldSector = positionSector;
                position = newPos;

                if (positionSector != oldSector)
                    invalidateSectorCache();
            }
        }


        private void removeEcc()
        {
            // clear current sync
            Array.Fill(currentSector, (byte)0x00, 0x1, 0x0A);

            // remove MSF ..
            currentSector[0x0C] = 0x00; // M
            currentSector[0x0D] = 0x00; // S
            currentSector[0x0E] = 0x00; // F

            // remove ecc

            // (only if this is not form2mode2 sector!)
            if (!(currentSector[0xF] == 0x2 && (currentSector[0x12] & 0x20) == 0x20))
                Array.Fill(currentSector, (byte)0x00, 0x818, 0x118);
            else if (position > 0x9300) // only clear if its past the system section ..
                Array.Fill(currentSector, (byte)0x00, 0x92C, 0x4);
        }

        private int findSector(long position)
        {
            long len = position;
            len -= len % CueTrack.MODE2_SECTOR_SZ;
            int sector = Convert.ToInt32(len / CueTrack.MODE2_SECTOR_SZ);
            return sector;
        }

        private long sectorToPos(int sector)
        {
            return sector * CueTrack.MODE2_SECTOR_SZ;
        }

        private void seekToSector(int sector)
        {
            baseStream.Seek(sectorToPos(sector), SeekOrigin.Begin);
        }
        private void invalidateSectorCache()
        {

            int sector = findSector(position);
            seekToSector(sector);
            baseStream.Read(currentSector, 0x00, currentSector.Length);
            removeEcc();

        }
        public override void Close()
        {
            baseStream.Close();
            base.Close();
        }
        public override void Flush()
        {
            baseStream.Flush();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            int effectiveCount = count;

            if (Position > Length) return 0;

            if (Position + effectiveCount > Length) effectiveCount = Convert.ToInt32(Length - Position);

            if (effectiveCount <= remainInSector) // read the data from the cached sector
            {
                Array.ConstrainedCopy(currentSector, positionInSector, buffer, offset, effectiveCount);
            }
            else if (effectiveCount > remainInSector) // read 1 sector at a time until count reached
            {
                int remain = effectiveCount;
                int total = 0;

                while (remain > 0)
                {
                    int toRead = Math.Min(remain, remainInSector);
                    int totalRead = Read(buffer, total + offset, toRead);

                    if (totalRead < toRead) break;

                    remain -= totalRead;
                    total += totalRead;
                }

                return total;
            }

            Position += effectiveCount;
            return effectiveCount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                default:
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.Current:
                    Position += offset;
                    break;

                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }

            return position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException("EccRemoverStream is read only.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException("EccRemoverStream is read only.");
        }
    }
}
