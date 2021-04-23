using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Waves;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;

namespace audio
{

    [ExecuteInEditMode]
    [RequireComponent(typeof (AudioSource))]
    public class SSAudioGeneration : MonoBehaviour {
        
        AudioSource source;
        Camera user;
        GameObject target;

        SinWave sinAudioWave;
        // SquareWave squareAudioWave;
        // SawWave sawAudioWave;

        SquareWave amplitudeModulationOscillator;
        SinWave frequencyModulationOscillator;

        [Header("Wave type")]
        public bool useSinWave = true;
        [Range(0.0f,1.0f)] public float sinWaveIntensity = 1.0f;
        // public bool useSquareWave  = false;
        // [Range(0.0f,1.0f)] public float squareWaveIntensity = 1.0f;
        // public bool useSawWave  = false;
        // [Range(0.0f,1.0f)] public float sawWaveIntensity = 1.0f;

        [Space(2)]
        public bool modulateSinWave = false;
        [Range(0.0f,1.0f)] public float modulationIntensity = 1.0f;
        
        [Space(10)]

        [Header("Distance mapping")]
        public float currentDistance;
        [Range(0,10)] public float minDist = 1;
        private float _minDist;
        [Range(0,500)] public float maxDist = 30;
        private float _maxDist;

        [Space(2)]
        public bool distanceToVolume = false;
        public LinkType distanceToVolumeLink;
        public float currentVolume;
        [Tooltip("0 is continuous output, and >= 1 is for automatic n-step discrete breaks")]
        [Range(-1,99)] public int stepsVolume = 0;
        private int _stepsVolume;
        public IDictionary<float, float> manualStepsVolume = new Dictionary<float, float>(){{5.0f, 1.0f}, {10.0f, 0.5f}, {30.0f, 0.25f}, {50.0f, 0.0f}};

        [Space(2)]
        public bool distanceToBip = true;
        public LinkType distanceToBipLink;
        public double currentBipFrequency;
        [Tooltip("0 is continuous output, and >= 1 is for automatic n-step discrete breaks")]
        [Range(-1,99)] public int stepsBip = 0;
        private int _stepsBip;
        public IDictionary<float, float> manualStepsBip = new Dictionary<float, float>(){{1.0f, 10.0f}, {30.0f, 5.0f}, {50.0f, 0.5f}};

        [Range(0.0f,10.0f)] public float minBipFreq = 0.5f;
        private float _minBipFreq;
        [Range(0.0f,10.0f)] public float maxBipFreq = 10.0f;
        private float _maxBipFreq;

        [Space(10)]

        [Header("Angle mapping")]
        public float currentAngle;
        [Range(0,180)] public float minAngle = 15.0f;
        private float _minAngle;
        // public bool correctMinAngle = false;
        // public float minAngleCorrected;
        [Range(0,180)] public float maxAngle = 90.0f;
        private float _maxAngle;

        [Space(2)]
        public bool angleToFrequency = true;
        public LinkType angleToFrequencyLink;
        public double currentFrequency;
        [Tooltip("0 is continuous output, and >= 1 is for automatic n-step discrete breaks")]
        [Range(-1,99)] public int stepsFreq = 0;
        private int _stepsFreq;
        public IDictionary<float, float> manualStepsFreq = new Dictionary<float, float>(){{15.0f, 1000.0f}, {90.0f, 200.0f}, {180.0f, 0.0f}};

        [Range(100,2000)] public float minFreq = 200.0f;
        private float _minFreq;
        [Range(100,2000)] public float maxFreq = 1000.0f;
        private float _maxFreq;

        [Space(2)]
        public bool angleToStereo = false;   // Not working for now
        public LinkType angleToStereoLink;
        //[Range(-1.0f,1.0f)]
        public Tuple<float, float> currentStereo = new Tuple<float, float>(1.0f, 1.0f);
        [Tooltip("0 is continuous output, and >= 1 is for automatic n-step discrete breaks")]
        [Range(-1,99)] public int stepsStereo = 0;
        private int _stepsStereo;
        public IDictionary<float, float> manualStepsStereo = new Dictionary<float, float>(){{15.0f, 0.0f}, {90.0f, 0.0f}, {180.0f, 0.0f}};

