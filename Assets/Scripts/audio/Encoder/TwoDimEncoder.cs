namespace audio
{
    public abstract class TwoDimEncoder : SSAudioGeneration
    {
        
        void Update()
        {
            if (pointsTarget)
            {
                noise.setNoise(true);
            }
            else
            {
                noise.setNoise(false);
                computeOTvector();
                computeAngle();
                setFrequency();
                //setGain();
                setStereo();
            }
        }

        protected abstract void setStereo();
    }
}