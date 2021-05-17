using UnityEngine;

namespace audio
{
    public class DimAssociatedEncoder : SSAudioGeneration
    {
        FrequencyComputer freqComputer;
        private float angle;
        // Use this for initialization
        void Start()
        {
            init();
            puredataInstance.SendFloat("left", 1);
            puredataInstance.SendFloat("right", 1);
            freqComputer = new ContinuousComputer(maxAngle);
        }
        
        public override void setFrequencyComputation(string computer)
        {
            if (computer=="continuous")
                freqComputer = new ContinuousComputer(maxAngle);
            //else freqComputer = new DiscreteComputer(maxAngle, angleThreshold);
        }

        // Update is called once per frame
        void Update()
        {
            if (pointsTarget)
            {
                if (!previousPointsTarget)
                {
                    puredataInstance.SendFloat("hits", 1);
                    previousPointsTarget = true;
                    puredataInstance.SendFloat("freq", 0);
                }
            }
            else
            {
                if (previousPointsTarget)
                {
                    puredataInstance.SendFloat("hits", 0);
                    previousPointsTarget = false;
                }

                userToTargetVector = target.transform.position - transform.position;
                angle = Vector3.Angle(userToTargetVector, transform.forward);

                if (angle < maxAngle)
                {
                    frequency = freqComputer.computeFrequency(angle);
                    puredataInstance.SendFloat("freq", frequency);
                }
                else if (frequency != 0)
                {
                    puredataInstance.SendFloat("freq", 0);
                    frequency = 0;
                }

                setContinuousGain();
            }
        }

        private abstract class FrequencyComputer
        {
            protected float k, b;
            protected FrequencyComputer(float maxAngle) { }

            public abstract float computeFrequency(float angle);
        }

        private class ContinuousComputer : FrequencyComputer
        {
            public ContinuousComputer(float maxAngle) : base(maxAngle)
            {
                k = Mathf.Log(Mathf.Pow(0.25f, 1.0f / Mathf.Log(maxAngle)));
                b = Mathf.Log(HIGH_FREQ);
            }

            public override float computeFrequency(float angle)
            {
                return Mathf.Exp(k * Mathf.Log(angle) + b);
            }
        }

    }


}