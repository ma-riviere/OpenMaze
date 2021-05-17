using UnityEngine;

namespace audio
{
    public class DimDissociatedEncoder : SSAudioGeneration
    {
        public float screenCenterY;
        public Vector3 verticalPointedDirection, targetCenterOnScreen,verticalScreenPoint;
        private Vector2 userToTargetHorizontalVector, horizontalPointedDirection;
        private float horizontalAngle, verticalAngle;
        protected bool previousLeft, previousRight;

        void Start()
        {
            init();
            screenCenterY = Cam.pixelHeight/2;
            verticalScreenPoint = new Vector3(targetCenterOnScreen.x, screenCenterY, targetCenterOnScreen.z);
        }

        public override void setFrequencyComputation(string computer)
        {

        }

        private void Update()
        {
            if (pointsTarget)
            {
                if (!previousPointsTarget)
                {
                    puredataInstance.SendFloat("hits", 1);
                    previousPointsTarget = true;
                    puredataInstance.SendFloat("right", 0);
                    previousRight = false;
                    puredataInstance.SendFloat("left", 0);
                    previousLeft = false;
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
                setStereo();
                setFrequency();
                setContinuousGain();
            }
        }

        void setStereo()
        {
            userToTargetHorizontalVector.Set(userToTargetVector.x, userToTargetVector.z);
            horizontalPointedDirection.Set(transform.forward.x, transform.forward.z);
            horizontalAngle = Vector2.SignedAngle(userToTargetHorizontalVector, horizontalPointedDirection);
            if (horizontalAngle < maxAngle && horizontalAngle > -angleThreshold)
            {
                if (!previousRight)
                {
                    puredataInstance.SendFloat("right", 1);
                    previousRight = true;
                }
            }
            else if (previousRight)
            {
                puredataInstance.SendFloat("right", 0);
                previousRight = false;
            }
            if (horizontalAngle > -maxAngle && horizontalAngle < angleThreshold)
            {
                if (!previousLeft)
                {
                    puredataInstance.SendFloat("left", 1);
                    previousLeft = true;
                }
            }
            else if (previousLeft)
            {
                puredataInstance.SendFloat("left", 0);
                previousLeft = false;
            }
        }

        void setFrequency()
        {
            targetCenterOnScreen = Cam.WorldToScreenPoint(target.transform.position);
            verticalScreenPoint.x = targetCenterOnScreen.x;
            verticalScreenPoint.z = targetCenterOnScreen.z;
            verticalPointedDirection = Cam.ScreenPointToRay(verticalScreenPoint).direction;
            verticalAngle = Vector3.Angle(userToTargetVector, verticalPointedDirection);

            if (verticalAngle < angleThreshold)
                frequency = MED_FREQ;
            else if (targetCenterOnScreen.y > screenCenterY)
                frequency = HIGH_FREQ;
            else frequency = LOW_FREQ;

            if (frequency != previousFrequency)
            {
                puredataInstance.SendFloat("freq", frequency);
                previousFrequency = frequency;
            }
        }

        private abstract class FrequencyComputer
        {
            protected FrequencyComputer(){}
            public abstract float computeFrequency(float angle,float criterium);
        }

        private class DiscreteComputer : FrequencyComputer
        {
            float threshold, screenCenterY;
            protected DiscreteComputer(float threshold,float screenCenterY) : base()
            {
                this.threshold = threshold;
                this.screenCenterY = screenCenterY;
            }
            public override float computeFrequency(float angle,float criterium)
            {
                if (angle < threshold)
                    return MED_FREQ;
                else if (criterium > screenCenterY)
                    return HIGH_FREQ;
                else return LOW_FREQ;
            }
        }

        private class ContinuousComputer : FrequencyComputer
        {
            float k, b;
            protected ContinuousComputer(float maxAngle)
            {
                k = Mathf.Log(Mathf.Pow(0.25f, 1.0f / Mathf.Log(maxAngle)));
                b = Mathf.Log(HIGH_FREQ);
            }
            public override float computeFrequency(float angle, float criterium)
            {
                return Mathf.Exp(k * Mathf.Log(angle) + b);
            }
        }

    }
}