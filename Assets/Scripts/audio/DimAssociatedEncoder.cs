using UnityEngine;

namespace audio
{
    public class DimAssociatedEncoder : SSAudioGeneration
    {
        private float angle;

        protected override void initStereo(bool stereo)
        {
        }

        // Use this for initialization
        void Awake()
        {
            init();
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
                    setFrequency(angle);
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
    }
}