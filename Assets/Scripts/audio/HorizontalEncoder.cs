using UnityEngine;

namespace audio
{
    public class HorizontalEncoder : SSAudioGeneration
    {
        bool stereo;
        float kStereo, rightGain;
        const float bStereo = 0.5f;
        private Vector2 userToTargetHorizontalVector, horizontalPointedDirection;
        private float horizontalAngle;

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
            setFrequency(horizontalAngle);
            if (stereo)
            {
                rightGain = kStereo * horizontalAngle + bStereo;
                puredataInstance.SendFloat("right", rightGain);
                puredataInstance.SendFloat("left", 1 - rightGain);
            }
            puredataInstance.SendFloat("freq", frequency);
        }

        protected override void initStereo(bool stereo)
        {
            this.stereo = stereo;
            kStereo = 1 / (maxAngle * 2);
        }
    }
}