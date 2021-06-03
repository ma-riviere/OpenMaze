namespace audio
{
    public abstract class TwoDimEncoder : SSAudioGeneration
    {
        private bool pointsTarget;
        private void FixedUpdate()
        {
            pointsTarget = goodDirectionComputer.pointsTarget(transform.position, transform.forward);
        }

        void Update()
        {
            if (pointsTarget)
                noise.setNoise();
            else
            {
                noise.unsetNoise();
                computeOTvector();
                computeAngle();
                setFrequency();
                setGain();
                setStereo();
            }
        }

        protected abstract void setStereo();

    }
}