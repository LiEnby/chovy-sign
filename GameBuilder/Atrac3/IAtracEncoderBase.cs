namespace GameBuilder.Atrac3
{
    public interface IAtracEncoderBase
    {
        public byte[] EncodeToAtrac(byte[] pcmData);
    }
}
