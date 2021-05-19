namespace audio
{
    public abstract class OneDEncoder : SSAudioGeneration
    {
        void Awake()
        {
            init();
        }
        protected void compute()
        {
            computeOTvector();
            computeAngle();
            setFrequency(angle);
            puredataInstance.SendFloat("freq", frequency);
        }

        protected override void initStereo(bool stereo)
        {
        }

    }
}