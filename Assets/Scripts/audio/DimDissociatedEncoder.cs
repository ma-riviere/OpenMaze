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

        void Awake()
        {
            init();
            screenCenterY = Cam.pixelHeight/2;
            verticalScreenPoint = new Vector3(targetCenterOnScreen.x, screenCenterY, targetCenterOnScreen.z);
        }

        public override void setFrequencyComputation(string computer)
        {
            if (computer.ToLower().Equals("continuous"))
                freqComputer = new ContinuousComputer(maxAngle);
            if (computer.ToLower().Equals("discrete")) 
                freqComputer = new DiscreteComputer(angleThreshold, screenCenterY);
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

            frequency = freqComputer.computeFrequency(verticalAngle, targetCenterOnScreen.y);

            if (frequency != previousFrequency)
            {
                puredataInstance.SendFloat("freq", frequency);
                previousFrequency = frequency;
            }
        }

        private class DiscreteComputer : FrequencyComputer
        {
            float threshold, screenCenterY;
            public DiscreteComputer(float threshold,float screenCenterY)
            {
                this.threshold = threshold;
                this.screenCenterY = screenCenterY;
            }
            public override float computeFrequency(float angle,float criterium)
            {
                if (angle < threshold)
                    return MED_FREQ;
                else if (criterium < screenCenterY)
                    return LOW_FREQ;
                else return HIGH_FREQ;
            }
        }

    }
}