using UnityEngine;

namespace audio
{
    public class VerticalEncoder : SSAudioGeneration
    {
        public float screenCenterY;
        public Vector3 verticalPointedDirection, targetCenterOnScreen, verticalScreenPoint;
        private float verticalAngle;

        void Awake()
        {
            init();
            targetCenterOnScreen = Cam.WorldToScreenPoint(target.transform.position);
            verticalScreenPoint = new Vector3(targetCenterOnScreen.x, Cam.pixelHeight / 2, targetCenterOnScreen.z);
        }

        protected override void initStereo(bool stereo)
        {
        }

        void Update()
        {
            userToTargetVector = target.transform.position - transform.position;
            targetCenterOnScreen = Cam.WorldToScreenPoint(target.transform.position);
            verticalScreenPoint.x = targetCenterOnScreen.x;
            verticalScreenPoint.z = targetCenterOnScreen.z;
            verticalPointedDirection = Cam.ScreenPointToRay(verticalScreenPoint).direction;
            verticalAngle = Vector3.Angle(userToTargetVector, verticalPointedDirection);
            setFrequency(verticalAngle);
            puredataInstance.SendFloat("freq", frequency);
        }

    }
}