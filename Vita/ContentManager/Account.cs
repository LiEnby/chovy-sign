using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vita.ContentManager
{
    public class Account
    {
        public Account(UInt64 accountId)
        {
            this.accountId = accountId;
        }
        private UInt64 accountId;
        public bool Devkit;

        public byte[] AccountId
        {
            get
            {
                if (Devkit) return new byte[8];
                return BitConverter.GetBytes(accountId);
            }
        }
        public string AccountIdStr
        {
            get
            {
                return BitConverter.ToString(AccountId).Replace("-", "").ToLowerInvariant();
            }
        }

        public string CmaKeyStr
        {
            get
            {
                return KeyGenerator.GenerateKeyStr(AccountIdStr);
            }
        }

        public byte[] CmaKey
        {
            get
            {
                return KeyGenerator.GenerateKey(AccountId);
            }
        }


    }
}
