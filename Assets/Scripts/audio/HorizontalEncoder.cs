using UnityEngine;

namespace audio
{
    public class HorizontalEncoder : SSAudioGeneration
    {
        bool stereo;
        float kStereo,rightGain;
        const float bStereo= 0.5f;
        private Vector2 userToTargetHorizontalVector, horizontalPointedDirection;
        private float horizontalAngle;
        public override void setFrequencyComputation(string computer)
        {
            if (computer.ToLower().Equals("continuous"))
                freqComputer = new ContinuousComputer(maxAngle);
            if (computer.ToLower().Equals("discrete"))
                freqComputer = new DiscreteComputer(angleThreshold);
        }

        protected override void setStereo(bool stereo)
        {
            this.stereo = stereo;
            puredataInstance.SendFloat("right", bStereo);
            puredataInstance.SendFloat("left", bStereo);
            kStereo = 1 / (maxAngle * 2);
        }

        void Awake()
        {
            init();
        }

        void Update()
        {
            userToTargetVector = target.transform.position - transform.position;
            userToTargetHorizontalVector.Set(userToTargetVector.x, userToTargetVector.z);
            horizontalPointedDirection.Set(transform.forward.x, transform.forward.z);
            horizontalAngle = Vector2.SignedAngle(userToTargetHorizontalVector, horizontalPointedDirection);
            if (Mathf.Abs(horizontalAngle) > maxAngle)
                frequency = 0;
            else
            {
                if (stereo)
                {
                    rightGain = kStereo * horizontalAngle + bStereo;
                    puredataInstance.SendFloat("right", rightGain);
                    puredataInstance.SendFloat("left", 1 - rightGain);
                }

                frequency = freqComputer.computeFrequency(Mathf.Abs(horizontalAngle), 0);
                if (frequency > HIGH_FREQ)
                    frequency = HIGH_FREQ;
            }
            
            puredataInstance.SendFloat("freq", frequency);

        }

        private class DiscreteComputer : FrequencyComputer
        {
            float threshold;
            public DiscreteComputer(float threshold)
            {
                this.threshold = threshold;
            }

            public override float computeFrequency(float angle, float criterium)
            {
                if (angle < threshold)
                    return HIGH_FREQ;
                else return LOW_FREQ;
            }

        }

    }
}