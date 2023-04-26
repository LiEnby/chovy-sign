using Li.Progress;
using Li.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Psp
{
    public class PbpBuilder : ProgressTracker , IDisposable
    {
        private byte[] paramSfo;
        private byte[] icon0Png;
        private byte[]? icon1Pmf;
        private byte[]? pic0Png;
        private byte[]? pic1Png;
        private byte[]? snd0At3;
        private NpDrmPsar psar;
        private short pbpVersion;
        private MemoryStream pbpStream;

        public PbpBuilder(byte[] paramSfo, byte[] icon0Png, byte[]? icon1Pmf,
                           byte[]? pic0Png, byte[]? pic1Png, byte[]? snd0At3,
                           NpDrmPsar dataPsar, short version = 1)
        {
            this.paramSfo = paramSfo;
            this.icon0Png = icon0Png;
            this.icon1Pmf = icon1Pmf;
            this.pic0Png = pic0Png;
            this.pic1Png = pic1Png;
            this.snd0At3 = snd0At3;
            this.psar = dataPsar;
            this.pbpVersion = version;

            pbpStream = new MemoryStream();
            psar.RegisterCallback(onProgress);
        }

        public MemoryStream PbpStream
        {
            get
            {
                return pbpStream;
            }
        }

        private void onProgress(ProgressInfo inf)
        {
            updateProgress(inf.Done, inf.Remain, inf.CurrentProcess);
        }

        public void CreatePsarAndPbp()
        {
            psar.CreatePsar();
            CreatePbp();
        }

        public void WritePbpToFile(string file)
        {
            using(FileStream pbpFile = File.OpenWrite(file))
            {
                pbpStream.Seek(0x00, SeekOrigin.Begin);
                copyToProgress(pbpStream, pbpFile, "Write to Disk");
            }
        }

        public void CreatePbp()
        {
            byte[] dataPsp = psar.GenerateDataPsp();

            int padLen = MathUtil.CalculatePaddingAmount(dataPsp.Length, 0x100);
            if(pbpVersion == 1)
                Array.Resize(ref dataPsp, dataPsp.Length + padLen);

            StreamUtil pbpUtil = new StreamUtil(pbpStream);
            pbpUtil.WriteByte(0x00);
            pbpUtil.WriteStr("PBP");
            pbpUtil.WriteInt16(pbpVersion);
            pbpUtil.WriteInt16(1);

            // param location
            uint loc = 0x28;
            if (paramSfo is null) { pbpUtil.WriteUInt32(loc); }
            else { pbpUtil.WriteUInt32(loc); loc += Convert.ToUInt32(paramSfo.Length); }

            // icon0 location
            if (icon0Png is null) { pbpUtil.WriteUInt32(loc); }
            else { pbpUtil.WriteUInt32(loc); loc += Convert.ToUInt32(icon0Png.Length); }

            // icon1 location
            if (icon1Pmf is null) { pbpUtil.WriteUInt32(loc); }
            else { pbpUtil.WriteUInt32(loc); loc += Convert.ToUInt32(icon1Pmf.Length); }

            // pic0 location
            if (pic0Png is null) { pbpUtil.WriteUInt32(loc); }
            else { pbpUtil.WriteUInt32(loc); loc += Convert.ToUInt32(pic0Png.Length); }

            // pic1 location
            if (pic1Png is null) { pbpUtil.WriteUInt32(loc); }
            else { pbpUtil.WriteUInt32(loc); loc += Convert.ToUInt32(pic1Png.Length); }

            // snd0 location
            if (snd0At3 is null) { pbpUtil.WriteUInt32(loc); }
            else { pbpUtil.WriteUInt32(loc); loc += Convert.ToUInt32(snd0At3.Length); }

            // datapsp location
            pbpUtil.WriteUInt32(loc); loc += Convert.ToUInt32(dataPsp.Length);

            // psar location
            pbpUtil.WriteUInt32(loc); loc += Convert.ToUInt32(psar.Psar.Length);

            // write pbp metadata
            if (paramSfo is not null) pbpUtil.WriteBytes(paramSfo);
            if (icon0Png is not null) pbpUtil.WriteBytes(icon0Png);
            if (icon1Pmf is not null) pbpUtil.WriteBytes(icon1Pmf);
            if (pic0Png is not null) pbpUtil.WriteBytes(pic0Png);
            if (pic1Png is not null) pbpUtil.WriteBytes(pic1Png);
            if (snd0At3 is not null) pbpUtil.WriteBytes(snd0At3);

            // write DATA.PSP
            pbpUtil.WriteBytes(dataPsp);

            // write DATA.PSAR
            copyToProgress(psar.Psar, pbpStream, "Build PBP");
        }


        public void Dispose()
        {
            psar.Dispose();
            pbpStream.Dispose();
        }
    }
}
