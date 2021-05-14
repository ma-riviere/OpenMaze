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
        private Vector3 _moveDirection = Vector3.zero;
        private float _currDelay;
        private float _iniRotation;
        private float _waitTime;
        private bool _isStarted = false;
        private bool _reset;
        private int localQuota;

        private float movSpeed;
        bool _playingSound;

        private const int HIGH_FREQ = 440, LOW_FREQ = 110, MED_FREQ = 220;
        private const int ANGLE_THRESHOLD = 15, HORIZONTAL_MAX_ANGLE = 80;
        private const int OPPOSITE_ANGLE_THRESHOLD = -ANGLE_THRESHOLD, OPPOSITE_HOR_MAX_ANGLE = -HORIZONTAL_MAX_ANGLE;
        float min = 0.01f;

        private LibPdInstance puredataInstance;
        private GameObject target;
        private SphereCollider targetCollider;

        private Vector3 userToTargetVector, verticalPointedDirection;
        private Vector3 targetCenterOnScreen, mousePosition, verticalPoint;
        private Ray pointedDirection;
        private RaycastHit hit;
        private bool pointsTarget, previousPointsTarget;
        private Vector2 userToTargetHorizontalVector, horizontalPointedDirection;
        private float distance;
        private float horizontalAngle, verticalAngle;
        private float frequency, previousFrequency;
        private bool previousLeft, previousRight;

        public float sensitivity = 1f;
        public float maxYAngle = 90f, minYAngle = -90f;
        private Vector2 currentRotation;
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

            puredataInstance = GetComponent<LibPdInstance>();
            pointsTarget = previousPointsTarget = false;
            previousLeft = previousRight = false;
            frequency = 0;
            verticalPoint = new Vector3(targetCenterOnScreen.x, mousePosition.y, targetCenterOnScreen.z);
            movSpeed = DS.GetData().CharacterData.MovementSpeed;
            mousePosition = new Vector3(Cam.pixelWidth / 2, Cam.pixelHeight / 2, 0);
        }

        // Start the character. If init from enclosure, this allows "s" to determine the start position
        public void ExternalStart(float pickX, float pickY, bool useEnclosure = false)
        {
            while (!_isStarted)
                Thread.Sleep(20);
            TrialProgress.GetCurrTrial().TrialProgress.TargetX = pickX;
            TrialProgress.GetCurrTrial().TrialProgress.TargetY = pickY;
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
                    var mag = Vector3.Distance(v, new Vector2(pickX, pickY));
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
                    p = new List<float>() { pickX, pickY };
                transform.position = new Vector3(p[0], DS.GetData().CharacterData.Height, p[1]);
            }
            Cam.transform.localPosition = _controller.center = Vector3.zero;
            _controller.radius = _controller.height = DS.GetData().CharacterData.Radius;
            target = GameObject.FindWithTag("Pickup");
            targetCollider = target.GetComponent<SphereCollider>();
        }

        // This is the collision system.
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Pickup")) return;

            GetComponent<AudioSource>().PlayOneShot(other.gameObject.GetComponent<PickupSound>().Sound, 10);
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
            updateAudio();
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
            pointedDirection = Cam.ScreenPointToRay(mousePosition);
            pointedDirection.origin = transform.position;
            pointsTarget = targetCollider.Raycast(pointedDirection, out hit, 1000);
            // This calculates the current amount of rotation frame rate independent
            // This calculates the forward speed frame rate independent
            _moveDirection.Set(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            _moveDirection = transform.TransformDirection(_moveDirection) * movSpeed;
            _controller.Move(_moveDirection * Time.deltaTime);
            // Here is the movement system
            // We move iff rotation is 0
            currentRotation.x += Input.GetAxis("Mouse X") * sensitivity;
            currentRotation.y -= Input.GetAxis("Mouse Y") * sensitivity;
            currentRotation.x = Mathf.Repeat(currentRotation.x, 360);
            currentRotation.y = Mathf.Clamp(currentRotation.y, minYAngle, maxYAngle);
            transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
        }

        private void updateAudio()
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
                userToTargetHorizontalVector.Set(userToTargetVector.x, userToTargetVector.z);
                horizontalPointedDirection.Set(pointedDirection.direction.x, pointedDirection.direction.z);
                targetCenterOnScreen = Cam.WorldToScreenPoint(target.transform.position);
                verticalPoint.Set(targetCenterOnScreen.x, mousePosition.y, targetCenterOnScreen.z);
                verticalPointedDirection = Cam.ScreenPointToRay(verticalPoint).direction;
                horizontalAngle = Vector2.SignedAngle(userToTargetHorizontalVector, horizontalPointedDirection);
                verticalAngle = Vector3.Angle(userToTargetVector, verticalPointedDirection);
                distance = userToTargetVector.magnitude - targetCollider.radius-_controller.radius;

                if (horizontalAngle < HORIZONTAL_MAX_ANGLE && horizontalAngle > OPPOSITE_ANGLE_THRESHOLD)
                {
                    if (!previousRight)
                    {
                        puredataInstance.SendFloat("right", 1);
                        previousRight = true;
                    }
                }
                else if (previousRight)
                {
                    puredataInstance.SendFloat("right", min);
                    previousRight = false;
                }
                if (horizontalAngle > OPPOSITE_HOR_MAX_ANGLE && horizontalAngle < ANGLE_THRESHOLD)
                {
                    if (!previousLeft)
                    {
                        puredataInstance.SendFloat("left", 1);
                        previousLeft = true;
                    }
                }
                else if (previousLeft)
                {
                    puredataInstance.SendFloat("left", min);
                    previousLeft = false;
                }

                if (verticalAngle < ANGLE_THRESHOLD)
                    frequency = MED_FREQ;
                else if (mousePosition.y < targetCenterOnScreen.y)
                    frequency = HIGH_FREQ;
                else frequency = LOW_FREQ;

                if (frequency != previousFrequency)
                {
                    puredataInstance.SendFloat("freq", frequency);
                    previousFrequency = frequency;
                }

                if (distance < 2)
                    puredataInstance.SendFloat("gain", 3.0f/127.0f);
                else puredataInstance.SendFloat("gain", 1.0f/127.0f);
            }
            
        }

        private void LateUpdate()
        {
            Debug.DrawRay(transform.position, userToTargetVector, Color.green);
            Debug.DrawRay(pointedDirection.origin, pointedDirection.direction, Color.red);
            Debug.DrawRay(Cam.transform.position, verticalPointedDirection, Color.blue);
            //Debug.Log(distance);
        }

    }
}