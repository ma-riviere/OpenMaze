using UnityEngine;
using audio.interfaces;

namespace audio.computers
{
    public abstract class FrequencyInterface : InternalAudioInterface
    {
        protected int maxAngle, minAngle;
        public const int HIGH_FREQ = 440, MED_FREQ = 220, LOW_FREQ = 110;
        protected bool previousOut;

        protected FrequencyInterface(int maxA)
        {
            maxAngle = maxA;
            minAngle = -maxA;
        }
        public abstract void computeFrequency(float angle);

    }

    public class ConstantInterface : FrequencyInterface
    {
        protected int freq1;
        public ConstantInterface(int maxA, int f) : base(maxA)
        {
            freq1 = f;
        }

        public override void computeFrequency(float angle)
        {
            if (angle > maxAngle || angle < minAngle)
            {
                if (!previousOut)
                {
                    audioInterface.setFrequency(0);
                    previousOut = true;
                }
            }
            else
            {
                if (previousOut)
                {
                    audioInterface.setFrequency(freq1);
                    previousOut = false;
                }
            }
        }

    }

    public class ContinuousInterface : FrequencyInterface
    {
        private float k, b, f;
        int maxF;
        public ContinuousInterface(int maxAngle, int maxF) : base(maxAngle)
        {
            k = Mathf.Log(Mathf.Pow(0.25f, 1.0f / Mathf.Log(maxAngle)));
            b = Mathf.Log(HIGH_FREQ);
            this.maxF = maxF;
        }

        public override void computeFrequency(float angle)
        {
            if (angle > maxAngle || angle < minAngle)
            {
                if (!previousOut)
                {
                    audioInterface.setFrequency(0);
                    previousOut = true;
                }
            }
            else
            {
                previousOut = false;
                f = Mathf.Exp(k * Mathf.Log(angle) + b);
                if (f > maxF)
                    f = maxF;
                audioInterface.setFrequency(f);
            }
        }

    }

    public class DiscreteInterface : ConstantInterface
    {
        int threshold, freq2;
        bool previousInFar, previousInClose;
        public DiscreteInterface(int max, int threshold, int freq1, int freq2) : base(max, freq1)
        {
            this.threshold = threshold;
            this.freq2 = freq2;
        }

        public override void computeFrequency(float angle)
        {
            if (angle > maxAngle || angle < minAngle)
            {
                if (!previousOut)
                {
                    audioInterface.setFrequency(0);
                    previousOut = true;
                    previousInFar = false;
                }
            }
            else if (angle < threshold)
            {
                if (!previousInClose)
                {
                    audioInterface.setFrequency(freq2);
                    previousInClose = true;
                    previousInFar = false;
                }
            }
            else if (!previousInFar)
            {
                audioInterface.setFrequency(freq1);
                previousInFar = true;
                if (previousInClose)
                    previousInClose = false;
                else previousOut = false;
            }
        }

    }


}