using UnityEngine;

namespace audio.Computer
{
    public abstract class FrequencyComputer
    {
        protected float maxAngle, minAngle;
        public const int HIGH_FREQ = 440, MED_FREQ=220,LOW_FREQ = 110;

        protected FrequencyComputer(float maxA)
        {
            maxAngle = maxA;
            minAngle = -maxA;
        }
        public abstract float computeFrequency(float angle);
    }

    public class ConstantComputer : FrequencyComputer
    {
        private float freq;
        public ConstantComputer(float maxA,float f):base(maxA)
        {
            freq = f;
        }

        public override float computeFrequency(float angle)
        {
            if (angle > maxAngle || angle < minAngle)
                return 0;
            else return freq;
        }
    }

    public class ContinuousComputer : FrequencyComputer
    {
        protected float k, b;
        public ContinuousComputer(float maxAngle) : base(maxAngle)
        {
            k = Mathf.Log(Mathf.Pow(0.25f, 1.0f / Mathf.Log(maxAngle)));
            b = Mathf.Log(HIGH_FREQ);
        }

        public override float computeFrequency(float angle)
        {
            if (angle > maxAngle || angle < minAngle)
                return 0;
            return Mathf.Exp(k * Mathf.Log(angle) + b);
        }
    }

    public class DiscreteComputer : FrequencyComputer
    {
        float threshold;
        public DiscreteComputer(float max, float threshold) : base(max)
        {
            this.threshold = threshold;
        }

        public override float computeFrequency(float angle)
        {
            if (angle > maxAngle || angle < minAngle)
                return 0;
            else if (angle < threshold)
                return HIGH_FREQ;
            else return LOW_FREQ;
        }

    }
}