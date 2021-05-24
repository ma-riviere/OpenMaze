namespace audio.Computer
{
    public class StereoComputer
    {
        float kStereo, bStereo, maxAngle, minAngle;

        public StereoComputer(float maxAngle)
        {
            kStereo = 1 / (maxAngle * 2);
            bStereo = 0.5f;
            this.maxAngle = maxAngle;
            this.minAngle = -maxAngle;
        }

        public float compute(float angle)
        {
            if (angle > maxAngle || angle < minAngle)
                return 0;
            return kStereo * angle + bStereo;
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
            c = new StereoComputer(maxAngle);
            this.i = i;
        }

        public void computeAndSend(float angle)
        {
            float right = c.compute(angle);
            i.setStereo(right, 1 - right);
        }
    }

}