using GameBuilder.Cue;
using Li.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Pops.LibCrypt
{
    public class SubChannel
    {
        public static byte[] CreateSubchannelDat(int magicWord)
        {
            using(MemoryStream subChannels = new MemoryStream())
            {
                StreamUtil subChannelsUtil = new StreamUtil(subChannels);
                // this header seems to mark the start of the sub channel data.
                subChannelsUtil.WriteUInt32(0xFFFFFFFF);
                subChannelsUtil.WriteUInt32(0x00000000);
                subChannelsUtil.WriteUInt32(0xFFFFFFFF);


                for (int i = 0; i < Constants.LIBCRYPT_PAIRS.Length; i++)
                {
                    if ((magicWord & (1 << ((Constants.LIBCRYPT_PAIRS.Length - 1) - i))) != 0)
                    {
                        int[] pair = Constants.LIBCRYPT_PAIRS[i];

                        foreach (int lcSector in pair)
                        {
                            DiscIndex sidx = CueReader.SectorToIdx(lcSector, 0);
                            sidx.Sdelta = -2;
                            int adjustedSector = CueReader.IdxToSector(sidx);

                            // write sector number offset by 2s
                            subChannelsUtil.WriteInt32(adjustedSector);
                            subChannelsUtil.WriteByte(0x01);
                            subChannelsUtil.WriteByte(0x01);

                            sidx.Fdelta = -1;
                            sidx.Sdelta = -2;

                            // write sector index 1 but corrupted
                            subChannelsUtil.WriteByte(sidx.M);
                            subChannelsUtil.WriteByte(sidx.S);
                            subChannelsUtil.WriteByte(sidx.F);

                            sidx.Sdelta = 0;

                            // write sector index 0 but corrupted
                            subChannelsUtil.WriteByte(sidx.M);
                            subChannelsUtil.WriteByte(sidx.S);
                            subChannelsUtil.WriteByte(sidx.F);
                        }
                    }

                }

                // this header seems to mark the end of the sub channel data.
                subChannelsUtil.WriteUInt32(0xFFFFFFFF);
                subChannelsUtil.WriteUInt32(0xFFFFFFFF);
                subChannelsUtil.WriteUInt32(0xFFFFFFFF);

                return subChannels.ToArray();
            }
        }
    }
}
