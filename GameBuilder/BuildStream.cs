using DiscUtils.Streams;
using GameBuilder.Psp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder
{
    public class BuildStream : Stream
    {
        public static StreamType BuildUsingStreamType = StreamType.TYPE_MEMORY_STREAM;
        private Stream underylingStream;

        private StreamType uStreamType;
        private string? filename = null;

        private void init()
        {
            this.uStreamType = BuildUsingStreamType;
            if (this.uStreamType == StreamType.TYPE_MEMORY_STREAM)
            {
                this.underylingStream = new MemoryStream();
            }
            else if (this.uStreamType == StreamType.TYPE_FILE_STREAM)
            {
                string tmpFolder = Path.Combine(Path.GetTempPath(), "chovysign2");
                Directory.CreateDirectory(tmpFolder);

                this.filename = Path.Combine(tmpFolder, Rng.RandomStr(10));
                this.underylingStream = File.Create(this.filename);
            }
            else
            {
                throw new Exception("unknown stream type");
            }
        }

        public BuildStream(byte[] data)
        {
            init();
            this.Write(data, 0, data.Length);
            this.Seek(0x00, SeekOrigin.Begin);
            if (this.underylingStream == null) throw new NullReferenceException("somehow underlying stream is null");
        }
        public BuildStream()
        {
            init();
            if (this.underylingStream == null) throw new NullReferenceException("somehow underlying stream is null");
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

        public override void Close()
        {
            this.underylingStream.Close();

            if(this.uStreamType == StreamType.TYPE_FILE_STREAM && this.filename is not null)
                File.Delete(this.filename);

            base.Close();

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

        public byte[] ToArray()
        {
            long oldLocation = this.Position;

            this.Seek(0x00, SeekOrigin.Begin);

            byte[] rdData = new byte[this.Length];
            this.Read(rdData, 0x00, rdData.Length);

            this.Seek(oldLocation, SeekOrigin.Begin);

            return rdData;
        }
    }
}
