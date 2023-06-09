using DiscUtils.Iso9660;
using DiscUtils.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBuilder.Psp
{
    public class UmdInfo : IDisposable
    {
        private string[] filesList = new string[]
        {
            Path.Combine("PSP_GAME","ICON0.PNG"),
            Path.Combine("PSP_GAME","ICON1.PMF"),
            Path.Combine("PSP_GAME","PARAM.SFO"),
            Path.Combine("PSP_GAME","PIC0.PNG"),
            Path.Combine("PSP_GAME","PIC1.PNG"),
            Path.Combine("PSP_GAME","SND0.AT3")
        };

        public Dictionary<string, byte[]?> DataFiles = new Dictionary<string, byte[]?>();
        public UmdInfo(string isoFile)
        {
            this.IsoFile = isoFile;
            this.IsoStream = File.OpenRead(isoFile);
            using (CDReader cdReader = new CDReader(this.IsoStream, true, true))
            {
                foreach (string file in this.filesList)
                {
                    string fname = Path.GetFileName(file).ToUpperInvariant();
                    if (cdReader.FileExists(file.Replace('/', '\\')))
                    {
                        using (SparseStream s = cdReader.OpenFile(file.Replace('/', '\\'), FileMode.Open))
                        {
                            byte[] data = new byte[s.Length];

                            s.Read(data, 0x00, data.Length);

                            DataFiles[fname] = data;
                        }
                    }
                    else
                    {
                        DataFiles[fname] = null;
                    }
                }
            }

            byte[]? paramFile = DataFiles["PARAM.SFO"];
            if (paramFile is null) throw new Exception("ISO contains no PARAM.SFO file, so this is not a valid PSP game.");
            
            Sfo sfo = Sfo.ReadSfo(paramFile);
            string? discId = sfo["DISC_ID"] as String;
            if (discId is null) throw new Exception("PARAM.SFO does not contain \"DISC_ID\"");

            this.DiscId = discId;

            // check minis
            if (sfo["ATTRIBUTE"] is UInt32)
                this.Minis = ((UInt32)sfo["ATTRIBUTE"] & 0b00000001000000000000000000000000) != 0;
            else 
                this.Minis = false;

            IsoStream.Seek(0x00, SeekOrigin.Begin);
        }


        public string IsoFile;
        public FileStream IsoStream;
        public bool Minis;
        public string DiscId;
        public string DiscIdSeperated
        {
            get
            {
                return this.DiscId.Substring(0, 4) + "-" + this.DiscId.Substring(4, 5);
            }
        }

        public void Dispose()
        {
            IsoStream.Dispose();
        }
    }
}
