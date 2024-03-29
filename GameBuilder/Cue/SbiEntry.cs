﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Cue
{
    public class SbiEntry
    {
        public DiscIndex MSF;
        public DiscTrack TOC;
        public int Sector
        {
            get
            {
                return CueReader.IdxToSector(MSF);
            }
        }

        public SbiEntry(DiscIndex Msf, DiscTrack Toc)
        {
            MSF = Msf;
            TOC = Toc;
        }

    }
}
