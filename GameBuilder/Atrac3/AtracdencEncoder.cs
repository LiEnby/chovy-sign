namespace GameBuilder.Atrac3
{
    public class AtracdencEncoder : BinaryAtracEncoder
    {
        public override string ProgramName
        {
            get
            {
                if (OperatingSystem.IsLinux())
                    return "atracdenc.elf";
                else if (OperatingSystem.IsWindows())
                    return "atracdenc.exe";
                else if (OperatingSystem.IsMacOS())
                    return "atracdenc.mach";

                throw new PlatformNotSupportedException("no atracdenc for this platform");
            }
        }
 
        public override string ProgramArguments
        {
            get
            {
                return "-e atrac3 --bitrate 132300 -i \"{0}\" -o \"{1}\"";
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
