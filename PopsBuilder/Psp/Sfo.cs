using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Runtime.CompilerServices;

// A Sfo Parser Written by SilicaAndPina
// Because all the others are overly-complicated for no reason!
// MIT Licensed.

namespace GameBuilder.Psp
{
    
    public class Sfo
    {

        private struct SfoEntry
        {
            internal string keyName;
            internal byte type;
            internal UInt32 valueSize;
            internal UInt32 totalSize;
            internal byte align;
            internal object value;
        }

        const int SFO_MAGIC = 0x46535000;
        const byte PSF_TYPE_BIN = 0;
        const byte PSF_TYPE_STR = 2;
        const byte PSF_TYPE_VAL = 4;

        private Dictionary<string, SfoEntry> sfoEntries;
        public Object this[string index]
        {
            get
            {
                return sfoEntries[index].value;
            }
            set
            {
                SfoEntry sfoEnt = sfoEntries[index];
                sfoEnt.value = value;

                // update sz
                sfoEnt.valueSize = getObjectSz(sfoEnt.value);

                if (sfoEnt.valueSize > sfoEnt.totalSize)
                    sfoEnt.totalSize = Convert.ToUInt32(MathUtil.CalculatePaddingAmount(Convert.ToInt32(sfoEnt.valueSize), sfoEnt.align));
                
                // update type
                sfoEnt.type = getPsfType(sfoEnt.value);

                sfoEntries[index] = sfoEnt;
            }
        }
        public Sfo()
        {
            sfoEntries = new Dictionary<string, SfoEntry>();
        }


        private static UInt32 getObjectSz(Object obj)
        {
            if (obj is Int32)  return 4;
            if (obj is UInt32) return 4;
            if (obj is String) return Convert.ToUInt32((obj as String).Length+1);
            if (obj is Byte[]) return Convert.ToUInt32((obj as Byte[]).Length);
            throw new Exception("Object is of unsupported type: " + obj.GetType());
        }

        private static byte getPsfType(Object obj)
        {
            if (obj is Int32 || obj is UInt32)  return PSF_TYPE_VAL;
            if (obj is String)                  return PSF_TYPE_STR;
            if (obj is Byte[])                  return PSF_TYPE_BIN;
            throw new Exception("Object is of unsupported type: " + obj.GetType());
        }

