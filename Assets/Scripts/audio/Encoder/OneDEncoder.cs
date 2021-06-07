namespace audio.encoders
{
    /// <summary>
    /// Encodes the distance, and the directional difference on one plane.
    /// </summary>
    public abstract class OneDEncoder : SSAudioGeneration
    {
        protected void compute()
        {
            computeOTvector();
            computeAngle();
            setFrequency();
            setGain();
        }

    }

}