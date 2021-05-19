using UnityEngine;
using wallSystem;
using data;

namespace audio
{
    public abstract class SSAudioGeneration : MonoBehaviour
    {
        #region properties
        protected float angleThreshold,maxAngle;
        protected const float GAIN_MIN = 1.0f / 127.0f, GAIN_MAX = 3.0f / 127.0f;
        protected float distanceMax;
        protected float kGain,b;

        protected LibPdInstance puredataInstance;
        protected float targetSelfRadiusSum;
        protected Camera Cam;
        protected GameObject target;
        protected SphereCollider targetCollider;

        protected Vector3 userToTargetVector;
        protected Ray pointedDirection;
        protected RaycastHit hit;
        protected bool pointsTarget, previousPointsTarget;
        protected float angle;
        protected float distance;
        protected float frequency,previousFrequency,gain, previousGain;

        protected AngleComputer angleComputer;
        protected FrequencyComputer freqComputer;
        #endregion

        public static void addSelectedAudioEncoder(Data.Audio audioData)
        {
            switch (audioData.encoder)
            {
                case "dissociated": FindObjectOfType<PlayerController>().gameObject.AddComponent<DimDissociatedEncoder>();break;
                case "associated": FindObjectOfType<PlayerController>().gameObject.AddComponent<DimAssociatedEncoder>(); break;
                case "horizontal": FindObjectOfType<PlayerController>().gameObject.AddComponent<HorizontalEncoder>(); break;
                case "vertical": FindObjectOfType<PlayerController>().gameObject.AddComponent<VerticalEncoder>(); break;
            }
            FindObjectOfType<PlayerController>().gameObject.GetComponent<SSAudioGeneration>().setParams(audioData);
        }

        void setParams(Data.Audio audioData)
        {
            angleThreshold = audioData.angleThreshold;
            maxAngle = audioData.maxAngle;
            distanceMax = audioData.distanceMax;
            setFrequencyComputation(audioData.frequencyComputer);
            initStereo(audioData.stereo);
        }

        private void setFrequencyComputation(string computer)
        {
            if (computer.Equals("continuous"))
                freqComputer = new ContinuousComputer(maxAngle);
            if (computer.Equals("discrete"))
                freqComputer = new DiscreteComputer(angleThreshold);
        }

        protected void computeOTvector()
        {
            userToTargetVector = target.transform.position - transform.position;
        }

        private void computeOPvector()
        {
            pointedDirection = new Ray(transform.position, transform.forward);
        }

        protected void computeAngle()
        {
            angle = angleComputer.compute(userToTargetVector, transform.forward);
        }

        protected abstract void initStereo(bool stereo);

        protected void init()
        {
            Cam = GetComponent<PlayerController>().Cam;
            puredataInstance = GetComponent<LibPdInstance>();
            kGain = Mathf.Log(Mathf.Pow(1.0F / 3.0f, 1.0f / (Mathf.Log(distanceMax) - Mathf.Log(0.01f))));
            b = Mathf.Exp(-kGain * Mathf.Log(distanceMax)) / 127;
            frequency = 0;
            target = GameObject.FindWithTag("Pickup");
            targetCollider = target.GetComponent<SphereCollider>();
            targetSelfRadiusSum = GetComponent<CharacterController>().radius + targetCollider.radius;
        }

        private void FixedUpdate()
        {
            computeOPvector();
            pointsTarget = targetCollider.Raycast(pointedDirection, out hit, 1000);
        }

        protected void setContinuousGain()
        {
            distance = userToTargetVector.magnitude - targetSelfRadiusSum;
            
            if (distance < distanceMax)
                gain = GAIN_MAX;
            else
                gain = GAIN_MIN;
            if (previousGain != gain)
            {
                puredataInstance.SendFloat("gain", gain);
                previousGain = gain;
            }
        }

        private void LateUpdate()
        {
            //Debug.DrawRay(transform.position, userToTargetVector, Color.green);
            //Debug.DrawRay(pointedDirection.origin, pointedDirection.direction, Color.red);
            //Debug.DrawRay(Cam.transform.position, verticalPointedDirection, Color.blue);
            //Debug.Log(distance);
        }

        protected void setFrequency(float signedAngle)
        {
            if (signedAngle > maxAngle)
                frequency = 0;
            else
            {
                frequency = freqComputer.computeFrequency(Mathf.Abs(signedAngle));
                if (frequency > AudioConstants.HIGH_FREQ)
                    frequency = AudioConstants.HIGH_FREQ;
            }
        }

        
    }
}