        [Range(-1.0f,1.0f)] public float minStereo = 0.0f;
        private float _minStereo;
        [Range(-1.0f,1.0f)] public float maxStereo = 0.0f;
        private float _maxStereo;
        
        [Space(5)]

        private double samplingRate;
        private double dataLen;
        private double chunkTime;			
        private double dspTimeStep;
        private double currentDspTime;

        /** Link function samples **/
        private int samplingAccuracy = 500;
        private double[] dist2volBreaks;
        private double[] dist2bipBreaks;
        private double[] angle2FreqBreaks;
        private double[] angle2StereoBreaks;
        
        private bool _update = true;


        /**
        * Awake is called when the script instance is being loaded.
        * Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        */
        void Awake() {
            sinAudioWave = new SinWave();
            // squareAudioWave = new SquareWave();
            // sawAudioWave = new SawWave ();

            amplitudeModulationOscillator = new SquareWave();
            frequencyModulationOscillator = new SinWave();

            samplingRate = AudioSettings.outputSampleRate;
            
            user = Camera.main;
            source = gameObject.GetComponent<AudioSource>();
            //gameObject.AddComponent<AudioSource>();
            target = GameObject.FindWithTag("Pickup"); //TODO: handle case if not found (should not happen, but still ...)

            /** Setting AudioSource parameters **/
            source.spatialize = false; // Custom spatializer effects improve the realism of sound propagation by incorporating the binaural head-related transfer function (HRTF)
            source.spatialBlend = 0.0f;
            source.priority = 0; // 0 = max priority
            source.panStereo = 0.0f; // -1.0 (full left) to 1.0 (full right)
            source.spread = 0; // 0 = all sound channels are located at the same speaker location and is 'mono'. 360 = all subchannels are located at the opposite speaker location to the speaker location that it should be according to 3D position.
            source.playOnAwake = false;


            currentDistance = Vector3.Distance(
                target.GetComponent<Collider>().ClosestPoint(user.transform.position), 
                user.transform.position
            );
            currentAngle = (180 / Mathf.PI) * (Mathf.Acos(Vector3.Dot(user.transform.forward, target.transform.forward)));
            // minAngleCorrected = minAngle;

            /** Sampling link functions **/
            if(distanceToVolume && stepsVolume > 0) dist2volBreaks = GetBreaks(minDist, maxDist, samplingAccuracy, UpdateVolume, stepsVolume);
            if(distanceToBip && stepsBip > 0) dist2bipBreaks = GetBreaks(minDist, maxDist, samplingAccuracy, UpdateBipFrequency, stepsBip);
            if(angleToFrequency && stepsFreq > 0) angle2FreqBreaks = GetBreaks(minAngle, maxAngle, samplingAccuracy, UpdateFrequency, stepsFreq);

            // Playing
            source.Play();
        }

        /** Update is called once per frame **/
        void Update() {
            if (_update) {
                currentDistance = Vector3.Distance(
                    target.GetComponent<Collider>().ClosestPoint(user.transform.position), 
                    user.transform.position
                );
                currentAngle = (180 / Mathf.PI) * (Mathf.Acos(Vector3.Dot(user.transform.forward, target.transform.forward)));
            }
            //else source.Stop();
        }

