using audio.interfaces;

namespace audio.computers
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
                audioInterface.setNoise(0.5f);
                audioInterface.setFrequency(0);
                active = true;
            }
        }

        public override void unsetNoise()
        {
            if (active)
            {
                audioInterface.setNoise(0);
                active = false;
            }
        }

    }

    /// <summary>
    /// Empty class for disabled option.
    /// </summary>
    public class Void : NoiseInterface
    {
        public override void setNoise() { }
        public override void unsetNoise() { }
    }

    

}