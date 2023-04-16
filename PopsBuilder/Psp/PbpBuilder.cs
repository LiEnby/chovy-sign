using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Psp
{
    public static class PbpBuilder
    {
        public static void CreatePbp(byte[]? paramSfo, byte[]? icon0Png, byte[]? icon1Png,
                              byte[]? pic0Png, byte[]? pic1Png, byte[]? snd0At3,
                              NpDrmPsar dataPsar, string outputFile, short version = 1)
        {

            using (FileStream pbpStream = File.Open(outputFile, FileMode.Create))
            {
                byte[] dataPsp = dataPsar.GenerateDataPsp();

                int padLen = MathUtil.CalculatePaddingAmount(dataPsp.Length, 0x100);
                
                Array.Resize(ref dataPsp, dataPsp.Length + padLen);

                StreamUtil pbpUtil = new StreamUtil(pbpStream);
                pbpUtil.WriteByte(0x00);
                pbpUtil.WriteStr("PBP");
                pbpUtil.WriteInt16(version);
                pbpUtil.WriteInt16(1);

                // param location
                uint loc = 0x28;
                if (paramSfo is null) { pbpUtil.WriteUInt32(loc); }
                else { pbpUtil.WriteUInt32(loc); loc += Convert.ToUInt32(paramSfo.Length); }

                // icon0 location
                if (icon0Png is null) { pbpUtil.WriteUInt32(loc); }
                else { pbpUtil.WriteUInt32(loc); loc += Convert.ToUInt32(icon0Png.Length); }

                // icon1 location
                if (icon1Png is null) { pbpUtil.WriteUInt32(loc); }
                else { pbpUtil.WriteUInt32(loc); loc += Convert.ToUInt32(icon1Png.Length); }

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
                pbpUtil.WriteUInt32(loc); loc += Convert.ToUInt32(dataPsar.Psar.Length);

                // write pbp metadata
                if (paramSfo is not null) pbpUtil.WriteBytes(paramSfo);
                if (icon0Png is not null) pbpUtil.WriteBytes(icon0Png);
                if (icon1Png is not null) pbpUtil.WriteBytes(icon1Png);
                if (pic0Png is not null) pbpUtil.WriteBytes(pic0Png);
                if (pic1Png is not null) pbpUtil.WriteBytes(pic1Png);
                if (snd0At3 is not null) pbpUtil.WriteBytes(snd0At3);

                // write DATA.PSP
                pbpUtil.WriteBytes(dataPsp);

                // write DATA.PSAR
                dataPsar.Psar.Seek(0x00, SeekOrigin.Begin);
                dataPsar.Psar.CopyTo(pbpStream);
            }

        }


    }
}