        /** Ressources:
        * - https://www.youtube.com/watch?v=GqHFGMy_51c&t=129s
        * - https://github.com/konsfik/Unity3D-Coding-Examples/tree/master/3-Procedural-Audio/ProceduralAudioUnityProject/Assets/Scripts
        * 
        * Tips:
        * - If there is more than one channel, the channel data is interleaved. This means each consecutive data sample in the array comes from a different channel until you run out of channels and loop back to the first.
        */
        void OnAudioFilterRead(float[] data, int channels) {

            if(distanceToVolume) currentVolume = (float)UpdateVolume(currentDistance, stepsVolume);
            if(distanceToBip) currentBipFrequency = UpdateBipFrequency(currentDistance, stepsBip);
            if(angleToFrequency) currentFrequency = UpdateFrequency(currentAngle, stepsFreq);
            if (channels >= 2 && angleToStereo) currentStereo = UpdateStereoPanning(currentAngle, stepsStereo);
            // if(correctMinAngle) minAngleCorrected = minAngle * Mathf.Min(currentDistance, 1.0f);

            currentDspTime = AudioSettings.dspTime;
            dataLen = data.Length / channels;	// Number of samples per channel,
            chunkTime = dataLen / samplingRate;	// The time that each chunk of data lasts
            dspTimeStep = chunkTime / dataLen;	// The time of each dsp step. (the time that each individual audio sample (actually a float value) lasts)
            double preciseDspTime;

            for (int i = 0; i < dataLen; i++) {
                preciseDspTime = currentDspTime +  i * dspTimeStep;
                double signalValue = 0.0;

                /** Creating the base wave **/
                if (useSinWave) {
                    signalValue += sinWaveIntensity * sinAudioWave.calculateSignalValue(preciseDspTime, currentFrequency);

                    // TODO: doesn't work
                    if (modulateSinWave) {
                        double amp = sinWaveIntensity + modulationIntensity;
                        double signalValue45 = modulationIntensity * frequencyModulationOscillator.calculateSignalValue(preciseDspTime, currentFrequency * 4.0/5.0);
                        double signalValue23 = modulationIntensity * frequencyModulationOscillator.calculateSignalValue(preciseDspTime, currentFrequency * 2.0/3.0);
                        signalValue = MinMaxD(signalValue + signalValue45, -amp, amp, 0.0, 1.0);
                        signalValue = MinMaxD(signalValue + signalValue23, -amp, amp, 0.0, 1.0);
                    }
                }
                // if (useSquareWave) signalValue += squareWaveIntensity * squareAudioWave.calculateSignalValue(preciseDspTime, currentFrequency);
                // if (useSawWave) signalValue += sawWaveIntensity * sawAudioWave.calculateSignalValue(preciseDspTime, currentFrequency);

                /** Modulating the amplitude **/
                if (distanceToBip) signalValue *= MinMaxD(amplitudeModulationOscillator.calculateSignalValue(preciseDspTime, currentBipFrequency), -1.0, 1.0, 0.0, 1.0);
                if (distanceToVolume) signalValue *= currentVolume;

                /** Output for both channels (speakers ?) **/
                data[i * channels] = (float)signalValue * currentStereo.Item1; // Front Left
                if (channels >= 2) data[i * channels + 1] = (float)signalValue * currentStereo.Item2; // Front Right
            }
        }


        /***************************[ Updating sound paramters ]***************************/

        double UpdateVolume(float distance, int nSteps = 0) {
            double y = 0.0;
            switch(distanceToVolumeLink) {
                case LinkType.Manual:
                    y = manualLink(distance, manualStepsVolume);
                    break;
                case LinkType.Linear:
                    y = piecewiseLinearLink(distance, minDist, 1, maxDist, 0);
                    break;
                case LinkType.Logistic:
                    y = piecewiseLogisticLink(distance, minDist, 1, maxDist, 0, 0.5f);
                    break;
            }
            if(nSteps > 0) return Discretize(y, dist2volBreaks);
            else return y;
        }

