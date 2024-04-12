using DiscUtils.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiLib
{
    public class ChovyStream : Stream
    {
        private Stream underylingStream;

        public ChovyStream()
        {
            underylingStream = new MemoryStream();
        }

        public override bool CanRead {
            get {
                return underylingStream.CanRead;
            }
        }

        public override bool CanSeek {
            get {
                return underylingStream.CanSeek;
            }
        }

        public override bool CanWrite {
            get {
                return underylingStream.CanWrite;
            }
        }

        public override long Length {
            get {
                return underylingStream.Length;
            }
        }

        public override long Position {
            get {
                return underylingStream.Position;
            }
            set {
                underylingStream.Position = value;
            }
        }

        public override void Flush()
        {
            underylingStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return underylingStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return underylingStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            underylingStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            underylingStream.Write(buffer, offset, count);
        }
    }
}
