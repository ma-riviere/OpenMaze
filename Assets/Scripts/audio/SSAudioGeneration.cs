using UnityEngine;
using wallSystem;
using data;
using audio.Computer;

namespace audio
{
    public abstract class SSAudioGeneration : MonoBehaviour
    {
        #region properties
        private AudioInterface puredataInstance;
        protected GameObject target;
        protected SphereCollider targetCollider;
        protected CharacterController coll;

        protected AbstractComputer goodDirectionComputer;
        protected AngleComputer angleComputer;
        protected FrequencyComputer freqComputer;
        protected StereoMono stereoMonoInterface;
        protected NoiseInterface noise;
        protected GainComputer gainComputer;

        protected Vector3 userToTargetVector;
        protected float angle;
        protected float distance;

        protected float frequency, gain, previousGain;
        
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

        private void setParams(Data.Audio audioData, CharacterController cc)
        {
            switch (audioData.frequencyComputer)
            {
                case "continuous": freqComputer = new ContinuousComputer(audioData.maxAngle); break;
                case "discrete": freqComputer = new DiscreteComputer(audioData.maxAngle, audioData.angleThreshold); break;
            }
            switch (audioData.gainComputer)
            {
                case "continuous": gainComputer = new ContinuousGainComputer(audioData.distanceMax); break;
                case "discrete": gainComputer = new DiscreteGainComputer(audioData.distanceMax); break;
            }
            if ((audioData.encoder.Equals("dissociated") || audioData.encoder.Equals("horizontal")) && audioData.stereo)
            {
                if(audioData.stereoComputer.Equals("continuous"))
                    stereoMonoInterface = new ContinuousStereoInterface(audioData.maxAngle, puredataInstance);
                else stereoMonoInterface = new DiscreteStereoInterface(audioData.maxAngle, audioData.angleThreshold, puredataInstance);
                if (audioData.encoder.Equals("horizontal"))
                    freqComputer = new ConstantComputer(audioData.maxAngle,FrequencyComputer.MED_FREQ);
            }
            else stereoMonoInterface = new MonoInterface();
            if (audioData.goodDir)
            {
                goodDirectionComputer = new GoodDirectionComputer(targetCollider);
                noise = new Noise(puredataInstance);
            }
            else {
                goodDirectionComputer = new VoidComputer();
                noise = new Void();
            }
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
            //kGain = Mathf.Log(Mathf.Pow(1.0F / 3.0f, 1.0f / (Mathf.Log(distanceMax) - Mathf.Log(0.01f))));
            //b = Mathf.Exp(-kGain * Mathf.Log(distanceMax)) / 127;
            frequency = 0;
            target = GameObject.FindWithTag("Pickup");
            targetCollider = target.GetComponent<SphereCollider>();
        }

        protected abstract void selectAngleComputing();

        protected void computeOTvector()
        {
            userToTargetVector = targetCollider.ClosestPoint(transform.position) - coll.ClosestPoint(target.transform.position);
        }

        protected void computeAngle()
        {
            angle = angleComputer.compute(userToTargetVector, transform.forward);
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

        protected void setGain()
        {
            distance = userToTargetVector.magnitude;
            gain = gainComputer.compute(distance);
            if (previousGain != gain)
            {
                puredataInstance.setGain(gain);
                previousGain = gain;
            }
        }

    }
}