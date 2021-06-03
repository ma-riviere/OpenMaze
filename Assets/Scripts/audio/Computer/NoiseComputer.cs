namespace audio.Computer
{
    public abstract class NoiseInterface : InternalAudioInterface
    {
        public abstract void setNoise();
        public abstract void unsetNoise();
    }

    public class Noise : NoiseInterface
    {
        private bool active;
        public Noise()
        {
            active = false;
        }

        public override void setNoise()
        {
            if (!active)
            {
                audioInterface.setHits(0.5f);
                audioInterface.setFreq(0);
                active = true;
            }
        }

        public override void unsetNoise()
        {
            if (active)
            {
                audioInterface.setHits(0);
                active = false;
            }
        }

    }

    public class Void : NoiseInterface
    {
        public override void setNoise() { }
        public override void unsetNoise() { }
    }

    

}