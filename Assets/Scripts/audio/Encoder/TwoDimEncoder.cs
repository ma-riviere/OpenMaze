namespace audio
{
    public abstract class TwoDimEncoder : SSAudioGeneration
    {
        void Update()
        {
            if (pointsTarget)
            {
                if (!previousPointsTarget)
                {
                    puredataInstance.setHits(0.5f);
                    previousPointsTarget = true;
                    puredataInstance.setFreq(0);
                }
            }
            else
            {
                if (previousPointsTarget)
                {
                    puredataInstance.setHits(0);
                    previousPointsTarget = false;
                }
                computeOTvector();
                computeAngle();
                setFrequency();
                setContinuousGain();
                setStereo();
            }
        }

        protected abstract void setStereo();
    }
}