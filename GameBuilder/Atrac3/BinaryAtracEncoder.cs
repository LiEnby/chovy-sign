using GameBuilder.Psp;
using Li.Utilities;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameBuilder.Atrac3
{
    public abstract class BinaryAtracEncoder : IAtracEncoderBase
    {
        public abstract string ProgramName { get; }
        public abstract string ProgramArguments { get; }


        [DllImport("libc", SetLastError = true)]
        private static extern int setenv(string name, string value, bool overwrite);
        [DllImport("libc", SetLastError = true)]
        private static extern int chmod(string pathname, int mode);

        public BinaryAtracEncoder()
        {
            ensureFilesAvailable();
        }
        ~BinaryAtracEncoder()
        {
            cleanup();
        }
        private const string LD_LIBRARY_PATH = "LD_LIBRARY_PATH";
        private string tmp_random = Rng.RandomStr(0x10);

        private string programFilepath
        {
            get
            {
                return Path.Combine(toolsDir, ProgramName);
            }
        }
        internal string toolsDir {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools");
            }
        }
        internal string tempDir
        {
            get
            {
                return Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(programFilepath) + "_tmp");
            }
        }
        internal string workingInput
        {
            get
            {
                return Path.Combine(tempDir, tmp_random + "_tmp.wav");
            }
        }
        internal string workingOutput
        {
            get
            {
                return Path.Combine(tempDir, tmp_random + "_tmp.at3");
            }
        }


        private string setupLibaryPath()
        {
            string? libaryPath = Environment.GetEnvironmentVariable(LD_LIBRARY_PATH);
            if (libaryPath is null) libaryPath = toolsDir;
            else libaryPath += ";" + toolsDir;

            // setup libraries for the file
            Environment.SetEnvironmentVariable(LD_LIBRARY_PATH, libaryPath);
            setenv(LD_LIBRARY_PATH, libaryPath, true);

            // make file executable
            chmod(programFilepath, 755);

            return libaryPath;
        }
        public virtual void RunProgram()
        {
            using (Process proc = new Process())
            {
                if (OperatingSystem.IsLinux())
                {
                    string library_path = setupLibaryPath();
                    proc.StartInfo.Environment.Add(LD_LIBRARY_PATH, library_path);
                }

                proc.StartInfo.WorkingDirectory = toolsDir;
                proc.StartInfo.FileName = programFilepath;
                proc.StartInfo.Arguments = String.Format(ProgramArguments, workingInput, workingOutput);

                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;

                proc.Start();
                proc.WaitForExit();

                if (proc.ExitCode != 0) throw new Exception("atrac encode process was unsuccessful exit code: "+proc.ExitCode);
            }
        }

        public virtual byte[] StripAtracHeader(string inFile)
        {
            using (FileStream at3Stream = File.OpenRead(inFile))
            {
                StreamUtil at3Util = new StreamUtil(at3Stream);
                string magic = at3Util.ReadStrLen(4);
                int filesz = at3Util.ReadInt32();
                string riffType = at3Util.ReadStrLen(4);

                if (magic == "RIFF" && riffType == "WAVE")
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

        public virtual void MakeWav(byte[] pcmData, string outputFile)
        {
            using (FileStream wavStream = File.Open(outputFile, FileMode.Create))
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
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            if (!Directory.Exists(toolsDir))
                Directory.CreateDirectory(toolsDir);

            if (!File.Exists(programFilepath))
                throw new FileNotFoundException("Cannot find "+ProgramName+" in \"" + toolsDir+"\"");

            tmp_random = Rng.RandomStr(10);
        }

        private void cleanup()
        {
            if (File.Exists(workingInput)) File.Delete(workingInput);
            if (File.Exists(workingOutput)) File.Delete(workingOutput);
        }

        public byte[] EncodeToAtrac(byte[] pcmData)
        {
            ensureFilesAvailable();

            MakeWav(pcmData, workingInput);
            RunProgram();
            byte[] rawAtracData = StripAtracHeader(workingOutput);

            cleanup();
            return rawAtracData;
        }
    }
}
