namespace GameBuilder.Atrac3
{
    public interface IAtracEncoderBase
    {
        public abstract byte[] EncodeToAtrac(byte[] pcmData);
    }
}
