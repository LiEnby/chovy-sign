/*using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CHOVY
{
    class KIRK
    {
        public MemoryStream kirk_buf = new MemoryStream(); // 1DC0 1DD4
        public unsafe struct MAC_KEY
        {
            public int type;
            public byte[] key;
            public byte[] pad;
            //public fixed byte key[0xF];
            //public fixed byte pad[0xF];
            public int pad_size;
        }
        
       
        public static int sceDrmBBMacInit(MAC_KEY mkey, int type)
        {
            mkey.type = type;
            mkey.pad_size = 0;

            mkey.key = new byte[0xF];
            mkey.pad = new byte[0xF];

            return 0;
        }




        int sceDrmBBMacUpdate(MAC_KEY mkey, Stream buf, int size)
        {
            int retv = 0;
            int ksize;
            int p;
            int type;
            kirk_buf.SetLength(0x0814);


            if (mkey.pad_size + size <= 16)
            {
                buf.Write(mkey.pad, 0x00, mkey.pad_size);
                buf.Seek(mkey.pad_size / -1, SeekOrigin.Current);
                mkey.pad_size += size;
                retv = 0;
            }
            else
            {
                //kbuf = kirk_buf + 0x14;
                kirk_buf.Seek(0x14, SeekOrigin.Begin);

                // copy pad data first
                kirk_buf.Write(mkey.pad, 0x0, mkey.pad_size);
                kirk_buf.Seek(mkey.pad_size / -1, SeekOrigin.Current);

                p = mkey.pad_size;

                mkey.pad_size += size;
                mkey.pad_size &= 0x0f;
                if (mkey.pad_size == 0)
                    mkey.pad_size = 16;

                size -= mkey.pad_size;
                // save last data to pad buf
                buf.Seek(size, SeekOrigin.Begin);
                buf.Write(mkey.pad, 0x00, mkey.pad_size);

                type = (mkey.type == 2) ? 0x3A : 0x38;

                while (size >= 0)
                {
                    ksize = (size + p >= 0x0800) ? 0x0800 : size + p;
                    kirk_buf.Seek(p,SeekOrigin.Current);
                    for(int i = 0; i < (ksize - p); i++)
                    {
                        byte by = (byte)buf.ReadByte();
                        kirk_buf.WriteByte(by);
                    }
                    retv = sub_158(kirk_buf, ksize, mkey.key, type);
                    if (retv)
                        goto _exit;
                    size -= (ksize - p);
                    buf.Seek(ksize - p,SeekOrigin.Current);
                    p = 0;
                }
            }

            _exit:
            return retv;

        }
        static int sub_158(byte[] buf, int size, byte[] key, int key_type)
        {
            int i, retv;

            for (i = 0; i < 16; i++)
            {
                buf[0x14 + i] ^= key[i];
            }

            retv = kirk4(buf, size, key_type);
            if (retv)
                return retv;

            // copy last 16 bytes to keys
            for (i = 0; i < 16; i++)
            {
                key[i] = buf[i + size + 4];
            } 

            return 0;
        }


        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int kirk_init();
      //  [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
     //   public unsafe static extern int sceDrmBBMacInit(MAC_KEY* mkey, int type);
        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int sceDrmBBMacUpdate(MAC_KEY* mkey, byte[] buf, int size);
        [DllImport("KIRK.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int bbmac_getkey(MAC_KEY* mkey, byte[] bbmac, byte[] vkey);


    }
}
*/