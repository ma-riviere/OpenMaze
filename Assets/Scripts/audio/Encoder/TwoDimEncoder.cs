namespace audio
{
    public abstract class TwoDimEncoder : SSAudioGeneration
    {
        private bool pointsTarget;
        private void FixedUpdate()
        {
            computeOTvector();
            pointsTarget = goodDirectionComputer.pointsTarget(transform.position, transform.forward, userToTargetVector);
        }

        void Update()
        {
            setGain();
            if (pointsTarget)
                noise.setNoise();
            else
            {
                noise.unsetNoise();
                computeAngle();
                setFrequency();
                setStereo();
            }
        }

        protected abstract void setStereo();

    }
}