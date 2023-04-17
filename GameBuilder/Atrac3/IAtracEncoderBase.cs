using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Atrac3
{
    public interface IAtracEncoderBase
    {
        public byte[] EncodeToAtrac(byte[] pcmData);
    }
}
