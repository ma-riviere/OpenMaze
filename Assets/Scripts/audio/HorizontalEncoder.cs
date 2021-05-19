namespace audio
{
    public class HorizontalEncoder : OneDEncoder
    {
        bool stereo;
        float kStereo, rightGain;
        const float bStereo = 0.5f;
        private void Awake()
        {
            init();
            angleComputer = new HorizontalComputer();
        }

        void Update()
        {
            compute();

            if (stereo)
            {
                rightGain = kStereo * angle + bStereo;
                puredataInstance.SendFloat("right", rightGain);
                puredataInstance.SendFloat("left", 1 - rightGain);
            }
        }

        protected override void initStereo(bool stereo)
        {
            this.stereo = stereo;
            kStereo = 1 / (maxAngle * 2);
        }
    }
}