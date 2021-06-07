using System;
using System.Collections.Generic;
using System.Threading;
using trial;
using UnityEngine;
using UnityEngine.UI;
using data;
using DS = data.DataSingleton;
using E = main.Loader;
using Random = UnityEngine.Random;
using audio;

// This class is the primary player script, it allows the participant to move around.

/// <summary>------------------VBar--------------
/// Adds spatial-audio encoder component, contains the trigger logic for the task completion.
/// Movement constraints added depending on the specified task.
/// TODO : subclasses for trigger? move PD binding to audio interface.
/// </summary>
namespace wallSystem
{
    public class PlayerController : MonoBehaviour
    {
        #region properties
        public Camera Cam;
        // The stream writer that writes data out to an output file.
        private readonly string _outDir;
        // This is the character controller system used for collision
        private CharacterController _controller;
        // The initial move direction is static zero.
        private float _currDelay;
        private float _iniRotation;
        private float _waitTime;
        private bool _isStarted = false;
        private bool _reset;
        private int localQuota;

        Mover mover;
        VictoryTrigger trigger;
        public SSAudioGeneration ssAudio;
        GameObject currentTarget;
        #endregion

        private void Start()
        {
            try
            {
                var trialText = GameObject.Find("TrialText").GetComponent<Text>();
                var blockText = GameObject.Find("BlockText").GetComponent<Text>();
                var currBlockId = E.Get().CurrTrial.BlockID;
                // This section sets the text
                trialText.text = E.Get().CurrTrial.trialData.DisplayText;
                blockText.text = DS.GetData().Blocks[currBlockId].DisplayText;

                if (!string.IsNullOrEmpty(E.Get().CurrTrial.trialData.DisplayImage))
                {
                    var filePath = DS.GetData().SpritesPath + E.Get().CurrTrial.trialData.DisplayImage;
                    var displayImage = GameObject.Find("DisplayImage").GetComponent<RawImage>();
                    displayImage.enabled = true;
                    displayImage.texture = Img2Sprite.LoadTexture(filePath);
                }
            }
            catch (NullReferenceException e)
            {
                Debug.LogWarning("Goal object not set: running an instructional trial");
            }
            Random.InitState(DateTime.Now.Millisecond);
            _currDelay = 0;
            // Choose a random starting angle if the value is not set in config
            if (E.Get().CurrTrial.trialData.StartFacing == -1)
                _iniRotation = Random.Range(0, 360);
            else
                _iniRotation = E.Get().CurrTrial.trialData.StartFacing;
            transform.Rotate(0, _iniRotation, 0);

            try
            {
                _controller = GetComponent<CharacterController>();
                Cam.transform.Rotate(0, 0, 0);
            }
            catch (NullReferenceException e)
            {
                Debug.LogWarning("Can't set controller object: running an instructional trial");
            }
            _waitTime = E.Get().CurrTrial.trialData.Rotate;
            _reset = false;
            localQuota = E.Get().CurrTrial.trialData.Quota;
            // This has to happen here for output to be aligned properly
            TrialProgress.GetCurrTrial().TrialProgress.TrialNumber++;
            TrialProgress.GetCurrTrial().TrialProgress.Instructional = TrialProgress.GetCurrTrial().trialData.Instructional;
            TrialProgress.GetCurrTrial().TrialProgress.EnvironmentType = TrialProgress.GetCurrTrial().trialData.Scene;
            TrialProgress.GetCurrTrial().TrialProgress.CurrentEnclosureIndex = TrialProgress.GetCurrTrial().trialData.Enclosure - 1;
            TrialProgress.GetCurrTrial().TrialProgress.BlockID = TrialProgress.GetCurrTrial().BlockID;
            TrialProgress.GetCurrTrial().TrialProgress.TrialID = TrialProgress.GetCurrTrial().TrialID;
            TrialProgress.GetCurrTrial().TrialProgress.TwoDim = TrialProgress.GetCurrTrial().trialData.TwoDimensional;
            TrialProgress.GetCurrTrial().TrialProgress.LastX = TrialProgress.GetCurrTrial().TrialProgress.TargetX;
            TrialProgress.GetCurrTrial().TrialProgress.LastY = TrialProgress.GetCurrTrial().TrialProgress.TargetY;
            TrialProgress.GetCurrTrial().TrialProgress.TargetX = 0;
            TrialProgress.GetCurrTrial().TrialProgress.TargetY = 0;

            _isStarted = true;
        }

