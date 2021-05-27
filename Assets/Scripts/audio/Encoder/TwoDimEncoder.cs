namespace audio
{
    public abstract class TwoDimEncoder : SSAudioGeneration
    {
        protected bool pointsTarget;
        private void FixedUpdate()
        {
            pointsTarget = goodDirectionComputer.pointsTarget(transform.position, transform.forward);
        }

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