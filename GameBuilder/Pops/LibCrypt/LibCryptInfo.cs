using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Pops.LibCrypt
{
    public abstract class LibCryptInfo
    {
        private LibCryptMethod method;
        public virtual LibCryptMethod Method
        {
            get
            {
                return method;
            }
            set
            {
                if (MagicWord == 0) return;
                method = value;
            }
        }
        public abstract int MagicWord { get; }

        public virtual int ObfuscatedMagicWord
        {
            get
            {
                if (Method == LibCryptMethod.METHOD_SUB_CHANNEL) return 0;
                return LibCrypt.MagicWord.ObfuscateMagicWord(this.MagicWord);
            }
        }

        public virtual byte[] Subchannels
        {
            get
            {
                return LibCrypt.SubChannel.CreateSubchannelDat(this.MagicWord);
            }
        }
    }
}
