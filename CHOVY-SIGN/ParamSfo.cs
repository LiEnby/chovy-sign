using System;
using System.Collections.Generic;
using System.IO;
using BasicDataTypes;

// A Sfo Parser Written by SilicaAndPina
// Because all the others are overly-complicated for no reason!
// MIT Licensed.

namespace ParamSfo
{
    
    class Sfo
    {
        const int PSF_TYPE_BIN = 0;
        const int PSF_TYPE_STR = 2;
        const int PSF_TYPE_VAL = 4;
        public static void WriteSfoString(Stream Sfo, string Option, String NewValue)
        {
            ReadSfo(Sfo, Option, NewValue);
        }

        public static Dictionary<String, Object> ReadSfo(Stream Sfo,string replaceOption="", string replaceValue="")
        {
            Dictionary<String, Object> SfoValues = new Dictionary<String, Object>();

            // Read Sfo Header
            UInt32 Magic = DataUtils.ReadUInt32(Sfo);
            UInt32 Version = DataUtils.ReadUInt32(Sfo);
            UInt32 KeyOffset = DataUtils.ReadUInt32(Sfo);
            UInt32 ValueOffset = DataUtils.ReadUInt32(Sfo);
            UInt32 Count = DataUtils.ReadUInt32(Sfo);

            if (Magic == 0x46535000) //\x00PSF
            {
                for(int i = 0; i < Count; i++)
                {
                    UInt16 NameOffset = DataUtils.ReadUInt16(Sfo);
                    Byte Alignment = (Byte)Sfo.ReadByte();
                    Byte Type = (Byte)Sfo.ReadByte();
                    UInt32 ValueSize = DataUtils.ReadUInt32(Sfo);
                    UInt32 TotalSize = DataUtils.ReadUInt32(Sfo);
                    UInt32 DataOffset = DataUtils.ReadUInt32(Sfo);

                    int KeyLocation = Convert.ToInt32(KeyOffset + NameOffset);
                    string KeyName = DataUtils.ReadStringAt(Sfo, KeyLocation);
                    int ValueLocation = Convert.ToInt32(ValueOffset + DataOffset);
                    object Value = "Unknown Type";

                    if(KeyName == replaceOption)
                    {
                        int padLen = Convert.ToInt32(TotalSize - replaceValue.Length);
                        DataUtils.WriteStringAt(Sfo, replaceValue, ValueLocation);

                        int newSz = replaceValue.Length + 1;
                        if(newSz > TotalSize)
                        {
                            throw new Exception("New Value is larger than Total Size.");
                        }

                        Sfo.Seek(-8, SeekOrigin.Current);
                        DataUtils.WriteInt32(Sfo, newSz);

                        byte[] zeros = new byte[padLen];
                        Sfo.Write(zeros, 0x00, zeros.Length);
                        return SfoValues;
                    }

                    switch (Type)
                    {
                        case PSF_TYPE_STR:
                            Value = DataUtils.ReadStringAt(Sfo, ValueLocation);
                            break; 

                        case PSF_TYPE_VAL:
                            Value = DataUtils.ReadUint32At(Sfo, ValueLocation + i);
                            break;

                        case PSF_TYPE_BIN:
                            Value = DataUtils.ReadBytesAt(Sfo,ValueLocation + i, Convert.ToInt32(ValueSize));
                            break;
                    }

                    SfoValues[KeyName] = Value;
                }
            }
            else
            {
                throw new InvalidDataException("Sfo Magic is Invalid.");
            }

             return SfoValues;
        }
        public static Dictionary<String,Object> ReadSfo(byte[] Sfo)
        {
            MemoryStream SfoStream = new MemoryStream(Sfo);
            return ReadSfo(SfoStream);
        }
    }
}