        // Start the character. If init from enclosure, this allows "s" to determine the start position
        public void ExternalStart(Vector3 pick, bool useEnclosure = false)
        {
            while (!_isStarted)
                Thread.Sleep(20);
            TrialProgress.GetCurrTrial().TrialProgress.TargetX = pick.x;
            TrialProgress.GetCurrTrial().TrialProgress.TargetY = pick.z;
            // No start pos specified so make it random.
            if (E.Get().CurrTrial.trialData.StartPosition.Count == 0)
            {
                // Try to randomly place the character, checking for proximity
                // to the pickup location
                var i = 0;
                while (i++ < 100)
                {
                    var CurrentTrialRadius = DS.GetData().Enclosures[E.Get().CurrTrial.TrialProgress.CurrentEnclosureIndex].Radius;
                    var v = Random.insideUnitCircle * CurrentTrialRadius * 0.9f;
                    var mag = Vector3.Distance(v, new Vector2(pick.x, pick.z));
                    if (mag > DS.GetData().CharacterData.DistancePickup)
                    {
                        transform.position = new Vector3(v.x, DS.GetData().CharacterData.Height, v.y);
                        return;
                    }
                }
                Debug.LogError("Could not randomly place player. Probably due to" +
                               " a pick up location setting");
            }
            else
            {
                var p = E.Get().CurrTrial.trialData.StartPosition;
                if (useEnclosure)
                    p = new List<float>() { pick.x, pick.z };
                transform.position = new Vector3(p[0], DS.GetData().CharacterData.Height, p[1]);
            }
            //Set camera and collider positions to GameObject position.
            Cam.transform.localPosition = _controller.center = Vector3.zero;
            //Sets collider's radius and height values according to data. Collider is a cylinder.
            _controller.radius = _controller.height = DS.GetData().CharacterData.Radius;
            //Depending on chosen type of spatial-audio encoder, in 2 (horizontal, vertical) or 3 dimensions, constraints the movement of the character controller.
            switch (DS.GetData().AudioData.encoder)
            {
                case "horizontal": mover = new HorizontalMover(transform); break;
                case "vertical": mover = new VerticalMover(transform); break;
                default: mover = new ThreeDMover(transform); break;
            }
            //If the task is limited to the horizontal or vertical plane, sticks the GameObject to this plane.
            mover.fixToPlane(pick);
            if (DS.GetData().AudioData.victoryDelay > 0)
                trigger = new DelayTrigger(DS.GetData().AudioData.victoryDelay);
            else trigger = new InstantVictoryTrigger();
            ssAudio = SSAudioGeneration.addSelectedAudioEncoder(DS.GetData().AudioData, _controller);
            //For the pureData interface, allows for the task completion audio.
            GetComponent<LibPdInstance>().Bind("end");
        }

        private void doInitialRotation()
        {
            var multiplier = 1.0f;
            // Smooth out the rotation as we approach the values
            var threshold1 = Math.Abs(_currDelay / _waitTime - 0.25f);
            var threshold2 = Math.Abs(_currDelay / _waitTime - 0.75f);
            if (threshold1 < 0.03 || threshold2 < 0.03)
                return;
            if (_currDelay / _waitTime > 0.25 && _currDelay / _waitTime < 0.75)
                multiplier *= -1;
            var anglePerSecond = 240 / _waitTime;
            var angle = Time.deltaTime * anglePerSecond;
            transform.Rotate(new Vector3(0, multiplier * angle, 0));
        }

        private void Update()
        {
            E.LogData(TrialProgress.GetCurrTrial().TrialProgress, TrialProgress.GetCurrTrial().TrialStartTime, transform);
            // This first block is for the initial rotation of the character
            if (_currDelay < _waitTime)
                doInitialRotation();
            else
            {
                // This section rotates the camera (potentiall up 15 degrees), basically deprecated code.
                if (!_reset)
                {
                    Cam.transform.Rotate(0, 0, 0);
                    _reset = true;
                    TrialProgress.GetCurrTrial().ResetTime();
                }
            }
            _currDelay += Time.deltaTime;
        }

        // ----VBar----- : added or modified
        private void FixedUpdate()
        {
            mover.rotate();
            _controller.Move(mover.computeMotion());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Pickup")) return;
            if (trigger.enter())
                ssAudio.playReachedTargetAudio();
            currentTarget = other.gameObject;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.gameObject.CompareTag("Pickup")) return;
            if (trigger.stay())
                ssAudio.playReachedTargetAudio();
        }

        private void OnTriggerExit(Collider other)
        {
            trigger.exits();
        }

        public void receiveEndBang(string sender)
        {
            Destroy(currentTarget);
            Destroy(ssAudio);
            // Tally the number collected per current block
            int BlockID = TrialProgress.GetCurrTrial().BlockID;
            TrialProgress.GetCurrTrial().TrialProgress.NumCollectedPerBlock[BlockID]++;
            TrialProgress.GetCurrTrial().NumCollected++;
            E.LogData(
                TrialProgress.GetCurrTrial().TrialProgress,
                TrialProgress.GetCurrTrial().TrialStartTime,
                transform,
                1
            );

            if (--localQuota > 0) return;

            E.Get().CurrTrial.Notify();
            E.LogData(TrialProgress.GetCurrTrial().TrialProgress, TrialProgress.GetCurrTrial().TrialStartTime, transform);
            TrialProgress.GetCurrTrial().Progress();
        }

        private interface VictoryTrigger
        {
            bool enter();
            bool stay();
            void exits();
        }

        private class InstantVictoryTrigger : VictoryTrigger
        {
            public InstantVictoryTrigger()
            {

            }

            public bool enter()
            {
                return true;
            }

            public bool stay() { return false; }
            public void exits()
            {
            }
        }

        private class DelayTrigger : VictoryTrigger
        {
            int ms;
            float timeElapsed;
            public DelayTrigger(int ms)
            {
                this.ms = ms;
            }

            public bool enter()
            {
                timeElapsed = Time.time;
                return false;
            }

            public bool stay()
            {
                return Time.time - timeElapsed >= ms;
            }

            public void exits() { timeElapsed = 0; }


        }

    }
}