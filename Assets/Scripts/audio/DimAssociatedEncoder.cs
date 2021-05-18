using UnityEngine;

namespace audio
{
    public class DimAssociatedEncoder : SSAudioGeneration
    {
        private float angle;
        // Use this for initialization
        void Awake()
        {
            init();
            puredataInstance.SendFloat("left", 1);
            puredataInstance.SendFloat("right", 1);
        }

        protected override void setStereo(bool stereo)
        {
            throw new System.NotImplementedException();
        }

        public override  void setFrequencyComputation(string computer)
        {
            if (computer.ToLower().Equals("continuous"))
                freqComputer = new ContinuousComputer(maxAngle);
            if (computer.ToLower().Equals("discrete"))
                freqComputer = new DiscreteComputer(angleThreshold);
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
                    frequency = freqComputer.computeFrequency(angle,angleThreshold);
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

        private class DiscreteComputer : FrequencyComputer
        {
            float threshold;
            public DiscreteComputer(float threshold)
            {
                this.threshold = threshold;
            }

            public override float computeFrequency(float angle,float criterium)
            {
                if (angle < threshold)
                    return HIGH_FREQ;
                else return LOW_FREQ;
            }

        }

    }
}