using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PsvImage
{
    static class Utils
    {
        public static SceDateTime ToSceDateTime(this DateTime dateTime)
        {
            var sceDateTime = new SceDateTime();
            sceDateTime.Year = (ushort)dateTime.Year;
            sceDateTime.Month = (ushort)dateTime.Month;
            sceDateTime.Day = (ushort)dateTime.Day;
            sceDateTime.Hour = (ushort)dateTime.Hour;
            sceDateTime.Minute = (ushort)dateTime.Minute;
            sceDateTime.Second = (ushort)dateTime.Second;
            sceDateTime.Microsecond = (uint)dateTime.Millisecond * 1000;
            return sceDateTime;
        }

        public static SceIoStat ToSceIoStat(string path)
        {
            var stats = new SceIoStat();
            var attributes = File.GetAttributes(path);

            if (attributes.HasFlag(FileAttributes.Directory))
            {
                stats.Mode |= SceIoStat.Modes.Directory;
                stats.Size = 0;
            }
            else
            {
                stats.Mode |= SceIoStat.Modes.File;
                stats.Size = (ulong)(new FileInfo(path).Length);
            }

            if (attributes.HasFlag(FileAttributes.ReadOnly))
            {
                stats.Mode |= SceIoStat.Modes.GroupRead;
                stats.Mode |= SceIoStat.Modes.OthersRead;
                stats.Mode |= SceIoStat.Modes.UserRead;
            }
            else
            {
                stats.Mode |= SceIoStat.Modes.GroupRead;
                stats.Mode |= SceIoStat.Modes.GroupWrite;
                stats.Mode |= SceIoStat.Modes.OthersRead;
                stats.Mode |= SceIoStat.Modes.OthersWrite;
                stats.Mode |= SceIoStat.Modes.UserRead;
                stats.Mode |= SceIoStat.Modes.UserWrite;
            }

            stats.CreationTime = File.GetCreationTimeUtc(path).ToSceDateTime();
            stats.AccessTime = File.GetLastAccessTimeUtc(path).ToSceDateTime();
            stats.ModificaionTime = File.GetLastWriteTimeUtc(path).ToSceDateTime();

            return stats;
        }
    }
}
