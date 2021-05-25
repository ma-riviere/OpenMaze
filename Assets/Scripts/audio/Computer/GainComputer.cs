namespace audio.Computer
{
    public abstract class GainComputer
    {
        protected const float GAIN_MIN = 1.0f / 127.0f, GAIN_MAX = 3.0f / 127.0f;
        protected float distanceMax;
        public abstract float compute(float value);
    }

    public class ContinuousGainComputer : GainComputer
    {
        private float kGain, b;

        public ContinuousGainComputer(float dMax)
        {
            distanceMax = dMax;
            kGain = -GAIN_MIN;
            b = GAIN_MAX;
        }

        public override float compute(float value)
        {
            if (value > distanceMax)
                return GAIN_MIN;
            else return value * kGain + b;
        }

    }

    public class DiscreteGainComputer : GainComputer
    {
        public DiscreteGainComputer(float dMax)
        {
            distanceMax = dMax;
        }

        public override float compute(float value)
        {
            if (value > distanceMax)
                return GAIN_MIN;
            else return GAIN_MAX;
        }
    }
}