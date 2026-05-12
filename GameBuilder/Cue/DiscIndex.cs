namespace GameBuilder.Cue
{
    public class DiscIndex
    {
        public byte IndexNumber;


        public short MRelative;
        public short SRelative;
        public short FRelative;

        public short Mdelta;
        public short Sdelta;
        public short Fdelta;

        internal int mRelative
        {
            get
            {
                return MRelative;
            }
        }
        internal int sRelative
        {
            get
            {
                return SRelative;
            }
        }
        internal int fRelative
        {
            get
            {
                return FRelative;
            }
        }

        internal int mTotal
        {
            get
            {
                return (mRelative + Mdelta);
            }
        }

        internal int sTotal
        {
            get
            {
                return (sRelative + Sdelta);
            }
        }

        internal int fTotal
        {
            get
            {
                return (fRelative + Fdelta);
            }
        }

        public byte m
        {
            get
            {
                int carryF = Convert.ToInt32(Math.Floor(Convert.ToDouble(fTotal) / 75.0));
                int carryS = Convert.ToInt32(Math.Floor(Convert.ToDouble(sTotal + carryF) / 60.0));

                return Convert.ToByte(mTotal + carryS);
            }
        }

        public byte s
        {
            get
            {
                int carryF = Convert.ToInt32(Math.Floor(Convert.ToDouble(fTotal) / 75.0));

                return Convert.ToByte(((SRelative + Sdelta) + carryF) % 60);
            }
        }

        public byte f
        {
            get
            {
                return Convert.ToByte((fTotal) % 75);
            }
        }
        public byte M 
        { 
            get
            {
                return CueReader.DecimalToBinaryDecimal(m);
            } 
        }

        public byte S
        {
            get
            {
                return CueReader.DecimalToBinaryDecimal(s);
            }
        }

        public byte F
        {
            get
            {
                return CueReader.DecimalToBinaryDecimal(f);
            }
        }

        internal DiscIndex(byte indexNumber)
        {
            IndexNumber = indexNumber;
            
            MRelative = 0;
            SRelative = 0;
            FRelative = 0;
            
            Mdelta = 0;
            Sdelta = 0;
            Fdelta = 0;

        }
    }
}
