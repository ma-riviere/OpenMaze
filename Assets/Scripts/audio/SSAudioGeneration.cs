using UnityEngine;
using wallSystem;
using data;
using audio.Computer;

namespace audio
{
    public abstract class SSAudioGeneration : MonoBehaviour
    {
        #region properties
        private const float GAIN_MIN = 1.0f / 127.0f, GAIN_MAX = 3.0f / 127.0f;
        private float distanceMax;
        private float kGain, b;

        protected PureDataSender puredataInstance;
        private GameObject target;
        private SphereCollider targetCollider;
        private Collider coll;

        protected Vector3 userToTargetVector;
        protected Ray pointedDirection;
        private RaycastHit hit;
        protected bool pointsTarget, previousPointsTarget;
        protected float angle;
        private float distance;
        private float frequency, gain, previousGain;

        protected AngleComputer angleComputer;
        protected FrequencyComputer freqComputer;
        protected StereoMono stereoMonoInterface;
        #endregion

        public static void addSelectedAudioEncoder(Data.Audio audioData, CharacterController cc)
        {
            switch (audioData.encoder)
            {
                case "dissociated": FindObjectOfType<PlayerController>().gameObject.AddComponent<DimDissociatedEncoder>(); break;
                case "associated": FindObjectOfType<PlayerController>().gameObject.AddComponent<DimAssociatedEncoder>(); break;
                case "horizontal": FindObjectOfType<PlayerController>().gameObject.AddComponent<HorizontalEncoder>(); break;
                case "vertical": FindObjectOfType<PlayerController>().gameObject.AddComponent<VerticalEncoder>(); break;
            }
            FindObjectOfType<PlayerController>().gameObject.GetComponent<SSAudioGeneration>().setParams(audioData, cc);
        }

        void setParams(Data.Audio audioData, CharacterController cc)
        {
            switch (audioData.frequencyComputer)
            {
                case "continuous": freqComputer = new ContinuousComputer(audioData.maxAngle); break;
                case "discrete": freqComputer = new DiscreteComputer(audioData.maxAngle, audioData.angleThreshold); break;
            }
            distanceMax = audioData.distanceMax;
            if (audioData.stereo)
                stereoMonoInterface = new StereoInterface(true, audioData.maxAngle, puredataInstance);
            else stereoMonoInterface = new MonoInterface();
            coll = cc;
        }

        void Awake()
        {
            init();
            selectAngleComputing();
        }

        private void init()
        {
            puredataInstance = new PureDataSender(GetComponent<LibPdInstance>());
            kGain = Mathf.Log(Mathf.Pow(1.0F / 3.0f, 1.0f / (Mathf.Log(distanceMax) - Mathf.Log(0.01f))));
            b = Mathf.Exp(-kGain * Mathf.Log(distanceMax)) / 127;
            frequency = 0;
            target = GameObject.FindWithTag("Pickup");
            targetCollider = target.GetComponent<SphereCollider>();
        }

        protected abstract void selectAngleComputing();

        protected void computeOTvector()
        {
            userToTargetVector = targetCollider.ClosestPoint(transform.position) - coll.ClosestPoint(target.transform.position);
        }

        private void computeOPvector()
        {
            pointedDirection = new Ray(transform.position, transform.forward);
        }

        protected void computeAngle()
        {
            angle = angleComputer.compute(userToTargetVector, transform.forward);
        }

        private void FixedUpdate()
        {
            computeOPvector();
            pointsTarget = targetCollider.Raycast(pointedDirection, out hit, 1000);
        }

        private void LateUpdate()
        {
            //Debug.DrawRay(transform.position, userToTargetVector, Color.green);
            //Debug.DrawRay(pointedDirection.origin, pointedDirection.direction, Color.red);
            //Debug.DrawRay(Cam.transform.position, verticalPointedDirection, Color.blue);
            //Debug.Log(distance);
        }

        protected void setFrequency()
        {
            frequency = freqComputer.computeFrequency(Mathf.Abs(angle));
            if (frequency > FrequencyComputer.HIGH_FREQ)
                frequency = FrequencyComputer.HIGH_FREQ;
            puredataInstance.setFreq(frequency);
        }

        protected void setContinuousGain()
        {
            distance = userToTargetVector.magnitude;
            if (distance < distanceMax)
                gain = GAIN_MAX;
            else
                gain = GAIN_MIN;
            if (previousGain != gain)
            {
                puredataInstance.setGain(gain);
                previousGain = gain;
            }
        }

    }
}