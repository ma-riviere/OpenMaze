namespace audio
{
    public interface AudioInterface
    {
        void setFreq(float value);
        void setStereo(float value1, float value2);
        void setGain(float value);
        void setHits(float value);
    }

    public class PureDataSender : AudioInterface
    {
        private LibPdInstance instance;
        public PureDataSender(LibPdInstance ins)
        {
            instance = ins;
        }

        public void setFreq(float value)
        {
            instance.SendFloat("freq", value);
        }

        public void setStereo(float left, float right)
        {
            instance.SendFloat("right", right);
            instance.SendFloat("left", left);
        }

        public void setGain(float gain)
        {
            instance.SendFloat("gain", gain);
        }

        public void setHits(float hits)
        {
            instance.SendFloat("hits", hits);
        }
    }
}