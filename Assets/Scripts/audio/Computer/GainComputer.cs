using UnityEngine;
using audio.interfaces;

namespace audio.computers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class GainInterface : InternalAudioInterface
    {
        protected const float GAIN_MIN = 1.0f / 127.0f, GAIN_MAX = 3.0f / 127.0f;
        protected float distanceMax;
        protected bool previousOut;
        protected GainInterface()
        {
            previousOut = false;
        }
        public abstract void computeAndSend(float value);

    }

    public class ContinuousGainInterface : GainInterface
    {
        private float kGain, b;

        public ContinuousGainInterface(float dMax) : base()
        {
            distanceMax = dMax;
            kGain = -GAIN_MIN;
            b = GAIN_MAX;
        }

        public override void computeAndSend(float value)
        {
            if (value > distanceMax)
            {
                if (!previousOut)
                {
                    audioInterface.setGain(GAIN_MIN);
                    previousOut = true;
                }
            }
            else
            {
                audioInterface.setGain(value * kGain + b);
                previousOut = false;
            }
        }

    }

    public class DiscreteGainInterface : GainInterface
    {
        public DiscreteGainInterface(float dMax) : base()
        {
            distanceMax = dMax;
        }

        public override void computeAndSend(float value)
        {
            Debug.Log(value);
            if (value > distanceMax)
            {
                if (!previousOut)
                {
                    audioInterface.setGain(GAIN_MIN);
                    previousOut = true;
                }
            }
            else
            {
                if (previousOut)
                {
                    audioInterface.setGain(GAIN_MAX);
                    previousOut = false;
                }
            }
        }

    }

}