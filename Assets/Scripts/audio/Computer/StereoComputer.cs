namespace audio.Computer
{
    public abstract class StereoComputer
    {
        protected float maxAngle, minAngle;
        protected StereoComputer(float max)
        {
            maxAngle = max;
            minAngle = -max;
        }

        public abstract float compute(float v);
    }
    public class ContinuousStereoComputer:StereoComputer
    {
        float kStereo, bStereo;

        public ContinuousStereoComputer(float maxAngle):base(maxAngle)
        {
            kStereo = 1 / (maxAngle * 2);
            bStereo = 0.5f;
        }

        public override float compute(float angle)
        {
            if (angle > maxAngle || angle < minAngle)
                return 0;
            return kStereo * angle + bStereo;
        }
    }

    public class DiscreteStereoComputer : StereoComputer
    {
        public DiscreteStereoComputer(float max) : base(max) { }
        public override float compute(float angle)
        {
            return 0;
        }
    }

    public interface StereoMono
    {
        void computeAndSend(float a);
    }

    public class MonoInterface : StereoMono
    {
        public MonoInterface() { }
        public void computeAndSend(float a)
        {

        }
    }

    public class StereoInterface : StereoMono
    {
        private StereoComputer c;
        private AudioInterface i;
        public StereoInterface(bool continuous, float maxAngle, AudioInterface i)
        {
            c = new ContinuousStereoComputer(maxAngle);
            this.i = i;
        }

        public void computeAndSend(float angle)
        {
            float right = c.compute(angle);
            if(right==0)
                i.setStereo(right, right);
            else i.setStereo(right, 1 - right);
        }
    }

}