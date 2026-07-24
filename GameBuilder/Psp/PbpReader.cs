using Li.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameBuilder.Psp
{
    public class PbpReader : IDisposable
    {
        // param location
        private int paramPtr = 0;
        private int paramSz = 0;
        
        // icon0 location
        private int icon0Ptr = 0;
        private int icon0Sz = 0;
        
        // icon1 location
        private int icon1Ptr = 0;
        private int icon1Sz = 0;
        
        // pic0 location
        private int pic0Ptr = 0;
        private int pic0Sz = 0;
        
        // pic1 location
        private int pic1Ptr = 0;
        private int pic1Sz = 0;
        
        // snd0 location
        private int snd0Ptr = 0;
        private int snd0Sz = 0;
        
        // datapsp location
        private int dataPspPtr = 0;
        private int dataPspSz = 0;
        
        // psar location
        private int psarPtr = 0;
        private int psarSz = 0;

        StreamUtil pbpUtil;
        public Stream BaseStream;
        public byte[] ParamSfo
        {
            get
            {
                return pbpUtil.ReadBytesAt(paramPtr, paramSz);
            }
        }

        public byte[] Icon0Png
        {
            get
            {
                return pbpUtil.ReadBytesAt(icon0Ptr, icon0Sz);
            }
        }

        public byte[]? Icon1Pmf
        {
            get
            {
                return pbpUtil.ReadBytesAt(icon1Ptr, icon1Sz);
            }
        }
        public byte[]? Pic0Png
        {
            get
            {
                return pbpUtil.ReadBytesAt(pic0Ptr, pic0Sz);
            }
        }
        public byte[]? Pic1Png
        {
            get
            {
                return pbpUtil.ReadBytesAt(pic1Ptr, pic1Sz);
            }
        }
        public byte[]? Snd0At3
        {
            get
            {
                return pbpUtil.ReadBytesAt(snd0Ptr, snd0Sz);
            }
        }

        public byte[]? DataPsp
        {
            get
            {
                return pbpUtil.ReadBytesAt(dataPspPtr, dataPspSz);
            }
        }
        public byte[]? DataPsar
        {
            get
            {
                return pbpUtil.ReadBytesAt(psarPtr, psarSz);
            }
        }

        public void SeekToParam()
        {
            BaseStream.Seek(paramPtr, SeekOrigin.Begin);
        }
        public void SeekToIcon0()
        {
            BaseStream.Seek(icon0Ptr, SeekOrigin.Begin);
        }
        public void SeekToIcon1()
        {
            BaseStream.Seek(icon1Ptr, SeekOrigin.Begin);
        }
        public void SeekToSnd0()
        {
            BaseStream.Seek(snd0Ptr, SeekOrigin.Begin);
        }
        public void SeekToDataPsp()
        {
            BaseStream.Seek(dataPspPtr, SeekOrigin.Begin);
        }
        public void SeekToPsar()
        {
            BaseStream.Seek(psarPtr, SeekOrigin.Begin);
        }

        void parsePbp(Stream pbpStream)
        {
            BaseStream = pbpStream;
            pbpUtil = new StreamUtil(BaseStream);

            
            string magic = pbpUtil.ReadCDStr(4);
            UInt16 version = pbpUtil.ReadUInt16();
            UInt16 version2 = pbpUtil.ReadUInt16();

            if(magic == "\0PBP")
            {
                // param location
                paramPtr     = pbpUtil.ReadInt32();
                paramSz      = pbpUtil.ReadInt32();

                // icon0 location
                icon0Ptr     = pbpUtil.ReadInt32();
                icon0Sz      = pbpUtil.ReadInt32();

                // icon1 location
                icon1Ptr     = pbpUtil.ReadInt32();
                icon1Sz      = pbpUtil.ReadInt32();

                // pic0 location
                pic0Ptr      = pbpUtil.ReadInt32();
                pic0Sz       = pbpUtil.ReadInt32();

                // pic1 location
                pic1Ptr      = pbpUtil.ReadInt32();
                pic1Sz       = pbpUtil.ReadInt32();

                // snd0 location
                snd0Ptr      = pbpUtil.ReadInt32();
                snd0Sz       = pbpUtil.ReadInt32();

                // datapsp location
                dataPspPtr   = pbpUtil.ReadInt32();
                dataPspSz    = pbpUtil.ReadInt32();

                // psar location
                psarPtr      = pbpUtil.ReadInt32();
                psarSz       = pbpUtil.ReadInt32();
            }



        }

        public void Dispose()
        {
            BaseStream.Dispose();

        }

        public PbpReader(string pbpFile)
        {
            parsePbp(File.OpenRead(pbpFile));
        }

        public PbpReader(byte[] pbpData)
        {
            parsePbp(new MemoryStream(pbpData));
        }

        public PbpReader(Stream pbpStream)
        {
            parsePbp(pbpStream);
        }

    }
}
