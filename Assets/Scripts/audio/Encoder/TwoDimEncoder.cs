namespace audio.encoders
{
    /// <summary>
    /// Adds a directional computer checking the target is pointed, a noise interface and an optional stereo interface.
    /// </summary>
    public abstract class TwoDimEncoder : SSAudioGeneration
    {
        private bool pointsTarget;

        /// <summary>
        /// Method called before physical updating
        ///  Computes the vector from pointer to target, checks if the target is pointed.
        /// </summary>
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