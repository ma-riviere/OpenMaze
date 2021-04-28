using UnityEngine;

namespace audio
{
    public class SSAudioPD : MonoBehaviour
    {
        private const float freqCoef = 5;

        LibPdInstance puredataInstance;
        Camera user;
        GameObject target;
        Collider targetCol;

        public Vector3 userToTargetVector;
        private Ray pointedDirection;
        public Vector2 userToTargetVerticalVector, userToTargetHorizontalVector, verticalPointedDirection, horizontalPointedDirection;
        private float distance;
        private float horizontalAngle, verticalAngle;

        private float frequency;

        // Use this for initialization
        void Awake()
        {
            puredataInstance = GetComponent<LibPdInstance>();
            user = Camera.main;
            target = GameObject.FindWithTag("Pickup");
            targetCol = target.GetComponent<Collider>();
            distance = Vector3.Distance(
               targetCol.ClosestPoint(user.transform.position),
               user.transform.position
           );
            userToTargetVector = target.transform.position - user.transform.position;
            pointedDirection = user.ScreenPointToRay(Input.mousePosition);
            userToTargetVerticalVector = new Vector2(userToTargetVector.y - pointedDirection.direction.y, userToTargetVector.z - pointedDirection.direction.z);
            userToTargetHorizontalVector = new Vector2(userToTargetVector.x - pointedDirection.direction.x, userToTargetVector.z - pointedDirection.direction.z);
            verticalPointedDirection = new Vector2(pointedDirection.direction.y, pointedDirection.direction.z);
            horizontalPointedDirection = new Vector2(pointedDirection.direction.x, pointedDirection.direction.z);
        }

        // Update is called once per frame
        void Update()
        {
            userToTargetVector = target.transform.position - user.transform.position;
            userToTargetHorizontalVector.Set(userToTargetVector.x, userToTargetVector.z);
            userToTargetVerticalVector.Set(userToTargetVector.y, userToTargetVector.z);
            pointedDirection = user.ScreenPointToRay(Input.mousePosition);
            verticalPointedDirection.Set(pointedDirection.direction.y, pointedDirection.direction.z);
            horizontalPointedDirection.Set(pointedDirection.direction.x, pointedDirection.direction.z);

            horizontalAngle = Vector2.SignedAngle(userToTargetHorizontalVector, horizontalPointedDirection);
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

            verticalAngle = Vector2.SignedAngle(userToTargetVerticalVector, verticalPointedDirection);
            if (verticalAngle < 15 && verticalAngle > -15)
                frequency = 200;
            else if (verticalAngle > 15)
                frequency = 100;
            else frequency = 400;
            //frequency = Mathf.Exp(Mathf.Log(1 / verticalAngle)) + 100;
            //Debug.Log(userToTargetVerticalVector + " " + r.direction + " vAngle :" + verticalAngle + " frequence:" + freq);
            //Debug.DrawRay(r.origin, r.direction * 10, Color.red);
            //Debug.DrawLine(user.transform.position, target.transform.position, Color.green);
            puredataInstance.SendFloat("freq", frequency);
            Debug.Log(horizontalAngle+" "+verticalAngle+" "+frequency+" "+ userToTargetVector.sqrMagnitude);

            /*distance = userToTargetVector.sqrMagnitude;
            if (distance < 1)
                puredataInstance.SendFloat("gain", 50);
            else puredataInstance.SendFloat("gain", 20);*/

        }
    }
}