        public byte[] WriteSfo(UInt32 version = 0x101, Byte align = 0x4)
        {
            using (MemoryStream sfoStream = new MemoryStream())
            {
                WriteSfo(sfoStream, version, align);
                byte[] sfoBytes = sfoStream.ToArray();
                return sfoBytes;
            }
        }
        public void WriteSfo(Stream SfoStream, UInt32 version=0x101, Byte align=0x4)
        {
            using(MemoryStream sfoStream = new MemoryStream())
            {
                StreamUtil sfoUtil = new StreamUtil(sfoStream);

                sfoUtil.WriteUInt32(SFO_MAGIC);
                sfoUtil.WriteUInt32(version);

                sfoUtil.WriteUInt32(0xFFFFFFFF); // key offset
                sfoUtil.WriteUInt32(0xFFFFFFFF); // value offset 
                // (will fill these in after the file is created)

                sfoUtil.WriteInt32(sfoEntries.Count);

                using (MemoryStream keyTable = new MemoryStream())
                {
                    StreamUtil keyUtils = new StreamUtil(keyTable);
                    using (MemoryStream valueTable = new MemoryStream())
                    {
                        StreamUtil valueUtils = new StreamUtil(valueTable);
                        foreach (SfoEntry entry in sfoEntries.Values)
                        {
                            // write name
                            sfoUtil.WriteUInt16(Convert.ToUInt16(keyTable.Position));
                            keyUtils.WriteCStr(entry.keyName);



                            // write entry

                            sfoUtil.WriteByte(align); // align
                            sfoUtil.WriteByte(entry.type); // type
                            sfoUtil.WriteUInt32(entry.valueSize); // valueSize
                            sfoUtil.WriteUInt32(entry.totalSize); // totalSize

                            // write data
                            sfoUtil.WriteUInt32(Convert.ToUInt32(valueTable.Position)); // dataOffset

                            switch (entry.type)
                            {
                                case PSF_TYPE_VAL:
                                    valueUtils.WriteUInt32(Convert.ToUInt32(entry.value));
                                    valueUtils.WritePadding(0x00, Convert.ToInt32(entry.totalSize - entry.valueSize));
                                    break;
                                case PSF_TYPE_STR:
                                    valueUtils.WriteStrWithPadding(entry.value as String, 0x00, Convert.ToInt32(entry.totalSize));
                                    break;
                                case PSF_TYPE_BIN:
                                    valueUtils.WriteBytesWithPadding(entry.value as Byte[], 0x00, Convert.ToInt32(entry.totalSize));
                                    break;
                            }
                        }


                        keyUtils.AlignTo(0x00, align);
                        UInt32 keyOffset = Convert.ToUInt32(sfoStream.Position);
                        keyTable.Seek(0x00, SeekOrigin.Begin);
                        keyTable.CopyTo(sfoStream);

                        UInt32 valueOffset = Convert.ToUInt32(sfoStream.Position);
                        valueTable.Seek(0x00, SeekOrigin.Begin);
                        valueTable.CopyTo(sfoStream);

                        sfoStream.Seek(0x8, SeekOrigin.Begin);
                        sfoUtil.WriteUInt32(keyOffset); // key offset
                        sfoUtil.WriteUInt32(valueOffset); // value offset 
                    }
                }

                sfoStream.Seek(0x0, SeekOrigin.Begin);
                sfoStream.CopyTo(SfoStream);
            }            

        }

        public static Sfo ReadSfo(Stream SfoStream)
        {
            Sfo sfoFile = new Sfo();
            StreamUtil DataUtils = new StreamUtil(SfoStream);

            // Read Sfo Header
            UInt32 magic = DataUtils.ReadUInt32();
            UInt32 version = DataUtils.ReadUInt32();
            UInt32 keyOffset = DataUtils.ReadUInt32();
            UInt32 valueOffset = DataUtils.ReadUInt32();
            UInt32 count = DataUtils.ReadUInt32();

            if (magic == SFO_MAGIC) //\x00PSF
            {
                for(int i = 0; i < count; i++)
                {
                    SfoEntry entry = new SfoEntry();

                    UInt16 nameOffset = DataUtils.ReadUInt16();
                    entry.align =       DataUtils.ReadByte();
                    entry.type =        DataUtils.ReadByte();
                    entry.valueSize =   DataUtils.ReadUInt32();
                    entry.totalSize =   DataUtils.ReadUInt32();
                    UInt32 dataOffset = DataUtils.ReadUInt32();

                    int keyLocation = Convert.ToInt32(keyOffset + nameOffset);
                    entry.keyName = DataUtils.ReadStringAt(keyLocation);
                    int valueLocation = Convert.ToInt32(valueOffset + dataOffset);


                    switch (entry.type)
                    {
                        case PSF_TYPE_STR:
                            entry.value = DataUtils.ReadStringAt(valueLocation);
                            break; 

                        case PSF_TYPE_VAL:
                            entry.value = DataUtils.ReadUint32At(valueLocation);
                            break;

                        case PSF_TYPE_BIN:
                            entry.value = DataUtils.ReadBytesAt(valueLocation, Convert.ToInt32(entry.valueSize));
                            break;
                    }


                    sfoFile.sfoEntries[entry.keyName] = entry;
                }

            }
            else
            {
                throw new InvalidDataException("Sfo Magic is Invalid.");
            }

             return sfoFile;
        }

        public static Sfo ReadSfo(byte[] Sfo)
        {
            using (MemoryStream SfoStream = new MemoryStream(Sfo))
            {
                return ReadSfo(SfoStream);
            }
        }
    }
}