        double UpdateBipFrequency(float distance, int nSteps = 0) {
            double y = 0.0;
            switch(distanceToBipLink) {
                case LinkType.Manual:
                    y = manualLink(distance, manualStepsBip);
                    break;
                case LinkType.Linear:
                    y = piecewiseLinearLink(distance, minDist, maxBipFreq, maxDist, minBipFreq);
                    break;
                case LinkType.Logistic:
                    y = piecewiseLogisticLink(distance, minDist, maxBipFreq, maxDist, minBipFreq, 0.5f);
                    break;
            }
            if(nSteps > 0) return Discretize(y, dist2bipBreaks);
            else return y;
        }

        double UpdateFrequency(float angle, int nSteps = 0) {
            double y = 0.0;
            switch(angleToFrequencyLink) {
                case LinkType.Manual:
                    y = manualLink(angle, manualStepsFreq);
                    break;
                case LinkType.Linear:
                    y = piecewiseLinearLink(angle, minAngle, maxFreq, maxAngle, minFreq);
                    break;
                case LinkType.Logistic:
                    y = piecewiseLogisticLink(angle, minAngle, maxFreq, maxAngle, minFreq, 0.5f);
                    break;
            }
            if(nSteps > 0) return Discretize(y, angle2FreqBreaks);
            else return y;
        }

        Tuple<float, float> UpdateStereoPanning(float angle, int nSteps = 0) {
            Tuple<float, float> y = new Tuple<float, float>(1.0f, 1.0f);
            switch(angleToFrequencyLink) {
                case LinkType.Manual:
                    // y = manualLink(angle, manualStepsStereoPanning);
                    break;
                case LinkType.Linear:
                    // y = piecewiseLinearLink(angle, minAngle, maxStereo, maxAngle, minStereo);
                    break;
                case LinkType.Logistic:
                    // y = piecewiseLogisticLink(angle, minAngle, maxStereo, maxAngle, minStereo, 0.5f);
                    break;
            }
            // if(nSteps > 0) return Discretize(y, angle2StereoBreaks);
            // else 
            return y;
        }


        /***************************[ Link functions ]***************************/

        public enum LinkType { Linear, Logistic, Manual };

        double manualLink(float x, IDictionary<float, float> mapping) {
            double[] xSteps = Array.ConvertAll(manualStepsVolume.Keys.Cast<float>().ToArray(), e => (double)e);
            float currentStepX = (float)Discretize((double)x, xSteps);
            return (double)mapping[currentStepX];
        }

        float piecewiseLinearLink(float x, float x0, float y0, float x1, float y1) {
            if(x <= x0) return(y0);
            else if(x >= x1) return(y1);
            else return(y0 + (x - x0) * (y1 - y0) / (x1 - x0));
        }

        /*
        * Avec cette paramétrisation:
        * - Le point d'inflexion est fixé au milieu du range des valeurs d'entrée ( 0.5*(x0 - x1) )
        * - Steepness (définie entre 0 et 1) représente le % du range des valeurs d'entrée ( x1 - x0 ) sur laquelle la courbe des y n'est pas constante.
        *      E.g.: Si x0 = 0 et x1 = 100, alors steepness = 0.1 impliquera que la partie incurvée s'étale sur 0.1 * 100 = 10 (axes des x). 
        *            Et comme le point d'inflexion est centré sur 0.5*(xMax - xMin) = 50, la partie incurvée s'étalera sur [45, 55]. Si x < 45, y = y0, et si x > 55, y = y1
        */
        float piecewiseLogisticLink(float x, float x0, float y0, float x1, float y1, float steepness) {
            if(x <= x0) return(y0);
            else if(x >= x1) return(y1);
            else return(y1 + ( (y0 - y1) / (1 + Mathf.Exp(1 /(steepness * 0.1f*(x1 - x0)) * (x - 0.5f*(x1 - x0))))));
        }

        float MinMax(float x, float fromMin, float fromMax, float toMin, float toMax) {
            return Math.Max(toMin + (x - fromMin) * (toMax - toMin) / (fromMax - fromMin), 0);
        }

