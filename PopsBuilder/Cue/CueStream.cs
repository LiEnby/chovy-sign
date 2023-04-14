using Org.BouncyCastle.Tls.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopsBuilder.Cue
{
    public class CueStream : Stream
    {
        internal long position;
        internal long start;
        internal long end;
        internal long length;
        public bool IsClosed;

        private Stream baseStream;
        public CueStream(Stream s, long start, long length)
        {
            this.IsClosed = false;
            this.baseStream = s;
            this.start = start;
            this.length = length;
            this.end = start + length;

            s.Seek(start, SeekOrigin.Begin);
        }
        private long remainLength
        {
            get
            {
                return this.Length - this.Position;
            }
        }

        public override bool CanRead
        {
            get
            {
                return this.baseStream.CanRead;
            }
        }

        public override bool CanSeek
        {
             
            get
            {
                return this.baseStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.baseStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return this.length;
            }
        }

        public override long Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
                if (this.position > this.end) this.position = this.end;
                if (this.position < 0) this.position = 0;

                this.baseStream.Position = (start + this.position);
            }
        }
        private void seekToPos()
        {
            if (this.baseStream.Position != this.position) 
                this.baseStream.Seek(start + this.position, SeekOrigin.Begin);
        }
        public override void Close()
        {
            IsClosed = true;
            this.baseStream.Dispose();
            base.Close();
        }
        public override void Flush()
        {
            this.baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            seekToPos();

            int nCount = count;
            if (nCount > remainLength) nCount = Convert.ToInt32(remainLength);
            if (nCount < 0) nCount = 0;

            int read = this.baseStream.Read(buffer, offset, count);
            this.position += read;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                default:
                case SeekOrigin.Begin:
                    this.Position = offset;
                    break;

                case SeekOrigin.Current:
                    this.Position += offset;
                    break;

                case SeekOrigin.End:
                    this.Position = this.Length - offset;
                    break;
            }

            return position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException("Cannot set length of CueStream.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            seekToPos();

            int nCount = count;
            if (nCount > remainLength) nCount = Convert.ToInt32(remainLength);
            if (nCount < 0) nCount = 0;

            this.baseStream.Write(buffer, offset, count);
            this.position += nCount;
        }
    }
}
