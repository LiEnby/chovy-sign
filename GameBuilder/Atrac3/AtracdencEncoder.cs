namespace GameBuilder.Atrac3
{
    public class AtracdencEncoder : BinaryAtracEncoder
    {
        public override byte[] StripAtracHeader(string inFile)
        {
            // no need to strip atrac header since it just outputs raw atrac3 data.
            return File.ReadAllBytes(inFile);
        }
        public override string ProgramName
        {
            get
            {
                if (OperatingSystem.IsLinux())
                    return Path.Combine("atracdenc", "atracdenc.elf");
                else if (OperatingSystem.IsWindows())
                    return Path.Combine("atracdenc", "atracdenc.exe");
                else if (OperatingSystem.IsMacOS())
                    return Path.Combine("atracdenc", "atracdenc.mach");

                throw new PlatformNotSupportedException("no atracdenc for this platform");
            }
        }
 
        public override string ProgramArguments
        {
            get
            {
                return "-e atrac3 --container raw  -i \"{0}\" -o \"{1}\"";
            }
        }

        public override string ExpectedOutput
        {
            get
            {
                return "Done";
            }
        }
    }
}