        double MinMaxD(double x, double fromMin, double fromMax, double toMin, double toMax) {
            return Math.Max(toMin + (x - fromMin) * (toMax - toMin) / (fromMax - fromMin), 0);
        }

        double Discretize(double y, double[] breaks) {
            double[] added = breaks.Append(y);
            Array.Sort(added);
            int index = Array.FindIndex(added, i => i == y); // Math.Min(Array.FindIndex(added, i => i == y), breaks.Length - 1);
            return(breaks[index]);
        }


        /***********************[ Link functions samplers ]***********************/

        // public delegate double UpdateFunction(float x, int n);

        /**
        *   Uses the Inverse Empirical CDF to find n breakpoints (@param nSteps) in the xrange of a function constrained by equal spacing of their y values (@param data).
        */
        double[] GetQuantiles(double[] data, int nSteps) {
            double[] seq = Generate.LinearSpaced(nSteps + 1, 0.0, 1.0);
            double[] quantiles = new double[nSteps + 1];
            for(int i = 0; i < seq.Length; i++) quantiles[i] = Statistics.Quantile(data, seq[i]);
            return(quantiles);
        }

        /**
        *   Generates samples from a function on a restricted xrange (@params x0 and x1) and uses it to find equally space breakpoints
        */
        double[] GetBreaks(double x0, double x1, int nSamples, Func<float, int, double> uf, int nSteps) {
            double[] xrange = Generate.LinearRange(Math.Min(x0, x1), Math.Abs(x1 - x0)/nSamples, Math.Max(x0, x1));
            double[] yrange = new double[xrange.Length];
            for(int i = 0; i < xrange.Length; i++) yrange[i] = uf((float)xrange[i], 0);
            return GetQuantiles(yrange, nSteps);
        }


        /***************************[ Observer trick ]***************************/

        /** 
        *   Simulating an observer to recalculate the breaks when one of the parameters they depend on changes
        *   TODO: find a way to observe/listen to multiple properties of the Unity Inspector instead of this dirty trick
        *       See: https://blog.terresquall.com/2020/07/organising-your-unity-inspector-fields-with-a-dropdown-filter/
        */
        public void OnValidate() {
            if(distanceToVolume && (_stepsVolume != stepsVolume || _minDist != minDist || _maxDist != maxDist)) {
                _stepsVolume = stepsVolume;
                _minDist = minDist;
                _maxDist = maxDist;
                if(stepsVolume > 0) dist2volBreaks = GetBreaks(minDist, maxDist, samplingAccuracy, UpdateVolume, stepsVolume);
            }
            if(distanceToBip && (_stepsBip != stepsBip || _minDist != minDist || _maxDist != maxDist || _minBipFreq != minBipFreq || _maxBipFreq != maxBipFreq)) {
                _stepsBip = stepsBip;
                _minDist = minDist;
                _maxDist = maxDist;
                _minBipFreq = minBipFreq;
                _maxBipFreq = maxBipFreq;
                if(stepsBip > 0) dist2bipBreaks = GetBreaks(minDist, maxDist, samplingAccuracy, UpdateBipFrequency, stepsBip);
            }
            if(angleToFrequency && (_stepsFreq != stepsFreq || _minAngle != minAngle || _maxAngle != maxAngle || _minFreq != minFreq || _maxFreq != maxFreq)) {
                _stepsFreq = stepsFreq;
                _minAngle = minAngle;
                _maxAngle = maxAngle;
                _minFreq = minFreq;
                _maxFreq = maxFreq;
                if(stepsFreq > 0) angle2FreqBreaks = GetBreaks(minAngle, maxAngle, samplingAccuracy, UpdateFrequency, stepsFreq);
            }
        }

        /***************************************************************************/

        void OnEnable() {
            Debug.Log("OnEnable");
        }

        void OnDisable() {
            _update = false;
            Debug.Log("OnDisable");
        }

        void OnDestroy() {
            _update = false;
            Debug.Log("OnDestroy");
        }
    }

}