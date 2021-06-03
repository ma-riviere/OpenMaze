namespace audio
{
    public abstract class OneDEncoder : SSAudioGeneration
    {
        protected void compute()
        {
            computeOTvector();
            computeAngle();
            setFrequency();
        }

    }

}