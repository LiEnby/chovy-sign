using Org.BouncyCastle.Crypto.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PopsBuilder.Atrac3
{
    public class Atrac3ToolEncoder : IAtracEncoderBase
    {
        private static Random rng = new Random();
        private static string TOOLS_DIRECTORY = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools");

        private static string AT3TOOL_WIN = Path.Combine(TOOLS_DIRECTORY, "at3tool.exe");
        private static string AT3TOOL_LINUX = Path.Combine(TOOLS_DIRECTORY, "at3tool.elf");
        
        private static string TEMP_DIRECTORY = Path.Combine(Path.GetTempPath(), "at3tool_tmp");

        // random name so that can generate multiple at once if wanted ..
        private string TEMP_WAV;
        private string TEMP_AT3;
        public Atrac3ToolEncoder()
        {
            string rdmPart = rng.Next().ToString("X");

            TEMP_WAV = Path.Combine(TEMP_DIRECTORY, rdmPart + "_tmp.wav");
            TEMP_AT3 = Path.Combine(TEMP_DIRECTORY, rdmPart + "_tmp.at3");

        }
         
        private static string AT3TOOL_LOCATION
        {
            get
            {
                if (OperatingSystem.IsWindows())
                    return AT3TOOL_WIN;
                else if (OperatingSystem.IsLinux())
                    return AT3TOOL_LINUX;
                else
                    throw new PlatformNotSupportedException("No at3tool binary for your platform");
            }
        }

        private void runAtrac3Tool()
        {
            using(Process proc = new Process())
            {
                proc.StartInfo.FileName = AT3TOOL_LOCATION;
                proc.StartInfo.Arguments = "-br 132 -e \"" + TEMP_WAV + "\" \"" + TEMP_AT3 + "\"";

                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;

                proc.Start();
                proc.WaitForExit();

                string stdout = proc.StandardOutput.ReadToEnd();
                if (!stdout.Contains("Total Encoded Bytes"))
                    throw new Exception(stdout);
            }
        }

        private byte[] stripAtracHeader()
        {
            using(FileStream at3Stream = File.OpenRead(TEMP_AT3))
            {
                StreamUtil at3Util = new StreamUtil(at3Stream);
                at3Stream.Seek(0x4C, SeekOrigin.Begin);
                int at3Len = at3Util.ReadInt32();
                return at3Util.ReadBytes(at3Len);
            }
        }

        private void makeWav(byte[] pcmData)
        {
            using (FileStream wavStream = File.Open(TEMP_WAV, FileMode.Create))
            {
                // CD-AUDIO standard settings
                int fileSize = pcmData.Length;
                int samplerate = 44100;
                short channels = 2; // channels
                short format = 16; // signed, 16 bit PCM

                StreamUtil wavUtil = new StreamUtil(wavStream);

                wavUtil.WriteStr("RIFF");
                wavUtil.WriteInt32((fileSize + (0x2C - 8)));

                wavUtil.WriteStr("WAVE");
                wavUtil.WriteStr("fmt ");
                wavUtil.WriteInt32(format); 

                wavUtil.WriteInt16(1);
                wavUtil.WriteInt16(channels); 
                wavUtil.WriteInt32(samplerate); 
                wavUtil.WriteInt32((samplerate * format * channels) / 8);
                wavUtil.WriteInt16(Convert.ToInt16(format * channels));
                wavUtil.WriteInt16(format);

                wavUtil.WriteStr("data");
                wavUtil.WriteInt32(fileSize);
                wavUtil.WriteBytes(pcmData);
            }
        }

        private void ensureFilesAvailable()
        {
            if (!Directory.Exists(TEMP_DIRECTORY))
                Directory.CreateDirectory(TEMP_DIRECTORY);

            if (!Directory.Exists(TOOLS_DIRECTORY))
                Directory.CreateDirectory(TOOLS_DIRECTORY);

            if (OperatingSystem.IsWindows())
            {
                if (!File.Exists(AT3TOOL_WIN))
                {
                    throw new FileNotFoundException("Cannot find at3tool at " + AT3TOOL_WIN);
                }
            }
            else if(OperatingSystem.IsLinux())
            {
                if (!File.Exists(AT3TOOL_LINUX))
                {
                    throw new FileNotFoundException("Cannot find at3tool at " + AT3TOOL_LINUX);
                }
            }
        }

        private void cleanup()
        {
            if (File.Exists(TEMP_WAV)) File.Delete(TEMP_WAV);
            if (File.Exists(TEMP_AT3)) File.Delete(TEMP_AT3);
        }

        public byte[] EncodeToAtrac(byte[] pcmData)
        {
            ensureFilesAvailable();

            makeWav(pcmData);
            runAtrac3Tool();
            byte[] rawAtracData = stripAtracHeader();
            
            cleanup();

            return rawAtracData;
        }
    }
}
