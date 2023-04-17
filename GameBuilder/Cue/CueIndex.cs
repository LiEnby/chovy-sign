using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Cue
{
    public class CueIndex
    {
        public byte IndexNumber;
        public short Mrel;
        public short Srel;
        public short Frel;

        public short Mdelta;
        public short Sdelta;
        public short Fdelta;

        internal int mTtl
        {
            get
            {
                return (Mrel + Mdelta);
            }
        }

        internal int sTtl
        {
            get
            {
                return (Srel + Sdelta);
            }
        }

        internal int fTtl
        {
            get
            {
                return (Frel + Fdelta);
            }
        }

        public byte m
        {
            get
            {
                int carryF = Convert.ToInt32(Math.Floor(Convert.ToDouble(fTtl) / 75.0));
                int carryS = Convert.ToInt32(Math.Floor(Convert.ToDouble(sTtl + carryF) / 60.0));

                return Convert.ToByte(mTtl + carryS);
            }
        }

        public byte s
        {
            get
            {
                int carryF = Convert.ToInt32(Math.Floor(Convert.ToDouble(fTtl) / 75.0));

                return Convert.ToByte(((Srel + Sdelta) + carryF) % 60);
            }
        }

        public byte f
        {
            get
            {
                return Convert.ToByte((fTtl) % 75);
            }
        }
        public byte M 
        { 
            get
            {
                return CueReader.BinaryDecimalConv(m);
            } 
        }

        public byte S
        {
            get
            {
                return CueReader.BinaryDecimalConv(s);
            }
        }

        public byte F
        {
            get
            {
                return CueReader.BinaryDecimalConv(f);
            }
        }

        internal CueIndex(byte indexNumber)
        {
            IndexNumber = indexNumber;
            Mrel = 0;
            Srel = 0;
            Frel = 0;
            Mdelta = 0;
            Sdelta = 0;
            Fdelta = 0;
        }
    }
}
