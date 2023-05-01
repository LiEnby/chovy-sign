using GameBuilder.Cue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Pops.LibCrypt
{
    public class DbLibCrypt : LibCryptInfo
    {
        private string discId;
        public override int MagicWord
        {
            get
            {
                return LibCrypt.MagicWord.LookupMagicWord(discId);
            }
        }

        public DbLibCrypt(string discId, LibCryptMethod method)
        {
            this.discId = discId;
            this.Method = method;
        }
    }
}
