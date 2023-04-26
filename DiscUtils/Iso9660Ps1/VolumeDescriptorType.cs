namespace DiscUtils.Iso9660Ps1
{
    internal enum VolumeDescriptorType : byte
    {
        Boot = 0,
        Primary = 1,
        Supplementary = 2,
        Partition = 3,
        SetTerminator = 255
    }
}