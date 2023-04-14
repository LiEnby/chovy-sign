using CommunityToolkit.HighPerformance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PbpResign
{
    internal class Sfo
    {
        private struct SfoHeader
        {
            public uint Magic;
            public uint Version;
            public int KeyTableStart;
            public int DataTableStart;
            public int TablesEntries;
        }

        private struct SfoIndexTableEntry
        {
            public ushort KeyOffset;
            public ushort DataFormat;
            public int DataLen;
            public int DataMaxLen;
            public int DataOffset;
        }

        private const ushort PSF_TYPE_BIN = 0x0004;
        private const ushort PSF_TYPE_STR = 0x0204;
        private const ushort PSF_TYPE_VAL = 0x0404;

        public static Dictionary<string, object> ReadSfo(ReadOnlySpan<byte> sfo)
        {
            var dic = new Dictionary<string, object>();
            var hdr = MemoryMarshal.Read<SfoHeader>(sfo);
            if (hdr.Magic == 0x46535000)
            {
                dic = new Dictionary<string, object>(hdr.TablesEntries);
                var entries = MemoryMarshal.Cast<byte, SfoIndexTableEntry>(sfo.Slice(20, hdr.TablesEntries * 16));
                unsafe
                {
                    foreach (var entry in entries)
                    {
                        var keyOffset = hdr.KeyTableStart + entry.KeyOffset;
                        string keyName;
                        fixed (byte* ptr = sfo[keyOffset..])
                        {
                            var strData = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(ptr);
                            keyName = Encoding.UTF8.GetString(strData);
                        }

                        var dataOffset = hdr.DataTableStart + entry.DataOffset;
                        var dataLen = entry.DataLen;
                        var maxLen = entry.DataMaxLen;
                        var data = sfo.Slice(dataOffset, dataLen);

                        switch (entry.DataFormat)
                        {
                            case PSF_TYPE_BIN:
                                dic[keyName] = data.ToArray();
                                break;
                            case PSF_TYPE_STR:
                                dic[keyName] = Encoding.UTF8.GetString(data).TrimEnd('\0');
                                break;
                            case PSF_TYPE_VAL:
                                dic[keyName] = MemoryMarshal.Read<uint>(data);
                                break;
                        }
                    }
                }
            }
            return dic;
        }
    }
}
