namespace audio.Computer
{
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

    public abstract class StereoInterface : StereoMono
    {
        protected float maxAngle, minAngle;
        protected AudioInterface i;
        protected bool previousOut;
        public StereoInterface(float max, AudioInterface i)
        {
            maxAngle = max;
            minAngle = -max;
            this.i = i;
        }

        public abstract void computeAndSend(float angle);
    }

    public class ContinuousStereoInterface : StereoInterface
    {
        float kStereo, bStereo,right;
        public ContinuousStereoInterface(float maxAngle, AudioInterface i) : base(maxAngle, i)
        {
            kStereo = 1 / (maxAngle * 2);
            bStereo = 0.5f;
        }

        public override void computeAndSend(float angle)
        {
            if (angle > maxAngle || angle < minAngle)
            {
                if (!previousOut)
                {
                    i.setStereo(0, 0);
                    previousOut = true;
                }
            }
            else
            {
                right = kStereo * angle + bStereo; ;
                i.setStereo(1 - right, right);
                if (previousOut)
                    previousOut = false;
            }
        }
    }

    public class DiscreteStereoInterface : StereoInterface
    {
        float threshold, oppositeThreshold;
        bool previousLeft, previousRight,previousIn;
        public DiscreteStereoInterface(float max, float thresh, AudioInterface i) : base(max, i)
        {
            threshold = thresh;
            oppositeThreshold = -thresh;
        }

        public override void computeAndSend(float signedAngle)
        {
            if (signedAngle > maxAngle || signedAngle < minAngle)
            {
                if (!previousOut)
                {
                    i.setStereo(0, 0);
                    previousOut = true;
                    previousLeft = previousRight = previousIn = false;
                }
            }
            else
            {
                if (signedAngle > threshold)
                {
                    if (!previousLeft)
                    {
                        i.setStereo(0, 0.5f);
                        previousLeft = true;
                        previousOut = previousRight = previousIn = false;
                    }
                }
                else if (signedAngle < oppositeThreshold)
                {
                    if (!previousRight)
                    {
                        i.setStereo(0.5f, 0);
                        previousRight = true;
                        previousOut = previousLeft = previousIn = false;
                    }
                }
                else
                {
                    if (!previousIn)
                    {
                        i.setStereo(0.5f, 0.5f);
                        previousIn = true;
                        previousOut = previousLeft = previousRight = false;
                    }
                }
            }
        }
    }

}