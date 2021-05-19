namespace audio
{
    public class DimDissociatedEncoder : SSAudioGeneration
    {
        private AngleComputer horizontalComputer;
        private float horizontalAngle;
        protected bool previousLeft, previousRight;

        void Awake()
        {
            init();
            angleComputer = new VerticalComputer(Cam, target);
            horizontalComputer = new HorizontalComputer();
        }

        protected override void initStereo(bool stereo)
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

                computeOTvector();
                computeAngle();
                setStereo();
                setFrequency();
                setContinuousGain();
            }
        }

        void setStereo()
        {
            horizontalAngle = horizontalComputer.compute(userToTargetVector, transform.forward);
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
            setFrequency(angle);
            if (frequency != previousFrequency)
            {
                puredataInstance.SendFloat("freq", frequency);
                previousFrequency = frequency;
            }
        }

    }
}