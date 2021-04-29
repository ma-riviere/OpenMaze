using UnityEngine;

namespace audio
{
    public class SSAudioPD : MonoBehaviour
    {
        LibPdInstance puredataInstance;
        Camera user;
        GameObject target;

        public Vector3 userToTargetVector, verticalPointedDirection;
        private Ray pointedDirection;
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
        }

        // Update is called once per frame
        void Update()
        {
            userToTargetVector = target.transform.position - user.transform.position;
            Debug.DrawLine(target.transform.position, user.transform.position, Color.green);
            userToTargetHorizontalVector.Set(userToTargetVector.x, userToTargetVector.z);
            pointedDirection = user.ScreenPointToRay(Input.mousePosition);
            horizontalPointedDirection.Set(pointedDirection.direction.x, pointedDirection.direction.z);
            Vector3 normalVertical = Vector3.Cross(userToTargetVector, new Vector3(userToTargetVector.x, 0, userToTargetVector.z));
            verticalPointedDirection = Vector3.ProjectOnPlane(pointedDirection.direction, normalVertical);
            Debug.DrawRay(pointedDirection.origin, pointedDirection.direction, Color.red);
            distance = userToTargetVector.sqrMagnitude;
            horizontalAngle = Vector2.SignedAngle(userToTargetHorizontalVector, horizontalPointedDirection);
            verticalAngle = Vector3.SignedAngle(userToTargetVector, verticalPointedDirection, normalVertical);

            if (horizontalAngle < 90 && horizontalAngle > -90)
            {
                if (horizontalAngle > 15)
                {
                    puredataInstance.SendFloat("left", 0);
                    puredataInstance.SendFloat("right", 1);
                }
                else if (horizontalAngle < -15)
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

            if (verticalAngle < 15 && verticalAngle > -15)
                frequency = 220;
            else if (verticalAngle > 15)
                frequency = 110;
            else frequency = 440;
            //frequency = Mathf.Exp(Mathf.Log(1 / verticalAngle)) + 100;
            //Debug.Log(userToTargetVerticalVector + " " + r.direction + " vAngle :" + verticalAngle + " frequence:" + freq);
            //Debug.DrawRay(r.origin, r.direction * 10, Color.red);
            //Debug.DrawLine(user.transform.position, target.transform.position, Color.green);
            puredataInstance.SendFloat("freq", frequency);
            Debug.Log(horizontalAngle + " " + verticalAngle + " " + distance);

            /*
            if (distance < 1)
                puredataInstance.SendFloat("gain", 50);
            else puredataInstance.SendFloat("gain", 20);*/

        }
    }
}