namespace audio.Computer
{
    public abstract class StereoMono:InternalAudioInterface
    {
        public abstract void computeAndSend(float a);

    }

    public class MonoInterface : StereoMono
    {
        public MonoInterface() { }
        public override void computeAndSend(float a) { }

    }

    public abstract class StereoInterface : StereoMono
    {
        protected float maxAngle, minAngle;
        protected bool previousOut;
        public StereoInterface(float max)
        {
            maxAngle = max;
            minAngle = -max;
        }

    }

    public class ContinuousStereoInterface : StereoInterface
    {
        float kStereo, bStereo, right;
        public ContinuousStereoInterface(float maxAngle) : base(maxAngle)
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
                    audioInterface.setStereo(0, 0);
                    previousOut = true;
                }
            }
            else
            {
                right = kStereo * angle + bStereo; ;
                audioInterface.setStereo(1 - right, right);
                if (previousOut)
                    previousOut = false;
            }
        }

    }

    public class DiscreteStereoInterface : StereoInterface
    {
        float threshold, oppositeThreshold;
        bool previousLeft, previousRight, previousIn;
        public DiscreteStereoInterface(float max, float thresh) : base(max)
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
                    audioInterface.setStereo(0, 0);
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
                        audioInterface.setStereo(0, 0.5f);
                        previousLeft = true;
                        previousOut = previousRight = previousIn = false;
                    }
                }
                else if (signedAngle < oppositeThreshold)
                {
                    if (!previousRight)
                    {
                        audioInterface.setStereo(0.5f, 0);
                        previousRight = true;
                        previousOut = previousLeft = previousIn = false;
                    }
                }
                else
                {
                    if (!previousIn)
                    {
                        audioInterface.setStereo(0.5f, 0.5f);
                        previousIn = true;
                        previousOut = previousLeft = previousRight = false;
                    }
                }

            }

        }


    }

}