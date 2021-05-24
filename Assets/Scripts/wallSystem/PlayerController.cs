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
namespace wallSystem
{
    public class PlayerController : MonoBehaviour
    {
        #region properties
        public Camera Cam;
        private GenerateGenerateWall _gen;
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
                _gen = GameObject.Find("WallCreator").GetComponent<GenerateGenerateWall>();
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
            Cam.transform.localPosition = _controller.center = Vector3.zero;
            _controller.radius = _controller.height = DS.GetData().CharacterData.Radius;

            DS.GetData().AudioData.encoder = DS.GetData().AudioData.encoder.ToLower();
            SSAudioGeneration.addSelectedAudioEncoder(DS.GetData().AudioData,_controller);
            switch (DS.GetData().AudioData.encoder)
            {
                case "horizontal": mover = new HorizontalMover(transform); break;
                case "vertical": mover = new VerticalMover(transform); break;
                default: mover = new ThreeDMover(transform); break;
            }
            mover.fixToPlane(pick);
        }

        // This is the collision system.
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Pickup")) return;

            Destroy(other.gameObject);
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

        private void FixedUpdate()
        {
            mover.rotate();
            _controller.Move(mover.computeMotion());
        }

        private abstract class Mover
        {
            protected Transform transform;
            private Vector3 motion=Vector3.zero;
            protected Vector2 r=Vector2.zero;
            private float speed;
            private float maxYAngle = 90f, minYAngle = -90f;
            protected Mover(Transform t)
            {
                speed =  DS.GetData().CharacterData.MovementSpeed;
                transform = t;
            }
            public abstract void fixToPlane(Vector3 v);

            public void rotate()
            {
                setRotation();
                twoDtoQuaternionRotation();
            }

            public Vector3 computeMotion()
            {
                motion.Set(getSideMotion(), 0, Input.GetAxis("Vertical"));
                return transform.TransformDirection(motion) * speed * Time.deltaTime;
            }

            protected abstract float getSideMotion();

            protected abstract void setRotation();

            private void twoDtoQuaternionRotation()
            {
                transform.rotation = Quaternion.Euler(r.y, r.x, 0);
            }

            protected void setXcurrentRotation()
            {
                r.x += Input.GetAxis("Mouse X");
                r.x = Mathf.Repeat(r.x, 360);
            }

            protected void setYcurrentRotation()
            {
                r.y -= Input.GetAxis("Mouse Y");
                r.y = Mathf.Clamp(r.y, minYAngle, maxYAngle);
            }

        }

        private class HorizontalMover : Mover
        {
            public HorizontalMover(Transform t):base(t)
            {
            }

            public override void fixToPlane(Vector3 v)
            {
                transform.position = new Vector3(transform.position.x, v.y, transform.position.z);
            }

            protected override void setRotation()
            {
                setXcurrentRotation();
            }

            protected override float getSideMotion()
            {
                return Input.GetAxis("Horizontal");
            }

            
        }

        private class VerticalMover : Mover
        {
            public VerticalMover(Transform t) : base(t) { }
            public override void fixToPlane(Vector3 v)
            {
                transform.LookAt(new Vector3(v.x, Random.value * 3, v.z));
                r.Set(transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.x);
            }

            protected override void setRotation()
            {
                setYcurrentRotation();
            }

            protected override float getSideMotion()
            {
                return 0;
            }
        }

        private class ThreeDMover : Mover
        {
            public ThreeDMover(Transform t) : base(t) { }

            public override void fixToPlane(Vector3 v)
            {
            }

            protected override void setRotation()
            {
                setXcurrentRotation();
                setYcurrentRotation();
            }

            protected override float getSideMotion()
            {
                return Input.GetAxis("Horizontal");
            }
        }

    }
}