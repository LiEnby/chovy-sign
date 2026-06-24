namespace GameBuilder.Atrac3
{
    public class Atrac3ToolEncoder : BinaryAtracEncoder
    {
        public override string ProgramName
        {
            get
            {
                if (OperatingSystem.IsWindows()) return Path.Combine("at3tool", "at3tool.exe");
                else if (OperatingSystem.IsLinux()) return Path.Combine("at3tool", "at3tool.elf");

                throw new PlatformNotSupportedException("No at3tool for your platform!");
            }
        }

        public override string ProgramArguments
        {
            get
            {
                return "-br 132 -e \"{0}\" \"{1}\"";
            }
        }

        public override string ExpectedOutput 
        {
            get
            {
                return "Total Encoded Bytes";
            }
        }

    }
}
