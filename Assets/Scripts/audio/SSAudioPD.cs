using UnityEngine;

namespace audio
{
    public class SSAudioPD : MonoBehaviour
    {
        private const int HIGH_FREQ = 440, LOW_FREQ = 110, MED_FREQ = 220;
        private const int ANGLE_THRESHOLD = 15, HORIZONTAL_MAX_ANGLE = 90;

        LibPdInstance puredataInstance;
        Camera user;
        GameObject target;
        SphereCollider collider;

        public Vector3 userToTargetVector, verticalPointedDirection;
        Vector3 normalVertical;
        private Ray pointedDirection;
        private RaycastHit hit;
        bool pointsTarget;
        public Vector2 userToTargetHorizontalVector, horizontalPointedDirection;
        private float distance;
        private float horizontalAngle, verticalAngle;
        private float frequency;

        // Use this for initialization
        void Awake()
        {
            puredataInstance = GetComponent<LibPdInstance>();
            user = Camera.main;
            target = GameObject.FindWithTag("Pickup");
            collider = target.GetComponent<SphereCollider>();
        }

        // Update is called once per frame
        void Update()
        {
            userToTargetVector = target.transform.position - user.transform.position;
            pointedDirection = user.ScreenPointToRay(Input.mousePosition);
            pointsTarget = collider.Raycast(pointedDirection, out hit, 500);
            userToTargetHorizontalVector.Set(userToTargetVector.x, userToTargetVector.z);
            horizontalPointedDirection.Set(pointedDirection.direction.x, pointedDirection.direction.z);
            normalVertical = Vector3.Cross(userToTargetVector, new Vector3(userToTargetVector.x, 0, userToTargetVector.z));
            verticalPointedDirection = Vector3.ProjectOnPlane(pointedDirection.direction, normalVertical);
            horizontalAngle = Vector2.SignedAngle(userToTargetHorizontalVector, horizontalPointedDirection);
            verticalAngle = Vector3.SignedAngle(userToTargetVector, verticalPointedDirection, normalVertical);
            distance = userToTargetVector.sqrMagnitude;

            if (pointsTarget)
                puredataInstance.SendFloat("hits", 1);
            else puredataInstance.SendFloat("hits", 0);

            if (horizontalAngle < HORIZONTAL_MAX_ANGLE && horizontalAngle > -HORIZONTAL_MAX_ANGLE)
            {
                if (horizontalAngle > ANGLE_THRESHOLD)
                {
                    puredataInstance.SendFloat("left", 0);
                    puredataInstance.SendFloat("right", 1);
                }
                else if (horizontalAngle < -ANGLE_THRESHOLD)
                {
                    puredataInstance.SendFloat("left", 1);
                    puredataInstance.SendFloat("right", 0);
                }
                else
                {
                    puredataInstance.SendFloat("left", 1);
                    puredataInstance.SendFloat("right", 1);
                }
            }
            else
            {
                puredataInstance.SendFloat("left", 0);
                puredataInstance.SendFloat("right", 0);
            }

            if (verticalAngle < ANGLE_THRESHOLD && verticalAngle > -ANGLE_THRESHOLD)
                frequency = MED_FREQ;
            else if (verticalAngle > ANGLE_THRESHOLD)
                frequency = LOW_FREQ;
            else frequency = HIGH_FREQ;

            puredataInstance.SendFloat("freq", frequency);

            /*
            if (distance < 1)
                puredataInstance.SendFloat("gain", 50);
            else puredataInstance.SendFloat("gain", 20);
            */

            Debug.DrawLine(target.transform.position, user.transform.position, Color.green);
            Debug.DrawRay(pointedDirection.origin, pointedDirection.direction, Color.red);
            Debug.Log(distance);
        }
    }
}