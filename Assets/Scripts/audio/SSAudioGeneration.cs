using UnityEngine;
using wallSystem;
using data;
using audio.Computer;

namespace audio
{
    public abstract class SSAudioGeneration : MonoBehaviour
    {
        #region properties
        private AudioInterface audioInstance;
        protected GameObject target;
        protected SphereCollider targetCollider;
        protected CharacterController controller;

        protected TargetDirectionComputer goodDirectionComputer;
        protected AngleComputer angleComputer;
        protected FrequencyInterface freqI;
        protected StereoMono stereoMonoInterface;
        protected NoiseInterface noise;
        protected GainInterface gainComputer;

        protected Vector3 userToTargetVector;
        protected float angle;
        protected float distance;

        protected float frequency, gain;
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
            FindObjectOfType<PlayerController>().ssAudio = FindObjectOfType<PlayerController>().gameObject.GetComponent<SSAudioGeneration>();
        }

        private void setParams(Data.Audio audioData, CharacterController cc)
        {
            audioInstance = new PureDataSender(GetComponent<LibPdInstance>());
            switch (audioData.frequencyComputer)
            {
                case "continuous": freqI = new ContinuousInterface(audioData.maxAngle, FrequencyInterface.HIGH_FREQ); break;
                case "discrete": freqI = new DiscreteInterface(audioData.maxAngle, audioData.angleThreshold, FrequencyInterface.MED_FREQ, FrequencyInterface.LOW_FREQ); break;
            }
            switch (audioData.gainComputer)
            {
                case "continuous": gainComputer = new ContinuousGainInterface(audioData.distanceMax); break;
                case "discrete": gainComputer = new DiscreteGainInterface(audioData.distanceMax); break;
            }
            if ((audioData.encoder.Equals("dissociated") || audioData.encoder.Equals("horizontal")) && audioData.stereo)
            {
                if (audioData.stereoComputer.Equals("continuous"))
                    stereoMonoInterface = new ContinuousStereoInterface(audioData.maxAngle);
                else stereoMonoInterface = new DiscreteStereoInterface(audioData.maxAngle, audioData.angleThreshold);
                if (audioData.encoder.Equals("horizontal"))
                    freqI = new ConstantInterface(audioData.maxAngle, FrequencyInterface.MED_FREQ);
            }
            else stereoMonoInterface = new MonoInterface();
            switch (audioData.goodDir)
            {
                case "angle":goodDirectionComputer = new RayDirectionComputer(targetCollider); 
                    noise = new Noise();
                    break;
                case "target":goodDirectionComputer = new AngleDirectionComputer(audioData.margin);
                    noise = new Noise();
                    break;
                default:
                    goodDirectionComputer = new VoidComputer();
                    noise = new Void();
                    break;

            }
            controller = cc;
            gainComputer.audioInterface = freqI.audioInterface = stereoMonoInterface.audioInterface = noise.audioInterface = audioInstance;
        }

        void Awake()
        {
            target = GameObject.FindWithTag("Pickup");
            targetCollider = target.GetComponent<SphereCollider>();
            selectAngleComputing();
        }

        protected abstract void selectAngleComputing();

        protected void computeOTvector()
        {
            userToTargetVector = targetCollider.ClosestPoint(transform.position) - controller.ClosestPoint(target.transform.position);
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
            freqI.computeFrequency(Mathf.Abs(angle));
        }

        protected void setGain()
        {
            distance = userToTargetVector.magnitude;
            gainComputer.computeAndSend(distance);
        }

        public void playReachedTargetAudio()
        {
            audioInstance.setHits(0);
            audioInstance.setFreq(0);
            audioInstance.setStereo(0.5f, 0.5f);
            audioInstance.setComplete();
        }

    }
}