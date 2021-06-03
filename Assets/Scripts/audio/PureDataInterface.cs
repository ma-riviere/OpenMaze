namespace audio
{
    public interface AudioInterface
    {
        void setFreq(float value);
        void setStereo(float value1, float value2);
        void setLeft(float v);
        void setRight(float v);
        void setGain(float value);
        void setHits(float value);
        void setComplete();

    }

    public abstract class InternalAudioInterface
    {
        public AudioInterface audioInterface;
    }

    public class PureDataSender : AudioInterface
    {
        private LibPdInstance instance;
        public PureDataSender(LibPdInstance ins)
        {
            instance = ins;
        }

        public void setComplete()
        {
            instance.SendFloat("complete", 1);
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

        public void setLeft(float v)
        {
            instance.SendFloat("left", v);
        }

        public void setRight(float v)
        {
            instance.SendFloat("right", v);
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