using GameBuilder.Psp;
using Li.Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameBuilder.Atrac3
{
    public class Atrac3ToolEncoder : IAtracEncoderBase
    {
        [DllImport("libc", SetLastError = true)]
        private static extern int setenv(string name, string value, bool overwrite);
        [DllImport("libc", SetLastError = true)]
        static extern int chmod(string pathname, int mode);


        private static string TOOLS_DIRECTORY = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools");

        private static string AT3TOOL_WIN = Path.Combine(TOOLS_DIRECTORY, "at3tool.exe");
        private static string AT3TOOL_LINUX = Path.Combine(TOOLS_DIRECTORY, "at3tool.elf");
        
        private static string TEMP_DIRECTORY = Path.Combine(Path.GetTempPath(), "at3tool_tmp");
        private static string LD_LIBRARY_PATH = "LD_LIBRARY_PATH";

        // random name so that can generate multiple at once if wanted ..
        private string tempWav;
        private string tempAt9;
        public Atrac3ToolEncoder()
        {
            string rdmPart = Rng.RandomStr(10);

            tempWav = Path.Combine(TEMP_DIRECTORY, rdmPart + "_tmp.wav");
            tempAt9 = Path.Combine(TEMP_DIRECTORY, rdmPart + "_tmp.at3");

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

        private string setupLibaryPath()
        {
            string? libaryPath = Environment.GetEnvironmentVariable(LD_LIBRARY_PATH);
            if (libaryPath is null) libaryPath = TOOLS_DIRECTORY;
            else libaryPath += ";" + TOOLS_DIRECTORY;

            // setup libraries for the file
            Environment.SetEnvironmentVariable(libaryPath, libaryPath);
            setenv(LD_LIBRARY_PATH, libaryPath, true);

            // make file executable
            chmod(AT3TOOL_LINUX, 755);

            return libaryPath;
        }
        private void runAtrac3Tool()
        {
            using(Process proc = new Process())
            {
                if (OperatingSystem.IsLinux())
                {

                    proc.StartInfo.Environment.Add(LD_LIBRARY_PATH, setupLibaryPath());

                }

                proc.StartInfo.FileName = AT3TOOL_LOCATION;
                proc.StartInfo.Arguments = "-br 132 -e \"" + tempWav + "\" \"" + tempAt9 + "\"";
                
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;

                proc.Start();
                string stdout = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                if (!stdout.Contains("Total Encoded Bytes"))
                    throw new Exception(stdout);
            }
        }

        private byte[] stripAtracHeader()
        {
            using(FileStream at3Stream = File.OpenRead(tempAt9))
            {
                StreamUtil at3Util = new StreamUtil(at3Stream);
                string magic = at3Util.ReadStrLen(4);
                int filesz = at3Util.ReadInt32();
                string riffType = at3Util.ReadStrLen(4);

                if(magic == "RIFF" && riffType == "WAVE")
                {
                    // read headers until we get data;
                    string blockName = "";
                    int blockSz = 0;
                    do
                    {
                        at3Stream.Seek(blockSz, SeekOrigin.Current);
                        blockName = at3Util.ReadStrLen(4);
                        blockSz = at3Util.ReadInt32();
                    } while (blockName != "data");

                    return at3Util.ReadBytes(blockSz);
                }

                throw new InvalidDataException("the encoded at3 file was not a RIFF");
            }
        }

        private void makeWav(byte[] pcmData)
        {
            using (FileStream wavStream = File.Open(tempWav, FileMode.Create))
            {
                // CD-AUDIO standard settings
                int fileSize = pcmData.Length;
                int samplerate = 44100;
                short channels = 2; // channels
                int fmtSize = 16;
                short format = 16; // signed, 16 bit PCM

                StreamUtil wavUtil = new StreamUtil(wavStream);

                wavUtil.WriteStr("RIFF");
                wavUtil.WriteInt32((fileSize + (0x2C - 8)));

                wavUtil.WriteStr("WAVE");
                wavUtil.WriteStr("fmt ");
                wavUtil.WriteInt32(fmtSize); 

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
            if (File.Exists(tempWav)) File.Delete(tempWav);
            if (File.Exists(tempAt9)) File.Delete(tempAt9);
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
