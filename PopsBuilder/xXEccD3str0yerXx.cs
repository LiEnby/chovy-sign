using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopsBuilder
{
    public class xXEccD3str0yerXx
    {
        const int SECTOR_SZ = 2352;
        const int COMPRESS_BLOCK_SZ = 0x9300;

        public static MemoryStream RemoveEcc(string iso)
        {
            using(FileStream eccIso = File.OpenRead(iso))
            {
                MemoryStream noEccIso = new MemoryStream();
                while (eccIso.Position < eccIso.Length)
                {
                    byte[] sector = new byte[SECTOR_SZ];

                    eccIso.Read(sector, 0, sector.Length);

                    // clear current sync
                    Array.Fill(sector, (byte)0x00, 0x1, 0x0A);

                    // remove MSF ..
                    sector[0x0C] = 0x00; // M
                    sector[0x0D] = 0x00; // S
                    sector[0x0E] = 0x00; // F

                    // remove ecc

                    // (only if this is not form2mode2 sector!)
                    if (!(sector[0xF] == 0x2 && (sector[0x12] & 0x20) == 0x20))
                        Array.Fill(sector, (byte)0x00, 0x818, 0x118);
                    else if(eccIso.Position > 0x9300) // only clear if its past the system section ..
                        Array.Fill(sector, (byte)0x00, 0x92C, 0x4);

                    noEccIso.Write(sector, 0, sector.Length);
                }

                // extend ISO image to compress block sz
                int padLen = COMPRESS_BLOCK_SZ - (Convert.ToInt32(noEccIso.Length) % COMPRESS_BLOCK_SZ);
                byte[] padding = new byte[padLen];
                noEccIso.Write(padding, 0x00, padding.Length);

                noEccIso.Seek(0, SeekOrigin.Begin);

                return noEccIso;
            }
        }
    }
}
